using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tgsdesktop;

namespace tgsdesktop.services {
    public class UserService : ServiceBase, infrastructure.IUserService {


        public UserService() {
        }

        public void Login(string userName, System.Security.SecureString password) {

            if (string.IsNullOrEmpty(userName))
                throw new models.InvalidLoginException("Invalid username");

            this.Db.Reset();
            this.Db.Command.CommandText = @"SELECT u.personId, password
FROM tbl_user u
WHERE u.userName=@userName AND u.archived=0";
            this.Db.Command.Parameters.AddWithValue("@userName", userName);

            int? personId = null;
            byte[] passwordHash = null;
            using (var dr = this.Db.ExecuteReader(System.Data.CommandBehavior.SingleRow)) {
                while (dr.Read()) {
                    personId = dr.GetInt32(0);
                    passwordHash = new byte[64];
                    dr.GetBytes(1, 0, passwordHash, 0, 64);
                }
            }

            if (!personId.HasValue) {
                User = null;
                throw new models.InvalidLoginException("Invalid login.");
            }

            var pwd = password.ConvertToUnSecureString();

            var hashVerified = SimpleHash.VerifyHash(pwd, "sha256", passwordHash);
            if (!hashVerified)
                throw new models.InvalidLoginException();

            var arservice = new AccountReceivableService();
            var person = arservice.GetPeople().SingleOrDefault(x => x.Id == personId);

            User = new models.User { Person = person, UserName = userName };

            return;
        }


        public void Logout() {
            User = null;
        }

        public models.User CurrentUser {
            get { return User; }
        }
    }
}
