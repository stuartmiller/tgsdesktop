using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace tgsdesktop.viewmodels.transaction {

    public class PaymentViewModel : ReactiveObject {

        public PaymentViewModel(models.transaction.Payment payment = null) {

            this.DeletePayment = ReactiveCommand.Create();
            this.PaymentMethods = new ReactiveList<KeyValuePair<int, string>>();
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

            if (payment != null) {
                this.PaymentMethod = new KeyValuePair<int, string>(payment.MethodId,
                    ((models.transaction.PaymentMethod)payment.MethodId).ToString());
                this.Amount = payment.Amount;
                this.CheckNumber = payment.CheckNumber;
            }
            this.WhenAnyValue(vm => vm.PaymentMethod)
                .Select(x => x.Key == (int)models.transaction.PaymentMethod.Check ? true : false)
                .ToProperty(this, x => x.CheckNumberEnabled, out _checkNumberEnabled);
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

        public ReactiveCommand<object> DeletePayment { get; private set; }

        public ReactiveList<KeyValuePair<int, string>> PaymentMethods { get; private set; }
    }
}
