using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using QueryBuilder.Attributes;

namespace QueryBuilder {
    public class QueryBuilderCache {
        private static Hashtable _cache = new Hashtable();
        public static List<PropertyInfo> GetPrimaryKeyColumns(Type t, List<string> fields) {
            List<PropertyInfo> primaryKeyProperties = new List<PropertyInfo>();
            foreach(string field in fields){
                PropertyInfo qoMember = t.GetProperty(field);
                if (IsPrimaryColumn(qoMember)) {
                    primaryKeyProperties.Add(qoMember);
                }
            }
            return primaryKeyProperties;
        }

        public static List<PropertyInfo> GetPrimaryKeyProperties(Type t) {
            List<PropertyInfo> primaryKeyProperties = new List<PropertyInfo>();
            foreach (PropertyInfo property in t.GetProperties()) {
                if (IsPrimaryColumn(property)) {
                    primaryKeyProperties.Add(property);
                }
            }
            return primaryKeyProperties;
        }

        public static List<PropertyInfo> GetProperties(Type t) {
            return t.GetProperties().ToList();
        }

        public static List<string> GetFields(List<PropertyInfo> properties) {
            List<string> fields = new List<string>();
            foreach (PropertyInfo property in properties) {
                fields.Add(property.Name);
            }
            return fields;
        }

        public static PropertyInfo GetIdentityColumn(Type t) {
            List<PropertyInfo> primaryKeyProperties = new List<PropertyInfo>();
            foreach (PropertyInfo qoMember in t.GetProperties()) {
                if (IsIdentityColumn(qoMember)) {
                    return qoMember;
                }
            }
            return null;
        }

        public static bool IsIdentityColumn(PropertyInfo property) {
            object[] attributes = property.GetCustomAttributes(false);
            IdentityColumnAttribute identityColAttr = attributes.Where(x => x.GetType() == typeof(IdentityColumnAttribute)).FirstOrDefault() as IdentityColumnAttribute;
            return identityColAttr != null;
        }

        public static bool IsPrimaryColumn(PropertyInfo property) {
            object[] attributes = property.GetCustomAttributes(false);
            PrimaryKeyAttribute primaryAttr = attributes.Where(x => x.GetType() == typeof(PrimaryKeyAttribute)).FirstOrDefault() as PrimaryKeyAttribute;
            return primaryAttr != null;
        }
    }

    class CacheEntry {
        public string TypeName = string.Empty;
        public List<PropertyInfo> PrimaryKeyProperties;
        public PropertyInfo IdentitycolumnProperty;
    }
}
