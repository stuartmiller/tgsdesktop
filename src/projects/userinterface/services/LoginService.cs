using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tgsdesktop;

namespace tgsdesktop.services {
    public class LoginService : ServiceBase, infrastructure.ILoginService {

        public LoginService() {
        }

        public void Login(string userName, System.Security.SecureString password) {

            if (string.IsNullOrEmpty(userName))
                throw new models.InvalidLoginException("Invalid username");

            this.Reset();
            this.Command.CommandText = @"SELECT u.personId
FROM tbl_user u
WHERE u.userName=@userName AND u.archived=0";
            this.Command.Parameters.AddWithValue("@userName", userName);
            var personId = this.ExecuteScaler<int>();
            if (personId < 1)
                throw new models.InvalidLoginException("Invalid login.");

            var pwd = password.ConvertToUnSecureString();
            if (pwd != "123")
                throw new models.InvalidLoginException();

            var arservice = new AccountReceivableService();
            var person = arservice.GetPeople().SingleOrDefault(x => x.Id == personId);

            ServiceBase.User = new models.User { Person = person, UserName = userName };

            return;
        }


        public void Logout() {
            User = null;
        }
    }
}
