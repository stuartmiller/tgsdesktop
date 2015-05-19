--IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='fk_transaction_accountId')
--	ALTER TABLE tbl_transaction DROP fk_transaction_accountId
--GO
--IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='fk_transactionAccount_personId')
--	ALTER TABLE tbl_transaction DROP fk_transactionAccount_personId
--GO
--IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='fk_cmAccount_accountId')
--	ALTER TABLE tbl_cmAccount DROP fk_cmAccount_accountId
--GO
--IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='fk_personContactInfo_personId')
--	ALTER TABLE tbl_personContactInfo DROP fk_personContactInfo_personId
--GO
--IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='fk_cmLinkedAccount_accountId')
--	ALTER TABLE tbl_cmLinkedLineItem DROP fk_cmLinkedAccount_accountId
--GO

USE tgs
GO
/*

IF OBJECT_ID('view_generalJournalSales') IS NOT NULL
	DROP VIEW view_generalJournalSales
GO
IF OBJECT_ID('view_accountsReceivable') IS NOT NULL
	DROP VIEW view_accountsReceivable
GO
IF OBJECT_ID('view_accountsReceivableAggregate') IS NOT NULL
	DROP VIEW view_accountsReceivableAggregate
GO



IF OBJECT_ID('proc_addStoreSale') IS NOT NULL
	DROP PROC proc_addStoreSale
GO
IF OBJECT_ID('proc_voidSale') IS NOT NULL
	DROP PROC proc_voidSale
GO
IF OBJECT_ID('proc_insertCashSale') IS NOT NULL
	DROP PROC proc_insertCashSale
GO

IF OBJECT_ID('tbl_invoiceItem') IS NOT NULL
	DROP TABLE tbl_invoiceItem
GO
IF OBJECT_ID('tbl_invoice') IS NOT NULL
	DROP TABLE tbl_invoice
GO
IF OBJECT_ID('tbl_cmTransaction') IS NOT NULL
	DROP TABLE tbl_cmTransaction
GO

IF OBJECT_ID('tbl_cmCamperSeason') IS NOT NULL
	DROP TABLE tbl_cmCamperSeason
GO
IF OBJECT_ID('tbl_cmFamilySeason') IS NOT NULL
	DROP TABLE tbl_cmFamilySeason
GO
IF OBJECT_ID('tbl_cmStaffSeason') IS NOT NULL
	DROP TABLE tbl_cmStaffSeason
GO
IF OBJECT_ID('tbl_session') IS NOT NULL
	DROP TABLE tbl_session
GO
IF OBJECT_ID('tbl_cabin') IS NOT NULL
	DROP TABLE tbl_cabin
GO

IF OBJECT_ID('tbl_tgsToCmAccountMapping') IS NOT NULL
	DROP TABLE tbl_tgsToCmAccountMapping
GO


IF OBJECT_ID('tbl_payment') IS NOT NULL
	DROP TABLE tbl_payment
GO
IF OBJECT_ID('tbl_bankDeposit') IS NOT NULL
	DROP TABLE tbl_bankDeposit
GO
IF OBJECT_ID('tbl_generalJournal') IS NOT NULL
	DROP TABLE tbl_generalJournal
GO
IF OBJECT_ID('tbl_transaction') IS NOT NULL
	DROP TABLE tbl_transaction
GO
IF OBJECT_ID('tbl_person') IS NOT NULL
	DROP TABLE tbl_person
GO
IF OBJECT_ID('tbl_household') IS NOT NULL
	DROP TABLE tbl_household
GO
IF OBJECT_ID('tbl_generalJournalTxn') IS NOT NULL
	DROP TABLE tbl_generalJournalTxn
GO
IF OBJECT_ID('tbl_tgsToCmAccountMapping') IS NOT NULL
	DROP TABLE tbl_tgsToCmAccountMapping
GO
IF OBJECT_ID('tbl_account') IS NOT NULL
	DROP TABLE tbl_account
GO
IF OBJECT_ID('tbl_accountType') IS NOT NULL
	DROP TABLE tbl_accountType
GO
IF OBJECT_ID('tbl_settings') IS NOT NULL
	DROP TABLE dbo.tbl_settings
GO

*/

IF OBJECT_ID('tbl_settings') IS NULL
	CREATE TABLE dbo.tbl_settings (
		currentSeasonId int NOT NULL,
		salesTaxRate decimal (4,4) NOT NULL,
		lastCmRefresh datetime DEFAULT '1900-01-01'
	)
GO

IF OBJECT_ID('tbl_accountType') IS NULL
	CREATE TABLE dbo.tbl_accountType (
		id int NOT NULL IDENTITY(100,1)
			CONSTRAINT pk_accountType PRIMARY KEY (id),
		name nvarchar(50) NOT NULL
	)
GO
-- SELECT * FROM tbl_accountType
-- SELECT * FROM tbl_account
-- ALTER TABLE tbl_accountType ADD sortOrder int NOT NULL DEFAULT 0
--INSERT INTO tbl_accountType (name) VALUES ('Contra Income')
-- DELETE tbl_accountType WHERE id=100
SET IDENTITY_INSERT tbl_accountType ON
	INSERT INTO tbl_accountType (id, name) VALUES (10, 'Contra Income')
SET IDENTITY_INSERT tbl_accountType OFF

IF OBJECT_ID('tbl_account') IS NULL
	CREATE TABLE dbo.tbl_account (
		id int NOT NULL IDENTITY(100,1)
			CONSTRAINT pk_account PRIMARY KEY (id),
		number nvarchar(10) NULL,
		name nvarchar(50) NOT NULL,
		typeId int NOT NULL
			CONSTRAINT fk_account_typeId FOREIGN KEY REFERENCES tbl_accountType(id),
		parentId int NULL
			CONSTRAINT fk_account_parent FOREIGN KEY REFERENCES tbl_account,
		isTaxable bit NOT NULL,
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE(),
		archivedUtc datetime NULL,
	)
GO
/*
SELECT * FROM tbl_account
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountId, seasonId) VALUES (10, 584051, 2015)
UPDATE tbl_account SET typeId=10 WHERE id=131
*/
IF OBJECT_ID('tbl_tgsToCmAccountMapping') IS NULL
	CREATE TABLE dbo.tbl_tgsToCmAccountMapping (
		accountId int NOT NULL
			CONSTRAINT fk_cmToTgsAccountMapping_accountId FOREIGN KEY REFERENCES tbl_account,
		cmAccountBillingItemId int NOT NULL,
			CONSTRAINT pk_tgsToCmAccountMapping PRIMARY KEY (accountId, cmAccountBillingItemId),
		seasonId int NOT NULL,
		includeMemo bit DEFAULT 0,
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE()
	)
GO
IF OBJECT_ID('tbl_household') IS NULL
	CREATE TABLE dbo.tbl_household (
		id int NOT NULL IDENTITY (100,1)
			CONSTRAINT pk_household PRIMARY KEY,
		name nvarchar(100) NULL,
		phone nvarchar(50) NULL,
		email nvarchar(100) NULL,
		address1 nvarchar(100) NULL,
		address2 nvarchar(100) NULL,
		city nvarchar(50) NULL,
		stateProvince nvarchar(50) NULL,
		postalCode nvarchar(20) NULL,
		country nvarchar(50) NULL,
		cmFamilyId int NULL, -- campminder family
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE()
	)
GO








/*
SELECT * FROM tbl_household
*/
IF OBJECT_ID('tbl_person') IS NULL
	CREATE TABLE dbo.tbl_person (
		id int NOT NULL IDENTITY (100,1)
			CONSTRAINT pk_person PRIMARY KEY,
		lastName nvarchar(50) NOT NULL,
		firstName nvarchar(50) NULL,
		nickName nvarchar(50) NULL,
		dob date NULL,
		genderId int NULL,
		householdId int NULL
			CONSTRAINT fk_person_arAccount FOREIGN KEY REFERENCES tbl_household,
		cmPersonId int NULL, -- campminder personID
		cmFamilyRole int NULL,
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE(),
		archivedUtc bit NULL
	)
GO


IF OBJECT_ID('tbl_user') IS NULL
	CREATE TABLE dbo.tbl_user (
		personId int NOT NULL
			CONSTRAINT pk_user PRIMARY KEY (personId)
			CONSTRAINT fk_user_personId FOREIGN KEY (personId) REFERENCES tbl_person,
		userName nvarchar(50) NOT NULL,
		archived bit NOT NULL
	)
GO
/*
	SELECT * FROM tbl_person WHERE lastName='miller' AND firstName='stuart'

	INSERT INTO tbl_user (personId, userName, archived) VALUES (100, 'stuart', 0)
*/

/*
SELECT * FROM tbl_person
*/
IF OBJECT_ID('tbl_transaction') IS NULL
	CREATE TABLE dbo.tbl_transaction (
		id int NOT NULL IDENTITY(100,1)
			CONSTRAINT pk_transaction PRIMARY KEY,
		postDateUtc datetime NOT NULL DEFAULT GETUTCDATE(),
		effectiveDate date NOT NULL DEFAULT GETUTCDATE(),
		invoiceNo nvarchar(100) NULL,
		memo nvarchar(300) NULL,
		reversedUtc datetime NULL,
		cmReversed datetime NULL,
		userId int NOT NULL
			CONSTRAINT fk_transaction_userId FOREIGN KEY (userId) REFERENCES tbl_person
	)
GO
/*

ALTER TABLE tbl_transaction ADD userId int
SELECT * FROM tbl_person
UPDATE tbl_transaction SET userId=100 WHERE userId IS NULL
ALTER TABLE tbl_transaction
ADD CONSTRAINT fk_transaction_userId FOREIGN KEY (userId)
    REFERENCES tbl_person(id);
ALTER TABLE tbl_transaction ALTER COLUMN userId int NOT NULL

*/

-- SELECT * FROM tbl_account
IF OBJECT_ID('tbl_transactionInternetSale') IS NULL
	CREATE TABLE dbo.tbl_transactionInternetSale (
		orderId int NOT NULL
			CONSTRAINT pk_transactionInternetSale PRIMARY KEY,
		txnId int NOT NULL
			CONSTRAINT fk_transactionInternetSale_txnId FOREIGN KEY REFERENCES tbl_transaction(id)
	)
GO

