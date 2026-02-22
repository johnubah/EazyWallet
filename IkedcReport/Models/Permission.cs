using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class Permission : IPersist
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }




        public void Create()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}