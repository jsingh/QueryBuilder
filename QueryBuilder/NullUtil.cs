using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryBuilder {
    [Serializable]
    public enum TriBool {
        Null = NullUtil.Int32Null,
        False = 0,
        True = 1
    }

    /// <summary>
    /// Handles the representation and evaluation of null values for 
    /// primitives that natively do not support null values.
    /// </summary>
    public class NullUtil {

        public const Int16 Int16Null = Int16.MinValue;
        /// <summary>
        /// Constant that represents a null value for an int.
        /// </summary>
        public const int Int32Null = Int32.MinValue;

        public const Int64 Int64Null = Int64.MinValue;

        /// <summary>
        /// Constant that represents a null value for a double.
        /// </summary>
        public const double DoubleNull = Double.MinValue;

        /// <summary>
        /// Constant that represents a null value for a DateTime.
        /// </summary>
        public static DateTime DateTimeNull = DateTime.MinValue;

        /// <summary>
        /// Constant that represents a null value for a Decimal.
        /// </summary>
        public static Decimal DecimalNull = Decimal.MinValue;

        /// <summary>
        /// Constant that represents a null value for a Byte.
        /// </summary>
        public static Byte ByteNull = Byte.MinValue;

        /// <summary>
        /// Evaluates whether an int value is null.
        /// </summary>
        /// <param name="value">The int value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(int value) {
            if (value == Int32Null) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Evaluates whether an int value is null.
        /// </summary>
        /// <param name="value">The int value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(int? value) {
            return value == null || IsNull(value.Value) ? true : false;
        }
        /// <summary>
        /// Evaluates whether a double value is null.
        /// </summary>
        /// <param name="value">The double value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(double value) {
            if (value == DoubleNull) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Evaluates whether a DateTime value is null.
        /// </summary>
        /// <param name="value">The DateTime value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(DateTime value) {
            DateTime sqlMinDateTime = new DateTime(1753, 1, 1);

            if (value < sqlMinDateTime) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Evaluates whether a DateTime value is null.
        /// </summary>
        /// <param name="value">The DateTime value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(DateTime? value) {
            return value == null || IsNull(value.Value) ? true : false;
        }

        /// <summary>
        /// Evaluates whether a string value is null.
        /// </summary>
        /// <param name="value">The string value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(string value) {
            if (value != null) {
                value = value.Trim();
            }

            if (value == null || value == string.Empty) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Evaluates whether a bool value is null.
        /// </summary>
        /// <param name="value">The TriBool value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(TriBool value) {
            if (value == TriBool.Null) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Evaluates whether an int value is null.
        /// </summary>
        /// <param name="value">The int value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(decimal? value) {
            return value == null || IsNull(value.Value) ? true : false;
        }

        /// <summary>
        /// Evaluates whether a decimal value is null.
        /// </summary>
        /// <param name="value">The decimal value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(decimal value) {
            if (value == DecimalNull || value == Int32Null) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Evaluates whether an enum value is null.
        /// </summary>
        /// <param name="value">The enum value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(System.Enum value) {

            if (value == null || !System.Enum.IsDefined(value.GetType(), value)) {
                return true;
            } else {
                return false;
            }
        }
    }
}
