using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace tgsdesktop.infrastructure {
    public class EnumerationManager {
        public static Array GetValues(Type enumType) {
            var values = Enum.GetValues(enumType).Cast<object>();
            var valuesAndDescriptions = from value in values
                                        select new {
                                            Value = value,
                                            Description = value.GetType()
                                                .GetMember(value.ToString())[0]
                                                .GetCustomAttributes(true)
                                                .OfType<DescriptionAttribute>()
                                                .First()
                                                .Description
                                        };
            return valuesAndDescriptions.ToArray();
        }
    }
}
