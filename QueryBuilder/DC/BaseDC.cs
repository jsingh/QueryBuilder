using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SystemDiagnostics = System.Diagnostics;
namespace QueryBuilder.DC {
    public class BaseDC {
       
        private string _connectionString = null;

        private BaseDC() { }

        public BaseDC(string connectionString) {
            _connectionString = connectionString;
        }

        #region ExecuteDataReader

        /// <summary>Main method that executes a reader</summary>
        /// <param name="connectionString">Defines the connection string</param>
        /// <param name="cmdType">Defines the type of sql command</param>
        /// <param name="sql">Defines the sql text (either stored procedure name or sql command text)</param>
        /// <param name="sqlParams">Array of sql parameters</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string connectionString, CommandType cmdType, string sql, SqlParameter[] sqlParams) {
            WriteDebugStatement(sql, "SQL");
            var conn = new SqlConnection(connectionString);
            var cmd = new SqlCommand(sql, conn);
            cmd.CommandType = cmdType;
            if (sqlParams != null) {
                foreach (var param in sqlParams) {
                    if (param.Value == null) {
                        param.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(param);
                }
            }
            conn.Open();
            var reader = cmd.ExecuteReader();
            return new CustomDataReader(reader, conn);
        }

        protected IDataReader ExecuteReader(CommandType cmdType, string sql, SqlParameter[] sqlParams) {
            return this.ExecuteReader(this._connectionString, cmdType, sql, sqlParams);
        }

        protected IDataReader ExecuteReader(string sql, SqlParameter[] sqlParams) {
            return this.ExecuteReader(CommandType.Text, sql, sqlParams);
        }

        protected IDataReader ExecuteReader(string connectionString, string sql) {
            return this.ExecuteReader(connectionString, CommandType.Text, sql, (SqlParameter[])null);
        }
        
        protected IDataReader ExecuteReader(string sql) {
            return this.ExecuteReader(sql, (SqlParameter[])null);
        }

        protected IDataReader ExecuteReader(CommandType commandType, string spName, List<SqlParameter> sqlParameters) {
            return ExecuteReader(commandType, spName, sqlParameters.ToArray());
        }

        //protected IDataReader ExecuteReader<ItemType>(string sql, Foundation.Model.QueryObject.BaseQueryObject baseQueryObject, Foundation.Model.Entity.BaseCollection<ItemType> baseCollection, SqlParameter[] sqlParams) where ItemType : Foundation.Model.Entity.BaseEntity<ItemType> {
        //    return ExecuteReader(DbUtil.SqlSelectCommandText(sql, _connectionString, baseQueryObject, baseCollection, sqlParams));
        //}

        //protected IDataReader ExecuteReader<ItemType>(string sql, Foundation.Model.QueryObject.BaseQueryObject baseQueryObject, Foundation.Model.Entity.BaseCollection<ItemType> baseCollection, List<SqlParameter> sqlParameters) where ItemType : Foundation.Model.Entity.BaseEntity<ItemType> {
        //    return ExecuteReader(DbUtil.SqlSelectCommandText(sql, _connectionString, baseQueryObject, baseCollection, sqlParameters.ToArray()), sqlParameters.ToArray());
        //}

        #endregion

        #region ExecuteNonQuery