--IF OBJECT_ID('tbl_salesInvoice') IS NOT NULL
--	DROP TABLE tbl_invoiceItem
--GO
--IF OBJECT_ID('tbl_salesInvoice') IS NOT NULL
--	DROP TABLE tbl_invoice
--GO
IF OBJECT_ID('tbl_salesInvoice') IS NULL
	CREATE TABLE dbo.tbl_salesInvoice (
		id int NOT NULL IDENTITY(100,1)
			CONSTRAINT pk_salesInvoice PRIMARY KEY,
		txnId int NOT NULL
			CONSTRAINT fk_salesInvoice_txnId FOREIGN KEY REFERENCES tbl_transaction(id),
		invoiceNo varchar(50) NOT NULL,
		personId int NULL
			CONSTRAINT fk_invoice_personId FOREIGN KEY REFERENCES tbl_person(id),
		paymentMethodId int NOT NULL,
		paymentTxnId int NULL
			CONSTRAINT fk_invoice_paymentTxnId FOREIGN KEY REFERENCES tbl_transaction(id)
	)
GO

ALTER TABLE tbl_salesInvoice ADD seasonId int NULL

IF OBJECT_ID('tbl_invoiceItem') IS NULL
	CREATE TABLE dbo.tbl_salesInvoiceItem (
		id int NOT NULL IDENTITY(100,1)
			CONSTRAINT pk_salesInvoiceItem PRIMARY KEY,
		[description] nvarchar(max) NOT NULL,
		invoiceId int NOT NULL
			CONSTRAINT fk_salesInvoiceItem_invoiceId FOREIGN KEY REFERENCES tbl_salesInvoice(id),
		productId int NULL
			CONSTRAINT fk_salesInvoiceItem_productId FOREIGN KEY REFERENCES Product,
		quantity int NOT NULL,
		unitCost money NULL,
		unitPrice money NOT NULL,
		isTaxable bit NOT NULL DEFAULT 1
	)
GO

/*
	SELECT * FROM tbl_salesInvoice
	SELECT * FROM tbl_salesInvoiceItem

	SET IDENTITY_INSERT tbl_salesInvoice ON
	INSERT INTO tbl_salesInvoice(id, txnId, invoiceNo, paymentMethodId) SELECT id, txnId, invoiceNo, 2 FROM tbl_invoice
	SET IDENTITY_INSERT tbl_salesInvoice OFF
	SET IDENTITY_INSERT tbl_salesInvoiceItem ON
	INSERT INTO tbl_salesInvoiceItem (id, description, invoiceId, quantity, unitPrice, isTaxable) SELECT id, description, invoiceId, quantity, unitPrice, isTaxable FROM tbl_invoiceItem
	SET IDENTITY_INSERT tbl_salesInvoiceItem OFF
*/

-- DROP TABLE tbl_product
IF OBJECT_ID('tbl_product') IS NULL
	CREATE TABLE dbo.tbl_product (
		id int IDENTITY (100, 1) NOT NULL
			CONSTRAINT pk_product PRIMARY KEY (id),
		[description] nvarchar(100) NOT NULL,
		price money NULL,
		cost money NULL,
		isTaxable bit NOT NULL,
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE(),
		modifiedUtc datetime NOT NULL DEFAULT GETUTCDATE(),
		version rowversion NOT NULL
	)
GO

--INSERT INTO tbl_product ([description], price, cost, isTaxable) VALUES ('Stamps', 9.80, 9.80, 0)
--INSERT INTO tbl_product ([description], isTaxable) VALUES ('Taxable Sales', 1)
--INSERT INTO tbl_product ([description], isTaxable) VALUES ('Non-Taxable Sales', 0)
-- SELECT p.id, p.description, p.cost, p.price, p.isTaxable FROM tbl_product p
-- SELECT * FROM [Product]

IF OBJECT_ID('tbl_generalJournal') IS NULL
	CREATE TABLE dbo.tbl_generalJournal (
		id int NOT NULL IDENTITY (100,1)
			CONSTRAINT pk_generalJournal PRIMARY KEY,
		txnId int NOT NULL
			CONSTRAINT fk_generalJournal_txnId FOREIGN KEY REFERENCES tbl_transaction (id),
		seasonId int NOT NULL,
		signedAmt money NOT NULL,
			CONSTRAINT ck_generalJournal_signedAmt CHECK (signedAmt <> 0),
		debitAmt AS (CASE WHEN signedAmt > 0 THEN signedAmt ELSE 0 END),
		creditAmt AS (CASE WHEN signedAmt < 0 THEN ABS(signedAmt) ELSE 0 END),
		accountId int NOT NULL
			CONSTRAINT fk_generalJournal_accountId FOREIGN KEY REFERENCES tbl_account,
		personId int NULL
			CONSTRAINT fk_transaction_personId FOREIGN KEY REFERENCES tbl_person,
		memo nvarchar(300) NULL
	)
GO
IF OBJECT_ID('tbl_bankDeposit') IS NULL
	CREATE TABLE dbo.tbl_bankDeposit(
		id int NOT NULL IDENTITY(100,1)
			CONSTRAINT pk_bankCheckDeposit PRIMARY KEY,
		depositDate date NULL,
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE()
	)
GO
IF OBJECT_ID('tbl_payment') IS NULL
	CREATE TABLE dbo.tbl_payment(
		id int NOT NULL IDENTITY(100,1)
			CONSTRAINT pk_payment PRIMARY KEY,
		txnId int NOT NULL
			CONSTRAINT fk_payment_gjId FOREIGN KEY REFERENCES tbl_generalJournal,
		methodId int NOT NULL, -- The payment type 1 = cash, 2 = check, 3 = AmEx, 4 = Visa, 5 = MasterCard, 6 = Discover
		amount money NOT NULL,
		checkNo nvarchar(50) NULL,
		depositId int NULL
			CONSTRAINT fk_bankCheck_depisitId FOREIGN KEY REFERENCES tbl_bankDeposit,
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE()
	)
GO
/*
	SELECT * FROM tbl_payment
*/
IF OBJECT_ID('tbl_cmTransaction') IS NULL
	CREATE TABLE dbo.tbl_cmTransaction(
		id int NOT NULL IDENTITY(100,1)
			CONSTRAINT pk_cmTransaction PRIMARY KEY,
		cmTxnId int NULL,
		gjId int NULL
			CONSTRAINT fk_cmTransaction_gjId FOREIGN KEY REFERENCES tbl_generalJournal,
		seasonId int NOT NULL,
		cmPostDateUtc datetime NULL,
		cmEffectiveDate date NOT NULL,
		cmIsCredit bit NOT NULL,
		cmAmount money NOT NULL,
		cmAccountBillingItemId int NULL,
		cmMemo nvarchar(300) NULL,
		cmPersonId int NULL,
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE(),
		deletedDateUTC datetime NULL
	)
GO

/*
-- apply missing cmTransactions from journal


*/

--IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='unq_cmTransaction_cmTxnId')
	CREATE UNIQUE INDEX unq_cmTransaction_cmTxnId ON tbl_cmTransaction(cmTxnId) WHERE cmTxnId IS NOT NULL
--GO
-- SELECT * FROM tbl_generalJournal



IF OBJECT_ID('tbl_cabin') IS NULL
	CREATE TABLE dbo.tbl_cabin (
		cmId int NOT NULL
			CONSTRAINT pk_cabin PRIMARY KEY,
		name nvarchar(50) NOT NULL,
		sortOrder int NOT NULL
	)
GO
IF OBJECT_ID('tbl_session') IS NULL
	CREATE TABLE dbo.tbl_session (
		cmId int NOT NULL
			CONSTRAINT pk_session PRIMARY KEY,
		name nvarchar(50) NOT NULL,
		sortOrder int NOT NULL
	)
GO

IF OBJECT_ID('tbl_cmCamperSeason') IS NULL
	CREATE TABLE dbo.tbl_cmCamperSeason (
		personId int NOT NULL,
		seasonId int NOT NULL,
		CONSTRAINT pk_cmCamperSeason PRIMARY KEY (personId, seasonId),
		cmSessionId int NOT NULL
			CONSTRAINT fk_cmCamperSeason_cmSessionId FOREIGN KEY REFERENCES tbl_session,
		cmCabinId int NULL
			CONSTRAINT fk_cmCamperSeason_cmCabinId FOREIGN KEY REFERENCES tbl_cabin,
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE()
	)
GO

IF OBJECT_ID('tbl_cmParentSeason') IS NULL
	CREATE TABLE dbo.tbl_cmParentSeason (
		personId int NOT NULL,
		seasonId int NOT NULL,
		CONSTRAINT pk_cmParentSeason PRIMARY KEY (personId, seasonId),
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE()
	)
GO

IF OBJECT_ID('tbl_cmFamilySeason') IS NULL
	CREATE TABLE dbo.tbl_cmFamilySeason (
		householdId int NOT NULL,
		seasonId int NOT NULL,
		CONSTRAINT pk_cmFamilySeason PRIMARY KEY (householdId, seasonId),
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE()
	)
GO
IF OBJECT_ID('tbl_cmStaffSeason') IS NULL
	CREATE TABLE dbo.tbl_cmStaffSeason (
		personId int NOT NULL,
		seasonId int NOT NULL,
		CONSTRAINT pk_cmStaffSeason PRIMARY KEY (personId, seasonId),
		createdUtc datetime NOT NULL DEFAULT GETUTCDATE()
	)
GO
-- select * from tbl_cmStaffSeason

-- insert data

/*
INSERT INTO dbo.tbl_settings VALUES (2015, .0675, NULL)
-- SELECT * FROM tbl_settings
SET IDENTITY_INSERT tbl_accountType ON
INSERT INTO tbl_accountType (id, name) Values (1, 'Current Asset')
INSERT INTO tbl_accountType (id, name) Values (2, 'Fixed Asset')
INSERT INTO tbl_accountType (id, name) Values (3, 'Other Asset')
INSERT INTO tbl_accountType (id, name) Values (4, 'Accounts Payable')
INSERT INTO tbl_accountType (id, name) Values (5, 'Current Liability')
INSERT INTO tbl_accountType (id, name) Values (6, 'Equity')
INSERT INTO tbl_accountType (id, name) Values (7, 'Income')
INSERT INTO tbl_accountType (id, name) Values (8, 'Cost of Goods Sold')
INSERT INTO tbl_accountType (id, name) Values (9, 'Expense')
SET IDENTITY_INSERT tbl_accountType OFF

*/

