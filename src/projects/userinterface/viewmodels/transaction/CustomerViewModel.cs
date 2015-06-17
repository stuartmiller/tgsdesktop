using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace tgsdesktop.viewmodels.transaction {

    public class CustomerViewModel : ReactiveObject {

        public CustomerViewModel(models.Person customer) {
            PersonModel = customer;
            Name = customer.FirstName + " " + customer.LastName +
                (!string.IsNullOrEmpty(customer.NickName) && customer.FirstName != customer.NickName ?
                " (" + customer.NickName + ')' : string.Empty);
            this.Balance = customer.Balance * -1; // change the sign so the ui shows negative for a debit balance
            if (customer.Balance != 0) {
                this.BalanceString = (customer.Balance > 0 ? "(" : string.Empty)
                    + (Math.Abs(customer.Balance)).ToString("C")
                    + (customer.Balance > 0 ? ")" : string.Empty);
            }
            DisplayText = this.Name;
            if (customer.Type == models.PersonType.Staff)
                DisplayText += " **Staff";
            SearchText = customer.LastName + ", "
                + (string.IsNullOrEmpty(customer.NickName) ? customer.FirstName : customer.NickName);
            var infoLines = new List<string>();
            if (customer is models.Camper) {
                var camper = customer as models.Camper;
                infoLines.Add(camper.Session.Name + (camper.Cabin == null ? string.Empty :
                    " " + camper.Cabin.Name));
            } else if (customer.IsParent) {
                DisplayText += " **Parent";
                var parent = customer as models.Parent;
                infoLines.Add(string.Join(", ", parent.Campers.Select(c => c.FirstName + (parent.LastName == c.LastName ? string.Empty : ' ' + c.LastName))));
            }

            var sb = new StringBuilder();
            sb.Append(customer.Household.City);
            if (!string.IsNullOrEmpty(customer.Household.StateProvince))
                sb.Append(sb.Length > 0 ? ", " : string.Empty).Append(customer.Household.StateProvince);
            infoLines.Add(sb.ToString());
            
            sb = new StringBuilder();
            var age = customer.Dob.GetAge();
            if (age.HasValue) {
                sb.Append(customer.Dob.Value.ToString("d"));
                sb.Append(" - ");
                sb.Append(age.Value.ToString());
                infoLines.Add(sb.ToString());
            }
            for (int i = 0; i < infoLines.Count; i++) {
                switch (i) {
                    case 0:
                        InfoLine1 = infoLines[i];
                        break;
                    case 1:
                        InfoLine2 = infoLines[i];
                        break;
                    case 2:
                        InfoLine3 = infoLines[i];
                        break;
                }
            }
            this.Household = new HouseholdViewModel(customer.Household);

            var parents = customer.Household.People.Where(p => p.HouseholdRoleId < 3).ToList();
            sb = new StringBuilder();
            foreach (var p in parents) {
                sb.Append(sb.Length > 0 ? ", " : string.Empty);
                sb.Append(p.FirstName);
            }
            this.ParentNames = sb.ToString();

            if (customer.IsStaff)
                this.StandardDiscount = .2m;
        }

        public models.Person PersonModel { get; set; }

        public string DisplayText { get; set; }
        public string SearchText { get; set; }

        public string Name { get; set; }
        public string InfoLine1 { get; set; }
        public System.Windows.Visibility InfoLine1Visibility { get { return string.IsNullOrEmpty(this.InfoLine1) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible; } }
        public string InfoLine2 { get; set; }
        public System.Windows.Visibility InfoLine2Visibility { get { return string.IsNullOrEmpty(this.InfoLine2) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible; } }
        public string InfoLine3 { get; set; }
        public System.Windows.Visibility InfoLine3Visibility { get { return string.IsNullOrEmpty(this.InfoLine3) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible; } }
        public string InfoLine4 { get; set; }
        public System.Windows.Visibility InfoLine4Visibility { get { return string.IsNullOrEmpty(this.InfoLine4) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible; } }
        public decimal Balance { get; set; }
        public string BalanceString { get; set; }
        public System.Windows.Visibility BalanceVisibility { get { return Balance == 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible; } }
        public string BalanceColor { get { return Balance > 0 ? "Green" : "Red"; } }

        public decimal CreditLimit { get; set; }

        public HouseholdViewModel Household { get; set; }

        public string ParentNames { get; set; }
        public Visibility ParentNamesVisible { get { return string.IsNullOrEmpty(this.ParentNames) ? Visibility.Collapsed : Visibility.Visible; } }
        public string Siblings { get; set; }

        public decimal StandardDiscount { get; set; }

    }
}
