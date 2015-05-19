using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models.transaction {
    public class Account {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public int? ParentId { get; set; }
        public int TypeId { get; set; }
        public bool IsTaxable { get; set; }
        public DateTime? Archived { get; set; }
    }
}
