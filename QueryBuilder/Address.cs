using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QueryBuilder.Attributes;
using QueryBuilder;

namespace QueryBuilder {
    [Serializable]
    public partial class Address {
        #region Declarations
        private int _addressID = 0;
        private int _entityID = 0;
        private int _addressTypeID = 0;
        private string _address1 = null;
        private string _address2 = null;
        private string _address3 = null;
        private string _city = null;
        private string _stProvince = null;
        private int? _state = null;
        private string _postalCode = null;
        private int _country = 0;
        private string _county = null;
        private bool _listed = false;
        private bool _isPreferred = false;
        private DateTime _createdDate = DateTime.MinValue;
        private int _createdBy = 0;
        private DateTime? _lastUpdatedDate = null;
        private int? _lastUpdatedBy = null;
        #endregion Declarations



        #region Constructors
        public Address() { }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Database Mapping: Address.AddressID
        /// </summary>
        [IdentityColumn]
        [PrimaryKey]
        public int AddressID {
            get { return _addressID; }
            set { _addressID = value; }
        }

        /// <summary>
        /// Database Mapping: Address.EntityID
        /// </summary>
        public int EntityID {
            get { return _entityID; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.EntityID = this.GetDirtyFlag(_entityID, value);
                }
                _entityID = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.AddressTypeID
        /// </summary>
        public int AddressTypeID {
            get { return _addressTypeID; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.AddressTypeID = this.GetDirtyFlag(_addressTypeID, value);
                }
                _addressTypeID = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.Address1
        /// </summary>
        public string Address1 {
            get { return _address1; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.Address1 = this.GetDirtyFlag(_address1, value);
                }
                _address1 = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.Address2
        /// </summary>
        public string Address2 {
            get { return _address2; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.Address2 = this.GetDirtyFlag(_address2, value);
                }
                _address2 = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.Address3
        /// </summary>
        public string Address3 {
            get { return _address3; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.Address3 = this.GetDirtyFlag(_address3, value);
                }
                _address3 = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.City
        /// </summary>
        public string City {
            get { return _city; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.City = this.GetDirtyFlag(_city, value);
                }
                _city = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.StProvince
        /// </summary>
        public string StProvince {
            get { return _stProvince; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.StProvince = this.GetDirtyFlag(_stProvince, value);
                }
                _stProvince = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.State
        /// </summary>
        public int? State {
            get { return _state; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.State = this.GetDirtyFlag(_state, value);
                }
                _state = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.PostalCode
        /// </summary>
        public string PostalCode {
            get { return _postalCode; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.PostalCode = this.GetDirtyFlag(_postalCode, value);
                }
                _postalCode = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.Country
        /// </summary>
        public int Country {
            get { return _country; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.Country = this.GetDirtyFlag(_country, value);
                }
                _country = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.County
        /// </summary>
        public string County {
            get { return _county; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.County = this.GetDirtyFlag(_county, value);
                }
                _county = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.Listed
        /// </summary>
        public bool Listed {
            get { return _listed; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.Listed = this.GetDirtyFlag(_listed, value);
                }
                _listed = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.IsPreferred
        /// </summary>
        public bool IsPreferred {
            get { return _isPreferred; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.IsPreferred = this.GetDirtyFlag(_isPreferred, value);
                }
                _isPreferred = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.CreatedDate
        /// </summary>
        public DateTime CreatedDate {
            get { return _createdDate; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.CreatedDate = this.GetDirtyFlag(_createdDate, value);
                }
                _createdDate = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.CreatedBy
        /// </summary>
        public int CreatedBy {
            get { return _createdBy; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.CreatedBy = this.GetDirtyFlag(_createdBy, value);
                }
                _createdBy = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.LastUpdatedDate
        /// </summary>
        public DateTime? LastUpdatedDate {
            get { return _lastUpdatedDate; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.LastUpdatedDate = this.GetDirtyFlag(_lastUpdatedDate, value);
                }
                _lastUpdatedDate = value;
            }
        }
        /// <summary>
        /// Database Mapping: Address.LastUpdatedBy
        /// </summary>
        public int? LastUpdatedBy {
            get { return _lastUpdatedBy; }
            set {
                if (this.CanDirty) {
                    this.DirtyFlags.LastUpdatedBy = this.GetDirtyFlag(_lastUpdatedBy, value);
                }
                _lastUpdatedBy = value;
            }
        }

        public AddressDirtyFlags DirtyFlags;
        internal bool CanDirty = true;
        #endregion Public Properties

        #region ObjectDataSource support
        [Serializable]
        public struct AddressDirtyFlags {
            public bool EntityID;
            public bool AddressTypeID;
            public bool Address1;
            public bool Address2;
            public bool Address3;
            public bool City;
            public bool StProvince;
            public bool State;
            public bool PostalCode;
            public bool Country;
            public bool County;
            public bool Listed;
            public bool IsPreferred;
            public bool CreatedDate;
            public bool CreatedBy;
            public bool LastUpdatedDate;
            public bool LastUpdatedBy;
            public bool IsDirty {
                get { return (EntityID || AddressTypeID || Address1 || Address2 || Address3 || City || StProvince || State || PostalCode || Country || County || Listed || IsPreferred || CreatedDate || CreatedBy || LastUpdatedDate || LastUpdatedBy); }
            }
        }

        protected bool IsDirty {
            get {
                return DirtyFlags.IsDirty;
            }
        }
        #endregion ObjectDataSource support

        protected bool GetDirtyFlag<T>(T field, T value) {
            // If you are setting a property, we will assume that the dirty flag should be set
            return true;
            bool flag = true;

            if (field == null && value == null) {
                flag = false;
            } else if (field == null) {
                flag = !value.Equals(field);
            } else {
                flag = !field.Equals(value);
            }
            return flag;
        }

        public static Address Fetch(int AddressID) {
            return QueryBuilder.QueryBuilder<Address>.Init().Fetch(new Address { AddressID = AddressID });
        }

        public void DoCreate() {
            QueryBuilder.QueryBuilder<Address>.Init().Create(this);
            //GetGroupDC().InsertGroup(this);
            //this.SaveChildObjects();
        }

        public void DoUpdate() {
            QueryBuilder.QueryBuilder<Address>.Init().Update(this);
            //GetGroupDC().UpdateGroup(this);
            //this.SaveChildObjects();
        }

        //protected bool GetDirtyFlag<T>(Nullable<T> field, Nullable<T> value) where T : struct {
        //    bool flag = true;
        //    if (field == null && value == null) {
        //        flag = false;
        //    } else if (field == null) {
        //        flag = !value.Equals(field);
        //    } else {
        //        flag = !field.Equals(value);
        //    }
        //    return flag;
        //}
    }
}