/*
DECLARE @accountId int
INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('1000', 1, 'Cash', 0)
INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('1010', 1, 'Accounts Receivable', 0)
INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('1011', 1, 'Opening Account Balance', 0) -- used for initial import
INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('1050', 1, 'Inter-Account Transfer', 0)
INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('1100', 1, 'Inventory', 0)

INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('1012', 1, 'Tuition Deposits Receivable', 0)

INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('5000', 5, 'Account Deposits', 0)
SELECT @accountId=id FROM tbl_account WHERE number='5000';
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5001', 5, @accountId, 'Table Girl Scholarship', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5003', 5, @accountId, 'Account Withdrawal', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5010', 5, @accountId, 'Cash Withdrawal', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5011', 5, @accountId, 'Travel Expense', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5100', 5, @accountId, 'Offering', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5102', 5, @accountId, 'Prescriptions', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5103', 5, @accountId, 'Great Day Donation', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5600', 5, @accountId, 'Inter Account Transfer', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5601', 5, @accountId, 'Balance/Credit Forward', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5602', 5, @accountId, 'Opening Season Balance/Credit', 0)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('5700', 5, @accountId, 'Personal Expense Refund', 0)
INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('5900', 5, 'Sales Tax Payable', 0)

INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('6000', 6, 'Opening Equity', 0)

INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('7000', 7, 'Sales', 1)
SELECT @accountId=id FROM tbl_account WHERE number='7000';
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('7000', 7, @accountId, 'Taxable Sales', 1) 
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('7001', 7, @accountId, 'Non-Taxable Sales', 0) -- really only for stamps
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('7002', 7, @accountId, '5 Year Ring', 1)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('7003', 7, @accountId, '5 Year Charm', 1)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('7004', 7, @accountId, 'Packages Mailed', 1)
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('7100', 7, @accountId, 'Taxable Sales Returns', 1) 
INSERT INTO tbl_account(number, typeId, parentId, name, isTaxable) VALUES ('7101', 7, @accountId, 'Non-Taxable Sales Returns', 1)
-- SELECT * FROM tbl_account

INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('7999', 7, 'Sales Discounts', 0)
SELECT * FROM tbl_account

INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('9000', 9, 'Payroll', 0)
INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('9001', 9, 'Shipping', 0)
INSERT INTO tbl_account(number, typeId, name, isTaxable) VALUES ('9002', 9, 'Bank Fees', 0)

GO
*/
-- SELECT * FROM tbl_account

/*
DECLARE @accountId int
SELECT @accountId=id FROM tbl_account WHERE number='5000'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId, includeMemo) VALUES (@accountId, 584068, 2015, 1) -- 'Payment'
SELECT @accountId=id FROM tbl_account WHERE number='5001'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 583941, 2015) -- 'Tablegirl Scholarship'
SELECT @accountId=id FROM tbl_account WHERE number='5010'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId, includeMemo) VALUES (@accountId, 12437325, 2015, 1) -- 'Cash Withdrawal'
SELECT @accountId=id FROM tbl_account WHERE number='5011'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId, includeMemo) VALUES (@accountId, 12437328, 2015, 1) -- 'Travel Expense'
SELECT @accountId=id FROM tbl_account WHERE number='5100'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 12437327, 2015) -- 'Offering'
SELECT @accountId=id FROM tbl_account WHERE number='5102'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 583928, 2015) -- 'Prescriptions'
SELECT @accountId=id FROM tbl_account WHERE number='5103'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 12437326, 2015) -- 'Great Day Fund Donation'
SELECT @accountId=id FROM tbl_account WHERE number='5600'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId, includeMemo) VALUES (@accountId, 12282360, 2015, 1) -- 'Inter-Account Transfer'
SELECT @accountId=id FROM tbl_account WHERE number='5601'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId, includeMemo) VALUES (@accountId, 12406928, 2015, 1) -- 'Balance/Credit forward'
SELECT @accountId=id FROM tbl_account WHERE number='5602'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId, includeMemo) VALUES (@accountId, 12406928, 2015, 1) -- 'Season Opening Balance'
SELECT @accountId=id FROM tbl_account WHERE number='5700'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 583934, 2015) -- 'Personal Expense Refund'



INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId, includeMemo) VALUES (129, 584068, 2015, 1) -- 'Payment'


SELECT @accountId=id FROM tbl_account WHERE number='7100'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 584051, 2015) -- 'Store Purchases'
SELECT @accountId=id FROM tbl_account WHERE number='7101'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 584051, 2015) -- 'Store Purchases'
SELECT @accountId=id FROM tbl_account WHERE number='7100'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 583978, 2015) -- 'Store Return'
SELECT @accountId=id FROM tbl_account WHERE number='7101'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 583978, 2015) -- 'Store Return'
SELECT @accountId=id FROM tbl_account WHERE number='7002'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 583932, 2015) -- '5 Yr ring'
SELECT @accountId=id FROM tbl_account WHERE number='7003'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 584004, 2015) -- '5 Yr charm'
SELECT @accountId=id FROM tbl_account WHERE number='7004'
INSERT INTO tbl_tgsToCmAccountMapping (accountId, cmAccountBillingItemId, seasonId) VALUES (@accountId, 583951, 2015) -- 'Lost and Found Mailed'
*/



/*
INSERT INTO tbl_session VALUES(369,	'Junior Camp', 1)
INSERT INTO tbl_session VALUES(366,	'June Camp', 2)
INSERT INTO tbl_session VALUES(365, 'Main Camp', 3)
INSERT INTO tbl_session VALUES(367,	'August Camp', 4)
*/

