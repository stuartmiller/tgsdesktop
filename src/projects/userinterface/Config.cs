using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop {
    public class Config : System.Configuration.ConfigurationSection {

        public enum DeploymentScope {
            Undefined = 0,
            Production = 1,
            Beta = 2
        }

        private const string SECTION_PATH = "tgsdesktop";
        private const string DEPLOYMENT_SCOPE_PROPERTY = "depscope";


        public static Config Instance {
            get { return ((Config)(ConfigurationManager.GetSection(SECTION_PATH))); }
        }

        [TypeConverter(typeof(DeploymentScopeConverter))]
        [ConfigurationPropertyAttribute(DEPLOYMENT_SCOPE_PROPERTY, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public DeploymentScope Deployment {
            get { return ((DeploymentScope)(base[DEPLOYMENT_SCOPE_PROPERTY])); }
            set { base[DEPLOYMENT_SCOPE_PROPERTY] = value; }
        }

        public partial class DeploymentScopeConverter : System.Configuration.ConfigurationConverterBase {
            private DeploymentScope ConvertFromStringToDeploymentScope(System.ComponentModel.ITypeDescriptorContext context,
                System.Globalization.CultureInfo culture, string value) {
                if (string.IsNullOrEmpty(value))
                    return DeploymentScope.Undefined;
                return (DeploymentScope)Enum.Parse(typeof(DeploymentScope), value, true);
            }

            private string ConvertToDeploymentScopeFromString(System.ComponentModel.ITypeDescriptorContext context,
                System.Globalization.CultureInfo culture,
                DeploymentScope value, System.Type type) {
                return value.ToString();
            }
        }
    }
}
