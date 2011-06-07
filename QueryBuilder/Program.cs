using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QueryBuilder.Fixture;

namespace QueryBuilder {
    class Program {
        static void Main(string[] args) {
            // Fetch
            Address address = QueryBuilder.QueryBuilder<Address>.Init().Fetch(new Address { City = "Irving", Country = 1 });
            // Create
            QueryBuilder.QueryBuilder<Address>.Init().Create(new Address() { Address1 = "123 Main", AddressTypeID = 1, City = "Irving", State = null, Country = 1 });
            // Update
            QueryBuilder.QueryBuilder<Address>.Init().Update(new Address() { Address1 = "123 Main", AddressTypeID = 1, City = "Irving", State = null, Country = 1 });
            // Delete

            // Builder pattern
            Address builderAddress = ((AddressBuilder)BaseFixture<Address>.Init().Builder()).Inject(InjectionType.Create);

            //Address address = QueryBuilder.QueryBuilder<Address>.Init().Where(a => { a.City = "Irnving"; a.PostalCode = "76021"; }).Fetch();
            var addr = new Address1();
            var b = addr.SetBeforeFetch(a => { a.City = "Irving"; a.Postal = "76021"; });
        }

        class Address1 {
            public Address1 SetBeforeFetch(Action<Address1> f) {
                f(this);
                return this;
            }

            public string City { get; set; }
            public string Postal { get; set; }
        }
    }
}
