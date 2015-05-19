using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models.transaction {
    public enum PaymentMethod {
        Undefined = 0,
        [Description("Cash")]
        Cash = 1,
        [Description("Check")]
        Check = 2,
        [Description("American Express")]
        AmEx = 3,
        [Description("Visa")]
        Visa = 4,
        [Description("Master Card")]
        MasterCard = 5,
        [Description("Discover")]
        Discover = 6,
        [Description("On Account")]
        Account = 7
    }
}