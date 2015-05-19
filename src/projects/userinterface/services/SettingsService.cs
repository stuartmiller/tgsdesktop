using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    public class SettingsService : ServiceBase, infrastructure.IGlobalSettingsAccessor {

        public models.Settings GetSettings() {

            this.Command.CommandText = "SELECT TOP 1 currentSeasonId, salesTaxRate, lastCmRefresh FROM tbl_settings";
            using (var dr = this.ExecuteReader(System.Data.CommandBehavior.SingleRow)) {
                dr.Read();
                return new models.Settings {
                    CurrentSeasonId = dr.GetInt32(0),
                    SalesTaxRate = dr.GetDecimal(1),
                    //LastCmRefresh = DateTime.SpecifyKind(dr.GetDateTime(2), DateTimeKind.Utc)
                };
            }
        }

        public void SetLastCmRefresh(DateTime datetime) {

            this.Command.CommandText = "UPDATE tbl_settings SET lastCmRefresh=@datetime";
            this.Command.Parameters.AddWithValue("@datetime", datetime.ToUniversalTime());
            this.Execute();
        }

        int infrastructure.IGlobalSettingsAccessor.CurrentSeasonId {
            get { return this.GetSettings().CurrentSeasonId; }
            set {
                throw new NotImplementedException();
            }
        }

        decimal infrastructure.IGlobalSettingsAccessor.SalesTaxRate {
            get { return this.GetSettings().SalesTaxRate; }
            set {
                throw new NotImplementedException();
            }
        }

        System.Windows.Controls.Control infrastructure.IGlobalSettingsAccessor.ActiveControl {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
    }
}
