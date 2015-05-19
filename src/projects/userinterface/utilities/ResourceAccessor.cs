using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.utilities {
    internal static class ResourceAccessor {

        public static string GetStringResource(string name) {
            System.Reflection.Assembly assem = Assembly.GetEntryAssembly();
            using (System.IO.Stream stream = assem.GetManifestResourceStream("tgsdesktop.Properties.Resources.resources")) {
                using (var resourceReader = new System.Resources.ResourceReader(stream)) {
                    string t;
                    byte[] bytes;
                    resourceReader.GetResourceData(name, out t, out bytes);
                    return UTF8Encoding.UTF8.GetString(bytes).Trim();
                }
            }
        }
    }
}
