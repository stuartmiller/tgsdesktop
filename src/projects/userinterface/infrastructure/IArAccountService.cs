using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface IArAccountService {

        models.Household GetArAccount(int id);
        List<models.Household> GetArAccounts();

        models.Person GetPerson(int id);
        List<models.Person> GetPeople();
        List<models.CmLinkedPerson> GetCampers(int seasonId);
        List<models.CmLinkedPerson> GetStaff(int seasonId);

    }
}
