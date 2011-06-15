using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryBuilder.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class IdentityColumnAttribute : Attribute {
        // Only one identity column per table is allowed
        public IdentityColumnAttribute() { }
    }
}