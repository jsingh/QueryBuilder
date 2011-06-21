using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace QueryBuilder.EF {
	[MetadataType(typeof(AddressMD))]
	public partial class Address {
        public List<string> ChangedProperties = new System.Collections.Generic.List<string>();
        public Address() {
            this.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Address_PropertyChanged);
        }

        void Address_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            ChangedProperties.Add(e.PropertyName);
        }

		public class AddressMD  {
			#region Primitive Properties
			[Required(ErrorMessage = "Address Type is required")]
			[Range(1, int.MaxValue, ErrorMessage = "Address Type is required")]
			public global::System.Int32 AddressTypeID {
				get;
				set;
			}

            [Required(ErrorMessage = "Address1 is required")]
			[StringLength(40, ErrorMessage = "Address1 must be under 40 characters.")]
			public global::System.String Address1 {
				get;
				set;
			}

			[StringLength(40, ErrorMessage = "Address2 must be under 40 characters.")]
			public global::System.String Address2 {
				get;
				set;
			}

			[StringLength(40, ErrorMessage = "Address3 must be under 40 characters.")]
			public global::System.String Address3 {
				get;
				set;
			}

            [Required(ErrorMessage = "City is required")]
			[StringLength(30, ErrorMessage = "City must be under 30 characters.")]
			public global::System.String City {
				get;
				set;
			}

			[StringLength(125, ErrorMessage = "StProvince must be under 125 characters.")]
			public global::System.String StProvince {
				get;
				set;
			}

			/// <summary>
			/// DB wise it is not required. but currently all the entities will
			/// be USA based, so we will be mandating this
			/// </summary>
			[Required(ErrorMessage = "State is required")]
			[Range(1, int.MaxValue, ErrorMessage = "State is required")]
			public Nullable<global::System.Int32> State {
				get;
				set;
			}

			[StringLength(10, ErrorMessage = "Postal Code must be under 10 characters.")]
			public global::System.String PostalCode {
				get;
				set;
			}

			[Required(ErrorMessage = "Country is required")]
			[Range(1, int.MaxValue, ErrorMessage = "Country is required")]
			public global::System.Int32 Country {
				get;
				set;
			}

			[StringLength(50, ErrorMessage = "County must be under 50 characters.")]
			public global::System.String County {
				get;
				set;
			}

            [Required(ErrorMessage = "Listed is required")]
            public global::System.Boolean Listed {
                get;
                set;
            }

            [Required(ErrorMessage = "IsPreferred is required")]
            public global::System.Boolean IsPreferred {
                get;
                set;
            }

            [Required(ErrorMessage = "CreatedDate is required")]
            public global::System.DateTime CreatedDate {
                get;
                set;
            }
			#endregion
		}
	}
}
