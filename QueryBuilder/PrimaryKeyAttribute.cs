﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryBuilder.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute {
        public PrimaryKeyAttribute() { }
    }
}
