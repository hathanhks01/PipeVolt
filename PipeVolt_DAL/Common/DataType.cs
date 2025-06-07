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
            processing=1,
            shipping=2,
            Completed = 3,
            Cancelled = 4,
            refund = 5
        }
        public enum WarrantyStatus
        {
            Active = 0,        // Sản phẩm còn trong thời gian bảo hành
            Claimed = 1,       // Sản phẩm đã được yêu cầu bảo hành
            Expired = 2,       // Sản phẩm đã hết thời gian bảo hành
            InProcessing = 3,  // Sản phẩm đang trong quá trình xử lý bảo hành
            Completed = 4,     // Quy trình bảo hành đã hoàn tất
            Rejected = 5       // Yêu cầu bảo hành đã bị từ chối
        }
        public enum PaymentTransactionStatus
        {
            Pending = 0,
            Suscess = 1,
            Failed = 2
        }
        public enum SenderType
        {
            Admin=0,
            Employee=1,
            Customer=2,
        }
        public enum MessageType
        {
            Text=0,
            Image=1,
            File=2,
            System=3
        }
        public enum ChatRoomStatus
        {
            Active=0,
            Closed=1,
            Pending=2,
        }
        public enum InvoiceStatus
        {
            Draft=0,
            UnPaid = 1,
            Paid=2,
            Cancelled = 3
        }
        public enum PaymentStatus
        {
            Paid=0,
            UnPaid=1,
            Partial =2
        }

    }
}
