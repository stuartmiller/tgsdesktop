//using ReactiveUI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace tgsdesktop.viewmodels.account {

//    public interface IPaymentEntryViewModel : IAccountSearchViewModel, IRoutableViewModel {
//        decimal? Amount { get; }
//        string CheckNumber { get; }
//        ReactiveCommand<object> AddPayment { get; }
//        ReactiveList<PaymentEntryViewModel.PaymentAccountViewModel> PaymentAccounts { get; }
//    }

//    public class PaymentEntryViewModel : AccountSearchViewModel, IPaymentEntryViewModel {

//        public PaymentEntryViewModel(IScreen screen)
//            : base(screen) {

//            this.EffectiveDate = DateTime.Today;

//            this.ApplyFilter(models.PersonType.Camper | models.PersonType.Staff);
//            this.PaymentAccounts = new ReactiveList<PaymentAccountViewModel> { ChangeTrackingEnabled = true };
//            this.PaymentAccounts.ItemsAdded.Subscribe(x => this.AccountPaymentTotal += x.Amount);
//            this.PaymentAccounts.ItemsRemoved.Subscribe(x => this.AccountPaymentTotal -= x.Amount);
//            this.PaymentAccounts.ItemChanged.Subscribe(x => this.AccountPaymentTotal = this.PaymentAccounts.Sum(t => t.Amount));
//            this.PamentSummaryList = new ReactiveList<PaymentSummaryViewModel>();

//            this.Payments = new ReactiveList<PaymentViewModel>();
//            this.Payments.ChangeTrackingEnabled = true;

//            this.AddPayment = ReactiveCommand.Create(this.WhenAny(
//                x => x.Amount,
//                x => x.AccountPaymentTotal,
//                //(a, b) => a.Value > 0 && b.Value.Sum(t => t.Amount) == a.Value));
//                (a, b) => {
//                    var val = a.Value > 0 && b.Value == a.Value;
//                    return val;
//                }));
//            //this.CommitPayments = ReactiveCommand.Create(this.WhenAny(x => this.Payments, x => { return x.Value.Sum(p => p.Amount) > 0; }));

//            this.CommitPayments = ReactiveCommand.Create(this.WhenAny(vm => vm.TotalPayments, _ => _.Value > 0));
//            this.CommitPayments.Subscribe(x => {
//                var viewModels = this.Payments.ToArray();
//                var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
//                foreach (var vm in viewModels) {
//                    var payment = new models.transaction.Payment2 {
//                            Amount = vm.Amount,
//                            CheckNumber = vm.CheckNumber,
//                            PaymentMethod = new Tuple<string,models.transaction.PaymentMethod>(vm.Method.ToString(), vm.Method),
//                            EffectiveDate = this.EffectiveDate,
//                            DepositOrderIndex = this.Payments.Count() };
//                    payment.PersonAccounts.AddRange(vm.Accounts.Select(a =>
//                        new models.transaction.Payment2.ArPersonAccount {
//                            ArPersonId = a.Id,
//                            Amount = a.Amount,
//                            SeasonId = this.SeasonId
//                        }));
//                    accountService.AddPayment(payment);
//                    this.Payments.Remove(vm);
//                    this.PamentSummaryList.Clear();
//                }
//            });


//            this.WhenAnyValue(x => x.SelectedItem).Subscribe(a => {
//                var zeroAmountAccounts = this.PaymentAccounts.Where(pa => pa.Amount == 0).ToArray();
//                foreach (var pa in zeroAmountAccounts)
//                    this.PaymentAccounts.Remove(pa);
//                if (a != null) {
//                    System.Diagnostics.Debug.WriteLine(a.Name + " Selected");
//                    var pmtAct = new PaymentAccountViewModel {
//                        Id = a.Id,
//                        Name = a.Name,
//                        Amount = this.Amount.Value - this.PaymentAccounts.Sum(_ => _.Amount)
//                    };
//                    this.PaymentAccounts.Add(pmtAct);
//                    //pmtAct.IsAmountFocused = true;
//                    //this.SelectedItem = null;
//                } else
//                    System.Diagnostics.Debug.WriteLine("Selected is null.");
//            });
//            this.AddPayment.Subscribe(x => {
//                System.Diagnostics.Debug.WriteLine("AddPayment called.");
//                var payment = new PaymentViewModel(this) {
//                    Accounts = this.PaymentAccounts.ToArray(),
//                    AccountsString = this.PaymentAccounts.Count > 1 ? string.Join(", ", this.PaymentAccounts.Select(n => n.Name + '-' + n.Amount.ToString("C")))
//                        : this.PaymentAccounts[0].Name,
//                    Method = models.transaction.PaymentMethod.Check,
//                    MethodString = models.transaction.PaymentMethod.Check.ToString(),
//                    Amount = this.Amount.Value,
//                    CheckNumber = this.CheckNumber,
//                };

//                this.Payments.Add(payment);

//                this.TotalPayments = this.Payments.Sum(p => p.Amount);
//                this.Amount = 0;
//                this.CheckNumber = null;
//                this.Amount = null;
//                this.PaymentAccounts.Clear();
//                this.IsCheckNumberFocused = true;
//                this.SelectedItem = null;
//                this.AccountPaymentTotal = 0;

