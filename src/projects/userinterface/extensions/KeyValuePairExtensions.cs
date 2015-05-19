using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop {
    public static class KeyValuePairExtensions {

        public static KeyValuePair<int, string> GetPaymentMethodKeyValuePair(this models.transaction.PaymentMethod paymentMethod) {
            switch (paymentMethod) {
                case models.transaction.PaymentMethod.Account:
                    return new KeyValuePair<int, string>((int)paymentMethod, "Account");
                case models.transaction.PaymentMethod.AmEx:
                    return new KeyValuePair<int, string>((int)paymentMethod, "American Express");
                case models.transaction.PaymentMethod.Check:
                    return new KeyValuePair<int, string>((int)paymentMethod, "Check");
                case models.transaction.PaymentMethod.Cash:
                    return new KeyValuePair<int, string>((int)paymentMethod, "Cash");
                case models.transaction.PaymentMethod.Discover:
                    return new KeyValuePair<int, string>((int)paymentMethod, "Discover");
                case models.transaction.PaymentMethod.MasterCard:
                    return new KeyValuePair<int, string>((int)paymentMethod, "Master Card");
                case models.transaction.PaymentMethod.Visa:
                    return new KeyValuePair<int, string>((int)paymentMethod, "Visa");
                default:
                    return new KeyValuePair<int, string>(0, "Choose a payment method");
            }
        }

        public static List<KeyValuePair<int, string>> GetPaymentMethods(this KeyValuePair<int, string> keyValuePair){
            var retVal = new List<KeyValuePair<int, string>>();
            for(int i = 0; i < 8; i++) 
                retVal.Add(((models.transaction.PaymentMethod)i).GetPaymentMethodKeyValuePair());
            return retVal;
        }
    }
}
