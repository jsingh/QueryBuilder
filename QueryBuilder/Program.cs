using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QueryBuilder.Fixture;

namespace QueryBuilder {
    class Program {
        static void Main(string[] args) {
            // Fetch
            //Address address = QueryBuilder.QueryBuilder<Address>.Init().Fetch(new Address { AddressID = 5 });
            // Create
            //QueryBuilder.QueryBuilder<Address>.Init().Create(new Address() { Address1 = "123 Main", AddressTypeID = 1, City = "Irving", State = null, Country = 1 });
            // Update
            //QueryBuilder.QueryBuilder<Address>.Init().Update(new Address() { AddressID = address.AddressID, Address1 = "123 MainU", AddressTypeID = 1, City = "Irving", State = null, Country = 1 });

            // Fetch
            Address address = Address.Fetch(5);
            // Create
            Address addressToCreate = new Address() { Address1 = "123 Main", AddressTypeID = 1, City = "Irving", State = null, Country = 1 };
            addressToCreate.DoCreate();
            // Update
            Address addressToUpdate = new Address();
            // Set the primary key. This is required
            addressToUpdate.AddressID = 5;
            // Now just set the fields you want to update
            addressToUpdate.City = "IrvingU";
            addressToUpdate.DoUpdate();
            // Delete

            // Builder pattern
            //Address builderAddress = ((AddressBuilder)BaseFixture<Address>.Init().Builder()).Inject(InjectionType.Create);

            //Address builderAddress = BaseFixture<Address>.Init().Inject(InjectionType.Create);
            Address builderAddressWithOverride = BaseFixture<Address>.Init().Inject(new Address() { AddressTypeID = 1, Country = 1, City = "Irving" }, InjectionType.Create);

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
