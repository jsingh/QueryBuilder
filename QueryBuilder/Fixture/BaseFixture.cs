using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web;
using System.Collections;

namespace QueryBuilder.Fixture {
    public class BaseFixture<T> : IBuilder<T> {
        public BaseFixture() {
            RangeValueMax.Add(typeof(byte), Int32.MinValue);
            RangeValueMax.Add(typeof(sbyte), sbyte.MinValue);
            RangeValueMax.Add(typeof(Int16), Int16.MinValue);
            RangeValueMax.Add(typeof(ushort), ushort.MinValue);
            RangeValueMax.Add(typeof(Int32), Int32.MinValue);
            RangeValueMax.Add(typeof(uint), uint.MinValue);
            RangeValueMax.Add(typeof(Int64), Int64.MinValue);
            RangeValueMax.Add(typeof(ulong), ulong.MinValue);
            RangeValueMax.Add(typeof(float), float.MinValue);
            RangeValueMax.Add(typeof(double), double.MinValue);
            RangeValueMax.Add(typeof(decimal), decimal.MinValue);
            RangeValueMax.Add(typeof(DateTime), DateTime.MinValue);
            RangeValueMax.Add(typeof(bool), false);
        }
        private Hashtable RangeValueMax = new Hashtable();
        public DataContextType DataContextType { get; set; }
        private List<int> Options = new List<int>();
        private string prefix = string.Empty;
        public int ChurchID { get; set; }
        private string Prefix {
            get {
                if (string.IsNullOrEmpty(prefix)) {
                    return "_";
                }
                return prefix;
            }
        }
        protected T Instance { get; set; }

        public virtual void SetupData() {
            Type type = typeof(T);
            SetProperties(type);
        }

        public BaseFixture<T> WithDataType(DataContextType dataContextType) {
            this.DataContextType = dataContextType;
            return this;
        }

