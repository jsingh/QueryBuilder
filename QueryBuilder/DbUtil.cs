using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace QueryBuilder {
    [Serializable]
	public enum ComparisonOperator {
		EqualTo = 1,
		GreaterThan,
		LessThan,
		GreaterThanOrEqualTo,
		LessThanOrEqualTo,
		NotEqualTo,
		Between,
		IsNotNull,
		IsNull,
		BeginsWith,
		EndsWith,
		Contains,
		None,
		DoesNotContain,
		Custom
	}

    [Serializable]
    public enum ValueTypeStructure {
        Null = 0,
        Boolean = 1,
        Byte = 2,
        Char = 3,
        DateTime = 4,
        Decimal = 5,
        Double = 6,
        Guid = 7,
        Int16 = 8,
        Int32 = 9,
        Int64 = 10,
        TriBool = 11
    }

    public class DbUtil  {
        private static string[] InjectionStmts = { "@@", "--", "xp_" };
        #region Safe SQL Methods
		/// <summary>
		/// Adds encloses string in ' ' and escapes ' within the string.
		/// </summary>
		/// <param name="value">valid sql string</param>
		/// <returns>properly escaped sql string</returns>
		public static string SqlSafeStringNoValidation(string value) {
			return "'" + ReplaceSqlLiteral(value) + "'";
		}

		public static string SqlSafeString(string value) {
			//Cannot exceed 8000 characters in SQL Server (varchar fields)
			return SqlSafeString(value, 8000, ComparisonOperator.EqualTo);
		}

		public static string SqlSafeString(string value, int columnSize) {
			return SqlSafeString(value, columnSize, ComparisonOperator.EqualTo);
		}

		public static string SqlSafeString(string value, ComparisonOperator comparisonOperator) {
			//Cannot exceed 8000 characters in SQL Server (varchar fields)
			return SqlSafeString(value, 8000, comparisonOperator);
		}

		public static string SqlSafeString(string value, int columnSize, ComparisonOperator comparisonOperator) {
			string sqlString = "null ";

			if (value != null 
				&& value.Length > 0
				&& value != NullUtil.Int32Null.ToString()
				&& value != NullUtil.DoubleNull.ToString()
				&& value != NullUtil.DateTimeNull.ToString()) {

				//Trucate the string if it's too long to be stored in the database column
				if(value.Length > columnSize) {
					value = value.Substring(0, columnSize - 1);	
				}

				//DbUtil.ValidateSqlValue(value);
				switch(comparisonOperator){
					case ComparisonOperator.Contains:
						sqlString = "LIKE '%" + ReplaceSqlLiteral(value) + "%' ";
						break;
					case ComparisonOperator.EndsWith:
						sqlString = "LIKE '%" + ReplaceSqlLiteral(value) + "' ";
						break;
					case ComparisonOperator.EqualTo:
						sqlString = "'" + ReplaceSqlLiteral(value) + "' ";
						break;
					case ComparisonOperator.BeginsWith:
						sqlString = "LIKE '" + ReplaceSqlLiteral(value) + "%' ";
						break;
					default:
						sqlString = "'" + ReplaceSqlLiteral(value) + "' ";
						break;
				}
			}

			return sqlString;
		}

		public static string SqlSafeString(int value) {
			string sqlString = "null ";

			if (!NullUtil.IsNull(value)) {
				sqlString = value.ToString();
			}

			return sqlString;
		}
        public static string SqlSafeString(int? value) {
            string sqlString = "null ";

            if (value != null) {
                sqlString = value.ToString();
            }

            return sqlString;
        }

		public static string SqlSafeString(double value) {
			string sqlString = "null ";

			if (!NullUtil.IsNull(value)) {
				sqlString = value.ToString();
			}

			return sqlString;
		}

		/// <summary>
		/// Use this method for between, "less than or equal to", and "greater than or equal to" date ranges
		/// </summary>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		public static string SqlSafeString(double? valueFrom, double? valueTo) {
			string sqlString = "null ";

			if (valueFrom != null && valueTo != null) {
				sqlString = " between '" + valueFrom + "' and '" + valueTo + "' ";
			}
			else if(valueFrom != null) {
				sqlString = " >= '" + valueFrom + "' ";
			}
			else if(valueTo != null) {
				sqlString = " <= '" + valueTo + "' ";
			}

			return sqlString;
		}

		public static string SqlSafeString(decimal value) {
			string sqlString = "null ";

			if (!NullUtil.IsNull(value)) {
				sqlString = value.ToString();
			}

			return sqlString;
		}

		/// <summary>
		/// Use this method for exact dates
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string SqlSafeString(DateTime value) {
			string sqlString = "null ";

			if (!NullUtil.IsNull(value)) {
				//sqlString = " '" + value.ToString("yyyy-MM-dd HH:m:ss.fff") + "' ";
				sqlString = " '" + ConvertToInvariant(value) + "' ";
			}

			return sqlString;
		}
        /// <summary>
        /// Use this method for exact dates
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SqlSafeString(DateTime? value) {
            string sqlString = "null ";

            if (value != null) {
                //sqlString = " '" + value.ToString("yyyy-MM-dd HH:m:ss.fff") + "' ";
                sqlString = " '" + ConvertToInvariant(value) + "' ";
            }

            return sqlString;
        }
		/// <summary>
		/// Use this method for between, "less than or equal to", and "greater than or equal to" date ranges
		/// </summary>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		public static string SqlSafeString(DateTime? dateFrom, DateTime? dateTo) {
			string sqlString = "null ";

			if (dateFrom.HasValue && dateTo.HasValue) {
				//sqlString = " between '" + dateFrom.ToString("yyyy-MM-dd HH:m:ss.fff") + "' and '" + dateTo.ToString("yyyy-MM-dd HH:m:ss.fff") + "' ";
				sqlString = " between '" + ConvertToInvariant(dateFrom.Value) + "' and '" + ConvertToInvariant(dateTo.Value) + "' ";
			}
			else if (dateFrom.HasValue) {
				//sqlString = " >= '" + dateFrom.ToString("yyyy-MM-dd HH:m:ss.fff") + "' ";
				sqlString = " >= '" + ConvertToInvariant(dateFrom.Value) + "' ";
			}
			else if (dateTo.HasValue) {
				//sqlString = " <= '" + dateTo.ToString("yyyy-MM-dd HH:m:ss.fff") + "' ";
				sqlString = " <= '" + ConvertToInvariant(dateTo.Value) + "' ";
			}

			return sqlString;
		}

		public static string SqlSafeString(bool value) {
			return Convert.ToInt16(value).ToString() + " ";
		}
        public static string SqlSafeString(bool? value) {
            string sqlString = "0 ";

            if (value != null) {
                sqlString = Convert.ToInt16(value).ToString() + " ";
            }
            return sqlString;
        }

		public static string SqlSafeString(TriBool value, bool defaultBoolValue) {
			string sqlString = DbUtil.SqlSafeString(defaultBoolValue);

			if (!NullUtil.IsNull(value)) {
				sqlString = Convert.ToInt32(value).ToString() + " ";
			}

			return sqlString;
		}

		public static string SqlSafeString(TriBool value) {
			string sqlString = " null ";

			if (!NullUtil.IsNull(value)) {
				sqlString = Convert.ToInt32(value).ToString() + " ";
			}

			return sqlString;
		}

		public static string SqlSafeString(string value, ValueTypeStructure valueTypeStructure){
			string returnValue = null;

			switch(valueTypeStructure) {
			case ValueTypeStructure.Boolean:
				returnValue = SqlSafeString(bool.Parse(value));
				break;
			case ValueTypeStructure.Byte:
				returnValue = SqlSafeString(value);
				break;
			case ValueTypeStructure.Char:
				returnValue = SqlSafeString(value);
				break;
			case ValueTypeStructure.DateTime:
				returnValue = SqlSafeString(DateTime.Parse(value));
				break;
			case ValueTypeStructure.Decimal:
				returnValue = SqlSafeString(decimal.Parse(value));
				break;
			case ValueTypeStructure.Double:
				returnValue = SqlSafeString(double.Parse(value));
				break;
			case ValueTypeStructure.Guid:
                returnValue = SqlSafeString(value);
				break;
			case ValueTypeStructure.Int16:
				returnValue = SqlSafeString(Int16.Parse(value));
				break;
			case ValueTypeStructure.Int32:
				returnValue = SqlSafeString(Int32.Parse(value));
				break;
			case ValueTypeStructure.Int64:
				returnValue = SqlSafeString(int.Parse(value));
				break;
			case ValueTypeStructure.TriBool:
				//Need to evaluate
				returnValue = SqlSafeString(bool.Parse(value));
				break;
			default:
				returnValue = SqlSafeString(value);
				break;
			}

			return returnValue;
		}
		public static SqlParameter SqlSafeParam(string parameterName, string value) {
			SqlParameter parameter = new SqlParameter(parameterName, DBNull.Value);

			if (value != null 
				&& value.Length > 0
				&& value != NullUtil.Int32Null.ToString()
				&& value != NullUtil.DoubleNull.ToString()
				&& value != NullUtil.DateTimeNull.ToString()) {
				
				//DbUtil.ValidateSqlValue(value);
				parameter.Value = value;
			}

			return parameter;
		}

		public static SqlParameter SqlSafeParam(string parameterName, int value) {
			SqlParameter parameter = new SqlParameter(parameterName, DBNull.Value);
			if (!NullUtil.IsNull(value)) {
				parameter.Value = value;
			}
			return parameter;
		}
        public static SqlParameter SqlSafeParam(string parameterName, int? value) {
            SqlParameter parameter = new SqlParameter(parameterName, DBNull.Value);
            if (value != null) {
                parameter.Value = value;
            }
            return parameter;
        }

		public static SqlParameter SqlSafeParam(string parameterName, double value) {
			SqlParameter parameter = new SqlParameter(parameterName, DBNull.Value);

			if (!NullUtil.IsNull(value)) {
				parameter.Value = value;
			}

			return parameter;
		}

		public static SqlParameter SqlSafeParam(string parameterName, DateTime value) {
			SqlParameter parameter = new SqlParameter(parameterName, DBNull.Value);

			if (!NullUtil.IsNull(value)) {
				parameter.Value = ConvertToInvariant(value);
			}

			return parameter;
		}

		public static SqlParameter SqlSafeParam(string parameterName, bool value) {
			SqlParameter parameter = new SqlParameter(parameterName, Convert.ToInt16(value));
			return parameter;
		}

        public static SqlParameter SqlSafeParam(string parameterName, bool? value) {
            SqlParameter parameter = new SqlParameter(parameterName, DBNull.Value);
            if (value != null) {
                parameter.Value = Convert.ToInt16(value);
            }
            return parameter;
        }

		public static SqlParameter SqlSafeParam(string parameterName, byte[] value, SqlDbType sqlDbType) {
			SqlParameter parameter = new SqlParameter(parameterName, sqlDbType);
			parameter.Value = DBNull.Value;

			if (value != null) {
				parameter.Value = value;
			}

			return parameter;
		}	
		public static SqlParameter SqlSafeParam(string parameterName, int value, SqlDbType sqlDbType) {
			SqlParameter parameter = new SqlParameter(parameterName, sqlDbType);
			parameter.Value = DBNull.Value;

			if (!NullUtil.IsNull(value)) {
				parameter.Value = value;
			}

			return parameter;
		}	
		public static SqlParameter SqlSafeParam(string parameterName, DateTime value, SqlDbType sqlDbType) {
			SqlParameter parameter = new SqlParameter(parameterName, sqlDbType);
			parameter.Value = DBNull.Value;

			if (!NullUtil.IsNull(value)) {
				parameter.Value = ConvertToInvariant(value);
			}

			return parameter;
		}
		public static SqlParameter SqlSafeParam(string parameterName, string value, SqlDbType sqlDbType) {
			SqlParameter parameter = new SqlParameter(parameterName, sqlDbType);
			parameter.Value = DBNull.Value;

			if (value != null 
				&& value.Length > 0
				&& value != NullUtil.Int32Null.ToString()
				&& value != NullUtil.DoubleNull.ToString()
				&& value != NullUtil.DateTimeNull.ToString()) {
				
				//DbUtil.ValidateSqlValue(value);
				parameter.Value = value;
			}

			return parameter;
		}
		public static SqlParameter SqlSafeParam(string parameterName, double value, SqlDbType sqlDbType) {
			SqlParameter parameter = new SqlParameter(parameterName, sqlDbType);
			parameter.Value = DBNull.Value;

			if (!NullUtil.IsNull(value)) {
				parameter.Value = value;
			}

			return parameter;
		}

		public static SqlParameter SqlSafeParam(string parameterName, bool value, SqlDbType sqlDbType) {
			SqlParameter parameter = new SqlParameter(parameterName, sqlDbType);
			parameter.Value = Convert.ToInt16(value);
			return parameter;
		}

		private static string ReplaceSqlLiteral(string sqlString) {
			return sqlString.Replace("'", "''");
		}

		private static void ValidateSqlValue(string value) {
			//Check for SQL injection
			foreach(string stmt in DbUtil.InjectionStmts) {
				if(value.IndexOf(stmt) > 0) {
					throw new Exception("FellowshipTech.Data.Util.DbUtil.ValidateSqlValue");
				}
			}
		}

        public static string CleanSQLStringParm(string parmValue) {
            string rVal = parmValue;
            string[] leathalSqlChars = new string[] { "--", "xp_" };
            //			string[] leathalSqlChars = new string[] {"select","delete","insert","update","drop",";","--","xp_"};
            try {
                for (int i = 0; i < leathalSqlChars.Length; i++) {
                    rVal = rVal.Replace(leathalSqlChars[i], " ");
                }
                return rVal;
            }
            catch {
                return rVal;
            }
        }
		#endregion

        /// <summary>
        /// "Converts a DateTime to an invariant format(Using the Patterns from the Invariant culture type)
        ///parameters
        ///	- sourceDate DateTime in any culture specific format
        /// Translates date to string formatted as: ""yyyy-MM-dd HH:m:ss.fff""
        ///(returns string)"
        /// </summary>
        /// <param name="sourceDate"></param>
        /// <returns></returns>
        public static string ConvertToInvariant(DateTime sourceDate) {

            string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            string convertedDate;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            convertedDate = sourceDate.ToString("yyyy-MM-dd HH:m:ss.fff");

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            return convertedDate;

        }

        public static string ConvertToInvariant(DateTime? sourceDate) {

            string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            string convertedDate;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            convertedDate = ((DateTime)sourceDate).ToString("yyyy-MM-dd HH:m:ss.fff");

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            return convertedDate;

        }

        #region Conversion Methods
        public static byte[] ConvertToByteArray(object value) {
            byte[] result = null;

            if (!Convert.IsDBNull(value)) {
                result = (byte[])value;
            }

            return result;
        }

        public static byte ConvertToByte(object value) {
            byte result = NullUtil.ByteNull;

            if (!Convert.IsDBNull(value)) {
                result = (byte)value;
            }

            return result;
        }

        public static byte? ConvertToByteNull(object value) {
            byte? result = null;

            if (!Convert.IsDBNull(value)) {
                result = (byte)value;
            }

            return result;
        }

        public static string ConvertToString(object value) {
            string result = null;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToString(value);
            }

            return result;
        }

        public static int ConvertToInt32(object value) {
            int result = NullUtil.Int32Null;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToInt32(value);
            }

            return result;
        }

        public static int? ConvertToInt32Null(object value) {
            int? result = null;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToInt32(value);
            }

            return result;
        }

        public static Int16? ConvertToInt16Null(object value) {
            Int16? result = null;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToInt16(value);
            }

            return result;
        }

        public static double ConvertToDouble(object value) {
            double result = NullUtil.DoubleNull;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToDouble(value);
            }

            return result;
        }

        public static decimal ConvertToDecimal(object value) {
            decimal result = NullUtil.DecimalNull;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToDecimal(value);
            }

            return result;
        }

        public static DateTime ConvertToDateTime(object value) {
            DateTime result = NullUtil.DateTimeNull;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToDateTime(value);
            }

            return result;
        }

        public static DateTime? ConvertToDateTimeNull(object value) {
            DateTime? result = null;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToDateTime(value);
            }

            return result;
        }


        public static bool ConvertToBoolean(object value) {
            bool result = false;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToBoolean(value);
            }

            return result;
        }

        public static bool? ConvertToBooleanNull(object value) {
            bool? result = null;

            if (!Convert.IsDBNull(value)) {
                result = Convert.ToBoolean(value);
            }

            return result;
        }

        public static TriBool ConvertToTriBool(object value) {
            TriBool result = TriBool.Null;

            if (!Convert.IsDBNull(value)) {
                result = (TriBool)Convert.ToInt16(value);
            }

            return result;
        }

        public static string ConvertToDelimited(List<int> values, string delimiter) {
            StringBuilder sqlToAppend = new StringBuilder();

            if (values != null && values.Count > 0) {
                for (int i = 0; i < values.Count; i++) {
                    if (i == 0) {
                        sqlToAppend.Append(values[i].ToString());
                    } else {
                        sqlToAppend.Append(delimiter + values[i].ToString());
                    }
                }
            }
            return sqlToAppend.ToString();
        }


        public static string ConvertToDelimited(ArrayList values, string delimiter) {
            string rVal = null;
            if (values != null && values.Count > 0) {
                switch (values[0].GetType().ToString()) {
                    case "System.String":
                        rVal = ConvertToDelimited(values, delimiter, SqlDbType.VarChar);
                        break;
                    default:
                        rVal = ConvertToDelimited(values, delimiter, SqlDbType.Int);
                        break;
                }
            }
            return rVal;
        }

        public static string ConvertToDelimited<T>(List<T> values, string delimiter) {
            string rVal = null;
            if (values != null && values.Count > 0) {
                switch (values[0].GetType().ToString()) {
                    case "System.String":
                        rVal = ConvertToDelimited(values, delimiter, SqlDbType.VarChar);
                        break;
                    default:
                        rVal = ConvertToDelimited(values, delimiter, SqlDbType.Int);
                        break;
                }
            }
            return rVal;
        }
        public static string ConvertToDelimited(ArrayList values, string delimiter, SqlDbType sqlDbType) {
            StringBuilder sqlToAppend = new StringBuilder();
            if (values != null && values.Count > 0) {
                switch (sqlDbType) {
                    case SqlDbType.VarChar:
                        for (int i = 0; i < values.Count; i++) {
                            if (i == 0) {
                                sqlToAppend.Append("'" + values[i].ToString().Replace("'", "''") + "'");
                            } else {
                                sqlToAppend.Append(delimiter + "'" + values[i].ToString().Replace("'", "''") + "'");
                            }
                        }
                        break;
                    default:
                        for (int i = 0; i < values.Count; i++) {
                            if (i == 0) {
                                sqlToAppend.Append(values[i].ToString());
                            } else {
                                sqlToAppend.Append(delimiter + values[i].ToString());
                            }
                        }
                        break;
                }
            } else {
                return null;
            }
            return sqlToAppend.ToString();
        }

        public static string ConvertToDelimited<T>(List<T> values, string delimiter, SqlDbType sqlDbType) {
            StringBuilder sqlToAppend = new StringBuilder();
            if (values != null && values.Count > 0) {
                switch (sqlDbType) {
                    case SqlDbType.VarChar:
                        for (int i = 0; i < values.Count; i++) {
                            if (i == 0) {
                                sqlToAppend.Append("'" + values[i].ToString().Replace("'", "''") + "'");
                            } else {
                                sqlToAppend.Append(delimiter + "'" + values[i].ToString().Replace("'", "''") + "'");
                            }
                        }
                        break;
                    default:
                        for (int i = 0; i < values.Count; i++) {
                            if (i == 0) {
                                sqlToAppend.Append(values[i].ToString());
                            } else {
                                sqlToAppend.Append(delimiter + values[i].ToString());
                            }
                        }
                        break;
                }
            } else {
                return null;
            }
            return sqlToAppend.ToString();
        }

        #endregion
    }
}
