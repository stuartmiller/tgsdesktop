
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'paymentMethodId' and Object_ID = Object_ID(N'tbl_salesInvoice')) BEGIN
    ALTER TABLE tbl_salesInvoice DROP COLUMN paymentMethodId
END
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'paymentTxnId' and Object_ID = Object_ID(N'tbl_salesInvoice')) BEGIN

	ALTER TABLE tbl_salesInvoice DROP CONSTRAINT [fk_invoice_paymentTxnId]
    ALTER TABLE tbl_salesInvoice DROP COLUMN paymentTxnId
END;

-- RENAME tbl_product
--IF OBJECT_ID('tbl_product') IS NOT NULL BEGIN
--	sp_rename @objname='tbl_product', @newName='tbl_salesItem';
--END
--GO;

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'itemId' and Object_ID = Object_ID(N'tbl_salesInvoiceItem')) BEGIN
    ALTER TABLE dbo.tbl_salesInvoiceItem ADD itemId int NULL
	ALTER TABLE [dbo].[tbl_salesInvoiceItem]  WITH CHECK ADD  CONSTRAINT [fk_salesInvoiceItem_itemId] FOREIGN KEY([itemId])
	REFERENCES [dbo].[tbl_salesItem] ([id])
	ALTER TABLE [dbo].[tbl_salesInvoiceItem] CHECK CONSTRAINT [fk_salesInvoiceItem_itemId]
END
GO
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'discount' and Object_ID = Object_ID(N'tbl_salesInvoiceItem')) BEGIN
    ALTER TABLE dbo.tbl_salesInvoiceItem ADD discount MONEY NOT NULL DEFAULT 0
END
GO






ALTER PROC dbo.proc_addTransactionJournalEntries(
	@invoiceNo nvarchar(100)=NULL,
	@accounts dbo.udt_accountJournalEntry READONLY,
	@payments dbo.udt_payment READONLY,
	@postDate datetime=NULL OUTPUT,
	@effectiveDate date=NULL OUTPUT,
	@txnMemo nvarchar(300)=NULL,
	@txnId int=NULL OUTPUT,
	@userId INT,
	@version rowversion=NULL OUTPUT
) AS

--SELECT * FROM @accounts
--SELECT * FROM @payments	

