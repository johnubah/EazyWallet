using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WalletReport.Models
{
    public class WalletLoadRequest
    {
        public Int64 Id { get; set; }
        public Int64 AgentId { get; set; }
        public Int64 DealerId { get; set; }
        public Decimal Amount { get; set; }
        public String ReferenceNumber { get; set; }
        public String Description { get; set; }
        public DateTime CreateDate { get; set; }
        public Int64 BankId { get; set; }
        public Int64 UserId { get; set; }
        private String _Status;
        public String Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
            }
        }
        public String ConfirmationCode { get; set; }

        public object AccountNumber { get; set; }
        public String IDReference { get; set; }
    }
}