/*
INSERT INTO tbl_cabin VALUES (1547, 'Backporch 1 Down', 0)
INSERT INTO tbl_cabin VALUES (1548, 'Backporch 1 Up', 1)
INSERT INTO tbl_cabin VALUES (1549, 'Backporch 2  Down', 2)
INSERT INTO tbl_cabin VALUES (1550, 'Backporch 2 Up', 3)
INSERT INTO tbl_cabin VALUES (1551, 'Backporch 3 Down', 4)
INSERT INTO tbl_cabin VALUES (1552, 'Backporch 3 Up', 5)
INSERT INTO tbl_cabin VALUES (1553, 'Backporch 4 Down', 6)
INSERT INTO tbl_cabin VALUES (1554, 'Backporch 4 Up', 7)
INSERT INTO tbl_cabin VALUES (1555, 'Backporch Down 1', 8)
INSERT INTO tbl_cabin VALUES (1556, 'Backporch Down 2', 9)
INSERT INTO tbl_cabin VALUES (1557, 'Backporch Down 3', 10)
INSERT INTO tbl_cabin VALUES (1558, 'Backporch Down 4', 11)
INSERT INTO tbl_cabin VALUES (1559, 'Backporch Up 1', 12)
INSERT INTO tbl_cabin VALUES (1560, 'Backporch Up 2', 13)
INSERT INTO tbl_cabin VALUES (1561, 'Backporch Up 3', 14)
INSERT INTO tbl_cabin VALUES (1562, 'Backporch Up 4', 15)
INSERT INTO tbl_cabin VALUES (1593, 'DC1', 16)
INSERT INTO tbl_cabin VALUES (1594, 'DC2', 17)
INSERT INTO tbl_cabin VALUES (1595, 'DC3', 18)
INSERT INTO tbl_cabin VALUES (1596, 'DC4', 19)
INSERT INTO tbl_cabin VALUES (1597, 'DC5', 20)
INSERT INTO tbl_cabin VALUES (1598, 'HM & Ann', 21)
INSERT INTO tbl_cabin VALUES (1599, 'Log Cabin', 22)
INSERT INTO tbl_cabin VALUES (1600, 'Not in camp', 23)
INSERT INTO tbl_cabin VALUES (1601, 'Staff B5L', 24)
INSERT INTO tbl_cabin VALUES (1602, 'Staff B5U', 25)
INSERT INTO tbl_cabin VALUES (1603, 'Staff B6L', 26)
INSERT INTO tbl_cabin VALUES (1604, 'Staff B6U', 27)
INSERT INTO tbl_cabin VALUES (1605, 'Staff BL5', 28)
INSERT INTO tbl_cabin VALUES (1606, 'Staff BL6', 29)
INSERT INTO tbl_cabin VALUES (1607, 'Staff BU5', 30)
INSERT INTO tbl_cabin VALUES (1608, 'Staff BU6', 31)
INSERT INTO tbl_cabin VALUES (1609, 'Staff C2', 32)
INSERT INTO tbl_cabin VALUES (1610, 'Staff C3', 33)
INSERT INTO tbl_cabin VALUES (1611, 'Staff C4', 34)
INSERT INTO tbl_cabin VALUES (1612, 'Staff C5', 35)
INSERT INTO tbl_cabin VALUES (1661, 'Uncle Roy', 36)
INSERT INTO tbl_cabin VALUES (1662, 'West 1', 37)
INSERT INTO tbl_cabin VALUES (1663, 'West 2', 38)
INSERT INTO tbl_cabin VALUES (1664, 'White House', 39)
INSERT INTO tbl_cabin VALUES (1511, '*T01L', 40)
INSERT INTO tbl_cabin VALUES (1613, 'T01L', 41)
INSERT INTO tbl_cabin VALUES (1637, 'TL01', 42)
INSERT INTO tbl_cabin VALUES (1512, '*T01U', 43)
INSERT INTO tbl_cabin VALUES (1649, 'TU01', 44)
INSERT INTO tbl_cabin VALUES (1513, '*T02L', 45)
INSERT INTO tbl_cabin VALUES (1615, 'T02L', 46)
INSERT INTO tbl_cabin VALUES (1638, 'TL02', 47)
INSERT INTO tbl_cabin VALUES (1514, '*T02U', 48)
INSERT INTO tbl_cabin VALUES (1616, 'T02U', 49)
INSERT INTO tbl_cabin VALUES (1650, 'TU02', 50)
INSERT INTO tbl_cabin VALUES (1515, '*T03L', 51)
INSERT INTO tbl_cabin VALUES (1617, 'T03L', 52)
INSERT INTO tbl_cabin VALUES (1639, 'TL03', 53)
INSERT INTO tbl_cabin VALUES (1516, '*T03U', 54)
INSERT INTO tbl_cabin VALUES (1651, 'TU03', 55)
INSERT INTO tbl_cabin VALUES (1618, 'T03U', 56)
INSERT INTO tbl_cabin VALUES (1517, '*T04L', 57)
INSERT INTO tbl_cabin VALUES (1640, 'TL04', 58)
INSERT INTO tbl_cabin VALUES (1619, 'T04L', 59)
INSERT INTO tbl_cabin VALUES (1518, '*T04U', 60)
INSERT INTO tbl_cabin VALUES (1652, 'TU04', 61)
INSERT INTO tbl_cabin VALUES (1620, 'T04U', 62)
INSERT INTO tbl_cabin VALUES (1519, '*T05L', 63)
INSERT INTO tbl_cabin VALUES (1641, 'TL05', 64)
INSERT INTO tbl_cabin VALUES (1621, 'T05L', 65)
INSERT INTO tbl_cabin VALUES (1520, '*T05U', 66)
INSERT INTO tbl_cabin VALUES (1653, 'TU05', 67)
INSERT INTO tbl_cabin VALUES (1622, 'T05U', 68)
INSERT INTO tbl_cabin VALUES (1521, '*T06L', 69)
INSERT INTO tbl_cabin VALUES (1642, 'TL06', 70)
INSERT INTO tbl_cabin VALUES (1623, 'T06L', 71)
INSERT INTO tbl_cabin VALUES (1522, '*T06U', 72)
INSERT INTO tbl_cabin VALUES (1654, 'TU06', 73)
INSERT INTO tbl_cabin VALUES (1624, 'T06U', 74)
INSERT INTO tbl_cabin VALUES (1523, '*T07L', 75)
INSERT INTO tbl_cabin VALUES (1643, 'TL07', 76)
INSERT INTO tbl_cabin VALUES (1625, 'T07L', 77)
INSERT INTO tbl_cabin VALUES (1524, '*T07U', 78)
INSERT INTO tbl_cabin VALUES (1655, 'TU07', 79)
INSERT INTO tbl_cabin VALUES (1626, 'T07U', 80)
INSERT INTO tbl_cabin VALUES (1525, '*T08L', 81)
INSERT INTO tbl_cabin VALUES (1644, 'TL08', 82)
INSERT INTO tbl_cabin VALUES (1627, 'T08L', 83)
INSERT INTO tbl_cabin VALUES (1526, '*T08U', 84)
INSERT INTO tbl_cabin VALUES (1656, 'TU08', 85)
INSERT INTO tbl_cabin VALUES (1628, 'T08U', 86)
INSERT INTO tbl_cabin VALUES (1527, '*T09L', 87)
INSERT INTO tbl_cabin VALUES (1645, 'TL09', 88)
INSERT INTO tbl_cabin VALUES (1629, 'T09L', 89)
INSERT INTO tbl_cabin VALUES (1528, '*T09U', 90)
INSERT INTO tbl_cabin VALUES (1657, 'TU09', 91)
INSERT INTO tbl_cabin VALUES (1630, 'T09U', 92)
INSERT INTO tbl_cabin VALUES (1529, '*T10L', 93)
INSERT INTO tbl_cabin VALUES (1646, 'TL10', 94)
INSERT INTO tbl_cabin VALUES (1631, 'T10L', 95)
INSERT INTO tbl_cabin VALUES (1530, '*T10U', 96)
INSERT INTO tbl_cabin VALUES (1658, 'TU10', 97)
INSERT INTO tbl_cabin VALUES (1632, 'T10U', 98)
INSERT INTO tbl_cabin VALUES (1531, '*T11L', 99)
INSERT INTO tbl_cabin VALUES (1647, 'TL11', 100)
INSERT INTO tbl_cabin VALUES (1633, 'T11L', 101)
INSERT INTO tbl_cabin VALUES (1532, '*T11U', 102)
INSERT INTO tbl_cabin VALUES (1659, 'TU11', 103)
INSERT INTO tbl_cabin VALUES (1634, 'T11U', 104)
INSERT INTO tbl_cabin VALUES (1533, '*T12L', 105)
INSERT INTO tbl_cabin VALUES (1648, 'TL12', 106)
INSERT INTO tbl_cabin VALUES (1635, 'T12L', 107)
INSERT INTO tbl_cabin VALUES (1534, '*T12U', 108)
INSERT INTO tbl_cabin VALUES (1660, 'TU12', 109)
INSERT INTO tbl_cabin VALUES (1636, 'T12U', 110)
INSERT INTO tbl_cabin VALUES (1563, 'BL1', 111)
INSERT INTO tbl_cabin VALUES (1535, 'B1L', 112)
INSERT INTO tbl_cabin VALUES (1569, 'BU1', 113)
INSERT INTO tbl_cabin VALUES (1536, 'B1U', 114)
INSERT INTO tbl_cabin VALUES (1564, 'BL2', 115)
INSERT INTO tbl_cabin VALUES (1537, 'B2L', 116)
INSERT INTO tbl_cabin VALUES (1538, 'B2U', 117)
INSERT INTO tbl_cabin VALUES (1570, 'BU2', 118)
INSERT INTO tbl_cabin VALUES (1565, 'BL3', 119)
INSERT INTO tbl_cabin VALUES (1571, 'BU3', 120)
INSERT INTO tbl_cabin VALUES (1539, 'B3L', 121)
INSERT INTO tbl_cabin VALUES (1540, 'B3U', 122)
INSERT INTO tbl_cabin VALUES (1566, 'BL4', 123)
INSERT INTO tbl_cabin VALUES (1572, 'BU4', 124)
INSERT INTO tbl_cabin VALUES (1541, 'B4L', 125)
INSERT INTO tbl_cabin VALUES (1542, 'B4U', 126)
INSERT INTO tbl_cabin VALUES (1567, 'BL5', 127)
INSERT INTO tbl_cabin VALUES (1573, 'BU5', 128)
INSERT INTO tbl_cabin VALUES (1543, 'B5L', 129)
INSERT INTO tbl_cabin VALUES (1544, 'B5U', 130)
INSERT INTO tbl_cabin VALUES (1568, 'BL6', 131)
INSERT INTO tbl_cabin VALUES (1574, 'BU6', 132)
INSERT INTO tbl_cabin VALUES (1545, 'B6L', 133)
INSERT INTO tbl_cabin VALUES (1546, 'B6U', 134)
INSERT INTO tbl_cabin VALUES (1575, 'C01L', 135)
INSERT INTO tbl_cabin VALUES (1591, 'CL01', 136)
INSERT INTO tbl_cabin VALUES (1576, 'C01U', 137)
INSERT INTO tbl_cabin VALUES (1592, 'CU01', 138)
INSERT INTO tbl_cabin VALUES (1577, 'C02', 139)
INSERT INTO tbl_cabin VALUES (1578, 'C03', 140)
INSERT INTO tbl_cabin VALUES (1579, 'C04', 141)
INSERT INTO tbl_cabin VALUES (1580, 'C05', 142)
INSERT INTO tbl_cabin VALUES (1581, 'C06', 143)
INSERT INTO tbl_cabin VALUES (1582, 'C07', 144)
INSERT INTO tbl_cabin VALUES (1583, 'C08', 145)
INSERT INTO tbl_cabin VALUES (1584, 'C09E', 146)
INSERT INTO tbl_cabin VALUES (1585, 'C09W', 147)
INSERT INTO tbl_cabin VALUES (1586, 'C10E', 148)
INSERT INTO tbl_cabin VALUES (1587, 'C10W', 149)
INSERT INTO tbl_cabin VALUES (1588, 'C11E', 150)
INSERT INTO tbl_cabin VALUES (1589, 'C11W', 151)
INSERT INTO tbl_cabin VALUES (1590, 'C12', 152)

GO
*/

/*****************************************/
/***************** PROCS *****************/
/*****************************************/



-- DROP TYPE udt_accountJournalEntry
IF TYPE_ID(N'udt_accountJournalEntry') IS NULL
    CREATE TYPE [dbo].[udt_accountJournalEntry] AS TABLE(
		accountId int NULL,
		seasonId int NULL,
		amount money NOT NULL,
		isCredit bit NOT NULL,
		personId int NULL,
		memo nvarchar(300) NULL
    )
GO


-- DROP TYPE udt_accountPaymentEntry
--IF TYPE_ID(N'udt_accountPaymentEntry') IS NULL
--	CREATE TYPE dbo.udt_accountPaymentEntry AS TABLE(
--		methodId int NOT NULL, -- The payment type 1 = cash, 2 = check, 3 = amex, 4 = visa, 5 = mc, 6 = discover, 7 = account
--		amount money NOT NULL,
--		checkNo nvarchar(50) ,
--		orderIndex int NULL
--	)
--GO
IF TYPE_ID(N'udt_payment') IS NULL
    CREATE TYPE [dbo].[udt_payment] AS TABLE(
		methodId int NOT NULL, -- The payment type 1 = cash, 2 = check, 3 = amex, 4 = visa, 5 = mc, 6 = discover, 7 = account
		amount money NOT NULL,
		checkNo nvarchar(50) NULL
    )
GO

IF TYPE_ID(N'udt_intIdArray') IS NULL
	CREATE TYPE dbo.udt_intIdArray AS TABLE(
		id int NOT NULL
	)
GO

IF OBJECT_ID('proc_addTransactionJournalEntries') IS NOT NULL
	DROP PROC proc_addTransactionJournalEntries
GO
CREATE PROC dbo.proc_addTransactionJournalEntries(
	@invoiceNo nvarchar(100)=NULL,
	@accounts dbo.udt_accountJournalEntry READONLY,
	@payments dbo.udt_payment READONLY,
	@postDate datetime=NULL OUTPUT,
	@effectiveDate date=NULL OUTPUT,
	@txnMemo nvarchar(300)=NULL,
	@txnId int=NULL OUTPUT,
	@userId int
) AS
	

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
		IF (SELECT SUM(amount) FROM @temp WHERE isCredit=0)<>(SELECT SUM(amount) FROM @payments)
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


/*
SELECT * FROM tbl_salesInvoice
SELECT * FROM tbl_salesInvoiceItem
SELECT * FROM tbl_account
*/

-- DROP TYPE udt_salesAccountItem
IF TYPE_ID(N'udt_salesInvoiceItem') IS NULL
    CREATE TYPE [dbo].[udt_salesAccountItem] AS TABLE(
        [description] nvachar(100) NOT NULL,
		productId int NULL,
		unitPrice money NOT NULL,
		unitCost money NULL,
		quantity int NOT NULL,
		isTaxable bit NOT NULL
    )
GO
IF TYPE_ID(N'udt_payment') IS NULL
    CREATE TYPE [dbo].[udt_payment] AS TABLE(
		methodId int NOT NULL, -- The payment type 1 = cash, 2 = check, 3 = amex, 4 = visa, 5 = mc, 6 = discover, 7 = account
		amount money NOT NULL,
		checkNo nvarchar(50) NULL
    )
GO
IF OBJECT_ID('proc_addSalesInvoice') IS NOT NULL
	DROP PROC proc_addSalesInvoice
