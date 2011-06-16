using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using QueryBuilder.DC;
using System.Configuration;
using QueryBuilder.Attributes;

namespace QueryBuilder {
    public class QueryBuilder<T>: BaseDC where T : class {
        protected internal T Instance { get; set; }

        // A table can have only one Identity column

        // A table can have only one primary key( that primary key may compose of multiple columns in case of a composite key)
        // The PRIMARY KEY constraint uniquely identifies each record in a database table. Primary keys must contain unique values.
        // A primary key column cannot contain NULL values.

        // Dirty Flags
        // Dirty Flags are generated for all Non Primary Key Columns. TO DO NOTE: [They should also not be generated for Identity Column is the Identity Column is not in a primary key]

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
            // How to generate the SELECT Query
            // SELECT TOP {Top} from {ColumnNames} from {TableName} {Where Clause}";
            // ColumnNames => All columns present on the entity
            // Where clause: All dirty columns in qo
            string fetchFormat = "SELECT " + (string.IsNullOrEmpty(top) ? string.Empty : "TOP " + top) + " {0} from {1} {2}";

            // query the dirty flags of the active record to find if the properties are set
            // the set properties are the ones which we want to query off of
            string tableName = GetTableName();

            FieldInfo dirtyFlag = typeof(T).GetField("DirtyFlags");
            object dirtyFlagsObj = dirtyFlag.GetValue(qo);
            List<string> possibleDirtyFields = new List<string>();
            List<string> dirtyFields = new List<string>();
            GetDirtyFields(dirtyFlagsObj, out possibleDirtyFields, out dirtyFields);

            StringBuilder selectFieldsBuilder = new StringBuilder();
            foreach (string field in Columns) {
                selectFieldsBuilder.Append(field).Append(",");
            }
            string selectFields = selectFieldsBuilder.ToString();
            selectFields = selectFields.Length > 0 ? selectFields.Substring(0, selectFields.Length - 1) : "*";

            StringBuilder whereClauseBuilder = new StringBuilder();

            // primary keys
            List<PropertyInfo> pkProperties = QueryBuilder.QueryBuilderCache.GetPrimaryKeyProperties(typeof(T));
            pkProperties.ForEach(delegate(PropertyInfo pkPpty) {
                object propertyVal = pkPpty.GetValue(qo, null);
                if (propertyVal != Default(qo.GetType())) { // Dirty
                    whereClauseBuilder.Append(string.Format(" {0} = {1} AND", pkPpty.Name, SqlSafeString(propertyVal, pkPpty.PropertyType)));
                }
            });

            // non-primary keys
            if (dirtyFields.Count > 0) {
               foreach (string dirtyField in dirtyFields) {
                    PropertyInfo qoMember = qo.GetType().GetProperty(dirtyField);
                    if (qoMember != null) {
                        whereClauseBuilder.Append(string.Format(" {0} = {1} AND", dirtyField, SqlSafeString(qoMember.GetValue(qo, null), qoMember.PropertyType)));
                    }
                }
            }

            string whereClause = whereClauseBuilder.ToString();
            whereClause = whereClause.EndsWith("AND") ? whereClause.Substring(0, whereClause.Length - 3) : whereClause;
            whereClause = string.IsNullOrEmpty(whereClause)? whereClause: " WHERE "+whereClause;

            string query = string.Format(fetchFormat, selectFields, tableName, whereClause);

