using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryBuilder.Fixture {
	interface ICollectionBuilder<T> {
		T Build(int count);
	}
}
