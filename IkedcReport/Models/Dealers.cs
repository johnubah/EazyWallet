using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{

  
    public class Dealers : IPersist
    {
        public Int64 BankId { get; set; }
        public int MerchantId { get; set; }
        public Int64 Id { get; set; }
        public String DealerName { get; set; }
        public String AddressLine1 { get; set; }
        public String AddressLine2 { get; set; }
        public String City { get; set; }
        public String State { get; set; }
        public String Country { get; set; }
        public String ContactAddress { get; set; }
        public String ContactEmail { get; set; }
        public String ContactMobile { get; set; }
        public String ContactName { get; set; }


        public void Create()
        {
            Processor.DealersProcessor.Create(this);
        }

        public void Update()
        {
            Processor.DealersProcessor.Update(this);
        }

        public decimal Balance { get; set; }

        public string AccountNumber { get; set; }
        public string SettleAccountNumber { get;  set; }
        public string UBASettleAccountNumber { get; set; }
    }
}