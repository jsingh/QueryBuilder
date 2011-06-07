using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QueryBuilder;

namespace QueryBuilder.Fixture {
    public class AddressBuilder : BaseFixture<Address>, IBuilder<Address> {

        public AddressBuilder() {
        }

        private AddressBuilder(DataContextType contextType = DataContextType.None) { }

        public static AddressBuilder Instance(DataContextType contextType = DataContextType.None) {
            return new AddressBuilder(contextType);
        }

        public Address Build() {
            return new Address();
        }
    }
}
