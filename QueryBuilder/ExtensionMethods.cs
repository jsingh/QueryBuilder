using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryBuilder {
    public static class ExtensionMethods {
        public static List<string> Exclude(this List<string> source, List<string> valuesToExclude) {
            List<string> reducedList = source;
            if (valuesToExclude != null && source != null && valuesToExclude.Count > 0 && source.Count > 0) {
                reducedList = new List<string>();
                source.ForEach(delegate(string val) {
                    if (!valuesToExclude.Contains(val)) {
                        reducedList.Add(val);
                    }
                });
            }
            return reducedList;
        }

        public static List<string> Exclude(this List<string> source, string valueToExclude) {
            if (valueToExclude != null) {
                return source.Exclude(new List<string> { valueToExclude });
            }
            return source;
        }
    }
}
