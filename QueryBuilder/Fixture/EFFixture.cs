using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects;
using StackExchange.DataExplorer.Helpers;

namespace QueryBuilder.Fixture {
    public class EFFixture<T> : BaseFixture<T> {
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
            EFFixture<T> obj = new EFFixture<T>();
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

        protected override void Create() {
            // Get the object context
            string typeName = typeof(T).ToString();
            Type type = typeof(T);
            int lastDotIndex = typeName.LastIndexOf(".");
            string ns = typeName.Substring(0, lastDotIndex);
            string tName = typeName.Substring(lastDotIndex + 1, typeName.Length - lastDotIndex -1);
            string contextFactoryName = ns + "." + tName;
            using (ObjectContext context = GetContext(ns)) {
                string plural = tName.MakePlural();
                Type contextType = context.GetType();
                PropertyInfo property = (contextType).GetProperty(plural);
                object objectSet = property.GetValue(context, null);
                Type qb = typeof(ObjectSet<>).MakeGenericType(typeof(T));
                qb.InvokeMember("AddObject",
                            BindingFlags.InvokeMethod,
                            null,
                            objectSet,
                            new object[] { this.Instance });
                contextType.InvokeMember("SaveChanges",
                    BindingFlags.InvokeMethod,
                            null,
                            context,
                            null);
            }

            //using (QueryBuilder.EF.QBEntities context = new EF.QBEntities()) {
            //    context.Addresses.AddObject(address);
            //    context.SaveChanges();
            //}
        }

        private ObjectContext GetContext(string ns) {
            string typeName = ns + ".ContextFactory";
            Type type = Type.GetType(typeName);
            PropertyInfo property = type.GetProperty("GetContext", System.Reflection.BindingFlags.Static | BindingFlags.Public);
            return (ObjectContext) property.GetValue(null, null);
        }
    }
}
