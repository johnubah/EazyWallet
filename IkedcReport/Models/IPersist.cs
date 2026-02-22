using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public interface IPersist
    {
        void Create();
        void Update();
    }
}