DECLARE @trancount int, @error int, @message varchar(4000), @xstate int;
SET @trancount=@@TRANCOUNT
BEGIN TRY

	IF NOT EXISTS (SELECT * FROM @accounts)
		RAISERROR('There are no journal entries.',6,1)
	
	IF @trancount=0 BEGIN TRAN
	ELSE SAVE TRAN addJournalEntry;
	SET NOCOUNT ON

	IF @txnId<1 SET @txnId=NULL
	IF @txnId IS NOT NULL
		RAISERROR('Cannot modify an existing transaction.', 16,1)

	-- check to make sure that the person ids are valid
	IF EXISTS(SELECT * FROM @accounts WHERE personId IS NOT NULL) BEGIN
		IF EXISTS(SELECT * FROM @accounts a LEFT OUTER JOIN tbl_person p ON a.personId=p.id WHERE a.personId IS NOT NULL AND p.id IS NULL) BEGIN
			RAISERROR('Invalid person id or cmPersonId.',16,1)
		END
	END
	
	-- declare a temp table, sense the one passed in is read only
	DECLARE @temp dbo.udt_accountJournalEntry

	-- insert all the records that have valid accountIds
	INSERT INTO @temp(accountId, seasonId, amount, isCredit, personId, memo)
	SELECT a.accountId, a.seasonId, ABS(a.amount), a.isCredit, a.personId, a.memo
	FROM @accounts a
		INNER JOIN tbl_account act ON a.accountId=act.id

	-- verify that the count is the same
	IF (SELECT COUNT(*) FROM @accounts)<>(SELECT COUNT(*) FROM @temp)
		RAISERROR('Incorrect account ids passed in.', 16,1)
	-- verify that the amount is the same
	IF (SELECT SUM (amount) FROM @temp)<>(SELECT SUM(amount) FROM @accounts)
		RAISERROR('Inconsistant amount.', 16,1)
	-- if payments are included, they must match the debit amount of the transaction
	IF EXISTS(SELECT * FROM @payments) BEGIN
		DECLARE @pmtTotal money, @dAmt money, @totalAmt money
        SELECT @pmtTotal=SUM(amount) FROM @payments
		-- make sure to account for sales discounts, id 131
		SELECT @dAmt=amount FROM @temp WHERE accountId=131
		SELECT @totalAmt=SUM(amount) FROM @temp WHERE isCredit=0
		--SELECT @pmtTotal, @dAmt, @totalAmt
		IF ((@totalAmt-ISNULL(@dAmt,0))<>@pmtTotal)
			RAISERROR('Payment amount much match transaction debit amount.',16,1)
	END

	-- make sure that if a receivable is passed in that there is a person associated with it
	IF EXISTS(SELECT * FROM @accounts WHERE accountId=101 AND personId IS NULL)
		RAISERROR('AccountsReceivable must have a person associated with it',16,1)

	DECLARE @journal TABLE (
		accountId int NOT NULL,
		seasonId int NOT NULL,
		signedAmt money NOT NULL,
		personId int NULL,
		memo nvarchar(300) NULL
	)

	INSERT INTO @journal (accountId, seasonId, signedAmt, personId, memo)
	SELECT t.accountId, CASE WHEN t.seasonId IS NULL THEN (SELECT TOP 1 currentSeasonId FROM tbl_settings) ELSE t.seasonId END,
		SUM(ABS(t.amount))*CASE WHEN t.isCredit=1 THEN -1 ELSE 1 END, personId, t.memo
	FROM @temp t
	WHERE t.amount<>0
	GROUP BY t.accountId, t.seasonId, t.isCredit, t.accountId, t.personId, t.memo

	IF (SELECT SUM(signedAmt) FROM @journal)<>0 BEGIN
		RAISERROR('Accounts are not in balance', 16,1)
		RETURN 1
	END

	SET @postDate=GETUTCDATE()
	IF @effectiveDate IS NULL
		SET @effectiveDate=CAST(@postDate AS date)

	SET NOCOUNT OFF

	INSERT INTO tbl_transaction (postDateUtc, effectiveDate, invoiceNo, memo, userId)
	VALUES (@postDate, @effectiveDate, @invoiceNo, @txnMemo, @userId)

	SELECT @txnId=SCOPE_IDENTITY()
	SELECT @version=version FROM dbo.tbl_transaction WHERE id=@txnId

	INSERT INTO tbl_generalJournal (txnID, seasonId, signedAmt, accountId, personId, memo)
	SELECT @txnId, t.seasonId, t.signedAmt, t.accountId, t.personId, t.memo
	FROM @journal t

	INSERT INTO tbl_payment (txnId, methodId, amount, checkNo, orderIndex)
	SELECT @txnId, methodId, amount, checkNo, 0
	FROM @payments

	-- insert any cmTransactions
	INSERT INTO tbl_cmTransaction (gjId, seasonId, cmEffectiveDate, cmIsCredit, cmAmount, cmAccountBillingItemId, cmPersonId, cmMemo)
	SELECT DISTINCT gjAr.id, gjAr.seasonId, txn.effectiveDate, CASE WHEN gjAr.signedAmt<1 THEN 1 ELSE 0 END, ABS(gjAr.signedAmt), m.cmAccountBillingItemId, p.cmPersonId, gj.memo
	FROM tbl_generalJournal gj
		INNER JOIN tbl_generalJournal gjAr ON gj.txnId=gjAr.txnId AND gjAr.accountId=101 AND gj.personId=gjAr.personId
		INNER JOIN tbl_transaction txn ON gj.txnId=txn.id
		INNER JOIN tbl_person p ON gj.personId=p.id
		INNER JOIN tbl_cmCamperSeason cs ON p.id=cs.personId AND cs.seasonId=gj.seasonId
		INNER JOIN tbl_tgsToCmAccountMapping m ON gj.accountId=m.accountId
		LEFT OUTER JOIN tbl_cmTransaction cmT ON gj.id=cmT.gjId
	WHERE gj.txnId=@txnId
		AND cmT.id IS NULL
	
	IF @trancount=0
		COMMIT TRAN

END TRY
BEGIN CATCH

	SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
	IF @xstate = -1 BEGIN
		ROLLBACK;
		--THROW
	END
	ELSE IF @xstate = 1 AND @trancount = 0 BEGIN
		ROLLBACK;
		--THROW
	END
	ELSE IF @xstate = 1 AND @trancount > 0
		ROLLBACK TRAN addJournalEntry;
	RAISERROR ('proc_addJournalEntry: %d: %s', 16, 1, @error, @message);
END CATCH


GO




IF OBJECT_ID('proc_addSalesInvoice') IS NOT NULL
	DROP PROC proc_addSalesInvoice