GO
CREATE PROC dbo.proc_addSalesInvoice (
	@invoiceNo nvarchar(100) = NULL,
	@seasonId int = NULL,
	@postDateUtc datetime = NULL OUTPUT,
	@effectiveDate date = NULL OUTPUT,
	@txnMemo nvarchar(300),
	@items dbo.udt_salesInvoiceItem READONLY,
	@discount money=NULL,
	@payments dbo.udt_payment READONLY,
	@personId int=NULL,
	@seasonId int=NULL,
	@salesTax money=NULL OUTPUT,
	@txnId int=NULL OUTPUT,
	@userId int
) AS

DECLARE @error int, @message varchar(4000), @xstate int;
BEGIN TRY

	SET NOCOUNT ON

	IF EXISTS(SELECT * FROM @payments WHERE methodId=7) AND @personId IS NULL
		RAISERROR('PersonId is needed when Account is specified in the payment methods.',16,1)
	IF @personId IS NOT NULL AND NOT EXISTS(SELECT * FROM tblPerson WHERE id=@personId)
		RAISERROR('Invalid person.',16,1)
	IF EXISTS(SELECT * FROM @salesAccounts s INNER JOIN tbl_account a ON s.accountId=a.id WHERE a.typeId<>7) -- must be sales account
		RAISERROR('Invalid sales account used.',16,1)

	DECLARE @taxableSales money, @nontaxableSales money, @total money
	SELECT @taxableSales=SUM(amount) FROM @items WHERE isTaxable=1
	SELECT @nontaxableSales=SUM(amount) FROM @items WHERE isTaxable=0
	IF @salesTax IS NULL
		SELECT @salesTax=ROUND(@taxableSales*salesTaxRate,2) FROM tbl_settings
	SELECT @total=@taxableSales + @nontaxableSales + @salesTax

	-- get the total of payments
	DECLARE @paymentTotal money
	SELECT @paymentTotal=SUM(amount) FROM @payments

	-- payments must match the total invoice
	IF @total<>@paymentTotal
		RAISERROR('Payment total must match the invoice total.',16,1)


	DECLARE @journalEntries [dbo].[udt_accountJournalEntry]

	-- add the taxable sales
	INSERT INTO @jounalEntries(accountId, seasonId, amount,	isCredit, personId)
	VALUES (120, @seasonId, ABS(@taxableSales), CASE WHEN @taxableSales<0 THEN 1 ELSE 0 END, personId)
	-- add the non-taxable sales
	INSERT INTO @jounalEntries(accountId, seasonId, amount,	isCredit, personId)
	VALUES (121, @seasonId, ABS(@nontaxableSales), CASE WHEN @taxableSales<0 THEN 1 ELSE 0 END, personId)
	-- add the discounts (it's a contra account, so it's the opposite of a sale)
	INSERT INTO @jounalEntries(accountId, seasonId, amount,	isCredit, personId)
	VALUES (131, @seasonId, ABS(ISNULL(@discount,0)), CASE WHEN @discount<0 THEN 0 ELSE 1 END, personId)
	-- add the sales tax
	INSERT INTO @jounalEntries(accountId, seasonId, amount,	isCredit, personId)
	VALUES (117, @seasonId, ABS(@salesTax), CASE WHEN @salesTax<0 THEN 1 ELSE 0 END, personId)

	-- add the payments
	INSERT INTO @jounalEntries(accountId, seasonId, amount,	isCredit, personId)
	SELECT CASE WHEN methodId=7 THEN 101 ELSE 100 END, @seasonId, SUM(ABS(amount)),
		CASE WHEN amount<0 THEN 1 ELSE 0 END, CASE WHEN methodId=7 THEN @personId ELSE NULL END
	FROM @payments
	GROUP BY methodID, amount
	
	--SELECT * FROM tbl_account
	SET NOCOUNT OFF

	EXEC proc_addTransactionJournalEntries
		@accounts=@jounalEntries,
		@payments=@payments,
		@postDate=@postDateUtc OUTPUT,
		@effectiveDate=@effectiveDate OUTPUT,
		@txnMemo=@txnMemo,
		@txnId=@txnId OUTPUT,		
		@userId=@userId
		

	DECLARE @invoiceId int
	INSERT INTO tbl_salesInvoice (txnId, invoiceNo, personId, seasonId)
	VALUES (@txnId, @invoiceNo, @personId, @seasonId)
END TRY
BEGIN CATCH
	SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
	RAISERROR ('proc_addJournalEntry: %d: %s', 16, 1, @error, @message);
END CATCH

GO

/*
SELECT * FROM tbl_accountLineItem WHERE 

SELECT DISTINCT cmLa.cmAccountId
FROM  tbl_cmLinkedLineItem cmLa
WHERE cmLa.accountId IN (7000, 7001, 101)

SELECT * FROM tbl_person WHERE householdId=796


*/



IF OBJECT_ID('proc_cmTransactionModify') IS NOT NULL
	DROP PROC proc_cmTransactionModify
GO
CREATE PROC dbo.proc_cmTransactionModify (
	@id int = NULL OUTPUT,
	@cmTxnId int,
	@gjId int,
	@seasonId int,
	@cmPostDateUtc datetime,
	@cmEffectiveDate date,
	@cmIsCredit bit,
	@cmAmount money,
	@cmActBillingItemId int,
	@cmMemo nvarchar(300),
	@cmPersonId int
) AS

IF @gjId IS NULL SELECT @gjId=gjId FROM tbl_cmTransaction WHERE cmTxnId=@cmTxnId

IF EXISTS(SELECT * FROM tbl_cmTransaction WHERE gjId=@gjId) BEGIN
	UPDATE tbl_cmTransaction SET cmTxnId=@cmTxnId, seasonId=@seasonId, cmEffectiveDate=@cmEffectiveDate,
		cmIsCredit=@cmIsCredit, cmAmount=ABS(@cmAmount), cmAccountBillingItemId=@cmActBillingItemId, cmMemo=@cmMemo, cmPersonId=@cmPersonId
	WHERE gjId=@gjId
END ELSE BEGIN
	INSERT INTO tbl_cmTransaction(cmTxnId, gjId, seasonId, cmPostDateUtc, cmEffectiveDate, cmIsCredit, cmAmount, cmAccountBillingItemId, cmMemo, cmPersonId)
	VALUES (@cmTxnId, @gjId, @seasonId, @cmPostDateUtc, @cmEffectiveDate, @cmIsCredit, ABS(@cmAmount), @cmActBillingItemId, @cmMemo, @cmPersonId)
END

GO
/*
	SELECT * FROM tbl_cmTransaction WHERE cmAmount<0
*/

--IF OBJECT_ID('proc_addCmTransaction') IS NOT NULL
--	DROP PROC proc_addCmTransaction
--GO
--CREATE PROC dbo.proc_addCmTransaction (
--	@id int = NULL OUTPUT,				-- passed in if this is an update
--	@cmTxnId int,
--	@gjId int = NULL OUTPUT,
--	@seasonId int,
--	@cmPostDate datetime,
--	@cmEffectiveDate date,
--	@cmIsCredit bit,
--	@cmAmount money,
--	@cmAccountBillingItemId int=NULL,
--	@lineItemId int=NULL,
--	@cmPersonId int=NULL,
--	@personId int=NULL,
--	@cmMemo nvarchar(300),
--	@deletedDateUTC datetime = NULL
--) AS

--IF @txnId<1 SET @txnId=NULL
--IF @cmTxnId<1 SET @cmTxnId=NULL
--IF @cmTxnId IS NULL AND @txnId IS NULL BEGIN
--	RAISERROR('Parameter @cmTxnId or @txnId must be provided',16,1)
--	RETURN 1
--END
--IF @txnId IS NOT NULL BEGIN
--	IF @personId IS NULL BEGIN
--		RAISERROR('Parameter @personId must be provided',16,1)
--		RETURN 1
--	END
--	-- make sure there is a camper record for the season in question, and that this is a sale on account
--	IF NOT EXISTS(SELECT * FROM tbl_generalJournal gj INNER JOIN tbl_cmCamperSeason cs ON gj.personId=cs.personId
--		WHERE gj.accountId=101 AND cs.personId=@personId AND cs.seasonId=@seasonId)
--		RETURN 0

--	SELECT @id=id FROM tbl_cmTransaction WHERE gjId=@gjId
	
--	IF @cmPersonId IS NULL
--		SELECT @cmPersonId=cmPersonId FROM tbl_person WHERE id=@personId
--END ELSE BEGIN
--	IF @cmPersonId IS NULL BEGIN
--		RAISERROR('Parameter @cmPersonId must be provided',16,1)
--		RETURN 1
--	END
--	IF @cmAccountBillingItemId IS NULL BEGIN
--		RAISERROR('Parameter @cmAccountBillingItemId must be provided',16,1)
--		RETURN 1
--	END
--	SELECT @id=id, @gjId=gjId FROM tbl_cmTransaction WHERE cmTxnId=@cmTxnId
--END



--DECLARE @trancount int, @error int, @message varchar(4000), @xstate int;
--DECLARE @err_msg varchar(max);
--SET @trancount=@@TRANCOUNT

--IF @id IS NOT NULL BEGIN
--	-- if there is an existing cm transaction, then the only thing we can do is set it to deleted if it's not already deleted
--	-- only set it if it's coming from campminder
--	IF EXISTS(SELECT * FROM tbl_cmTransaction WHERE id=@id AND cmTxnId=@cmTxnId AND deletedDateUTC IS NULL AND @deletedDateUtc IS NULL) BEGIN
--		RAISERROR('CM Transaction %i already exists.',16,1, @cmTxnId)
--		RETURN 1
--	END ELSE IF EXISTS(SELECT * FROM tbl_cmTransaction WHERE id=@id AND cmTxnId=@cmTxnId AND @txnId IS NOT NULL AND deletedDateUTC IS NULL AND @deletedDateUtc IS NOT NULL) BEGIN
--		BEGIN TRY
--			IF @trancount=0 BEGIN TRAN
--			ELSE SAVE TRAN addCmTransaction2;

--			UPDATE tbl_cmTransaction SET deletedDateUTC=@deletedDateUTC WHERE cmTxnId=@cmTxnId
--			UPDATE t SET reversedUtc=@deletedDateUTC FROM tbl_transaction t INNER JOIN tbl_cmTransaction cmt ON t.id=cmt.txnId WHERE cmt.id=@id

--			IF @trancount = 0	
--				COMMIT;

--		END TRY
--		BEGIN CATCH
--			SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
--			if @xstate = -1
--				ROLLBACK;
--			if @xstate = 1 and @trancount = 0
--				ROLLBACK
--			if @xstate = 1 and @trancount > 0
--				ROLLBACK TRAN addCmTransaction2;
--		END CATCH
--	END
--	RETURN
--END

