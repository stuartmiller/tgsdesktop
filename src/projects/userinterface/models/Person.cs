using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {

    [Flags]
    public enum PersonType {
        Camper = 2,
        Parent = 4,
        Staff = 8,
        Other = 16
    }

    public class Household {

        public Household() {
            this.People = new List<Person>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public List<Person> People { get; private set; }
        public decimal Balance { get { return People.Sum(x => x.Balance); } }
    }

    public class Person {

        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string NickName { get; set; }
        public int? GenderId { get; set; }
        public DateTime? Dob { get; set; }
        public Household Household { get; set; }
        public int? HouseholdRoleId { get; set; }
        public PersonType Type { get; set; }

        public bool IsCamper { get; set; }
        public bool IsStaff { get; set; }
        public bool IsParent { get; set; }

        public decimal Balance { get; set; }
        public decimal CreditLimit { get; set; }

        public string Name {
            get {
                var sb = new StringBuilder();
                sb.Append(this.FirstName).Append(' ');
                sb.Append(this.LastName);
                if (this.NickName != null)
                    sb.Append(" (").Append(this.NickName).Append(')');
                return sb.ToString();

            }
        }

    }

    public class Camper : Person {
        public Camper() : base() {
        }
        public KeyNamePair<int, string> Cabin { get; set; }
        public KeyNamePair<int, string> Session { get; set; }
        public int SeasonId { get; set; }
    }
    public class Parent : Person {
        public Parent()
            : base() {

            this.Campers = new List<Camper>();
        }

        public List<Camper> Campers { get; private set; }
    }

}