GO
DROP TYPE udt_salesInvoiceItem
IF TYPE_ID(N'udt_salesInvoiceItem') IS NULL
    CREATE TYPE [dbo].[udt_salesInvoiceItem] AS TABLE(
        [description] nvarchar(100) NOT NULL,
		productId int NULL,
		unitPrice money NOT NULL,
		unitCost money NULL,
		quantity int NOT NULL,
		isTaxable bit NOT NULL,
		discount money NULL DEFAULT 0
    )
GO
IF TYPE_ID(N'udt_payment') IS NULL
    CREATE TYPE [dbo].[udt_payment] AS TABLE(
		methodId int NOT NULL, -- The payment type 1 = cash, 2 = check, 3 = amex, 4 = visa, 5 = mc, 6 = discover, 7 = account
		amount money NOT NULL,
		checkNo nvarchar(50) NULL
    )
GO
CREATE PROC dbo.proc_addSalesInvoice (
	@id INT = NULL OUTPUT,
	@invoiceNo nvarchar(100) = NULL,
	@seasonId int = NULL OUTPUT,
	@postDateUtc datetime = NULL OUTPUT,
	@effectiveDate date = NULL OUTPUT,
	@txnMemo nvarchar(300),
	@items dbo.udt_salesInvoiceItem READONLY,
	@payments dbo.udt_payment READONLY,
	@personId int=NULL,
	@salesTax money=NULL OUTPUT,
	@txnId int=NULL OUTPUT,
	@userId INT,
	@txnVerison rowversion = NULL OUTPUT
) AS

DECLARE @trancount int, @error int, @message varchar(4000), @xstate int;
BEGIN TRY

	SET NOCOUNT ON

	IF EXISTS(SELECT * FROM @payments WHERE methodId=7) AND @personId IS NULL
		RAISERROR('PersonId is needed when Account is specified in the payment methods.',16,1)
	IF @personId IS NOT NULL AND NOT EXISTS(SELECT * FROM tbl_person WHERE id=@personId)
		RAISERROR('Invalid person.',16,1)

	DECLARE @taxableSales money, @nontaxableSales money, @total money, @discount money
	SELECT @taxableSales=SUM(unitPrice) FROM @items WHERE isTaxable=1
	SELECT @nontaxableSales=SUM(unitPrice) FROM @items WHERE isTaxable=0
	IF @salesTax IS NULL
		SELECT @salesTax=ROUND(@taxableSales*salesTaxRate,2) FROM tbl_settings
	SELECT @total=ISNULL(@taxableSales,0) + ISNULL(@nontaxableSales,0) + ISNULL(@salesTax,0)
	SELECT @discount=SUM(ISNULL(discount,0)) FROM @items

	-- get the total of payments
	DECLARE @paymentTotal money
	SELECT @paymentTotal=SUM(amount) FROM @payments

	-- payments must match the total invoice
	IF (@total-ISNULL(@discount,0))<>@paymentTotal
		RAISERROR('Payment total must match the invoice total.',16,1)

	IF ISNULL(@discount,0)>@taxableSales
		RAISERROR('Discount cannot be greater than the taxable amount',16,1)

	DECLARE @journalEntries [dbo].[udt_accountJournalEntry]

	-- add the taxable sales
	IF ISNULL(@taxableSales,0)<>0
		INSERT INTO @journalEntries(accountId, seasonId, amount, isCredit, personId)
		VALUES (120, @seasonId, ABS(@taxableSales), CASE WHEN @taxableSales<0 THEN 0 ELSE 1 END, @personId)
	-- add the non-taxable sales
	IF ISNULL(@nontaxableSales,0)<>0
		INSERT INTO @journalEntries(accountId, seasonId, amount, isCredit, personId)
		VALUES (121, @seasonId, ABS(@nontaxableSales), CASE WHEN @nontaxableSales<0 THEN 0 ELSE 1 END, @personId)
	-- add the discounts (it's a contra account, so it's the opposite of a sale)
	IF ISNULL(@discount,0)<>0
		INSERT INTO @journalEntries(accountId, seasonId, amount, isCredit, personId)
		VALUES (131, @seasonId, ABS(@discount), CASE WHEN @discount<0 THEN 1 ELSE 0 END, @personId)
	-- add the sales tax
	IF ISNULL(@salesTax,0)<>0
		INSERT INTO @journalEntries(accountId, seasonId, amount,	isCredit, personId)
		VALUES (117, @seasonId, ABS(@salesTax), CASE WHEN @salesTax<0 THEN 0 ELSE 1 END, @personId)

	-- add the payments
	INSERT INTO @journalEntries(accountId, seasonId, amount, isCredit, personId)
	SELECT CASE WHEN methodId=7 THEN 101 ELSE 100 END, @seasonId, SUM(ABS(amount)),
		CASE WHEN amount<0 THEN 1 ELSE 0 END, CASE WHEN methodId=7 THEN @personId ELSE NULL END
	FROM @payments
	GROUP BY methodID, amount

	--SELECT je.*, a.name FROM @journalEntries je INNER JOIN tbl_account a ON je.accountId=a.id
	--SELECT * FROM @payments
	--RETURN

	IF ISNULL(@seasonId,0)<1
		SELECT @seasonId=currentSeasonId FROM dbo.tbl_settings

	SET @trancount=@@TRANCOUNT
	IF @trancount=0 BEGIN TRAN

	SET NOCOUNT OFF

	EXEC proc_addTransactionJournalEntries
		@accounts=@journalEntries,
		@payments=@payments,
		@postDate=@postDateUtc OUTPUT,
		@effectiveDate=@effectiveDate OUTPUT,
		@txnMemo=@txnMemo,
		@txnId=@txnId OUTPUT,		
		@userId=@userId,
		@version=@txnVerison OUTPUT

	INSERT INTO tbl_salesInvoice (txnId, invoiceNo, personId, seasonId)
	VALUES (@txnId, @invoiceNo, @personId, @seasonId)
	SELECT @id=SCOPE_IDENTITY()

	INSERT INTO tbl_salesInvoiceItem ([description], invoiceId, productId, quantity, unitCost, unitPrice, isTaxable, discount)
	SELECT [description], @id, productId, quantity, unitCost, unitPrice, isTaxable, discount
	FROM @items

	IF @trancount=0
		COMMIT TRAN
