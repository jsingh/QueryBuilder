using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QueryBuilder.Fixture;
using AR = QueryBuilder.ActiveRecord;
using EF = QueryBuilder.EF;

namespace QueryBuilder {
    class Program {
        static void Main(string[] args) {
            // Active Record Methods using QueryBuilder
            // Fetch
            //Address Address = QueryBuilder.QueryBuilder<Address>.Init().Fetch(new Address { AddressID = 5 });
            // Create
            //QueryBuilder.QueryBuilder<Address>.Init().Create(new Address() { Address1 = "123 Main", AddressTypeID = 1, City = "Irving", State = null, Country = 1 });
            // Update
            //QueryBuilder.QueryBuilder<Address>.Init().Update(new Address() { AddressID = Address.AddressID, Address1 = "123 MainU", AddressTypeID = 1, City = "Irving", State = null, Country = 1 });

            // Fetch using Active Record metaphor.
            // Fetch
            AR.Address Address = AR.Address.Fetch(5);
            // Create
            AR.Address AddressToCreate = new AR.Address() { Address1 = "123 Main", AddressTypeID = 1, City = "Irving", State = null, Country = 1 };
            AddressToCreate.DoCreate();
            // Update
            // You dont need to get the entity first to update it. Just set the properties you want to update
            AR.Address AddressToUpdate = new AR.Address();
            // Set the primary key. This is required
            AddressToUpdate.AddressID = 5;
            // Now just set the fields you want to update
            AddressToUpdate.City = "IrvingU";
            AddressToUpdate.DoUpdate();
            // Delete. Not implemented yet

            // Builder pattern
            // Address builderAddress = ((AddressBuilder)BaseFixture<Address>.Init().Builder()).Inject(InjectionType.Create);

            // Build an Address but with some properties
            // Active Record
            AR.Address builderAddressWithOverride = BaseFixture<AR.Address>.Init().Inject(new AR.Address() { AddressTypeID = 1, Country = 1, City = "Irving" }, InjectionType.Create);
            // Entity Framework
            EF.Address address = EFFixture<EF.Address>.Init().Inject(new EF.Address() { AddressTypeID = 1, Country = 1, City = "Irving" }, InjectionType.Create);

            //Address Address = QueryBuilder.QueryBuilder<Address>.Init().Where(a => { a.City = "Irnving"; a.PostalCode = "76021"; }).Fetch();
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
