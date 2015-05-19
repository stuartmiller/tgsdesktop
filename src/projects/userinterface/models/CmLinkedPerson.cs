using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class CmLinkedPerson {
        public CmLinkedPerson() {
            this.CamperSeasonIds = new List<int>();
            this.StaffSeasonIds = new List<int>();
        }

        public int CmPersonId { get; set; }
        public int? CmFamilyId { get; set; }
        public int? CmFamilyRoleId { get; set; }
        public List<int> CamperSeasonIds { get; private set; }
        public List<int> StaffSeasonIds { get; private set; }
    }
}
