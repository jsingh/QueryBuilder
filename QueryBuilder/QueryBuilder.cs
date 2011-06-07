using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using QueryBuilder.DC;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QueryBuilder {
    public class QueryBuilder<T>: BaseDC where T : class {
        protected internal T Instance { get; set; }


        //Func<int, int, bool> areEqual = (x, y) => x.CompareTo(y) == 0;
        //Func<int, int, QueryBuilder<T>> areEqual = (x, y) => x.CompareTo(y) == 0;
        public static QueryBuilder<T> Init() {
            QueryBuilder<T> obj = new QueryBuilder<T>();
            T instance = Activator.CreateInstance<T>();
            obj.Instance = instance;
            return obj;
        }

        public QueryBuilder() : base(System.Configuration.ConfigurationManager.AppSettings["QB.databaseConnection"]) { } 

        public T Fetch(T qo, string top = "") {
            // query the dirty flags of the active record to find if the properties are set
            // the set properties are the ones which we want to query off of
            //AddressDirtyFlags
            string tableName = GetTableName();
            FieldInfo dirtyFlag = typeof(T).GetField("DirtyFlags");
            object dirtyFlagsObj = dirtyFlag.GetValue(qo);
            List<string> fields = new List<string>();
            List<string> dirtyFields = new List<string>();
            GetFields(dirtyFlagsObj, out fields, out dirtyFields);

            string fetchFormat = "SELECT " + (string.IsNullOrEmpty(top) ? string.Empty : "TOP " + top) + " {0} from {1} {2}";

            StringBuilder selectFieldsBuilder = new StringBuilder();
            StringBuilder whereClauseBuilder = new StringBuilder();

            if (fields.Count > 0) {
                foreach (string field in fields) {
                    selectFieldsBuilder.Append(field).Append(",");
                }
            }

            string selectFields = selectFieldsBuilder.ToString();
            selectFields = selectFields.Length > 0 ? selectFields.Substring(0, selectFields.Length - 1) : "*";

            if (dirtyFields.Count > 0) {
                whereClauseBuilder.Append(" WHERE ");
                foreach (string dirtyField in dirtyFields) {
                    PropertyInfo qoMember = qo.GetType().GetProperty(dirtyField);
                    if (qoMember != null) {
                        whereClauseBuilder.Append(string.Format(" {0} = {1} AND", dirtyField, SqlSafeString(qoMember.GetValue(qo, null), qoMember.PropertyType)));
                    }
                }
            }
            string whereClause = whereClauseBuilder.ToString();
            whereClause = whereClause.EndsWith("AND") ? whereClause.Substring(0, whereClause.Length - 3) : whereClause;

            string query = string.Format(fetchFormat, selectFields, tableName, whereClause);

            using (IDataReader dr = this.ExecuteReader(query)) {
                if (dr.Read()) {
                    this.Instance = PopulateFromDataReader(dr, fields);
                }
            }

            Console.WriteLine(query);
            return this.Instance;
        }

        private T PopulateFromDataReader(IDataReader dr, List<string> fields) {
            T instance = (T)Activator.CreateInstance(typeof(T));
            // instance.CanDirty = false;
            if (fields.Count > 0) {
                foreach (string field in fields) {
                    PropertyInfo qoMember = instance.GetType().GetProperty(field);
                    if (qoMember != null) {
                        object value = GetValueFromDataReader(dr[qoMember.Name]);
                        qoMember.SetValue(instance, value, null);
                    }
                }
            }

           

            return instance;
        }

        public T First() {
            T obj = (T)Activator.CreateInstance(typeof(T));
            return Fetch(obj, "1");
        }

        public T Create(T qo) {
            string tableName = GetTableName();
            FieldInfo dirtyFlag = qo.GetType().GetField("DirtyFlags");
            object dirtyFlagsObj = dirtyFlag.GetValue(qo);
            List<string> fields = new List<string>();
            List<string> dirtyFields = new List<string>();
            GetFields(dirtyFlagsObj, out fields, out dirtyFields);
            string insertStatementFormat = "INSERT INTO {0} ({1}) VALUES({2}); SELECT SCOPE_IDENTITY();";
            
            StringBuilder valuesBuilder = new StringBuilder();
            StringBuilder columnsBuilder = new StringBuilder();
            if (fields.Count > 0) {
                foreach (string field in fields) {
                    PropertyInfo qoMember = qo.GetType().GetProperty(field);
                    if (qoMember != null) {
                        columnsBuilder.Append(field).Append(",");
                        valuesBuilder.Append(SqlSafeString(qoMember.GetValue(qo, null), qoMember.PropertyType)).Append(",");
                    }
                }
            }

            string columns = columnsBuilder.ToString();
            columns = columns.EndsWith(",") ? columns.Substring(0, columns.Length - 1): columns;
            string values = valuesBuilder.ToString();
            values = values.EndsWith(",") ? values.Substring(0, values.Length - 1): values;

            string query = string.Format(insertStatementFormat, tableName, columns, values);
            // TO DO:
            // Investigate T to see if it has a Primary Key Column which is an identity column. If it is, then make sure there is
            // only one such column. Assign the return value from ExecuteScalar to that property
            // eg: group.ID = this.ExecuteScalar(query);
            this.ExecuteScalar(query);
            Console.WriteLine(query);
            return this.Instance;
        }

        public T Update(T qo) {
            string tableName = GetTableName();

            FieldInfo dirtyFlag = qo.GetType().GetField("DirtyFlags");
            object dirtyFlagsObj = dirtyFlag.GetValue(qo);

            List<string> fields = new List<string>();
            List<string> dirtyFields = new List<string>();
            GetFields(dirtyFlagsObj, out fields, out dirtyFields);
            string updateStatementFormat = "UPDATE {0} SET {1};";
            

            StringBuilder updateBuilder = new StringBuilder();
            if (dirtyFields.Count > 0) {
                foreach (string dirtyField in dirtyFields) {
                    PropertyInfo qoMember = qo.GetType().GetProperty(dirtyField);
                    if (qoMember != null) {
                        updateBuilder.Append(string.Format("{0} = {1},", dirtyField, DbUtil.SqlSafeString(qoMember.GetValue(qo, null).ToString())));
                    }
                }
            }

            string updateString = updateBuilder.ToString();
            updateString = updateString.EndsWith(",") ? updateString.Substring(0, updateString.Length - 1) : updateString;

            string query = string.Format(updateStatementFormat, tableName, updateString);
            Console.WriteLine(query);
            this.ExecuteNonQuery(query);
            return this.Instance;
        }

        //public T Delete(T qo) {
        //}


        private string GetTableName() {
                string typeName = typeof(T).ToString();
                string tableName = typeName.Substring(typeName.LastIndexOf('.') + 1);
                return tableName;
        }

        private void GetFields(object dirtyFlagsObj, out List<string> fields, out List<string> dirtyFields) {
            fields = new List<string>();
            dirtyFields = new List<string>();
            FieldInfo[] flags = dirtyFlagsObj.GetType().GetFields();
            foreach (FieldInfo flag in flags) {
                if (flag.FieldType.Name == "Boolean") {
                    bool isDirty = (bool)flag.GetValue(dirtyFlagsObj);
                    if (isDirty) {
                        dirtyFields.Add(flag.Name);
                    }
                    fields.Add(flag.Name);
                }
            }
        }

        private string SqlSafeString(object obj, Type type) {
            // string
            // int, int?
            if (type == typeof(int)) {
                return DbUtil.SqlSafeString((int)obj);
            } else if (type == typeof(int?)) {
                return DbUtil.SqlSafeString((int?)obj);
            }
                // double , double ?
            else if (type == typeof(double)) {
                return DbUtil.SqlSafeString((double)obj);
            }
                // decimal
            else if (type == typeof(decimal)) {
                return DbUtil.SqlSafeString((decimal)obj);
            }
                // datetime, datetime?
            else if (type == typeof(DateTime)) {
                return DbUtil.SqlSafeString((DateTime)obj);
            } else if (type == typeof(DateTime?)) {
                return DbUtil.SqlSafeString((DateTime?)obj);
            }
                // bool, bool?
            else if (type == typeof(bool)) {
                return DbUtil.SqlSafeString((bool)obj);
            } else if (type == typeof(bool?)) {
                return DbUtil.SqlSafeString((bool?)obj);
            }
            string str = null;
            return DbUtil.SqlSafeString(str);
        }

        private object GetValueFromDataReader(object obj) {
            Type type = obj.GetType();

            if (type == typeof(int)) {
                return DbUtil.ConvertToInt32(obj);
            } else if (type == typeof(int?)) {
                return DbUtil.ConvertToInt32Null(obj);
            }
            if (type == typeof(short)) {
                return DbUtil.ConvertToInt32(obj);
            } else if (type == typeof(short?)) {
                return DbUtil.ConvertToInt32Null(obj);
            }
                // double , double ?
            else if (type == typeof(double)) {
                return DbUtil.ConvertToDouble(obj);
            }
                // TO DO:
                //} else if (type == typeof(double?)) {
                //    return DbUtil.ConvertToDoubleNull(obj);
                //}
                // decimal
         else if (type == typeof(decimal)) {
                return DbUtil.ConvertToDecimal((decimal)obj);
            }
                // TO DO:
                //} else if (type == typeof(decimal?)) {
                //    return DbUtil.ConvertToDecimalNull(obj);
                //}
                // datetime, datetime?
             else if (type == typeof(DateTime)) {
                return DbUtil.ConvertToDateTime(obj);
            } else if (type == typeof(DateTime?)) {
                return DbUtil.ConvertToDateTimeNull(obj);
            }
                // bool, bool?
            else if (type == typeof(bool)) {
                return DbUtil.ConvertToBoolean(obj);
            } else if (type == typeof(bool?)) {
                return DbUtil.ConvertToBooleanNull(obj);
            }
                // byte, byte?
            else if (type == typeof(byte)) {
                return DbUtil.ConvertToByte(obj);
            } else if (type == typeof(byte?)) {
                return DbUtil.ConvertToByteNull(obj);
            } else if (type == typeof(byte[])) {
                return DbUtil.ConvertToByteArray(obj);
            }

            return DbUtil.ConvertToString(obj);
        }

        //public T Fetch() {
        //    return Fetch(this);
        //}

        //public T Where(Action<T> f) {
        //    f(Instance);
        //    return Instance;
        //}
    }
}
