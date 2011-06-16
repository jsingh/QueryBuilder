using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryBuilder.Fixture {
    public enum DataContextType {
        None = 0,
        //Valid = 1,
        //Invalid = 2,
        //Invalid_RequiredFields = 3,
        //Invalid_Range = 4,
        FetchFromDatabase = 5
    }

    public enum InjectionType {
        /// <summary>
        /// Gets the first record in the database
        /// </summary>
        First = 0,
        /// <summary>
        /// Gets the first record in the database if exists, create a new record in the database and return it if it doesnt
        /// </summary>
        CreateIfDoesntExist = 1,
        /// <summary>
        /// Creates an entry
        /// </summary>
        Create = 2,
        /// <summary>
        /// Fetch from database using a query object
        /// </summary>
        Fetch = 3
    }

    public enum Options {
        /// <summary>
        /// If set, created dummy data for string properties
        /// </summary>
        DummyStringData = 1
    }

    public struct Regex {
        public static string Email = "^([_a-zA-Z0-9-]+)(\\.[_a-zA-Z0-9-]+)*@([a-zA-Z0-9-]+\\.)+([a-zA-Z]{2,3})$";
        public static string Zip = "^(\\d{5}-\\d{4}|\\d{5}|\\d{9})$|^([a-zA-Z]\\d[a-zA-Z]\\d[a-zA-Z]\\d)$";
    }
}
