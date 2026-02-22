using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletReport.Processor
{
    public class TerminalOwnerCache
    {
        public static List<Models.TerminalOwner> GetTerminalByEmail(String emailAddress)
        {
            List<Models.TerminalOwner> terminals = new List<Models.TerminalOwner>();
            try
            {
                LoadTerminalToCache();
                Dictionary<String, Models.TerminalOwner> o = System.Web.HttpContext.Current.Cache.Get("TerminalOwner") as Dictionary<String, Models.TerminalOwner>;

                if (o != null)
                {
                    foreach (KeyValuePair<String, Models.TerminalOwner> item in o)
                    {
                        if (item.Value.UserName.ToLower().Trim() == emailAddress.ToLower().Trim())
                        {
                            terminals.Add(item.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return terminals;

        }
        private static void LoadTerminalToCache()
        {
            //try
            //{
            //    Dictionary<String, Models.TerminalOwner> o = System.Web.HttpContext.Current.Cache.Get("TerminalOwner") as Dictionary<String, Models.TerminalOwner>;
                
            //    if (o == null)
            //    {
            //        o = TerminalProcessor.GetTerminalOwner();
            //        System.Web.HttpContext.Current.Cache.Add("TerminalOwner", o, null, DateTime.Now.AddMinutes(10), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Low, null);
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
        }
        public static Models.TerminalOwner GetTerminalOwner(String terminalId)
        {
            Models.TerminalOwner Owner = null;

            try
            {
                LoadTerminalToCache();

                Dictionary<String, Models.TerminalOwner> o = System.Web.HttpContext.Current.Cache.Get("TerminalOwner") as Dictionary<String, Models.TerminalOwner>;

                if (o != null)
                {
                    if (o.ContainsKey(terminalId.Trim()))
                    {
                        Owner = o[terminalId.Trim()];

                    }
                }
            }
            catch (Exception ex)
            {
            }
            return Owner;
        }
    }
}