using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface ILoginService {

        void Login(string userName, System.Security.SecureString password);
    }
}
