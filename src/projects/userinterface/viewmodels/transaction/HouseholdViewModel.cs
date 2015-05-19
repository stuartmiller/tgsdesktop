using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace tgsdesktop.viewmodels.transaction {
    public class HouseholdViewModel : ReactiveObject {

        public HouseholdViewModel(models.Household household) {
            this.Address1 = household.Address1;
            this.Address2 = household.Address2;
            this.City = household.City;
            this.StateProvince = household.StateProvince;
            this.PostalCode = household.PostalCode;
            this.Country = household.Country;

            this.Balance = household.Balance;
        }

        #region properties

        public string Address1 { get; set; }
        public Visibility Address1Visibility { get { return string.IsNullOrEmpty(Address1) ? Visibility.Collapsed : Visibility.Visible; } }
        public string Address2 { get; set; }
        public Visibility Address2Visibility { get { return string.IsNullOrEmpty(Address2) ? Visibility.Collapsed : Visibility.Visible; } }
        public string City { get; set; }
        public Visibility CityVisibility { get { return string.IsNullOrEmpty(City) ? Visibility.Collapsed : Visibility.Visible; } }
        public string StateProvince { get; set; }
        public Visibility StateProvinceVisibility { get { return string.IsNullOrEmpty(StateProvince) ? Visibility.Collapsed : Visibility.Visible; } }
        public string PostalCode { get; set; }
        public Visibility PostalCodeVisibility { get { return string.IsNullOrEmpty(PostalCode) ? Visibility.Collapsed : Visibility.Visible; } }
        public string Country { get; set; }
        public Visibility CountryVisibility { get { return string.IsNullOrEmpty(Country) ? Visibility.Collapsed : Visibility.Visible; } }

        public decimal Balance { get; set; }

        #endregion
    }
}
