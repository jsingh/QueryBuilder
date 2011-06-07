using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryBuilder.Fixture {
    public interface IBuilder<T>  {
        T Build();
        T Inject(InjectionType injectionType = InjectionType.CreateIfDoesntExist);
        IBuilder<T> Builder();
    }
}