END TRY
BEGIN CATCH
	SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
	IF @xstate = -1 BEGIN
		ROLLBACK;
		--THROW
	END
	ELSE IF @xstate = 1 AND @trancount = 0 BEGIN
		ROLLBACK;
		--THROW
	END
	ELSE IF @xstate = 1 AND @trancount > 0
		ROLLBACK TRAN addJournalEntry;
	RAISERROR ('proc_addSalesInvoice: %d: %s', 16, 1, @error, @message);
END CATCH

GO

/*

SELECT MAX(id) FROM tbl_transaction
SELECT TOP 10 * FROM tbl_transaction ORDER BY id DESC
SELECT gj.*, a.name AS accountName FROM tbl_generalJournal gj INNER JOIN tbl_account a ON gj.accountId=a.id WHERE txnId=1578
SELECT pmt.* FROM tbl_payment pmt WHERE pmt.txnId=1578

SELECT RAND(100), RAND(), RAND() 

*/



/*
*************************** CASH PAYMENT ***************************

DECLARE @invoiceNo nvarchar(50), @items dbo.udt_salesInvoiceItem, @payments dbo.udt_payment, @postDate datetime, @effectiveDate datetime, @txnId int, @id int,
	@checkNo nvarchar(50), @txnMemo nvarchar(50)
INSERT INTO @items([description], productId, unitPrice, unitCost, quantity, isTaxable)
VALUES ('Some sales item', NULL, 5, NULL, 1, 1)
INSERT INTO @payments(methodId, amount) VALUES (1, 5.34)

SELECT @invoiceNo=CAST(RAND() AS nvarchar(50))
SET @txnMemo = 'Sale for invoice ' + @invoiceNo
EXEC proc_addSalesInvoice
	@id=@id OUTPUT,
	@invoiceNo=@invoiceNo,
	@seasonId=2015,
	@postDateUtc=@postDate OUTPUT,
	@effectiveDate=@effectiveDate OUTPUT,
	@txnMemo='Sale for invoice #123456',
	@items=@items,
	@payments=@payments,
	@personId=NULL,
	@salesTax=0.34,
	@txnId=@txnId OUTPUT,
	@userId=100
SELECT @invoiceNo AS invoiceNo, @id AS invoiceId, @txnId AS txnId, @postDate AS postDate, @effectiveDate AS effectiveDate
SELECT * FROM tbl_transaction WHERE id=@txnId
SELECT gj.*, a.name AS accountName FROM tbl_generalJournal gj INNER JOIN tbl_account a ON gj.accountId=a.id WHERE txnId=@txnId
SELECT * FROM tbl_salesInvoice WHERE txnId=@txnId
SELECT * FROM tbl_salesInvoiceItem WHERE invoiceId=@id
SELECT * FROM tbl_payment WHERE txnId=@txnId
*/


