using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Models
{
    public class UserType
    {
        public int UserTypeID { get; set; }
        public String Description { get; set; }


        public static List<UserType> GetUserType ()
        {
            List<UserType> usertypes = new List<UserType>();
            usertypes.Add(new UserType() { UserTypeID = -1, Description = "Choose one" });

            User oUser = HttpContext.Current.Session["CurrentUser"] as User;

            if (oUser.IsSuperUserOrAdmin)
            {
                usertypes.Add(new UserType() { UserTypeID = 1, Description = "Bank" });
                usertypes.Add(new UserType() { UserTypeID = 2, Description = "Dealer" });
                usertypes.Add(new UserType() { UserTypeID = 3, Description = "Agent" });
                usertypes.Add(new UserType() { UserTypeID = 4, Description = "Etop" });
            }
            else if(oUser.isBank)
            {
                usertypes.Add(new UserType() { UserTypeID = 2, Description = "Dealer" });
                usertypes.Add(new UserType() { UserTypeID = 3, Description = "Agent" });
            }
            else if (oUser.IsDealer)
            {
                usertypes.Add(new UserType() { UserTypeID = 3, Description = "Agent" });
            }
            return usertypes;
        }
    }
}