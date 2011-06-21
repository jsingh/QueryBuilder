using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;

namespace QueryBuilder.EF {
    public class ContextFactory {
        public static ObjectContext GetContext {
            get {
                return new QBEntities();
            }
        }
    }
}
