using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop {
    public static class DateTimeExtensions {

        public static int? GetAge(this DateTime? dob) {
            int? age = null;
            if (dob.HasValue) {
                age = DateTime.Now.Year - dob.Value.Year;
                if (DateTime.Now.Month < dob.Value.Month || (DateTime.Now.Month == dob.Value.Month && DateTime.Now.Day < dob.Value.Day))
                    age--;
                return age;
            }
            return age;
        }
    }
}