/*
*************************** CHECK PAYMENT ***************************

DECLARE @invoiceNo nvarchar(50), @items dbo.udt_salesInvoiceItem, @payments dbo.udt_payment, @postDate datetime, @effectiveDate datetime, @txnId int, @id int,
	@checkNo nvarchar(50), @txnMemo nvarchar(50)

INSERT INTO @items([description], productId, unitPrice, unitCost, quantity, isTaxable, discount)
VALUES ('Some sales item', NULL, 5, NULL, 1, 1, 5)
INSERT INTO @items([description], productId, unitPrice, unitCost, quantity, isTaxable)
VALUES ('Stamps', NULL, 9.8, NULL, 1, 0)

SELECT @invoiceNo=CAST(RAND() AS nvarchar(50))
SET @checkNo=SUBSTRING (@invoiceNo,3 ,4)
INSERT INTO @payments(methodId, checkNo, amount) VALUES (2, @checkNo, 9.8)
SELECT @invoiceNo=CAST(RAND() AS nvarchar(50))
SET @txnMemo = 'Sale for invoice ' + @invoiceNo
EXEC proc_addSalesInvoice
	@invoiceNo=@invoiceNo,
	@seasonId=2015,
	@postDateUtc=@postDate OUTPUT,
	@effectiveDate=@effectiveDate OUTPUT,
	@txnMemo=@txnMemo,
	@items=@items,
	@payments=@payments,
	@personId=NULL,
	@salesTax=0,
	@txnId=@txnId OUTPUT,
	@id=@id OUTPUT,
	@userId=100
SELECT @invoiceNo AS invoiceNo, @id AS invoiceId, @txnId AS txnId, @postDate AS postDate, @effectiveDate AS effectiveDate
SELECT * FROM tbl_transaction WHERE id=@txnId
SELECT gj.*, a.name AS accountName FROM tbl_generalJournal gj INNER JOIN tbl_account a ON gj.accountId=a.id WHERE txnId=@txnId
SELECT * FROM tbl_salesInvoice WHERE txnId=@txnId
SELECT * FROM tbl_salesInvoiceItem WHERE invoiceId=@id
SELECT * FROM tbl_payment WHERE txnId=@txnId

*/

-- SELECT * FROM tbl_cmCamperSeason
-- SELECT * FROM tbl_person WHERE id=105
-- SELECT * FROM [Product]

/*
*************************** CAMPER ACCOUNT PAYMENT ***************************

DECLARE @invoiceNo nvarchar(50), @items dbo.udt_salesInvoiceItem, @payments dbo.udt_payment, @postDate datetime, @effectiveDate datetime, @txnId int, @id int,
	@checkNo nvarchar(50), @txnMemo nvarchar(50)
INSERT INTO @items([description], productId, unitPrice, unitCost, quantity, isTaxable)
VALUES ('Clipboard Mainstreet Collection', 311, 10, 4.75, 1, 1)
INSERT INTO @items([description], productId, unitPrice, unitCost, quantity, isTaxable)
VALUES ('Stamps', NULL, 9.8, NULL, 1, 0)
SELECT @invoiceNo=CAST(RAND() AS nvarchar(50))
SET @checkNo=SUBSTRING (@invoiceNo,3 ,4)
INSERT INTO @payments(methodId, amount) VALUES (7, 20.48)
SELECT @invoiceNo=CAST(RAND() AS nvarchar(50))
SET @txnMemo = 'Sale for invoice ' + @invoiceNo
EXEC proc_addSalesInvoice
	@invoiceNo=@invoiceNo,
	@seasonId=2015,
	@postDateUtc=@postDate OUTPUT,
	@effectiveDate=@effectiveDate OUTPUT,
	@txnMemo=@txnMemo,
	@items=@items,
	@payments=@payments,
	@personId=105,
	@salesTax=.68,
	@txnId=@txnId OUTPUT,
	@id=@id OUTPUT,
	@userId=100
SELECT @invoiceNo AS invoiceNo, @id AS invoiceId, @txnId AS txnId, @postDate AS postDate, @effectiveDate AS effectiveDate
SELECT * FROM tbl_transaction WHERE id=@txnId
SELECT gj.*, a.name AS accountName FROM tbl_generalJournal gj INNER JOIN tbl_account a ON gj.accountId=a.id WHERE txnId=@txnId
SELECT * FROM tbl_salesInvoice WHERE txnId=@txnId
SELECT * FROM tbl_salesInvoiceItem WHERE invoiceId=@id
SELECT * FROM tbl_payment WHERE txnId=@txnId

*/