--SET @cmAmount=ABS(@cmAmount)

--BEGIN TRY

--	IF @trancount=0
--		BEGIN TRAN
--	ELSE
--		SAVE TRAN addCmTransaction

--	DECLARE @actId int
--	IF @cmTxnId IS NOT NULL BEGIN
--		SELECT @personId=id FROM tbl_person WHERE cmPersonId=@cmPersonId
--		SELECT @lineItemId=lineItemId FROM tbl_cmLinkedLineItem WHERE cmAccountId=@cmAccountBillingItemId
--		SELECT @actId=accountId FROM tbl_accountLineItem WHERE id=@lineItemId
--	END ELSE IF @txnId IS NOT NULL BEGIN
--		SELECT @cmPersonId=id FROM tbl_person WHERE id=@personId
--		SELECT @cmAccountBillingItemId=cmAccountId FROM tbl_cmLinkedLineItem WHERE lineItemId=@lineItemId
--		SELECT @cmMemo=CASE WHEN cmLi.includeMemo=1 THEN txn.memo ELSE NULL END FROM tbl_transaction txn INNER JOIN tbl_cmLinkedLineItem cmLi ON txn.cmLineItemId=cmLi.lineItemId WHERE txn.id=@txnId
--	END

--	IF @cmPostDate IS NOT NULL 
--		SELECT @cmPostDate=dbo.udf_toUtcDateTime(@cmPostDate, 'MT')

--	IF ISNULL(@id,0)<1 BEGIN
--		INSERT INTO tbl_cmTransaction (cmTxnId, txnId, seasonId, cmPostDateUtc, cmEffectiveDate, cmIsCredit, cmAmount, cmAccountBillingItemId, cmPersonId, cmMemo, deletedDateUTC)
--		VALUES (@cmTxnId, @txnId, @seasonId, @cmPostDate, @cmEffectiveDate, @cmIsCredit, @cmAmount, @cmAccountBillingItemId, @cmPersonId, @cmMemo, @deletedDateUTC)
--	END ELSE BEGIN
--		UPDATE tbl_cmTransaction SET cmTxnId=@cmTxnId, txnId=@txnId, seasonId=@seasonId, cmPostDateUtc=@cmPostDate,
--			cmEffectiveDate=@cmEffectiveDate, cmIsCredit=@cmIsCredit, cmAmount=@cmAmount, cmAccountBillingItemId=@cmAccountBillingItemId,
--			cmPersonId=@cmPersonId, cmMemo=@cmMemo, deletedDateUTC=@deletedDateUTC
--	END
--	SELECT @id=SCOPE_IDENTITY()

--	IF @txnId IS NULL AND @actId IS NOT NULL BEGIN

--		DECLARE @items dbo.udt_accountJournalEntry
--		-- add the account entry
--		INSERT INTO @items(accountId, amount, isCredit, memo)
--		VALUES (@actId, @cmAmount, @cmIsCredit, NULL)
--		-- add the a/r entry, accountId 101
--		INSERT INTO @items(accountId, amount, isCredit, memo)
--		VALUES (101, @cmAmount, CASE WHEN @cmIsCredit=1 THEN 0 ELSE 1 END, NULL)

--		DECLARE @invNo varchar(100)
--		SET @invNo='CM-'+ CAST(@cmTxnId AS varchar(max))
--		EXEC proc_addJournalEntry
--			@invoiceNo=@invNo,
--			@accounts=@items,
--			@effectiveDate=@cmEffectiveDate OUTPUT,
--			@personId=@personId,
--			@txnMemo=@cmMemo,			
--			@cmLinkedLineItemId=@lineItemId,
--			@txnId=@txnId OUTPUT,
--			@cmTxnId=@id

--		UPDATE tbl_cmTransaction SET txnId=@txnId WHERE id=@id

--	END
	
--	IF @trancount = 0	
--		COMMIT;
--END TRY
--BEGIN CATCH
--	SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
--	IF @xstate = -1 BEGIN
--		ROLLBACK;
--		THROW
--	END
--	ELSE IF @xstate = 1 AND @trancount = 0 BEGIN
--		ROLLBACK;
--		THROW
--	END
--	ELSE IF @xstate = 1 AND @trancount > 0
--		ROLLBACK TRAN addCmTransaction;

--	--THROW;
--	RAISERROR ('proc_addCmTransaction: %d: %s', 16, 1, @error, @message);
--END CATCH

--GO


IF OBJECT_ID('proc_voidTransaction') IS NOT NULL
	DROP PROC proc_voidTransaction
GO
CREATE PROC dbo.proc_voidTransaction(
	@txnId int
) AS

UPDATE tbl_transaction SET reversedUtc=GETUTCDATE() WHERE id=@txnId

GO

/*

EXEC proc_voidTransaction @txnId=101
SELECT * FROM tbl_transaction

*/


IF OBJECT_ID('proc_arAccountModify') IS NOT NULL
	DROP PROC proc_arAccountModify
GO
IF OBJECT_ID('proc_householdModify') IS NOT NULL
	DROP PROC proc_householdModify
GO
CREATE PROC dbo.proc_householdModify(
	@id int = NULL OUTPUT,
	@name nvarchar(100),
	@phone nvarchar(50),
	@email nvarchar(100),
	@address1 nvarchar(100),
	@address2 nvarchar(100),
	@city nvarchar(50),
	@stateProvince nvarchar(50),
	@postalCode nvarchar(20),
	@country nvarchar(50),	
	@cmFamilyId int, -- campminder family
	@cmPersonId int
) AS

IF ISNULL(@cmFamilyId,0)>0 AND EXISTS(SELECT * FROM tbl_household WHERE cmFamilyId=@cmFamilyId)
	SELECT @id=id FROM tbl_household WHERE cmFamilyId=@cmFamilyId
ELSE IF ISNULL(@cmPersonId,0)>0 AND EXISTS(SELECT * FROM tbl_person WHERE cmPersonId=@cmPersonId)
	SELECT @id=householdId FROM tbl_person WHERE cmPersonId=@cmPersonId

IF ISNULL(@id,0)<1 BEGIN
	INSERT INTO tbl_household (name, phone, email, address1, address2, city, stateProvince, postalCode, country, cmFamilyId)
	VALUES (@name, @phone, @email, @address1, @address2, @city, @stateProvince, @postalCode, @country, @cmFamilyId)

	SELECT @id=SCOPE_IDENTITY()
END ELSE BEGIN
	UPDATE tbl_household SET name=@name, phone=@phone, email=@email, address1=@address1, address2=@address2, city=@city, stateProvince=@stateProvince,
		postalCode=@postalCode, country=@country, cmFamilyId=@cmFamilyId
	WHERE id=@id
END

GO

/*
SELECT * FROM tbl_household
*/

IF OBJECT_ID('proc_personModify') IS NOT NULL
	DROP PROC proc_personModify
GO
CREATE PROC dbo.proc_personModify (
	@id int = NULL OUTPUT,
	@lastName nvarchar(50),
	@firstName nvarchar(50),
	@nickName nvarchar(50),
	@dob datetime = NULL,
	@genderId int = NULL,
	@householdId int,
	@cmPersonId int, -- campminder personID
	@cmFamilyId int,
	@cmFamilyRole int
) AS

IF ISNULL(@cmFamilyId,0)>0 BEGIN
	SELECT @householdId=id FROM tbl_household WHERE cmFamilyId=@cmFamilyId
END
IF ISNULL(@cmPersonId,0)>0 BEGIN
	SELECT @id=id FROM tbl_person WHERE cmPersonId=@cmPersonId
END

IF ISNULL(@id,0)<1 BEGIN
	INSERT INTO tbl_person (lastName, firstName, nickName, dob, genderId, householdId, cmPersonId, cmFamilyRole)
	VALUES (@lastName, @firstName, @nickName, @dob, @genderId, @householdId, @cmPersonId, @cmFamilyRole)
	SELECT @id=SCOPE_IDENTITY()
END ELSE BEGIN
	UPDATE tbl_person SET lastName=@lastName, firstName=@firstName, nickName=@nickName, dob=@dob, genderId=@genderId,
		householdId=@householdId, cmPersonId=@cmPersonId, cmFamilyRole=@cmFamilyRole
	WHERE id=@id
END
	

GO
/*
SELECT * FROM tbl_household WHERE id>1150
SELECT * FROM tbl_person WHERE householdId=101
*/
IF OBJECT_ID('proc_cmCamperSeasonModify') IS NOT NULL
	DROP PROC proc_cmCamperSeasonModify
GO
CREATE PROC dbo.proc_cmCamperSeasonModify (
	@personId int,
	@seasonId int,
	@sessionId int,
	@cabinId int
) AS

IF NOT EXISTS(SELECT * FROM tbl_cmCamperSeason WHERE personId=@personId AND seasonId=@seasonId)
	INSERT INTO tbl_cmCamperSeason (personId, seasonId, cmSessionId, cmCabinId) VALUES (@personId, @seasonId, @sessionId, @cabinId)
ELSE
	UPDATE tbl_cmCamperSeason SET cmSessionId=@sessionId, cmCabinId=@cabinId WHERE personId=@personId AND seasonId=@seasonId

GO
IF OBJECT_ID('proc_cmParentSeasonModify') IS NOT NULL
	DROP PROC proc_cmParentSeasonModify
GO
CREATE PROC dbo.proc_cmParentSeasonModify (
	@personId int,
	@seasonId int
) AS

IF NOT EXISTS(SELECT * FROM tbl_cmParentSeason WHERE personId=@personId AND seasonId=@seasonId)
	INSERT INTO tbl_cmParentSeason (personId, seasonId) VALUES (@personId, @seasonId)

GO
IF OBJECT_ID('proc_cmFamilySeasonModify') IS NOT NULL
	DROP PROC proc_cmFamilySeasonModify
GO
CREATE PROC dbo.proc_cmFamilySeasonModify (
	@householdId int,
	@seasonId int
) AS

IF NOT EXISTS(SELECT * FROM tbl_cmFamilySeason WHERE householdId=@householdId AND seasonId=@seasonId)
	INSERT INTO tbl_cmFamilySeason (householdId, seasonId) VALUES (@householdId, @seasonId)

GO
/*
	SELECT COUNT(*) FROM tbl_cmCamperSeason
*/
IF OBJECT_ID('proc_deleteCamperSeason') IS NOT NULL
	DROP PROC proc_deleteCamperSeason