        /// <summary>Main method that executes a non query</summary>
        /// <param name="connectionString">Defines the type connection string</param>
        /// <param name="cmdType">Defines the type of sql command</param>
        /// <param name="CommandText">Defines the sql text (either stored procedure name or sql command text)</param>
        /// <param name="sqlParams">Array of sql parameters</param>
        /// <param name="timeout">Allows you to set the timeout for the command</param>
        /// <returns>int</returns>
        protected int ExecuteNonQuery(string connectionString, CommandType cmdType, string CommandText, SqlParameter[] sqlParams, int? timeout) {
            WriteDebugStatement(CommandText, "SQL");
            using (var conn = new SqlConnection(connectionString)) {
                using (var cmd = new SqlCommand(CommandText, conn)) {
                    cmd.CommandType = cmdType;
                    if (timeout.HasValue) {
                        cmd.CommandTimeout = timeout.Value;
                    }
                    if (sqlParams != null) {
                        foreach (var param in sqlParams) {
                            if (param.Value == null) {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);
                        }
                    }
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        protected int ExecuteNonQuery(string connectionString, CommandType cmdType, string CommandText, SqlParameter[] sqlParams) {
            return ExecuteNonQuery(connectionString, cmdType, CommandText, sqlParams, null);
        }
        
        protected int ExecuteNonQuery(CommandType cmdType, string CommandText, SqlParameter[] sqlParams, int? timeout) {
            return ExecuteNonQuery(this._connectionString, cmdType, CommandText, sqlParams, null);
        }

        protected int ExecuteNonQuery(string spName, List<SqlParameter> sqlParameters) {
            return ExecuteNonQuery(CommandType.StoredProcedure, spName, sqlParameters.ToArray());
        }

        protected int ExecuteNonQuery(string connectionString, string sql) {
            return ExecuteNonQuery(connectionString, CommandType.Text, sql, (SqlParameter[])null, null);
        }

        protected int ExecuteNonQuery(string sql) {
            return ExecuteNonQuery(CommandType.Text, sql, (SqlParameter[])null);
        }

        protected int ExecuteNonQuery(string sql, SqlParameter[] sqlParams) {
            return ExecuteNonQuery(CommandType.Text, sql, sqlParams);
        }

        protected int ExecuteNonQuery(CommandType cmdType, string CommandText, SqlParameter[] sqlParams) {
            return ExecuteNonQuery(cmdType, CommandText, sqlParams, null);
        }

        #endregion

        #region ExecuteScalar

        /// <summary>Main method that executes scalar</summary>
        /// <param name="connectionString">Defines the connection string</param>
        /// <param name="cmdType">Defines the type of sql command</param>
        /// <param name="CommandText">Defines the sql text (either stored procedure name or sql command text)</param>
        /// <param name="sqlParams">Array of sql parameters</param>
        /// <returns>int</returns>
        protected int ExecuteScalar(string connectionString, CommandType cmdType, string CommandText, SqlParameter[] sqlParams) {
            WriteDebugStatement(CommandText, "SQL");
            using (var conn = new SqlConnection(connectionString)) {
                using (var cmd = new SqlCommand(CommandText, conn)) {
                    cmd.CommandType = cmdType;
                    if (sqlParams != null) {
                        foreach (var param in sqlParams) {
                            if (param.Value == null) {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);
                        }
                    }
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        protected int ExecuteScalar(CommandType cmdType, string CommandText, SqlParameter[] sqlParams) {
            return ExecuteScalar(this._connectionString, cmdType, CommandText.ToString(), sqlParams);
        }
        
        protected int ExecuteScalar(string connectionString, string sql) {
            return ExecuteScalar(connectionString, CommandType.Text, sql.ToString(), (SqlParameter[])null);
        }
        
        protected int ExecuteScalar(string sql) {
            return ExecuteScalar(CommandType.Text, sql.ToString(), (SqlParameter[])null);
        }

        protected int ExecuteScalar(string sql, SqlParameter[] sqlParams) {
            return ExecuteScalar(CommandType.Text, sql.ToString(), sqlParams);
        }

        #endregion

        /// <summary>Main method that executes a dataset</summary>
        /// <param name="connectionString">Defines the connection string</param>
        /// <param name="cmdType">Defines the type of sql command</param>
        /// <param name="CommandText">Defines the sql text (either stored procedure name or sql command text)</param>
        /// <param name="sqlParams">Array of sql parameters</param>
        /// <returns>DataSet</returns>
        protected DataSet ExecuteDataset(string connectionString, CommandType cmdType, string CommandText, SqlParameter[] sqlParams) {
            WriteDebugStatement(CommandText, "SQL");
            using (var conn = new SqlConnection(connectionString)) {
                using (var cmd = new SqlCommand(CommandText, conn)) {
                    cmd.CommandType = cmdType;
                    if (sqlParams != null) {
                        foreach (var param in sqlParams) {
                            if (param.Value == null) {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);
                        }
                    }
                    conn.Open();
                    var da = new SqlDataAdapter(cmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
        }

        protected string ConnectionString {
            get { return _connectionString; }
        }

        private void WriteDebugStatement(string stringToWrite, string category) {
            SystemDiagnostics.Debug.WriteLine(stringToWrite, category);
        }

        //protected void ResetEntityState(Foundation.Model.Entity.IEntity entity) {
        //    entity.EntityState = Foundation.Model.Enum.EntityState.UnChanged;
        //}
    }
    public class CustomDataReader : IDataReader, IDisposable {
        public CustomDataReader(SqlDataReader dataReader, SqlConnection connection) {
            this.DataReader = dataReader;
            this.Connection = connection;
        }

        protected SqlDataReader DataReader { get; set; }
        protected SqlConnection Connection { get; set; }

        public void Dispose() {
            while (this.DataReader.Read()) {
                // just want to make sure we get to the end of the result set
            }

            this.DataReader.Close();
            this.DataReader.Dispose();

            if (Connection.State == ConnectionState.Open) {
                this.Connection.Close();
            }
            this.Connection.Dispose();
        }

        public void Close() {
            this.DataReader.Close();
        }

        public int Depth { get { return this.DataReader.Depth; } }

        public DataTable GetSchemaTable() { return this.DataReader.GetSchemaTable(); }

        public bool IsClosed { get { return this.DataReader.IsClosed; } }

        public bool NextResult() { return this.DataReader.NextResult(); }

        public bool Read() { return this.DataReader.Read(); }

        public int RecordsAffected { get { return this.DataReader.RecordsAffected; } }

        public int FieldCount { get { return this.DataReader.FieldCount; } }

        public bool GetBoolean(int i) { return this.DataReader.GetBoolean(i); }

        public byte GetByte(int i) { return this.DataReader.GetByte(i); }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            return this.DataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i) { return this.DataReader.GetChar(i); }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            return this.DataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i) { throw new NotImplementedException(); }

        public string GetDataTypeName(int i) { return this.DataReader.GetDataTypeName(i); }

        public DateTime GetDateTime(int i) { return this.DataReader.GetDateTime(i); }

        public decimal GetDecimal(int i) { return this.DataReader.GetDecimal(i); }

        public double GetDouble(int i) { return this.DataReader.GetDouble(i); }

        public Type GetFieldType(int i) { return this.DataReader.GetFieldType(i); }

        public float GetFloat(int i) { return this.DataReader.GetFloat(i); }

        public Guid GetGuid(int i) { return this.DataReader.GetGuid(i); }

        public short GetInt16(int i) { return this.DataReader.GetInt16(i); }

        public int GetInt32(int i) { return this.DataReader.GetInt32(i); }

        public long GetInt64(int i) { return this.DataReader.GetInt64(i); }

        public string GetName(int i) { return this.DataReader.GetName(i); }

        public int GetOrdinal(string name) { return this.DataReader.GetOrdinal(name); }

        public string GetString(int i) { return this.DataReader.GetString(i); }

        public object GetValue(int i) { return this.DataReader.GetValue(i); }

        public int GetValues(object[] values) { return this.DataReader.GetValues(values); }

        public bool IsDBNull(int i) { return this.DataReader.GetBoolean(i); }

        public object this[string name] {
            get { return this.DataReader[name]; }
        }

        public object this[int i] {
            get { return this.DataReader[i]; }
        }
    }
}
