using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace QueryBuilder.Fixture {
    public class MetaDataAttributeValidator<T> : BaseFixture<T> {
        public override void SetupData() {
            Type type = typeof(T);

            object[] attributes = type.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; i++) {
                if (attributes[i] is MetadataTypeAttribute) {
                    MetadataTypeAttribute attr = attributes[i] as MetadataTypeAttribute;
                    if (attr != null) {
                        Type metaDataClassType = attr.MetadataClassType;
                        SetProperties(metaDataClassType);
                    }
                }
            }
        }

        public static BaseFixture<T> Init() {
            MetaDataAttributeValidator<T> obj = new MetaDataAttributeValidator<T>();
            return obj;
        }

        protected override void Override(T copyFrom) {
            // Query the Dirty Flags of copyFrom object and copy those to copyTo
            FieldInfo dirtyFlag = copyFrom.GetType().GetField("ChangedProperties");
            object dirtyFlagsObj = dirtyFlag.GetValue(copyFrom);
            List<string> changedProperties = (List<string>)dirtyFlagsObj;
            foreach (string ppty in changedProperties) {
                PropertyInfo property = GetProperty(ppty);
                property.SetValue(this.Instance, GetValue(ppty, copyFrom), null);
            }
        }
    }
}