//                var paymentDict = new Dictionary<models.transaction.PaymentMethod, Tuple<int, decimal>>();
//                foreach (var p in this.Payments) {
//                    if (!paymentDict.ContainsKey(p.Method))
//                        paymentDict.Add(p.Method, new Tuple<int, decimal>(1, p.Amount));
//                    else
//                        paymentDict[p.Method] = new Tuple<int,decimal>(paymentDict[p.Method].Item1 + 1, paymentDict[p.Method].Item2 + p.Amount);
//                }
//                this.PamentSummaryList.Clear();
//                foreach (var key in paymentDict.Keys)
//                    this.PamentSummaryList.Add(new PaymentSummaryViewModel { Summary = string.Concat(key.ToString(), ": ", paymentDict[key].Item1.ToString(), " paymnts totaling ", paymentDict[key].Item2.ToString("C")) });
//                this.PamentSummaryList.Add(new PaymentSummaryViewModel { Summary = string.Concat(paymentDict.Sum(v => v.Value.Item1).ToString(), "  payments totaling ", paymentDict.Sum(v => v.Value.Item2).ToString("C")) });
//            });

//            this.IsCheckNumberFocused = true;
//        }

//        public override string UrlPathSegment { get { return "account/paymententry"; } }

//        decimal _totalPayments;
//        public decimal TotalPayments {
//            get { return _totalPayments; }
//            set { this.RaiseAndSetIfChanged(ref _totalPayments, value); }
//        }

//        decimal? _amount;
//        public decimal? Amount {
//            get { return _amount; }
//            set { this.RaiseAndSetIfChanged(ref _amount, value); }
//        }

//        decimal _accountPaymentTotal;
//        public decimal AccountPaymentTotal {
//            get { return _accountPaymentTotal; }
//            set { this.RaiseAndSetIfChanged(ref _accountPaymentTotal, value); }
//        }

//        bool _isAmountFocused;
//        public bool IsAmountFocused {
//            get { return _isAmountFocused; }
//            set { this.RaiseAndSetIfChanged(ref _isAmountFocused, value); }
//        }

//        models.transaction.PaymentMethod _method;
//        public models.transaction.PaymentMethod Method {
//            get { return _method; }
//            set { this.RaiseAndSetIfChanged(ref _method, value); }
//        }

//        string _checkNumber;
//        public string CheckNumber {
//            get { return _checkNumber; }
//            set { this.RaiseAndSetIfChanged(ref _checkNumber, value); }
//        }

//        bool _isCheckNumberFocused;
//        public bool IsCheckNumberFocused {
//            get { return _isCheckNumberFocused; }
//            set { this.RaiseAndSetIfChanged(ref _isCheckNumberFocused, value); }
//        }

//        public ReactiveList<PaymentSummaryViewModel> PamentSummaryList { get; private set; }

//        DateTime _effectiveDate;
//        public DateTime EffectiveDate {
//            get { return _effectiveDate; }
//            set { this.RaiseAndSetIfChanged(ref _effectiveDate, value); }
//        }

//        int? _seasonId;
//        public int? SeasonId {
//            get { return _seasonId; }
//            set { this.RaiseAndSetIfChanged(ref _seasonId, value); }
//        }

//        public ReactiveList<PaymentAccountViewModel> PaymentAccounts { get; private set; }

//        public ReactiveCommand<object> AddPayment { get; private set; }

//        public ReactiveCommand<object> CommitPayments { get; private set; }

//        public ReactiveList<PaymentViewModel> Payments { get; private set; }

//        public class PaymentAccountViewModel : ReactiveObject {

//            int _id;
//            public int Id {
//                get { return _id; }
//                set { this.RaiseAndSetIfChanged(ref _id, value); }
//            }

//            decimal _amount;
//            public decimal Amount {
//                get { return _amount; }
//                set { this.RaiseAndSetIfChanged(ref _amount, value); }
//            }

//            string  _name;
//            public string Name {
//                get { return _name; }
//                set { this.RaiseAndSetIfChanged(ref _name, value); }
//            }
//        }

//        public class PaymentViewModel {

//            public PaymentViewModel(PaymentEntryViewModel parent) {
//                this.DeletePayment = ReactiveCommand.Create();
//                this.DeletePayment.Subscribe(pd => {
//                    parent.Payments.Remove(this);

//                    parent.TotalPayments = parent.Payments.Sum(p => p.Amount);
//                    //parent.TotalNumberOfChecks = parent.Payments.Count;
//                    //parent.TotalAmountOfChecks = parent.Payments.Sum(p => p.Amount).ToString("C");
//                });
//            }

//            public decimal Amount { get; set; }
//            public models.transaction.PaymentMethod Method { get; set; }
//            public string CheckNumber { get; set; }
//            public string AccountsString { get; set; }
//            public string MethodString { get; set; }
//            public PaymentAccountViewModel[] Accounts { get; set; }

//            public ReactiveCommand<object> DeletePayment { get; set; }

//        }

//        public class PaymentSummaryViewModel {
//            public string Summary { get; set; }
//        }
//    }
//}