        public BaseFixture<T> WithOption(Options option, string data) {
            this.Options.Add((int)option);
            if (option == Fixture.Options.DummyStringData) {
                prefix = data;
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">type of the class containing the DataAnnotations attributes</param>
        protected virtual void SetProperties(Type type) {
            // NOTE: Currently, just implement the Fixture.DataContextType.Valid
            //# Range – Enables you to validate whether the value of a property falls between a specified range of values.
            // Range attribute uses type's IComparable.CompareTo to determine whether a range falls inside the specified range of values.
            //# ReqularExpression – Enables you to validate whether the value of a property matches a specified regular expression pattern. [If the value of the property is null or an empty string (""), the value automatically passes validation for the RegularExpressionAttribute attribute. To validate that the value is not null or an empty string, use the RequiredAttribute attribute.]
            //# Required – Enables you to mark a property as required.
            //# StringLength – Enables you to specify a maximum length for a string property.

            // Get all the public properties
            // NOTE: type is of the type of the class referred to by MetadataTypeAttribute.
            // The type will be interogated for the properties that have any DataAnnotation attributes on top of it.
            // Then the same propertie on the object will be modified accordingly.
            PropertyInfo[] properties = type.GetProperties();
            bool useDummyData = Options.Contains((int)QueryBuilder.Fixture.Options.DummyStringData);
            // PropertyInfo[] properties = this.Instance.GetType().GetProperties();
            foreach (PropertyInfo ppty in properties) {
                // for each property, find the validation attributes it has
                object[] attributes = ppty.GetCustomAttributes(false);
                Type propertyType = ppty.PropertyType;
                PropertyInfo property = GetProperty(ppty.Name);
                // If the property on the instance exists, then create values.
                if (property != null) {
                    StringLengthAttribute strLenAttr = attributes.Where(x => x.GetType() == typeof(StringLengthAttribute)).FirstOrDefault() as StringLengthAttribute;
                    RequiredAttribute reqAttr = attributes.Where(x => x.GetType() == typeof(RequiredAttribute)).FirstOrDefault() as RequiredAttribute;
                    RangeAttribute rangeAttr = attributes.Where(x => x.GetType() == typeof(RangeAttribute)).FirstOrDefault() as RangeAttribute;
                    RegularExpressionAttribute regExAttr = attributes.Where(x => x.GetType() == typeof(RegularExpressionAttribute)).FirstOrDefault() as RegularExpressionAttribute;
                    // for strings if it has Required as well as StringLength, then we use StringLength only
                    if (propertyType == typeof(string)) {
                        // Validation if any of the following is true
                        // If there is a required attribute (reqAttr != null)
                        // If there is no required attribute, but we need to create some valid data ((reqAttr == null) && (this.DataContextType == Fixture.DataContextType.Valid && useDummyData))
                        if (reqAttr != null || useDummyData) {
                            if (regExAttr != null) {
                                if (regExAttr.Pattern == Regex.Email) {
                                    string emailFormat = "{0}@b.com";
                                    string prefix = "a";
                                    // check to see if there is string length attribute also. If there is one, we need to make sure that
                                    // the constraints of that attribute are also met
                                    // The string length attribute's Maximum length should be >=5
                                    if (strLenAttr != null && strLenAttr.MaximumLength >= 7) {
                                        prefix = GetString(strLenAttr.MaximumLength - 6, null);
                                    }
                                    property.SetValue(this.Instance, string.Format(emailFormat, prefix), null);

                                } else if (regExAttr.Pattern == Regex.Zip) {
                                    // we need to create a valid zip. 
                                    string validZip = "12345";
                                    property.SetValue(this.Instance, validZip, null);
                                }
                            } else if (strLenAttr != null) {
                                if (strLenAttr.MaximumLength > 0) {
                                    property.SetValue(this.Instance, GetString(strLenAttr.MaximumLength, property.Name), null);
                                } else if (strLenAttr.MinimumLength > 0) {
                                    property.SetValue(this.Instance, GetString(strLenAttr.MinimumLength, property.Name), null);
                                }
                            } else if (reqAttr != null || Options.Contains((int)QueryBuilder.Fixture.Options.DummyStringData)) {
                                string data = Prefix;
                                data += property.Name;
                                property.SetValue(this.Instance, data, null);
                            }
                        }
                    } else {
                        // Find if the type is nullable or not
                        // For non nullable types, the RequiredAttribute doesnt make sense, as you cannot make them null
                        // If(NotNullable) {
                        //      Check for rangeAttr
                        // } else {
                        //       Check for rangeAttr, else make them nullable
                        // }
                        // For other datatypes, if they have Range as well as Required, then we just use the Range
                        ////////////////////////////Range Attr//////////////////////////
                        if (rangeAttr != null) {
                            object min = rangeAttr.Minimum;
                            object max = rangeAttr.Maximum;
                            if (rangeAttr.OperandType != typeof(int) && rangeAttr.OperandType != typeof(double)) {
                                min = Convert.ChangeType(rangeAttr.Minimum, rangeAttr.OperandType);
                                max = Convert.ChangeType(rangeAttr.Maximum, rangeAttr.OperandType);
                            }

                            property.SetValue(this.Instance, min, null);

                        } else if (reqAttr != null) {
                            property.SetValue(this.Instance, this.MaxValueForType(propertyType), null);
                        }
                    }
                }
            }
        }

        private void SetStringLengthConstraint(PropertyInfo property, StringLengthAttribute strLenAttr) {

        }

        private void SetRequiredConstraint() {
        }

        private void SetRegularExpressionConstraint() {
        }

        private void SetRangeConstraint() {
        }

        protected PropertyInfo GetProperty(string propertyName) {
            PropertyInfo[] properties = this.Instance.GetType().GetProperties();
            return properties.Where(x => x.Name.Equals(propertyName)).FirstOrDefault();
        }

        public virtual T Build() {
            T instance = Activator.CreateInstance<T>();
            this.Instance = instance;
            this.SetupData();
            return this.Instance;
        }

        public virtual T Build(T overrideObject) {
            Build();
            Override(overrideObject);
            return this.Instance;
        }

        protected virtual T InjectT(InjectionType injectionType) {
            switch (injectionType) {
                case InjectionType.Create:
                    // use reflection to call Create on the object
                    Create();
                    break;
                case InjectionType.CreateIfDoesntExist:
                    object returnInstance = First();
                    if (returnInstance != null) {
                        T ins = (T)returnInstance;
                        this.Instance = ins;
                    } else {
                        // Create an instance
                        Create();
                    }
                    break;
                case InjectionType.First:
                    First();
                    break;
            }

            return this.Instance;
        }

        public virtual T Inject(InjectionType injectionType = InjectionType.CreateIfDoesntExist) {
            //T instance = Activator.CreateInstance<T>();
            //this.Instance = instance;
            if (injectionType == InjectionType.Fetch) {
                return First();
            }
            Build();
            return InjectT(injectionType);
        }

        public virtual T Inject(T overrideObject, InjectionType injectionType = InjectionType.CreateIfDoesntExist) {
            //T instance = Activator.CreateInstance<T>();
            //this.Instance = instance;
            if (injectionType == InjectionType.Fetch) {
                return Fetch(overrideObject);
            }
            Build(overrideObject);
            return InjectT(injectionType);
        }

        protected virtual void Override(T copyFrom) {
            // Query the Dirty Flags of copyFrom object and copy those to copyTo
            FieldInfo dirtyFlag = copyFrom.GetType().GetField("DirtyFlags");
            object dirtyFlagsObj = dirtyFlag.GetValue(copyFrom);
            // all the possible dirty fields
            List<string> possibleDirtyFields = new List<string>();
            // fields that are actually dirty
            List<string> dirtyFields = new List<string>();
            GetDirtyFields(dirtyFlagsObj, out possibleDirtyFields, out dirtyFields);
            // for each dirty flag, set the property on the copyTo object
            foreach (string df in dirtyFields) {
                PropertyInfo property = GetProperty(df);
                property.SetValue(this.Instance, GetValue(df, copyFrom), null);
            }
        }

        protected object GetValue(string propertyName, T obj) {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            PropertyInfo ppty = properties.Where(x => x.Name.Equals(propertyName)).FirstOrDefault();
            if (ppty != null) {
                return ppty.GetValue(obj, null);
            }
            return null;
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

        protected virtual void Create() {
            Type ty = typeof(T);
            Type qb = typeof(QueryBuilder.QueryBuilder<>).MakeGenericType(typeof(T));
            MethodInfo method = qb.GetMethod("Init", System.Reflection.BindingFlags.Static | BindingFlags.Public);
            object returnInstance = method.Invoke(null, null);
            if (returnInstance != null) {
                qb.InvokeMember("Create",
                            BindingFlags.InvokeMethod,
                            null,
                            returnInstance,
                            new object[]{this.Instance});
            }
        }

        protected virtual T First() {
            // QueryBuilder.QueryBuilder<Address>.Init().Fetch(new Address { City = "Irving", Country = 1 });
            Type ty = typeof(T);
            Type qb = typeof(QueryBuilder.QueryBuilder<>).MakeGenericType(typeof(T));
            MethodInfo method = qb.GetMethod("Init", System.Reflection.BindingFlags.Static | BindingFlags.Public);
            object returnInstance = (T)method.Invoke(null, null);
            if (returnInstance != null) {
                Type type = typeof(T);
                return (T)(type.InvokeMember("First",
                             BindingFlags.Default | BindingFlags.InvokeMethod,
                             null,
                             returnInstance,
                             null));
            }
            return default(T);
        }

        protected virtual T Fetch(T qo) {
            // QueryBuilder.QueryBuilder<Address>.Init().Fetch(new Address { City = "Irving", Country = 1 });
            Type ty = typeof(T);
            Type qb = typeof(QueryBuilder.QueryBuilder<>).MakeGenericType(typeof(T));
            MethodInfo method = qb.GetMethod("Init", System.Reflection.BindingFlags.Static | BindingFlags.Public);
            object returnInstance = (T)method.Invoke(null, null);
            if (returnInstance != null) {
                Type type = typeof(T);
                return (T)(type.InvokeMember("Fetch",
                             BindingFlags.Default | BindingFlags.InvokeMethod,
                             null,
                             returnInstance,
                             new object[] { qo }));
            }
            return default(T);
        }

        public virtual IBuilder<T> Builder() {
            T instance = Activator.CreateInstance<T>();
            string builderType = instance.GetType().ToString();
            builderType = "QueryBuilder.Fixture." + builderType.Substring(builderType.LastIndexOf(".") + 1) + "Builder";
            Type type = Type.GetType(builderType);
            return (IBuilder<T>)Activator.CreateInstance(type);
        }

        public virtual FormCollection SetUpForm(string prefix = "") {
            T instance = Build();
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            FormCollection form = new FormCollection();
            // PropertyInfo[] properties = this.Instance.GetType().GetProperties();
            foreach (PropertyInfo ppty in properties) {
                Type propertyType = ppty.PropertyType;
                PropertyInfo property = GetProperty(ppty.Name);
                object val = property.GetValue(this.Instance, null);
                form.Add(prefix + property.Name, val == null ? string.Empty : val.ToString());
            }
            return form;
        }

        public virtual FormCollection SetUpForm(T obj) {
            this.Instance = obj;
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            FormCollection form = new FormCollection();
            // PropertyInfo[] properties = this.Instance.GetType().GetProperties();
            foreach (PropertyInfo ppty in properties) {
                Type propertyType = ppty.PropertyType;
                PropertyInfo property = GetProperty(ppty.Name);
                object val = property.GetValue(this.Instance, null);
                form.Add(property.Name, val == null ? string.Empty : val.ToString());
            }
            return form;
        }


        //public static T Init() {
        //    T instance = Activator.CreateInstance<T>();
        //    this.Instance = instance;
        //    return instance
        //}

        public static BaseFixture<T> Init() {
            BaseFixture<T> obj = new BaseFixture<T>();
            return obj;
        }

        public BaseFixture<T> WithChurchID(int churchID) {
            this.ChurchID = churchID;
            return this;
        }

        #region Helpers
        private string GetString(int length, string suffix) {
            if (suffix == null) {
                suffix = string.Empty;
            }
            string data = Prefix + suffix;
            if (data.Length <= length) {
                string filler = " "; ;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < (length - data.Length); i++) {
                    sb.Append(filler);
                }
                data += sb.ToString();
            } else {
                data = data.Substring(0, length);
            }
            return data;
        }

        //public static object DefaultForType(Type targetType) {
        //    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        //}

        private object MaxValueForType(Type targetType) {
            //return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            if (targetType.IsGenericType
            && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
                targetType = Nullable.GetUnderlyingType(targetType);
            }
            FieldInfo field = targetType.GetField("MaxValue");
            if (field != null) {
                return field.GetValue(Activator.CreateInstance(targetType));
            }

            return RangeValueMax[targetType];
        }

        private object MinValueForType(Type targetType) {
            //return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            FieldInfo field = targetType.GetField("MinValue");
            if (field != null) {
                return field.GetValue(Activator.CreateInstance(targetType));
            }

            return RangeValueMax[targetType];
            // return targetType.GetField("MinValue").GetValue(Activator.CreateInstance(targetType));
        }

        private object GetOutOfRangeValue(Type targetType, RangeAttribute rangeAttr) {
            object min = rangeAttr.Minimum;
            object max = rangeAttr.Maximum;

            if (targetType.IsValueType) {
                Type type = targetType.GetType();
                if (type == typeof(byte)) {
                    if ((byte)min > byte.MinValue) {
                        return byte.MinValue;
                    } else {
                        return byte.MaxValue;
                    }
                }

                if (type == typeof(sbyte)) {
                    if ((sbyte)min > byte.MinValue) {
                        return sbyte.MinValue;
                    } else {
                        return sbyte.MaxValue;
                    }
                }

                if (type == typeof(Int16)) {
                    if ((Int16)min > Int16.MinValue) {
                        return Int16.MinValue;
                    } else {
                        return Int16.MaxValue;
                    }
                }
                if (type == typeof(ushort)) {
                    if ((ushort)min > ushort.MinValue) {
                        return ushort.MinValue;
                    } else {
                        return ushort.MaxValue;
                    }
                }
                if (type == typeof(uint)) {
                    if ((uint)min > uint.MinValue) {
                        return uint.MinValue;
                    } else {
                        return uint.MaxValue;
                    }
                }
                if (type == typeof(Int64)) {
                    if ((Int64)min > Int64.MinValue) {
                        return Int64.MinValue;
                    } else {
                        return Int64.MaxValue;
                    }
                }
                if (type == typeof(ulong)) {
                    if ((ulong)min > ulong.MinValue) {
                        return ulong.MinValue;
                    } else {
                        return ulong.MaxValue;
                    }
                }
                if (type == typeof(double)) {
                    if ((double)min > double.MinValue) {
                        return double.MinValue;
                    } else {
                        return double.MaxValue;
                    }
                }
                if (type == typeof(decimal)) {
                    if ((decimal)min > decimal.MinValue) {
                        return decimal.MinValue;
                    } else {
                        return decimal.MaxValue;
                    }
                }
                if (type == typeof(DateTime)) {
                    if ((DateTime)min > DateTime.MinValue) {
                        return DateTime.MinValue;
                    } else {
                        return DateTime.MaxValue;
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