GO
CREATE PROC dbo.proc_deleteCamperSeason (
	@personId int,
	@seasonId int
) AS

	IF EXISTS(SELECT * FROM tbl_cmCamperSeason WHERE personId=@personId AND seasonId=@seasonId)
		DELETE tbl_cmCamperSeason WHERE personId=@personID AND seasonId=@seasonId

GO


IF OBJECT_ID('proc_cmStaffSeasonModify') IS NOT NULL
	DROP PROC proc_cmStaffSeasonModify
GO
CREATE PROC dbo.proc_cmStaffSeasonModify (
	@personId int,
	@seasonId int
) AS

IF NOT EXISTS(SELECT * FROM tbl_cmStaffSeason WHERE personId=@personId AND seasonId=@seasonId) BEGIN
	INSERT INTO tbl_cmStaffSeason (personId, seasonId) VALUES (@personId, @seasonId)
END
GO
/*


SELECT COUNT(*) FROM tbl_household --1604
SELECT COUNT(*) FROM tbl_person WHERE cmPersonId=194525--4908
SELECT * FROM tbl_household --1604
SELECT * FROM tbl_person WHERE cmPersonId=194525--4908
SELECT * FROM tbl_household WHERE address1='17 Augusta Court'

1674	Abigail Umberger	404-791-8019	aguwriter@gmail.com	1157 Etowah River Rd.	NULL	Dawsonville	GA	30534	US	NULL	2015-01-30 18:02:24.600
1703	Kylie Dodds	011-64-211-548271	kyliedodds@outlook.com	124 Crene Road	RD1	Takahue	Kaitaia	0481	NZ	NULL	2015-01-30 18:02:29.987

*/



/*****************************************/
/***************** VIEWS *****************/
/*****************************************/

IF OBJECT_ID('view_generalJournalSales') IS NOT NULL
	DROP VIEW view_generalJournalSales
GO
CREATE VIEW dbo.view_generalJournalSales
AS

SELECT txn.id, arAct.id AS arActId, arAct.name AS name, gj.seasonId, txn.postDateUtc, txn.effectiveDate, txn.memo,
	(SELECT SUM(signedAmt) FROM tbl_generalJournal xgj INNER JOIN tbl_account a ON xgj.accountId=a.id WHERE xgj.txnId=txn.id AND a.typeId=7) AS totalSales,	
	ISNULL((SELECT SUM(signedAmt) FROM tbl_generalJournal xgj INNER JOIN tbl_account a ON xgj.accountId=a.id WHERE xgj.txnId=txn.id AND a.typeId=7 AND a.isTaxable=1),0) AS taxableSales,	
	ISNULL((SELECT SUM(signedAmt) FROM tbl_generalJournal xgj INNER JOIN tbl_account a ON xgj.accountId=a.id WHERE xgj.txnId=txn.id AND a.typeId=7 AND a.isTaxable=0),0) AS nonTaxableSales,	
	ISNULL((SELECT SUM(signedAmt) FROM tbl_generalJournal WHERE txnId=txn.id AND accountId=5000),0) AS salesTax
FROM tbl_transaction txn
	INNER JOIN tbl_generalJournal gj ON txn.id=gj.txnId
	INNER JOIN tbl_account a ON gj.accountId=a.id AND a.typeId=7
	LEFT OUTER JOIN tbl_person p ON gj.personId=p.id
	LEFT OUTER JOIN tbl_household arAct ON p.householdId=arAct.id
WHERE txn.reversedUtc IS NULL
GO

/*

SELECT * FROM view_generalJournalSales
SELECT * FROM tbl_generalJournal
*/


--SELECT DISTINCT txn.id, gj.accountId, cmLa.cmAccountId, SUM(gj.signedAmt)
--FROM tbl_transaction txn
--	INNER JOIN tbl_household arAct ON txn.householdId=arAct.id
--	INNER JOIN tbl_person p ON arAct.id=p.householdId
--	INNER JOIN tbl_cmCamperSeason cs ON p.id=cs.personId
--	INNER JOIN tbl_generalJournal gj ON txn.id=gj.txnId
--	LEFT OUTER JOIN tbl_cmLinkedAccount cmLa ON gj.accountId=cmLa.accountId
--WHERE EXISTS(SELECT * FROM tbl_generalJournal WHERE txnId=txn.id AND accountId=101)
--	AND txn.id=100
--GROUP BY txn.id, gj.accountId, cmLa.cmAccountId


IF OBJECT_ID('view_generalJournal') IS NOT NULL
	DROP VIEW view_generalJournal
GO
CREATE VIEW dbo.view_generalJournal
AS

SELECT gj.id AS generalJournalId, gj.txnId, gj.accountId, act.name AS accountName, gj.debitAmt, gj.creditAmt, gj.signedAmt, gj.memo,
	txn.postDateUtc, txn.effectiveDate, gj.seasonId, txn.invoiceNo, txn.memo AS txnMemo,
	gj.personId, p.firstName + ' ' + p.lastName AS personName, txn.reversedUtc,
	cmt.cmTxnId
FROM tbl_generalJournal gj
	INNER JOIN tbl_transaction txn on gj.txnId=txn.id
	INNER JOIN tbl_account act ON gj.accountId=act.id
	LEFT OUTER JOIN tbl_person p ON gj.personId=p.id
	LEFT OUTER JOIN tbl_cmTransaction cmt ON gj.id=cmt.gjId
GO

/*
SELECT * FROM view_generalJournal WHERE personId=102 ORDER BY accountId
SELECT txnId, accountId, personId, personName, txnMemo, creditAmt FROM view_generalJournal WHERE accountId=101 AND signedAmt<0
SELECT * FROM tbl_cmTransaction

*/


IF OBJECT_ID('view_arAccountBalances') IS NOT NULL
	DROP VIEW view_arAccountBalances
GO
IF OBJECT_ID('view_householdBalances') IS NOT NULL
	DROP VIEW view_householdBalances
GO
CREATE VIEW dbo.view_householdBalances
AS

SELECT gj.personId, p.firstName + ' ' + p.lastName AS personName, SUM(gj.debitAmt) AS debitBalance,
	SUM(gj.creditAmt) AS creditBalance, SUM(gj.signedAmt) AS signedBalance
FROM tbl_generalJournal gj
	INNER JOIN tbl_transaction txn on gj.txnId=txn.id
	INNER JOIN tbl_account act ON gj.accountId=act.id
	INNER JOIN tbl_person p ON gj.personId=p.id
WHERE gj.accountId=101
	AND txn.reversedUtc IS NULL
GROUP BY gj.personId, p.firstName + ' ' + p.lastName
GO
/*
SELECT * FROM view_householdBalances WHERE personId=1069
SELECT * FROM view_householdBalances WHERE lastName LIKE 'h%'
SELECT * FROM tbl_person WHERE lastName='hereford'
SELECT * FROM tbl_generalJournal WHERE personId=624

*/


IF OBJECT_ID('view_accountsReceivableTransaction') IS NOT NULL
	DROP VIEW view_accountsReceivableTransaction
GO
CREATE VIEW view_accountsReceivableTransaction AS

SELECT p.id AS personId, p.cmPersonId, p.firstName + ' ' + p.lastName AS personName, arAct.id AS householdId, arAct.name AS householdName,
	txn.id AS txnId, txn.invoiceNo, txn.memo, txn.effectiveDate,
	(SELECT SUM(signedAmt) FROM tbl_generalJournal WHERE accountId=101 AND txnId=txn.id AND personId=p.id) AS amount
FROM tbl_generalJournal gj
	INNER JOIN tbl_transaction txn ON gj.txnId=txn.id
	INNER JOIN tbl_person p ON gj.personId=p.id
	INNER JOIN tbl_household arAct ON p.householdId=arAct.id
WHERE txn.reversedUtc IS NULL
	AND gj.accountId=101
	AND txn.reversedUtc IS NULL

GO
/*
SELECT * FROM view_accountsReceivableTransaction WHERE personId>104
SELECT * FROM tbl_cmTransaction
SELECT * FROM tbl_transaction
*/

IF OBJECT_ID('view_cmAccountReconciliation') IS NOT NULL
	DROP VIEW view_cmAccountReconciliation
GO
CREATE VIEW view_cmAccountReconciliation AS

SELECT p.id AS personId, p.firstName + ' ' + p.lastName AS personName, p.cmPersonId localCmPid, cmT.cmPersonId, txn.id AS txnId, gj.id AS genJnlId, gj.signedAmt, gj.seasonId,
	cmT.cmTxnId, cmT.gjId, cmT.cmPostDateUtc, cmT.cmEffectiveDate, cmT.cmIsCredit, cmT.cmAmount, am.cmAccountBillingItemId, cmT.cmMemo, cmT.deletedDateUtc
FROM tbl_person p
	INNER JOIN tbl_cmCamperSeason cs ON p.id=cs.personId
	INNER JOIN tbl_generalJournal gj ON p.id=gj.personId AND gj.accountId=101 AND cs.seasonId=gj.seasonId
	INNER JOIN tbl_transaction txn ON gj.txnId=txn.id
	LEFT JOIN tbl_cmTransaction cmT ON gj.id=cmT.gjId
	LEFT OUTER JOIN tbl_tgsToCmAccountMapping am ON am.accountId=(SELECT DISTINCT accountId FROM tbl_generalJournal WHERE txnId=gj.txnId AND personId=gj.personId AND seasonId=gj.seasonId AND id<>gj.id)
	--INNER JOIN tbl_generalJournal gjx ON 
WHERE gj.accountId=101
	AND txn.reversedUtc IS NULL

GO
/*
SELECT * FROM view_cmAccountReconciliation WHERE cmTxnId IS NULL
SELECT * FROM tbl_generalJournal WHERE id=107
*/







IF OBJECT_ID('view_cmCamper') IS NOT NULL
	DROP VIEW view_cmCamper
GO
CREATE VIEW dbo.view_cmCamper AS

SELECT p.id, p.lastName, p.firstName, p.nickName, p.cmPersonId, cs.seasonId
FROM tbl_person p
	INNER JOIN tbl_cmCamperSeason cs ON p.id=cs.personId

GO
/*
SELECT * FROM view_cmCamper
SELECT * FROM view_cmStaff
*/
IF OBJECT_ID('view_cmStaff') IS NOT NULL
	DROP VIEW view_cmStaff
GO
CREATE VIEW dbo.view_cmStaff AS

SELECT p.id, p.lastName, p.firstName, p.nickName, p.cmPersonId, ss.seasonId
FROM tbl_person p
	INNER JOIN tbl_cmStaffSeason ss ON p.id=ss.personId

GO


IF OBJECT_ID('view_arAccount') IS NOT NULL
	DROP VIEW view_arAccount