            using (IDataReader dr = this.ExecuteReader(query)) {
                if (dr.Read()) {
                    this.Instance = PopulateFromDataReader(dr, Columns);
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
            // How to generate the INSERT Query
            // INSERT INTO {TableName} ({ColumnNames}) VALUES({ColumnValues}); {scope_identity}
            // ColumnNames => Non Identity Columns which are also not special columns Note:[For insert, do we need to exclude PK columns. For the moment, i think no]
            // special columns => audit columns like deleteddate etc. EXCLUDING CreatedDate and CreatedBy columns. You still want to insert the values for CreatedDate and CreatedBy columns
            // ColumnValues => values on qo for corresponding ColumnNames. If the column name is CreatedDate, then use getdate(). TO DO: get the value of CreatedBy from a common place and inject that
            // NOTE: for Create, we dont have to inspect the dirty fields, as we assume everyting is dirty when creating
            string tableName = GetTableName();
            List<string> columnsForCreateQuery = ColumnsForCreateQuery;
            string insertStatementFormat = "INSERT INTO {0} ({1}) VALUES({2}); {3}";
            
            StringBuilder valuesBuilder = new StringBuilder();
            StringBuilder columnsBuilder = new StringBuilder();
            if (columnsForCreateQuery.Count > 0) {
                foreach (string field in columnsForCreateQuery) {
                    PropertyInfo qoMember = qo.GetType().GetProperty(field);
                    if (qoMember != null) {
                        columnsBuilder.Append(field).Append(",");
                        // If it is CreatedDate, then we need to use getdate()
                        if (IsCreatedDateColumn(field)) {
                            valuesBuilder.Append(" getdate(),");
                        } else {
                            valuesBuilder.Append(SqlSafeString(qoMember.GetValue(qo, null), qoMember.PropertyType)).Append(",");
                        }
                    }
                }
            }

            string columns = columnsBuilder.ToString();
            columns = columns.EndsWith(",") ? columns.Substring(0, columns.Length - 1): columns;
            string values = valuesBuilder.ToString();
            values = values.EndsWith(",") ? values.Substring(0, values.Length - 1): values;

            string query = string.Format(insertStatementFormat, tableName, columns, values, IdentityColumn != null ? "SELECT SCOPE_IDENTITY();" : string.Empty);
            // TO DO:
            // Investigate T to see if it has a Primary Key Column which is an identity column. If it is, then make sure there is
            // only one such column. Assign the return value from ExecuteScalar to that property
            // eg: group.ID = this.ExecuteScalar(query);
            int retValue = this.ExecuteScalar(query);
            
            if (IdentityKey != null) {
                if (IdentityKey.PropertyType == typeof(int) || IdentityKey.PropertyType == typeof(int?)) {
                    IdentityKey.SetValue(qo, retValue, null);
                }
            }
            Console.WriteLine(query);
            this.Instance = qo;
            return this.Instance;
        }

        public T Update(T qo) {
            // How to generate the UPDSTE Query
            // UPDATE {TableName} SET {UpdateQuery} {WhereClause}
            // UpdayeQuery => format x=y. Column included = DirtyFlags - Audit Columns + (LastUpdatedBy and LastUpdatedDate)
            // query the dirty flags to find which columns have changed and only add those. Since dirty flags excluse the primary key columns, the PK columns dont get updated.
            // TO DO: Dirty flag dont include identity column. In case identity column is not part of the primary key, a dirty flag will be generated for that column. So the user can technically update
            // that identity column onthe entity which will trigger the dirty flag, thus resulting in updatequery to have that column. That will result in an error as identity columns cannot be updated. Rectify this by
            // making sure the column is not an identity column.(Fix:U1). If we make sure the dirty flag dont include the identity column for the case where identity column is not part of the primary key, U1 fix
            // will not be needed.
            // Exclude updates for All Audit Columns except LastUpdatedBy and LastUpdatedDate. LastUpdatedDate should be getdate()
            
            // Non Identity Columns which are also not special columns Note:[For insert, do we need to exclude PK columns. For the moment, i think no]
            // special columns => audit columns like deleteddate etc. EXCLUDING CreatedDate and CreatedBy columns. You still want to insert the values for CreatedDate and CreatedBy columns
            // ColumnValues => values on qo for corresponding ColumnNames. If the column name is CreatedDate, then use getdate(). TO DO: get the value of CreatedBy from a common place and inject that
            // NOTE: for Create, we dont have to inspect the dirty fields, as we assume everyting is dirty when creating
            
            string updateStatementFormat = "UPDATE {0} SET {1} {2};";
            string tableName = GetTableName();

            FieldInfo dirtyFlag = qo.GetType().GetField("DirtyFlags");
            object dirtyFlagsObj = dirtyFlag.GetValue(qo);
            // all the possible dirty fields
            List<string> possibleDirtyFields = new List<string>();
            // fields that are actually dirty
            List<string> dirtyFields = new List<string>();
            GetDirtyFields(dirtyFlagsObj, out possibleDirtyFields, out dirtyFields);

            // DirtyFlags - Audit Columns + (LastUpdatedBy and LastUpdatedDate)
            List<string> updateColumns = dirtyFields.Exclude(AuditColumns).Exclude(AuditColumns);
            if (Columns.Contains(UpdatedDateColumnName)) {
                updateColumns.Add(UpdatedDateColumnName);
            }
            if (Columns.Contains(UpdatedByColumnName)) {
                updateColumns.Add(UpdatedByColumnName);
            }
            
            StringBuilder updateBuilder = new StringBuilder();
            if (updateColumns.Count > 0) {
                foreach (string updateColumn in updateColumns) {
                    PropertyInfo qoMember = qo.GetType().GetProperty(updateColumn);
                    if (qoMember != null) {
                        updateBuilder.Append(string.Format("{0} = {1},", updateColumn, SqlSafeString(qoMember.GetValue(qo, null), qoMember.PropertyType)));
                    }
                }
            }
            string updateString = updateBuilder.ToString();
            updateString = updateString.EndsWith(",") ? updateString.Substring(0, updateString.Length - 1) : updateString;

            string whereClause = GetPrimaryKeyWhereClause(qo);
            whereClause = whereClause.Length > 0 ? " WHERE " + whereClause : string.Empty;
            string query = string.Format(updateStatementFormat, tableName, updateString, whereClause);
            Console.WriteLine(query);
            this.ExecuteNonQuery(query);
            return this.Instance;
        }

        //public T Delete(T qo) {
        //}

        #region Private methods

        private object GetDefault<Z>(Z obj) {
            return default(Z);
        }

        private bool IsDirty(string propertyName, object entity, object dirtyFlagObject) {
            List<PropertyInfo> properties = QueryBuilder.QueryBuilderCache.GetProperties(entity.GetType());
            PropertyInfo qoMember = properties.Where(x=>x.Name == propertyName).FirstOrDefault();
            object propertyVal = qoMember.GetValue(entity, null);
            return propertyVal == Default(entity.GetType());

//            Type typeofClassWithGenericStaticMethod = typeof(ClassWithGenericStaticMethod);

//// Grabbing the specific static method
//MethodInfo methodInfo = typeofClassWithGenericStaticMethod.GetMethod("PrintName", System.Reflection.BindingFlags.Static | BindingFlags.Public);

//// Binding the method info to generic arguments
//Type[] genericArguments = new Type[] { typeof(Program) };
//MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments);

//// Simply invoking the method and passing parameters
//// The null parameter is the object to call the method from. Since the method is
//// static, pass null.
//object returnValue = genericMethodInfo.Invoke(null, new object[] { "hello
        }

        private object Default(Type type) {
            if (type.IsValueType) {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        private string GetPrimaryKeyWhereClause(object obj) {
            List<KeyValuePair<string, string>> values = GetPrimaryKeyValues(obj);
            StringBuilder whereClause = new StringBuilder();
            values.ForEach(delegate(KeyValuePair<string, string> pk) {
                whereClause.Append(string.Format(" {0} = {1} AND", pk.Key, pk.Value));
            });
            string retVal = whereClause.ToString();
            return retVal.EndsWith("AND") ? retVal.Substring(0, retVal.Length - 3) : retVal;
        }

        private List<KeyValuePair<string, string>> GetPrimaryKeyValues(object obj) {
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            List<PropertyInfo> pkProperties = QueryBuilderCache.GetPrimaryKeyProperties(obj.GetType());
            foreach (PropertyInfo info in pkProperties) {
                KeyValuePair<string, string> value = new KeyValuePair<string,string>(info.Name, SqlSafeString(info.GetValue(obj, null), info.PropertyType));
                values.Add(value);
            }
            return values;
        }

        private string GetTableName() {
                string typeName = typeof(T).ToString();
                string tableName = typeName.Substring(typeName.LastIndexOf('.') + 1);
                return tableName;
        }

        /// <summary>
        /// Returns the dirty fields on the object
        /// </summary>
        /// <param name="dirtyFlagsObj"></param>
        /// <param name="possibleDirtyFields">All the fields that can be dirty</param>
        /// <param name="dirtyFields">Fields that are actually dirty</param>
        private void GetDirtyFields(object dirtyFlagsObj, out List<string> possibleDirtyFields, out List<string> dirtyFields) {
            possibleDirtyFields = new List<string>();
            dirtyFields = new List<string>();
            FieldInfo[] flags = dirtyFlagsObj.GetType().GetFields();
            foreach (FieldInfo flag in flags) {
                if (flag.FieldType.Name == "Boolean") {
                    bool isDirty = (bool)flag.GetValue(dirtyFlagsObj);
                    if (isDirty) {
                        dirtyFields.Add(flag.Name);
                    }
                    possibleDirtyFields.Add(flag.Name);
                }
            }
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

        private List<string> ColumnsForCreateQuery {
            get {
                // ColumnNames => Non Identity Columns which are also not special columns Note:[For insert, do we need to exclude PK columns. For the moment, i think no]
                // special columns => audit columns like deleteddate etc. EXCLUDING CreatedDate and CreatedBy columns. You still want to insert the values for CreatedDate and CreatedBy columns
                return Columns.Exclude(IdentityColumn).Exclude(AuditColumns.Exclude(CreatedDateColumnName).Exclude(CreatedByColumnName));
            }
        }

        private List<string> Columns {
            get {
                List<string> columns = new List<string>();
                List<PropertyInfo> properties =  QueryBuilder.QueryBuilderCache.GetProperties(typeof(T));
                properties.ForEach(delegate(PropertyInfo property) {
                    columns.Add(property.Name);
                });
                return columns;
            }
        }

        private string IdentityColumn {
            get {
                PropertyInfo info = IdentityKey;
                return info != null ? info.Name : null;
            }
        }

        private PropertyInfo IdentityKey {
            get {
                return QueryBuilder.QueryBuilderCache.GetIdentityColumn(typeof(T));
            }
        }

        private List<string> PrimaryKeyColumns {
            get {
                List<string> columns = new List<string>();
                List<PropertyInfo> properties = QueryBuilder.QueryBuilderCache.GetPrimaryKeyProperties(typeof(T));
                properties.ForEach(delegate(PropertyInfo property) {
                    columns.Add(property.Name);
                });
                return columns;
            }
        }

        private List<string> NonPrimaryKeyColumns {
            get {
                List<string> columns = Columns;
                List<string> pkCols = PrimaryKeyColumns;
                List<string> nonPkCols = new List<string>();
                columns.ForEach(delegate(string col) {
                    if (!pkCols.Contains(col)) {
                        nonPkCols.Add(col);
                    }
                });
                return nonPkCols;
            }
        }

        private List<string> AuditColumns {
            get {
                List<string> columns = Columns;
                List<string> auditCols = new List<string>();
                columns.ForEach(delegate(string col) {
                    if (IsAuditColumn(col)) {
                        auditCols.Add(col);
                    }
                });
                return auditCols;
            }
        }

        #region Sql
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
            if (obj != null) {
                str = obj.ToString();
            }
            return DbUtil.SqlSafeString(str);
        }

        public bool IsAuditColumn(string column) {
            bool result = false;
            if (IsLoginColumn(column)
                || IsCreatedDateColumn(column)
                || IsUpdatedDateColumn(column)
                || IsDeletedDateColumn(column)
                || IsCreatedByColumn(column)
                || IsUpdatedByColumn(column)
                || IsDeletedByColumn(column)) {
                result = true;
            }
            return result;
        }

        public bool IsCreatedByColumn(string column) {
            bool result = false;
            if (column.ToUpper() == CreatedByColumnName || column.ToUpper() == GetShortName(CreatedByColumnName).ToUpper()) {
                result = true;
            }
            return result;
        }

        public bool IsUpdatedByColumn(string column) {
            bool result = false;
            if (column.ToUpper() == UpdatedByColumnName || column.ToUpper() == GetShortName(UpdatedByColumnName).ToUpper()) {
                result = true;
            }
            return result;
        }

        public bool IsDeletedByColumn(string column) {
            bool result = false;
            if (column.ToUpper() == DeletedByColumnName || column.ToUpper() == GetShortName(DeletedByColumnName).ToUpper()) {
                result = true;
            }
            return result;
        }
        public bool IsCreatedDateColumn(string column) {
            bool result = false;
            if (column.ToUpper() == CreatedDateColumnName || column.ToUpper() == GetShortName(CreatedDateColumnName).ToUpper()) {
                result = true;
            }
            return result;
        }
        public bool IsUpdatedDateColumn(string column) {
            bool result = false;
            if (column.ToUpper() == UpdatedDateColumnName || column.ToUpper() == GetShortName(UpdatedDateColumnName).ToUpper()) {
                result = true;
            }
            return result;
        }
        public bool IsDeletedDateColumn(string column) {
            bool result = false;
            if (column.ToUpper() == DeletedDateColumnName || column.ToUpper() == GetShortName(DeletedDateColumnName).ToUpper()) {
                result = true;
            }
            return result;
        }

        public bool IsLoginColumn(string column) {
            bool result = false;
            if (column.ToUpper() == "CREATED_BY_LOGIN" || column.ToUpper() == "LAST_UPDATED_BY_LOGIN"
                || column.ToUpper() == "DELETED_BY_LOGIN") {
                result = true;
            }
            return result;
        }

        public bool IsMultiTenantColumn(string column) {
            bool result = false;
            if (column.ToUpper() == MultiTenantColumnName.ToUpper() || column.ToUpper() == GetShortName(MultiTenantColumnName).ToUpper()) {
                result = true;
            }
            return result;
        }

        public string MultiTenantColumnName {
            get { return "Church_ID"; }
        }
        public string CreatedByColumnName {
            get { return "CREATED_BY_INDIVIDUAL_ID"; }
        }

        public string UpdatedByColumnName {
            get { return "LAST_UPDATED_BY_INDIVIDUAL_ID"; }
        }

        public string DeletedByColumnName {
            get { return "DELETED_BY_INDIVIDUAL_ID"; }
        }

        public string CreatedDateColumnName {
            get { return "CREATED_DATE"; }
        }

        public string UpdatedDateColumnName {
            get { return "LAST_UPDATED_DATE"; }
        }

        public string DeletedDateColumnName {
            get { return "DELETED_DATE"; }
        }

        private string GetShortName(string name) {
            return name.Replace("_", string.Empty);
        }
        #endregion

        //public T Fetch() {
        //    return Fetch(this);
        //}

        //public T Where(Action<T> f) {
        //    f(Instance);
        //    return Instance;
        //}
        #endregion
    }
}
