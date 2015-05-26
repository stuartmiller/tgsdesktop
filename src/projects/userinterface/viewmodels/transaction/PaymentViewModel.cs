using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace tgsdesktop.viewmodels.transaction {

    public class PaymentViewModel : ReactiveObject {

        static int _lastSelectedPaymentMethodId = 0;

        public PaymentViewModel(models.transaction.Payment payment = null) {

            this.DeletePayment = ReactiveCommand.Create();
            this.PaymentMethods = new ReactiveList<KeyValuePair<int, string>>(new ReactiveList<KeyValuePair<int, string>> {
                models.transaction.PaymentMethod.Undefined.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.AmEx.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Visa.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Check.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Cash.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.MasterCard.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Discover.GetPaymentMethodKeyValuePair()
            });

            var paymentString = this.WhenAny(
                vm => vm.PaymentMethod,
                vm => vm.CheckNumber,
                vm => vm.Amount,
                (pm, cn, a) => {
                    StringBuilder retVal = new StringBuilder();
                    if (a.GetValue().HasValue)
                        retVal.Append(a.GetValue().Value.ToString("C"));
                    if(pm.GetValue().Key != 0)
                        retVal.Append(' ').Append(pm.GetValue().Value);
                    if (!string.IsNullOrEmpty(cn.Value))
                        retVal.Append(" #").Append(cn.Value);
                    return retVal.ToString();
                });
            this._paymentString = paymentString.ToProperty(this, x => x.PaymentString);

            this.PaymentMethod = ((models.transaction.PaymentMethod)_lastSelectedPaymentMethodId).GetPaymentMethodKeyValuePair();
            if (payment != null) {
                this.PaymentMethod = new KeyValuePair<int, string>(payment.MethodId,
                    ((models.transaction.PaymentMethod)payment.MethodId).ToString());
                this.Amount = payment.Amount;
                this.CheckNumber = payment.CheckNumber;
            }
            this.WhenAnyValue(vm => vm.PaymentMethod)
                .Select(x => x.Key == (int)models.transaction.PaymentMethod.Check ? true : false)
                .ToProperty(this, x => x.CheckNumberEnabled, out _checkNumberEnabled);

            this.WhenAny(
                vm => vm.Amount,
                vm => vm.PaymentMethod,
                vm => vm.CheckNumber,
                (a, pm, cn) => {
                    var amt = a.GetValue();
                    var pmtMethod = pm.GetValue();
                    var chkNo = cn.GetValue();
                    if (amt.HasValue && amt.Value > 0) {
                        if (pmtMethod.Key > 0) {
                            if (pmtMethod.Key != (int)models.transaction.PaymentMethod.Check)
                                return true;
                            else if (!string.IsNullOrEmpty(chkNo))
                                return true;
                        }
                    }
                    return false;
                }).ToProperty(this, x => x.IsValid, out _isValid);

             this.WhenAnyValue(vm => vm.PaymentMethod).Subscribe(vm => _lastSelectedPaymentMethodId = vm.Key );
        }

        KeyValuePair<int, string> _paymentMethod;
        public KeyValuePair<int, string> PaymentMethod { get { return _paymentMethod; }
            set { 
                this.RaiseAndSetIfChanged(ref _paymentMethod, value);
            } }
        string _checkNumber;
        public string CheckNumber { get { return this._checkNumber; } set { this.RaiseAndSetIfChanged(ref _checkNumber, value); } }
        decimal? _amount;
        public decimal? Amount { get { return _amount; } set { this.RaiseAndSetIfChanged(ref _amount, value); } }

        readonly ObservableAsPropertyHelper<string> _paymentString;
        public string PaymentString { get { return _paymentString.Value; } }
        readonly ObservableAsPropertyHelper<bool> _checkNumberEnabled;
        public bool CheckNumberEnabled {
            get { return _checkNumberEnabled.Value; }
        }
        readonly ObservableAsPropertyHelper<bool> _isValid;
        public bool IsValid { get { return _isValid.Value; } }

        public ReactiveCommand<object> DeletePayment { get; private set; }

        public ReactiveList<KeyValuePair<int, string>> PaymentMethods { get; private set; }
    }
}