GO
IF OBJECT_ID('view_household') IS NOT NULL
	DROP VIEW view_household
GO
CREATE VIEW view_household AS

SELECT p.id AS personID, arAct.id AS householdId, arAct.cmFamilyId, p.cmFamilyRole
FROM tbl_person p
	INNER JOIN tbl_household arAct ON p.householdId=arAct.id

GO
/*
	SELECT * FROM view_household
*/



IF OBJECT_ID('view_cmCamperSeason') IS NOT NULL
	DROP VIEW view_cmCamperSeason
GO
CREATE VIEW dbo.view_cmCamperSeason AS

SELECT p.id AS personId, p.householdId, p.lastName, p.firstName, p.nickName, arAct.cmFamilyId,
	cs.seasonId
FROM tbl_person p
	INNER JOIN tbl_household arAct ON p.householdId=arAct.id
	INNER JOIN tbl_cmCamperSeason cs ON p.id=cs.personId

GO

-- SELECT * FROM view_cmCamperSeason



IF OBJECT_ID('view_receivableAccountPayment') IS NOT NULL
	DROP VIEW view_receivableAccountPayment
GO
CREATE VIEW dbo.view_receivableAccountPayment AS

	SELECT d.id AS depositId, d.depositDate,
		(SELECT COUNT(*) FROM tbl_bankDeposit WHERE depositDate=d.depositDate AND id<d.id)+1 AS depositNumber,
		pmt.txnId, pmt.methodId AS paymentMethodId, pmt.amount,
		p.id AS personId, p.lastName, p.firstName
	FROM tbl_transaction t
		INNER JOIN tbl_generalJournal gj ON t.id=gj.txnId
		INNER JOIN tbl_person p	ON gj.personId=p.id
		INNER JOIN tbl_payment pmt ON t.id=pmt.txnId
		INNER JOIN tbl_bankDeposit d ON pmt.depositId=d.id

GO
--SELECT * FROM view_receivableAccountPayment WHERE lastName='coker'
/*
SELECT p.*
FROM tbl_person p
	--INNER JOIN tbl_cmCamperSeason cs ON cs.personID=p.id
	INNER JOIN tbl_household h ON p.householdId=h.id

WHERE lastName='coker'
	AND p.cmFamilyRole=3
*/


IF OBJECT_ID('view_usDaylightDates') IS NOT NULL
	DROP VIEW view_usDaylightDates
GO
CREATE VIEW dbo.view_usDaylightDates AS
select yr, zone, standard, daylight, rulename, strule, edrule, yrstart, yrend,
    dateadd(day, (stdowref + stweekadd), stmonthref) dstlow,
    dateadd(day, (eddowref + edweekadd), edmonthref)  dsthigh
from (
  select yrs.yr, z.zone, z.standard, z.daylight, z.rulename, r.strule, r.edrule, 
    yrs.yr + '-01-01 00:00:00' yrstart,
    yrs.yr + '-12-31 23:59:59' yrend,
    yrs.yr + r.stdtpart + ' ' + r.cngtime stmonthref,
    yrs.yr + r.eddtpart + ' ' + r.cngtime edmonthref,
    case when r.strule in ('1', '2', '3') then case when datepart(dw, yrs.yr + r.stdtpart) = '1' then 0 else 8 - datepart(dw, yrs.yr + r.stdtpart) end
    else (datepart(dw, yrs.yr + r.stdtpart) - 1) * -1 end stdowref,
    case when r.edrule in ('1', '2', '3') then case when datepart(dw, yrs.yr + r.eddtpart) = '1' then 0 else 8 - datepart(dw, yrs.yr + r.eddtpart) end
    else (datepart(dw, yrs.yr + r.eddtpart) - 1) * -1 end eddowref,
    datename(dw, yrs.yr + r.stdtpart) stdow,
    datename(dw, yrs.yr + r.eddtpart) eddow,
    case when r.strule in ('1', '2', '3') then (7 * CAST(r.strule AS Integer)) - 7 else 0 end stweekadd,
    case when r.edrule in ('1', '2', '3') then (7 * CAST(r.edrule AS Integer)) - 7 else 0 end edweekadd
from (
    select '2005' yr union select '2006' yr -- old us rules
    UNION select '2007' yr UNION select '2008' yr UNION select '2009' yr UNION select '2010' yr UNION select '2011' yr
    UNION select '2012' yr UNION select '2013' yr UNION select '2014' yr UNION select '2015' yr UNION select '2016' yr
    UNION select '2017' yr UNION select '2018' yr UNION select '2018' yr UNION select '2020' yr UNION select '2021' yr
    UNION select '2022' yr UNION select '2023' yr UNION select '2024' yr UNION select '2025' yr UNION select '2026' yr
) yrs
cross join (
    SELECT 'ET' zone, '-05:00' standard, '-04:00' daylight, 'US' rulename
    UNION SELECT 'CT' zone, '-06:00' standard, '-05:00' daylight, 'US' rulename
    UNION SELECT 'MT' zone, '-07:00' standard, '-06:00' daylight, 'US' rulename
    UNION SELECT 'PT' zone, '-08:00' standard, '-07:00' daylight, 'US' rulename
    UNION SELECT 'CET' zone, '+01:00' standard, '+02:00' daylight, 'EU' rulename
) z
join (
    SELECT 'US' rulename, '2' strule, '-03-01' stdtpart, '1' edrule, '-11-01' eddtpart, 2007 firstyr, 2099 lastyr, '02:00:00' cngtime
    UNION SELECT 'US' rulename, '1' strule, '-04-01' stdtpart, 'L' edrule, '-10-31' eddtpart, 1900 firstyr, 2006 lastyr, '02:00:00' cngtime
    UNION SELECT  'EU' rulename, 'L' strule, '-03-31' stdtpart, 'L' edrule, '-10-31' eddtpart, 1900 firstyr, 2099 lastyr, '01:00:00' cngtime
) r on r.rulename = z.rulename
    and datepart(year, yrs.yr) between firstyr and lastyr
) dstdates

GO


IF OBJECT_ID('udf_toUtcDateTime') IS NOT NULL
	DROP FUNCTION udf_toUtcDateTime
GO
CREATE FUNCTION dbo.udf_toUtcDateTime (
	@date datetime,
	@zone char(2)
) RETURNS datetime AS BEGIN

	DECLARE @utcDate datetime
	SELECT @utcDate=SWITCHOFFSET(TODATETIMEOFFSET(createdon, case when createdon >= dstlow and createdon < dsthigh then dst.daylight else dst.standard end), '+00:00')
	FROM (SELECT @date createdon) t
	LEFT OUTER JOIN view_usDaylightDates dst on createdon between yrstart and yrend and zone = @zone

	RETURN @utcDate
END
GO


/*
	SELECT dbo.udf_toUtcDateTime(GETDATE(), 'PT')
*/



IF OBJECT_ID('view_transaction') IS NOT NULL
	DROP VIEW view_transaction
GO
CREATE VIEW dbo.view_transaction AS

SELECT t.*
FROM tbl_transaction t
WHERE t.reversedUtc IS NULL

GO

/*
	SELECT * FROM view_transaction
*/

IF OBJECT_ID('view_salesInvoice') IS NOT NULL
	DROP VIEW view_salesInvoice
GO
IF OBJECT_ID('view_report_salesInvoice') IS NOT NULL
	DROP VIEW view_report_salesInvoice
GO
CREATE VIEW dbo.view_report_salesInvoice AS
SELECT i.id AS invoiceId, i.invoiceNo, i.personId,
	t.effectiveDate,
	p.firstName + ' ' + p.lastName AS customerName,
	CAST (CASE WHEN cs.personId IS NOT NULL THEN 1 ELSE 0 END AS bit) AS isCamper,
	CAST (CASE WHEN ss.personId IS NOT NULL THEN 1 ELSE 0 END AS bit) AS isStaff,
	h.address1, h.address2, h.city + ', ' + h.stateProvince + h.postalCode AS citystatezip, h.country,
	item.id AS itemId,
	item.id, item.[description], item.unitPrice, item.quantity,
	ISNULL((SELECT signedAmt*-1 FROM tbl_generalJournal WHERE txnId=t.id AND accountId=117),0) AS salesTax,
	(SELECT SUM(gj.signedAmt*-1) FROM tbl_generalJournal gj INNER JOIN tbl_account a ON gj.accountId=a.id WHERE gj.txnId=t.id AND a.typeId=7 AND a.id<>131) AS subTotal,
	ISNULL((SELECT SUM(gj.signedAmt) FROM tbl_generalJournal gj WHERE gj.txnId=t.id AND gj.accountId=131),0) AS discount,
	(SELECT SUM(gj.signedAmt*-1) FROM tbl_generalJournal gj INNER JOIN tbl_account a ON gj.accountId=a.id WHERE gj.txnId=t.id AND a.typeId=7)
		+ ISNULL((SELECT signedAmt*-1 FROM tbl_generalJournal WHERE txnId=t.id AND accountId=117),0) AS total
FROM tbl_salesInvoice i
	INNER JOIN tbl_transaction t ON i.txnId=t.id
	INNER JOIN tbl_salesInvoiceItem item ON i.id=item.invoiceId
	LEFT OUTER JOIN tbl_person p ON i.personID=p.id
	LEFT OUTER JOIN tbl_household h ON p.householdId=h.id
	LEFT OUTER JOIN tbl_cmCamperSeason cs ON p.id=cs.personID AND i.seasonId=cs.seasonId
	LEFT OUTER JOIN tbl_cmStaffSeason ss ON p.id=cs.personID AND i.seasonId=ss.seasonId

GO

/*
	SELECT * FROM view_report_salesInvoice
*/

IF OBJECT_ID('view_ringsAndCharms') IS NOT NULL
	DROP VIEW view_ringsAndCharms
GO
CREATE VIEW dbo.view_ringsAndCharms AS
SELECT p.id, gj.txnId, gj.seasonId, gj.personId, p.FirstName + ' ' + p.lastName AS name, gj.accountId, a.name AS accountName, gj.memo, h.address1, h.city, h.stateProvince, h.phone
FROM tbl_generalJournal gj
	INNER JOIN tbl_transaction t ON gj.txnId=t.id
	LEFT OUTER JOIN tbl_person p ON gj.personId=p.id
	INNER JOIN tbl_account a ON gj.accountId=a.id
	LEFT OUTER JOIN tbl_household h ON p.householdId=h.id
WHERE gj.accountId IN (122,123)
	AND t.reversedUtc IS NULL
GO
/*
	SELECT * FROM view_ringsAndCharms
*/