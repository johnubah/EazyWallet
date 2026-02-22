using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class District : IPersist
    {
        public Int64 Id { get; set; }
        public String Code { get; set; }
        public String district_name { get; set; }
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
            WalletReport.Processor.DistrictProcessor.Create(this);
        }

        public void Update()
        {
            WalletReport.Processor.DistrictProcessor.Update(this);
        }

    }
}