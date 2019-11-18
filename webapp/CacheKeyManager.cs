using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.Web
{
    public static class CacheKeyManager
    {
        public static readonly string CustomersTable = "CustomersTable";
        public static readonly string LeadsTable = "CustomersTable";
        public static string GetCustomersTableInfo(string customersTableKey, string userId)
        {
            return $"Table_{customersTableKey}_{userId}";
        }

    }
}