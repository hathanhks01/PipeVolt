using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.Common
{
    public class DataType
    {
        public enum UserType
        {
            Admin = 0,
            Employee = 1,
            Customer = 2
        }
        public enum UserStatus
        {
            Inactive = 0,
            Active = 1,
            Suspended = 2
        }
        public enum SaleStatus
        {
            Pending = 0,
            Completed = 1,
            Cancelled = 2,
            Paid=3
        }
        public enum PaymentTransactionStatus
        {
            Pending = 0,
            Suscess = 1,
            Failed = 2
        }
      
    }
}
