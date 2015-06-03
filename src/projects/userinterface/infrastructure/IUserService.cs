using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface IUserService {

        models.User CurrentUser { get; }

        void Login(string userName, System.Security.SecureString password);
        void Logout();
    }
}
