using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class InvalidLoginException : ApplicationException {

        public InvalidLoginException() : base() { }
        public InvalidLoginException(string message) : base(message) { }
    }
}
