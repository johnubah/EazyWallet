using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WalletReport.Models;
using System.Data;
using WalletReport.DBConnector;
using System.Text;
using Microsoft.Ajax.Utilities;

namespace WalletReport.Processor
{
    public class TransactionQueryProcessor
    {
        public static WalletTransaction GetWalletTransactionByID(String Id)
        {
            System.Text.StringBuilder builder = new StringBuilder();
            builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_setup_trans_history a left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber left join tbl_setup_agent as c on a.agent_id = c.Id");
            builder.Append(" where tranid=@Id");
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

                command.AddParamWithValue(DbType.AnsiString, "@Id", Id);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                List<WalletTransaction> transactions = GetWalletTransaction(rs);
                rs.Close();

                if (transactions != null && transactions.Count() > 0)
                {
                    return transactions[0];
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return null;

        }


        public static List<Transaction> GetTransaction(IDataReader rs)
        {

          //  List<District> Districts = DistrictProcessor.GetAllDistrict();

            List<Transaction> transactions = new List<Transaction>();


            while (rs.Read())
            {
                try
                {
                    
                    Transaction t = new Transaction();
                    t.AccountNumber = DataHelper.GetValue<String>(rs["accountnumber"]);
                    t.CustomerName = DataHelper.GetValue<String>(rs["customername"]);
                    t.MeterNumber = DataHelper.GetValue<String>(rs["meternumber"]);
                    t.MeterType = DataHelper.GetValue<String>(rs["metertype"]);
                    t.TransactionAmt = Convert.ToDecimal(rs["transact_amt"]);
                    t.TotalCharge = rs["TotalChages"] == DBNull.Value ? 0 : Convert.ToDecimal(rs["transact_amt"]);
                    t.TransactionDate = Convert.ToDateTime(rs["TransactDate"]);
                    t.ReceiptNumber = DataHelper.GetValue<String>(rs["receiptnumber"]);
                    t.Reference = DataHelper.GetValue<String>(rs["reference"]);
                    t.District = DataHelper.GetValue<String>(rs["district"]);
                    t.AVRdownload = DataHelper.GetValue<String>(rs["avrdownload"]);
                    t.TerminalID = DataHelper.GetValue<String>(rs["terminal_id"]);
                    t.Token = DataHelper.GetValue<String>(rs["prepaid_token"]);
                    if (rs["transactDate"] != DBNull.Value)
                    {
                        t.TransactionDate = Convert.ToDateTime(rs["transactDate"]);
                    }
                    else
                    {
                        t.TransactionDate = DateTime.Now;
                    }
                    t.BusinessUnit = rs["district"] == DBNull.Value ? "UNKNOWN" : rs["district"].ToString();

                   TerminalOwner owner =  TerminalOwnerCache.GetTerminalOwner(t.TerminalID);

                   if (owner != null)
                   {
                       StringBuilder terminalBuilder = new StringBuilder();

                       if (!String.IsNullOrEmpty(owner.Firstname))
                       {
                           terminalBuilder.Append(owner.Firstname);
                       }
                       if (!String.IsNullOrEmpty(owner.LastName))
                       {
                           terminalBuilder.Append(" ").Append(owner.LastName);
                       }

                       if (terminalBuilder.ToString().Trim().Length == 0)
                       {
                           terminalBuilder.Append(owner.UserName);
                       }

                       t.TerminalOwner = terminalBuilder.ToString();
                   }


                    //if (Districts != null && Districts.Count() > 0)
                    //{
                    //    try
                    //    {
                    //        if (t.District != null)
                    //        {
                    //            var query = (from c in Districts
                    //                         where c.Code == t.District.Trim()
                    //                         select c).FirstOrDefault();
                    //            if (query != null)
                    //            {
                    //                t.BusinessUnit = query.district_name;

                    //            }
                    //            else
                    //            {
                    //                t.BusinessUnit = String.Empty;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            t.BusinessUnit = String.Empty;
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //    }

                    //}

                    transactions.Add(t);
                }
                catch (Exception ex)
                {
                }
            }
            rs.Close();

            return transactions;
        }
        public static List<Transaction> GetAgentTransaction(User oUser, int count)
        {

            List<Transaction> transactions = null;
            StringBuilder Query = new StringBuilder();
            Query.Append("SELECT * from tbl_setup_tran_log  WHERE  (reversed is null or reversed ='N') AND ");
            Query.Append(" EXISTS(SELECT * from tbl_setup_terminal  where  tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND tbl_setup_terminal.user_id=@user_id) order by TransactDate DESC");

            if (count > 0)
            {
                Query.Append(" LIMIT 0,").Append(count);

            }

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = Query.ToString();
                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        public static List<Transaction> GetAgentTransaction(User oUser, DateTime fromDate, DateTime toDate,int count)
        {

            List<Transaction> transactions = null;
            StringBuilder Query = new StringBuilder();
            Query.Append("SELECT * from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND ");
            Query.Append(" EXISTS(SELECT * from tbl_setup_terminal  where  tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND tbl_setup_terminal.user_id=@user_id) order by TransactDate DESC");

            if (count > 0)
            {
                Query.Append(" LIMIT 0,").Append(count);

            }

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
                command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromDate);
                command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        public static List<Transaction> GetAgentTransaction(User oUser, DateTime fromDate, DateTime toDate)
        {

            List<Transaction> transactions = null;
            StringBuilder Query = new StringBuilder();
            Query.Append("SELECT * from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND ");
            Query.Append(" EXISTS(SELECT * from tbl_setup_terminal  where  tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND tbl_setup_terminal.user_id=@user_id)");


            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
                command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromDate);
                command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);
                
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        public static List<Transaction> GetLastNTransaction(User oUser, int count)
        {
            List<Transaction> transactions = new List<Transaction>();
            try
            {

                if (oUser.IsSuperUserOrAdmin || oUser.IsManager || (oUser.IsDistrictUser && oUser.ISALLDistrict()))
                {
                    transactions = GetTransactionAdmin(count);
                }
                else
                {

                    if (oUser.IsDealer && oUser.DealerID > 0)
                    {
                        if (oUser.IsAgent)
                        {
                            return GetAgentTransaction(oUser,count);
                        }
                        else
                        {
                            transactions = GetDealerTransaction(oUser,count);
                        }
                    }
                    else
                    {
                        transactions = GetDistrictTransaction(oUser, count);

                    }

                }

            }
            catch (Exception ex)
            {
            }

            return transactions;

        }
        public static List<Transaction> GetTransactionAdmin(int rowCount)
        {


            List<Transaction> transactions = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                String Query = String.Empty;

                if (rowCount <= 0)
                {
                    Query = "SELECT * from tbl_setup_tran_log WHERE  (reversed is null or reversed ='N') order by TransactDate DESC";
                }
                else
                {
                    Query = String.Format("SELECT * FROM tbl_setup_tran_log WHERE (reversed is null or reversed ='N') order by TransactDate DESC LIMIT 0,{0}", rowCount);
                }
                command.CommandText = Query;


                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        public static List<Transaction> GetTransaction(User oUser,DateTime fromDate, DateTime toDate,int count)
        {
            List<Transaction> transactions = new List<Transaction>();
            try
            {

                if (oUser.IsSuperUserOrAdmin || oUser.IsManager || (oUser.IsDistrictUser && oUser.ISALLDistrict()))
                {
                    transactions = GetTransactionAdmin(fromDate, toDate, count);
                }
                else
                {
                    
                        if (oUser.IsDealer  && oUser.DealerID > 0)
                        {
                            if (oUser.IsAgent)
                            {
                                return GetAgentTransaction(oUser, fromDate, toDate, count);
                            }
                            else
                            {
                                transactions = GetDealerTransaction(oUser, fromDate, toDate, count);
                            }
                        }
                        else
                        {
                            transactions = GetDistrictTransaction(oUser, fromDate, toDate, count);
                           
                        }
                    
                }

            }
            catch (Exception ex)
            {
            }

            return transactions;

            
        }
        private static List<Transaction> GetDistrictTransaction(User oUser, int count)
        {
            List<Transaction> transactions = null;
            StringBuilder Query = new StringBuilder();
            Query.Append("SELECT * from tbl_setup_tran_log  WHERE  (reversed is null or reversed ='N') AND");
            Query.Append(" EXISTS(SELECT * from tbl_setup_district,tbl_setup_user_district");
            Query.Append(" where tbl_setup_district.Id =tbl_setup_user_district.district_id and  tbl_setup_tran_log.district = tbl_setup_district.district_name and tbl_setup_user_district.user_id=@user_id ) order by TransactDate DESC");

            if (count > 0)
            {
                Query.Append(" LIMIT 0,").Append(count);

            }
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = Query.ToString();

                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        private static List<Transaction> GetDistrictTransaction(User oUser, DateTime fromDate, DateTime toDate,int count)
        {
            List<Transaction> transactions = null;
            StringBuilder Query = new StringBuilder();
            Query.Append("SELECT * from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND");
            Query.Append(" EXISTS(SELECT * from tbl_setup_district,tbl_setup_user_district");
            Query.Append(" where tbl_setup_district.Id =tbl_setup_user_district.district_id and  tbl_setup_tran_log.district = tbl_setup_district.district_name and tbl_setup_user_district.user_id=@user_id ) order by TransactDate DESC");

            if (count > 0)
            {
                Query.Append(" LIMIT 0,").Append(count);

            }
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = Query.ToString();

                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
                command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromDate);
                command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        private static List<Transaction> GetDistrictTransaction(User oUser, DateTime fromDate, DateTime toDate)
        {
            List<Transaction> transactions = null;
            StringBuilder Query = new StringBuilder();
            Query.Append("SELECT * from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND");
            Query.Append(" EXISTS(SELECT * from tbl_setup_district,tbl_setup_user_district");
            Query.Append(" where tbl_setup_district.Id =tbl_setup_user_district.district_id and  tbl_setup_tran_log.district = tbl_setup_district.district_name and tbl_setup_user_district.user_id=@user_id )");

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = Query.ToString();

                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
                command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromDate);
                command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }

        private static List<Transaction> GetDealerTransaction(User oUser, int count)
        {
            List<Transaction> transactions = null;
            StringBuilder Query = new StringBuilder();
            String terminalPrefix = HttpContext.Current.Session["TerminalPrefix"] as String;
            if(String.IsNullOrEmpty(terminalPrefix))
            {
                terminalPrefix = String.Empty;
            }

            terminalPrefix = terminalPrefix.Trim();
            int len = terminalPrefix.Length;

          
            IDbConnection conn = null;
            try
            {
                Query.Append("SELECT * from tbl_setup_tran_log  WHERE  (reversed is null or reversed ='N') AND");
                Query.Append(" substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");


                if (count > 0)
                {
                    Query.Append(" LIMIT 0,").Append(count);
                }

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = Query.ToString();

                //command.AddParamWithValue(DbType.Int64, "@dealer_id", oUser.DealerID);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        
        private static List<Transaction> GetDealerTransaction(User oUser, DateTime fromDate, DateTime toDate,int count)
        {
            List<Transaction> transactions = null;
            StringBuilder Query = new StringBuilder();

            String terminalPrefix = HttpContext.Current.Session["TerminalPrefix"] as String;
            if (String.IsNullOrEmpty(terminalPrefix))
            {
                terminalPrefix = String.Empty;
            }

            terminalPrefix = terminalPrefix.Trim();
            int len = terminalPrefix.Length;

           
            IDbConnection conn = null;
            try
            {
                Query.Append("SELECT * from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND");
                Query.Append(" substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");

                if (count > 0)
                {
                    Query.Append(" LIMIT 0,").Append(count);
                }

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = Query.ToString();

               // command.AddParamWithValue(DbType.Int64, "@dealer_id", oUser.DealerID);
                command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromDate);
                command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        private static List<Transaction> GetDealerTransaction(User oUser, DateTime fromDate, DateTime toDate)
        {


            String terminalPrefix = HttpContext.Current.Session["TerminalPrefix"] as String;
            if (String.IsNullOrEmpty(terminalPrefix))
            {
                terminalPrefix = String.Empty;
            }

            terminalPrefix = terminalPrefix.Trim();
            int len = terminalPrefix.Length;

            List<Transaction> transactions = null;
           

            IDbConnection conn = null;
            try
            {
                StringBuilder Query = new StringBuilder();
                Query.Append("SELECT * from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND");
                Query.Append(" substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = Query.ToString();

               // command.AddParamWithValue(DbType.Int64, "@dealer_id", oUser.DealerID);
                command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromDate);
                command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }

        public static List<GroupTransaction> GetGroupTransaction(IDbCommand command)
        {
            List<GroupTransaction> group = new List<GroupTransaction>();

           IDataReader rs =  command.ExecuteReader(CommandBehavior.CloseConnection);
           while (rs.Read())
           {
               try
               {
                   GroupTransaction g = new GroupTransaction();
                   try
                   {
                       try
                       {
                           g.MeterType = DataHelper.GetValue<String>(rs["metertype"]);
                       }
                       catch (Exception ex)
                       {
                       }
                       try
                       {
                           g.Amount = Convert.ToDecimal(rs["transact_amt"]);
                       }
                       catch (Exception ex)
                       {
                       }
                       try
                       {
                           g.TransCount = Convert.ToInt32(rs["transCount"]);
                       }
                       catch (Exception ex)
                       {
                       }
                   }
                   catch (Exception ex)
                   {
                   }
                   group.Add(g);
               }
               catch (Exception ex)
               {
               }
           }
           rs.Close();
           return group;
        }
        public static List<Models.MeterType> GetMeterType()
        {
            List<MeterType> meterTypes = new List<MeterType>();
            meterTypes.Add(new MeterType() { Code = "ALL", Description = "ALL" });
            meterTypes.Add(new MeterType() { Code = "STS", Description = "PREPAID STS" });
            meterTypes.Add(new MeterType() { Code = "POSTPAID", Description = "POSTPAID" });
            meterTypes.Add(new MeterType() { Code = "UNISTAR", Description = "UNISTAR" });
            return meterTypes;
        }
        public static List<GroupTransaction> GetDashboardTransaction(User oUser, DateTime fromdate, DateTime toDate)
        {
            String Query = String.Empty;


           
            List<GroupTransaction> groups = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();
                IDbCommand command = conn.CreateCommand();

                IDbCommand commandCount = conn.CreateCommand();

                if (oUser.IsSuperUserOrAdmin || oUser.IsManager ||  (oUser.IsDistrictUser && oUser.ISALLDistrict()))
                {
                    Query = "SELECT metertype,sum(transact_amt) as transact_amt, count(metertype) as transCount from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') group by metertype";
                    command.CommandText = Query;

                    command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromdate);
                    command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);

                }
                else if (oUser.IsAgent || (oUser.IsDealer && oUser.DealerID > 0))
                {
                    if (oUser.IsAgent)
                    {
                        StringBuilder AgentQuery = new StringBuilder();
                        AgentQuery.Append("SELECT metertype,sum(transact_amt) as transact_amt, count(metertype) as transCount from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND ");
                        AgentQuery.Append(" EXISTS(SELECT * from tbl_setup_terminal  where  tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND tbl_setup_terminal.user_id=@user_id) group by metertype");
                        Query = AgentQuery.ToString();
                        
                        command.CommandText = Query;

                        command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromdate);
                        command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);
                        command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);

                    }
                    else
                    {
                        String terminalPrefix = HttpContext.Current.Session["TerminalPrefix"] as String;
                        if (String.IsNullOrEmpty(terminalPrefix))
                        {
                            terminalPrefix = String.Empty;
                        }

                        terminalPrefix = terminalPrefix.Trim();
                        int len = terminalPrefix.Length;

                        StringBuilder DealerQuery = new StringBuilder();
                        DealerQuery.Append("SELECT metertype,sum(transact_amt) as transact_amt, count(metertype) as transCount from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND");
                        DealerQuery.Append(" substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("' group by metertype");
                        Query = DealerQuery.ToString();
                        command.CommandText = Query;

                        command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromdate);
                        command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);
                       // command.AddParamWithValue(DbType.Int64, "@dealer_id", oUser.DealerID);
                    }
                }
                else
                {
                    //IKEDC EXECUTIVE
                    StringBuilder DistrictQuery = new StringBuilder();
                    DistrictQuery.Append("SELECT metertype,sum(transact_amt) as transact_amt, count(metertype) as transCount from tbl_setup_tran_log  WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') AND");
                    DistrictQuery.Append(" EXISTS(SELECT * from tbl_setup_district,tbl_setup_user_district");
                    DistrictQuery.Append(" where tbl_setup_district.Id =tbl_setup_user_district.district_id and  tbl_setup_tran_log.district = tbl_setup_district.district_name and tbl_setup_user_district.user_id=@user_id ) group by metertype");
                    Query = DistrictQuery.ToString();

                    command.CommandText = Query;

                    command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromdate);
                    command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);
                    command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
                }
               
                groups = GetGroupTransaction(command);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return groups;
        }
        public static List<Transaction> GetTransactionAdmin(DateTime fromdate, DateTime toDate, int rowCount)
        {


            List<Transaction> transactions = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                String Query = String.Empty;

                if (rowCount <= 0)
                {
                    Query = "SELECT * from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N') order by TransactDate DESC";
                }
                else
                {
                    Query = String.Format("SELECT * FROM tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <= @ToDate AND (reversed is null or reversed ='N') order by TransactDate DESC LIMIT 0,{0}", rowCount);
                }
                command.CommandText = Query;

                command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromdate);
                command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                transactions = GetTransaction(rs);
          
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transactions;
        }
        //public static DashBoardModel GetDashboardTransaction(DateTime fromdate, DateTime toDate, int rowCount)
        //{
        //    DashBoardModel dashboard = new DashBoardModel();
        //    IDbConnection conn = null;
        //    try
        //    {
        //        conn = ConnectionManager.GetConnection();
        //        conn.OpenIfClosed();

        //        IDbCommand command = conn.CreateCommand();

        //        String Query = String.Empty;
        //        if (rowCount <= 0)
        //        {
        //            Query = "SELECT * from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate";
        //        }
        //        else
        //        {
        //            Query = String.Format("SELECT TOP {0}  * from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <= @ToDate", rowCount);
        //        }
        //        command.CommandText = Query;

        //        command.AddParamWithValue(DbType.DateTime, "@TransactDate", fromdate);
        //        command.AddParamWithValue(DbType.DateTime, "@ToDate", toDate);

        //        IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
        //        List<Transaction> transactions = GetTransaction(rs);
        //        dashboard.Transactions = transactions;
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    finally
        //    {
        //        ConnectionManager.Close(conn);
        //    }
        //    return dashboard;
        //}
        //internal static List<Transaction> GetTransactionPaging(Models.User oUser, DateTime startdate, DateTime endDate, long dealer_id, String district, Models.SearchModel Search)
        //{
        //    List<Transaction> transaction = null;
        //    IDbConnection conn = null;
        //    District curdistrict = null;
        //    Dealers dealer = null;
        //    try
        //    {
        //        conn = ConnectionManager.GetConnection();
        //        conn.OpenIfClosed();

        //        StringBuilder queryBuilder = new StringBuilder();

        //        StringBuilder queryBuilderCount = new StringBuilder();

        //        queryBuilder.Append("SELECT * from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N')");
                
        //        queryBuilderCount.Append("SELECT Count(*) as icount,Sum(transact_amt) as trans_amt from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N')");


              
        //        IDbCommand command = conn.CreateCommand();
        //        IDbCommand commandCount = conn.CreateCommand();
        //        if (Search.MeterType != "ALL")
        //        {
        //            queryBuilder.Append(" AND metertype=@metertype");
        //            queryBuilderCount.Append(" AND metertype=@metertype");
        //        }
        //        if (!String.IsNullOrEmpty(district) && district.Trim() != "-00")
        //        {
        //            //we need to get the district

        //            curdistrict = DistrictProcessor.GetDistrictByID(Convert.ToInt64(district));

        //            if (curdistrict != null)
        //            {
        //                queryBuilder.Append(" AND district=@district");
        //                queryBuilderCount.Append(" AND district=@district");
        //            }
        //        }
        //        else
        //        {
        //            if (!(oUser.IsSuperUserOrAdmin || oUser.IsAgent || oUser.IsDealer || oUser.IsManager || oUser.ISALLDistrict()))
        //            {
        //                queryBuilder.Append(" AND exists(select * from tbl_setup_district,tbl_setup_user_district where tbl_setup_district.Id = tbl_setup_user_district.district_id AND tbl_setup_district.district_name = tbl_setup_tran_log.district AND tbl_setup_user_district.user_id=@user_id)");
        //                queryBuilderCount.Append(" AND exists(select * from tbl_setup_district,tbl_setup_user_district where tbl_setup_district.Id = tbl_setup_user_district.district_id AND tbl_setup_district.district_name = tbl_setup_tran_log.district AND tbl_setup_user_district.user_id=@user_id)");
        //            }
        //        }
        //        String terminalPrefix = String.Empty;
        //        if (dealer_id > 0)
        //        {
        //           dealer = DealersProcessor.GetDealersByID(dealer_id);
        //           if (dealer != null)
        //           {
        //               terminalPrefix = dealer.TerminalPrefix;
        //               if (String.IsNullOrEmpty(terminalPrefix))
        //               {
        //                   terminalPrefix = String.Empty;
        //               }


        //               terminalPrefix = terminalPrefix.Trim();
        //               int len = terminalPrefix.Length;
        //               queryBuilder.Append(" AND substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");
        //               queryBuilderCount.Append(" AND substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");
        //           }
        //           if (oUser.IsDealer)
        //           {
        //               if (oUser.IsAgent)
        //               {
        //                   queryBuilder.Append(" AND exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.user_id = @user_id)");
        //                   queryBuilderCount.Append(" AND exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.user_id = @user_id)");
        //               }
        //           }

        //        }
        //        else
        //        {
        //            if (oUser.IsDealer)
        //            {
        //                if (oUser.IsAgent)
        //                {
        //                    queryBuilder.Append(" AND exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.user_id = @user_id)");
        //                    queryBuilderCount.Append(" AND exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.user_id = @user_id)");
        //                }
        //                else
        //                {

        //                    terminalPrefix = HttpContext.Current.Session["TerminalPrefix"] as String;
        //                    if (String.IsNullOrEmpty(terminalPrefix))
        //                    {
        //                        terminalPrefix = String.Empty;
        //                    }


        //                    terminalPrefix = terminalPrefix.Trim();
        //                    int len = terminalPrefix.Length;
        //                    queryBuilder.Append(" AND substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");
        //                    queryBuilderCount.Append(" AND substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");
        //                }

        //            }
                   
        //        }

        //        //AdditionalCriteria.Add("TerminalID");
        //        //AdditionalCriteria.Add("AccountNumber");
        //        //AdditionalCriteria.Add("MeterNumber");
        //        //AdditionalCriteria.Add("ReceiptNumber");
        //        //AdditionalCriteria.Add("Reference");

        //        switch (Search.CustomSearch)
        //        {
        //            case "ALL":
        //                break;
        //            case "TerminalID":
        //                queryBuilder.Append(" AND terminal_id = @SearchValue");
        //                queryBuilderCount.Append(" AND terminal_id = @SearchValue");
        //                break;
        //            case "AccountNumber":
        //                queryBuilder.Append(" AND accountnumber = @SearchValue");
        //                queryBuilderCount.Append(" AND accountnumber = @SearchValue");
        //                break;
        //            case "MeterNumber":
        //                queryBuilder.Append(" AND meternumber = @SearchValue");
        //                queryBuilderCount.Append(" AND meternumber = @SearchValue");
        //                break;
        //            case "ReceiptNumber":
        //                queryBuilder.Append(" AND receiptnumber = @SearchValue");
        //                queryBuilderCount.Append(" AND receiptnumber = @SearchValue");
        //                break;
        //            case "Reference":
        //                queryBuilder.Append(" AND reference = @SearchValue");
        //                queryBuilderCount.Append(" AND reference = @SearchValue");
        //                break;
        //            case "Dealer Email":

                        
        //                List<Models.TerminalOwner> owners = TerminalOwnerCache.GetTerminalByEmail(Search.CustomSearchValue);
        //                StringBuilder builderTerminal = new StringBuilder();
        //                if (owners != null)
        //                {
        //                    int z = -1;
        //                    foreach (Models.TerminalOwner owner in owners)
        //                    {
        //                        if (++z == 0)
        //                        {
        //                            builderTerminal.Append(String.Format("'{0}'",owner.TerminalId));
        //                        }
        //                        else
        //                        {
        //                            builderTerminal.Append(String.Format(",'{0}'", owner.TerminalId));
        //                        }
        //                    }
        //                }
        //                if (builderTerminal.ToString().Length > 0)
        //                {
        //                    queryBuilder.Append(String.Format(" AND terminal_id in({0})", builderTerminal.ToString()));
        //                    queryBuilderCount.Append(String.Format(" AND terminal_id in({0})", builderTerminal.ToString()));
        //                }
        //                else
        //                {
        //                    queryBuilder.Append(String.Format(" AND terminal_id in({0})", "'NONE'"));
        //                    queryBuilderCount.Append(String.Format(" AND terminal_id in({0})", "'NONE'"));
        //                }
        //                break;
        //        }
             

        //        queryBuilder.Append(" order by TransactDate DESC");

        //        if (Search.PageSize > 0)
        //        {
        //            queryBuilder.Append(String.Format(" limit {0},{1}", Search.Skip, Search.PageSize));
        //        }
        //        command.CommandText = queryBuilder.ToString();
        //        commandCount.CommandText = queryBuilderCount.ToString();

        //        command.AddParamWithValue(DbType.DateTime, "@TransactDate", startdate);
        //        command.AddParamWithValue(DbType.DateTime, "@ToDate", endDate);

        //        commandCount.AddParamWithValue(DbType.DateTime, "@TransactDate", startdate);
        //        commandCount.AddParamWithValue(DbType.DateTime, "@ToDate", endDate);

        //        if (Search.MeterType != "ALL")
        //        {
        //            command.AddParamWithValue(DbType.AnsiString, "@metertype", Search.MeterType);
        //            commandCount.AddParamWithValue(DbType.AnsiString, "@metertype", Search.MeterType);
        //        }
        //        if (!String.IsNullOrEmpty(district) && district.Trim() != "-00")
        //        {
        //            if (curdistrict != null)
        //            {
        //                command.AddParamWithValue(DbType.AnsiString, "@district", curdistrict.district_name);
        //                commandCount.AddParamWithValue(DbType.AnsiString, "@district", curdistrict.district_name);
        //            }
        //        }
        //        else
        //        {
        //            if (!(oUser.IsSuperUserOrAdmin || oUser.IsAgent || oUser.IsDealer || oUser.IsManager || oUser.ISALLDistrict()))
        //            {
        //                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
        //                commandCount.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
        //            }
        //        }

               
        //        if (oUser.IsDealer)
        //        {

        //            if (oUser.IsAgent)
        //            {
        //                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
        //                commandCount.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
        //            }
        //        }
        //        switch (Search.CustomSearch)
        //        {
        //            case "ALL":
        //                break;
        //            case "TerminalID": case "AccountNumber": case "MeterNumber": case "ReceiptNumber": case "Reference":
        //                command.AddParamWithValue(DbType.AnsiString, "@SearchValue", Search.CustomSearchValue);
        //                commandCount.AddParamWithValue(DbType.AnsiString, "@SearchValue", Search.CustomSearchValue);
        //                break;
        //        }
        //        IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
        //        transaction = GetTransaction(rs);

        //        if (conn.State != ConnectionState.Open)
        //            conn.Open();

        //        rs = commandCount.ExecuteReader(CommandBehavior.CloseConnection);

        //        while (rs.Read())
        //        {
        //            Search.ItemCount = rs["icount"] == DBNull.Value ? 0 :  Convert.ToInt32(rs["icount"]);
        //            Search.TotalAmount = rs["trans_amt"] == DBNull.Value ? 0 : Convert.ToDecimal(rs["trans_amt"]);

        //        }
        //        rs.Close();


        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        ConnectionManager.Close(conn);
        //    }
        //    return transaction;
        //}
        //internal static List<Transaction> GetTransaction(Models.User oUser, DateTime startdate, DateTime endDate, long dealer_id, String district, Models.SearchModel Search)
        //{
        //    List<Transaction> transaction = null;
        //    IDbConnection conn = null;
        //    District curdistrict = null;
        //    Dealers dealer = null;
        //    try
        //    {
        //        conn = ConnectionManager.GetConnection();
        //        conn.OpenIfClosed();

        //        StringBuilder queryBuilder = new StringBuilder();


        //        queryBuilder.Append("SELECT * from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N')");




        //        IDbCommand command = conn.CreateCommand();
        //        if (Search.MeterType != "ALL")
        //        {
        //            queryBuilder.Append(" AND metertype=@metertype");
        //        }
        //        if (!String.IsNullOrEmpty(district) && district.Trim() != "-00")
        //        {
        //            //we need to get the district

        //            curdistrict = DistrictProcessor.GetDistrictByID(Convert.ToInt64(district));

        //            if (curdistrict != null)
        //            {
        //                queryBuilder.Append(" AND district=@district");
        //            }
        //        }
        //        else
        //        {
        //            if (!(oUser.IsSuperUserOrAdmin || oUser.IsAgent || oUser.IsDealer || oUser.IsManager || oUser.ISALLDistrict()))
        //            {
        //                queryBuilder.Append(" AND exists(select * from tbl_setup_district,tbl_setup_user_district where tbl_setup_district.Id = tbl_setup_user_district.district_id AND tbl_setup_district.district_name = tbl_setup_tran_log.district AND tbl_setup_user_district.user_id=@user_id)");
        //            }
        //        }
        //        String terminalPrefix = String.Empty;
        //        if (dealer_id > 0)
        //        {
        //            dealer = DealersProcessor.GetDealersByID(dealer_id);
        //            if (dealer != null)
        //            {
        //                //terminalPrefix = dealer.TerminalPrefix;
        //                if (String.IsNullOrEmpty(terminalPrefix))
        //                {
        //                    terminalPrefix = String.Empty;
        //                }


        //                terminalPrefix = terminalPrefix.Trim();
        //                int len = terminalPrefix.Length;
        //                queryBuilder.Append(" AND substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");
        //            }
        //            if (oUser.IsDealer)
        //            {
        //                if (oUser.IsAgent)
        //                {
        //                    queryBuilder.Append(" AND exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.user_id = @user_id)");
        //                }
        //            }

        //        }
        //        else
        //        {
        //            if (oUser.IsDealer)
        //            {
        //                if (oUser.IsAgent)
        //                {
        //                    queryBuilder.Append(" AND exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.user_id = @user_id)");
        //                }
        //                else
        //                {

        //                    terminalPrefix = HttpContext.Current.Session["TerminalPrefix"] as String;
        //                    if (String.IsNullOrEmpty(terminalPrefix))
        //                    {
        //                        terminalPrefix = String.Empty;
        //                    }


        //                    terminalPrefix = terminalPrefix.Trim();
        //                    int len = terminalPrefix.Length;
        //                    queryBuilder.Append(" AND substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");
        //                }

        //            }

        //        }


        //        switch (Search.CustomSearch)
        //        {
        //            case "ALL":
        //                break;
        //            case "TerminalID":
        //                queryBuilder.Append(" AND terminal_id = @SearchValue");
        //                break;
        //            case "AccountNumber":
        //                queryBuilder.Append(" AND accountnumber = @SearchValue");
        //                break;
        //            case "MeterNumber":
        //                queryBuilder.Append(" AND meternumber = @SearchValue");
        //                break;
        //            case "ReceiptNumber":
        //                queryBuilder.Append(" AND receiptnumber = @SearchValue");
        //                break;
        //            case "Reference":
        //                queryBuilder.Append(" AND reference = @SearchValue");
        //                break;
        //            case "Dealer Email":

        //                List<Models.TerminalOwner> owners = TerminalOwnerCache.GetTerminalByEmail(Search.CustomSearchValue);
        //                StringBuilder builderTerminal = new StringBuilder();
        //                if (owners != null)
        //                {
        //                    int z = -1;
        //                    foreach (Models.TerminalOwner owner in owners)
        //                    {
        //                        if (++z == 0)
        //                        {
        //                            builderTerminal.Append(String.Format("'{0}'", owner.TerminalId));
        //                        }
        //                        else
        //                        {
        //                            builderTerminal.Append(String.Format(",'{0}'", owner.TerminalId));
        //                        }
        //                    }
        //                }
        //                if (builderTerminal.ToString().Length > 0)
        //                {
        //                    queryBuilder.Append(String.Format(" AND terminal_id in({0})", builderTerminal.ToString()));
        //                }
        //                else
        //                {
        //                    queryBuilder.Append(String.Format(" AND terminal_id in({0})", "'NONE'"));
        //                }
        //                break;

        //        }


        //        queryBuilder.Append(" order by TransactDate DESC");
        //        // queryBuilderCount.Append(" order by TransactDate DESC");

               
        //        command.CommandText = queryBuilder.ToString();

        //        command.AddParamWithValue(DbType.DateTime, "@TransactDate", startdate);
        //        command.AddParamWithValue(DbType.DateTime, "@ToDate", endDate);

        //        if (Search.MeterType != "ALL")
        //        {
        //            command.AddParamWithValue(DbType.AnsiString, "@metertype", Search.MeterType);
        //        }
        //        if (!String.IsNullOrEmpty(district) && district.Trim() != "-00")
        //        {
        //            if (curdistrict != null)
        //            {
        //                command.AddParamWithValue(DbType.AnsiString, "@district", curdistrict.district_name);
        //            }
        //        }
        //        else
        //        {
        //            if (!(oUser.IsSuperUserOrAdmin || oUser.IsAgent || oUser.IsDealer || oUser.IsManager || oUser.ISALLDistrict()))
        //            {
        //                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
        //            }
        //        }


        //        if (oUser.IsDealer)
        //        {

        //            if (oUser.IsAgent)
        //            {
        //                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
        //            }
        //        }
        //        switch (Search.CustomSearch)
        //        {
        //            case "ALL":
        //                break;
        //            case "TerminalID":
        //            case "AccountNumber":
        //            case "MeterNumber":
        //            case "ReceiptNumber":
        //            case "Reference":
        //                command.AddParamWithValue(DbType.AnsiString, "@SearchValue", Search.CustomSearchValue);
        //                break;
        //        }
        //        IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
        //        transaction = GetTransaction(rs);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        ConnectionManager.Close(conn);
        //    }
        //    return transaction;
        //}
        //internal static List<Transaction> GetTransaction(Models.User oUser,DateTime startdate, DateTime endDate, long dealer_id, String district,Models.SearchModel Search)
        //{
        //    List<Transaction> transaction = null;
        //    IDbConnection conn = null;
        //    try
        //    {
        //        conn = ConnectionManager.GetConnection();
        //        conn.OpenIfClosed();

        //        StringBuilder queryBuilder = new StringBuilder();

        //        queryBuilder.Append("SELECT * from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N')");


        //        IDbCommand command = conn.CreateCommand();
        //        if (Search.MeterType != "ALL")
        //        {
        //            queryBuilder.Append(" AND metertype=@metertype");
        //        }
        //        if (!String.IsNullOrEmpty(district) && district != "-00")
        //        {
        //            queryBuilder.Append(" AND district=@district");
        //        }
        //        else
        //        {
        //            if (!(oUser.IsSuperUserOrAdmin || oUser.IsAgent || oUser.IsDealer || oUser.IsManager || oUser.ISALLDistrict()))
        //            {
        //                queryBuilder.Append(" AND exists(select * from tbl_setup_district,tbl_setup_user_district where tbl_setup_district.Id = tbl_setup_user_district.district_id AND tbl_setup_district.district_name = tbl_setup_tran_log.district AND tbl_setup_user_district.user_id=@user_id)");
        //            }
        //        }

        //        bool TerminalPrefixFound = false;
        //        if (dealer_id > 0)
        //        {
        //           Models.Dealers dealer =  Processor.DealersProcessor.GetDealersByID(dealer_id);
        //           if (dealer == null)
        //           {
        //               queryBuilder.Append(" AND exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.dealer_id = @dealer_id)");
        //           }
        //           else
        //           {
        //               TerminalPrefixFound = true;
        //               String terminalPrefix = dealer.TerminalPrefix;
        //               if (String.IsNullOrEmpty(terminalPrefix))
        //               {
        //                   terminalPrefix = String.Empty;
        //               }

        //               terminalPrefix = terminalPrefix.Trim();
        //               int len = terminalPrefix.Length;
        //               queryBuilder.Append(" AND substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");
        //           }
        //        }

        //        if (oUser.IsAgent)
        //        {
        //            queryBuilder.Append(" AND exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.user_id = @user_id)");
        //        }
              
        //        queryBuilder.Append(" order by TransactDate DESC");
        //        command.CommandText = queryBuilder.ToString();

        //        command.AddParamWithValue(DbType.DateTime, "@TransactDate", startdate);
        //        command.AddParamWithValue(DbType.DateTime, "@ToDate", endDate);
        //        if (Search.MeterType != "ALL")
        //        {
        //            command.AddParamWithValue(DbType.AnsiString, "@metertype", Search.MeterType);
        //        }
        //        if (!String.IsNullOrEmpty(district) && district != "-00")
        //        {
        //            command.AddParamWithValue(DbType.AnsiString, "@district", district);
        //        }
        //        else
        //        {
        //            if (!(oUser.IsSuperUserOrAdmin || oUser.IsAgent || oUser.IsDealer || oUser.IsManager || oUser.ISALLDistrict()))
        //            {
        //                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
        //            }
        //        }

        //        if (dealer_id > 0 && !TerminalPrefixFound)
        //        {
                    
        //            command.AddParamWithValue(DbType.Int64, "@dealer_id", dealer_id);
        //        }
        //        if (oUser.IsAgent)
        //        {
        //            command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
        //        }

        //        IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
        //        transaction = GetTransaction(rs);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        ConnectionManager.Close(conn);
        //    }
        //    return transaction;
        //}
        internal static List<Transaction> GetTransaction(DateTime startdate, DateTime endDate, long dealer_id, String district)
        {
            List<Transaction> transaction = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                StringBuilder queryBuilder = new StringBuilder();

                queryBuilder.Append("SELECT * from tbl_setup_tran_log WHERE TransactDate >=@TransactDate AND TransactDate <=@ToDate AND (reversed is null or reversed ='N')");

                IDbCommand command =  conn.CreateCommand();
                if (!String.IsNullOrEmpty(district) &&  district != "-00")
                {
                    queryBuilder.Append(" AND district=@district");
                }

                bool TerminalPrefixFound = false;
                if (dealer_id > 0)
                {
                    Models.Dealers dealers =  Processor.DealersProcessor.GetDealersByID(dealer_id);
                    if (dealers == null)
                    {
                        queryBuilder.Append(" AND WHERE exists(select * from tbl_setup_terminal where tbl_setup_terminal.terminal_id = tbl_setup_tran_log.terminal_id AND  tbl_setup_terminal.dealer_id = @dealer_id)");
                    }
                    else
                    {
                        TerminalPrefixFound = true;
                        String terminalPrefix = null;// dealers.TerminalPrefix;

                        if (String.IsNullOrEmpty(terminalPrefix))
                        {
                            terminalPrefix = String.Empty;
                        }

                        terminalPrefix = terminalPrefix.Trim();
                        int len = terminalPrefix.Length;
                        queryBuilder.Append(" AND substring(terminal_id,1,").Append(len).Append(")").Append(" ='").Append(terminalPrefix).Append("'");

                    }
                }
                command.CommandText = queryBuilder.ToString();

                command.AddParamWithValue(DbType.DateTime, "@TransactDate", startdate);
                command.AddParamWithValue(DbType.DateTime, "@ToDate", endDate);
                if (!String.IsNullOrEmpty(district) &&  district != "-00")
                {
                    command.AddParamWithValue(DbType.AnsiString, "@district", district);
                }

                if (dealer_id > 0 && !TerminalPrefixFound)
                {
                    command.AddParamWithValue(DbType.Int64, "@dealer_id", dealer_id);
                }

               IDataReader rs =  command.ExecuteReader(CommandBehavior.CloseConnection);
               transaction = GetTransaction(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return transaction;
        }

        public static List<WalletTransaction> GetWalletTransactionDownload(User oUser, DateTime StartDate, DateTime EndDate, long BankID, long DealerID, long AgentID, SearchModel search)
        {
            List<WalletTransaction> transactions = null;
            StringBuilder builder = new StringBuilder();


            builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_setup_trans_history a left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber left join tbl_setup_agent as c on a.agent_id = c.Id");
            builder.Append(" where date_created >=@startdate AND date_created <=@enddate");

            if (BankID > 0)
            {
                builder.Append(" and a.bank_id =@bank_id");
            }
            if (DealerID > 0)
            {
                Dealers dealer = DealersProcessor.GetDealersByID(DealerID);
                List<String> dealerAccountInfo = new List<String>();
                if (!String.IsNullOrWhiteSpace(dealer.AccountNumber))
                {
                    dealerAccountInfo.Add(dealer.AccountNumber);
                }
                if (!String.IsNullOrWhiteSpace(dealer.SettleAccountNumber))
                {
                    dealerAccountInfo.Add(dealer.SettleAccountNumber);
                }
                if (dealerAccountInfo.Count > 0)
                {
                    String accountNumber = String.Join(",", dealerAccountInfo.Select(c => string.Format("'{0}'", c)).ToArray());

                    builder.Append(String.Format(" and (b.Id =@dealer_id or a.acctnumber in(0) or EXISTS(select 1 from tbl_setup_agent ag where a.agent_id = ag.Id  and ag.dealer_id=@dealer_id))", accountNumber));
                }
                else
                {
                    builder.Append(" and (b.Id =@dealer_id or EXISTS(select 1 from tbl_setup_agent ag where a.agent_id = ag.Id  and ag.dealer_id=@dealer_id))");
                }

            }
            if (AgentID > 0)
            {
                builder.Append(" and a.agent_id =@agent_id");
            }

            switch (search.CustomSearch)
            {
                case "Terminal ID": ;
                    builder.Append(" and a.terminal_id=@terminal_id");
                    break;
                case "Customer Ref":
                    builder.Append(" and a.customerref=@customer_ref");
                    break;
                case "Reference":
                    builder.Append(" and a.trans_ref=@trans_ref");
                    break;

            }

          

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();





                command.CommandText = builder.ToString();


                command.AddParamWithValue(DbType.DateTime, "@startdate", StartDate);
                command.AddParamWithValue(DbType.DateTime, "@enddate", EndDate);

                if (BankID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@bank_id", BankID);
                }
                if (DealerID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@dealer_id", DealerID);
                }
                if (AgentID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@agent_id", AgentID);
                }



                switch (search.CustomSearch)
                {
                    case "Terminal ID":
                        command.AddParamWithValue(DbType.AnsiString, "@terminal_id", search.CustomSearchValue);
                        break;
                    case "Customer Ref":
                        command.AddParamWithValue(DbType.AnsiString, "@customerref", search.CustomSearchValue);
                        break;
                    case "Reference":
                        command.AddParamWithValue(DbType.AnsiString, "@trans_ref", search.CustomSearchValue);
                        break;

                }



                IDataReader rs = command.ExecuteReader();
                transactions = GetWalletTransaction(rs);

              
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }

            return transactions;
        }

        public static List<WalletTransaction> GetWalletTransactionDownloadArch(User oUser, DateTime StartDate, DateTime EndDate, long BankID, long DealerID, long AgentID, SearchModel search)
        {
            List<WalletTransaction> transactions = null;
            StringBuilder builder = new StringBuilder();


            builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_setup_trans_history_arch a left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber left join tbl_setup_agent as c on a.agent_id = c.Id");
            builder.Append(" where date_created >=@startdate AND date_created <=@enddate");

            if (BankID > 0)
            {
                builder.Append(" and a.bank_id =@bank_id");
            }
            if (AgentID > 0)
            {
                builder.Append(" and a.agent_id =@agent_id");
            }
            if (DealerID > 0)
            {
                Dealers dealer = DealersProcessor.GetDealersByID(DealerID);
                List<String> dealerAccountInfo = new List<String>();
                if (!String.IsNullOrWhiteSpace(dealer.AccountNumber))
                {
                    dealerAccountInfo.Add(dealer.AccountNumber);
                }
                if (!String.IsNullOrWhiteSpace(dealer.SettleAccountNumber))
                {
                    dealerAccountInfo.Add(dealer.SettleAccountNumber);
                }
                if (dealerAccountInfo.Count > 0)
                {
                    String accountNumber = String.Join(",", dealerAccountInfo.Select(c => string.Format("'{0}'", c)).ToArray());

                    builder.Append(String.Format(" and (b.Id =@dealer_id or a.acctnumber in(0) )", accountNumber));
                }
                else
                {
                    builder.Append(" and (b.Id =@dealer_id)");
                }

            }
            

            switch (search.CustomSearch)
            {
                case "Terminal ID":
                    ;
                    builder.Append(" and a.terminal_id=@terminal_id");
                    break;
                case "Customer Ref":
                    builder.Append(" and a.customerref=@customer_ref");
                    break;
                case "Reference":
                    builder.Append(" and a.trans_ref=@trans_ref");
                    break;

            }



            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();





                command.CommandText = builder.ToString();
                command.CommandTimeout = 700;

                command.AddParamWithValue(DbType.DateTime, "@startdate", StartDate);
                command.AddParamWithValue(DbType.DateTime, "@enddate", EndDate);

                if (BankID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@bank_id", BankID);
                }
                if (DealerID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@dealer_id", DealerID);
                }
                if (AgentID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@agent_id", AgentID);
                }



                switch (search.CustomSearch)
                {
                    case "Terminal ID":
                        command.AddParamWithValue(DbType.AnsiString, "@terminal_id", search.CustomSearchValue);
                        break;
                    case "Customer Ref":
                        command.AddParamWithValue(DbType.AnsiString, "@customerref", search.CustomSearchValue);
                        break;
                    case "Reference":
                        command.AddParamWithValue(DbType.AnsiString, "@trans_ref", search.CustomSearchValue);
                        break;

                }



                IDataReader rs = command.ExecuteReader();
                transactions = GetWalletTransaction(rs);


            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }

            return transactions;
        }


        public static List<WalletTransaction> GetWalletTransactionPaging(User oUser, DateTime StartDate, DateTime EndDate,long BankID ,long DealerID, long AgentID, SearchModel search)
        {
            List<WalletTransaction> transactions = null;
            StringBuilder builder = new StringBuilder();

            StringBuilder builderCount = new StringBuilder();

            builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_setup_trans_history a left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber left join tbl_setup_agent as c on a.agent_id = c.Id");
            builder.Append(" where date_created >=@startdate AND date_created <=@enddate");

            if (BankID > 0)
            {
                builder.Append(" and a.bank_id =@bank_id");
            }
            //if (DealerID > 0)
            //{
            //    //I want to get the whole
            //    builder.Append(" and b.Id =@dealer_id");
            //}
            if (AgentID > 0 && DealerID > 0)
            {
                builder.Append(" and a.agent_id =@agent_id");
            }
            else
            {
                if (DealerID > 0)
                {
                    Dealers dealer = DealersProcessor.GetDealersByID(DealerID);
                    List<String> dealerAccountInfo = new List<String>();
                    if (!String.IsNullOrWhiteSpace(dealer.AccountNumber))
                    {
                        dealerAccountInfo.Add(dealer.AccountNumber);
                    }
                    if (!String.IsNullOrWhiteSpace(dealer.SettleAccountNumber))
                    {
                        dealerAccountInfo.Add(dealer.SettleAccountNumber);
                    }
                    if (dealerAccountInfo.Count > 0)
                    {
                        String accountNumber = String.Join(",", dealerAccountInfo.Select(c => string.Format("'{0}'", c)).ToArray());

                        builder.Append(String.Format(" and (b.Id =@dealer_id or a.acctnumber in(0))", accountNumber));
                    }
                    else
                    {
                        //builder.Append(" and (b.Id =@dealer_id or EXISTS(select 1 from tbl_setup_agent ag where a.agent_id = ag.Id  and ag.dealer_id=@dealer_id))");
                        builder.Append(" and (b.Id =@dealer_id )");
                    }

                }

                if (AgentID > 0)
                {
                    builder.Append(" and a.agent_id =@agent_id");
                }
            }
            
            if(!search.TransactionType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append(" and a.trans_type =@trans_type");
            }

            switch (search.CustomSearch)
            {
                case "Terminal ID": ;
                    builder.Append(" and a.terminal_id=@terminal_id");
                    break;
                case "Customer Ref":
                    builder.Append(" and a.customerref=@customer_ref");
                    break;
                case "Reference":
                    builder.Append(" and a.trans_ref=@trans_ref");
                    break;

            }

            String QueryCount = builder.ToString().Replace("a.*,b.dealer_name,c.agent_name", " SUM(CASE WHEN a.trans_type='CR' THEN a.trans_amt ELSE 0 END) AS 'TCR',SUM(Case WHEN a.trans_type='DR' THEN a.trans_amt ELSE 0 END) AS 'TDR',Count(*) as 'Icount' ");

             if (search.PageSize > 0)
             {
                 builder.Append(String.Format(" limit {0},{1}", search.Skip, search.PageSize));
             }

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                IDbCommand commandCount = conn.CreateCommand();


                


                command.CommandText = builder.ToString();
                commandCount.CommandText = QueryCount;


                command.AddParamWithValue(DbType.DateTime, "@startdate", StartDate);
                command.AddParamWithValue(DbType.DateTime, "@enddate", EndDate);

                if (BankID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@bank_id", BankID);
                }
                if(DealerID >0 && AgentID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@agent_id", AgentID);
                }
                else
                {
                    if (DealerID > 0)
                    {
                        command.AddParamWithValue(DbType.Int64, "@dealer_id", DealerID);
                    }
                    if (AgentID > 0)
                    {
                        command.AddParamWithValue(DbType.Int64, "@agent_id", AgentID);
                    }
                }
              
                if (!search.TransactionType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
                    command.AddParamWithValue(DbType.AnsiString, "@trans_type", search.TransactionType);
                   
                }



                switch (search.CustomSearch)
                {
                    case "Terminal ID":
                        command.AddParamWithValue(DbType.AnsiString, "@terminal_id", search.CustomSearchValue);
                        break;
                    case "Customer Ref":
                        command.AddParamWithValue(DbType.AnsiString, "@customer_ref", search.CustomSearchValue);
                        break;
                    case "Reference":
                        command.AddParamWithValue(DbType.AnsiString, "@trans_ref", search.CustomSearchValue);
                        break;

                }


               
                IDataReader rs = command.ExecuteReader();
                transactions = GetWalletTransaction(rs);

                foreach (IDataParameter p in command.Parameters)
                {
                    commandCount.Parameters.Add(p);
                }

                rs = commandCount.ExecuteReader(CommandBehavior.CloseConnection);
                DataTable t = rs.GetSchemaTable();
                while (rs.Read())
                {
                    if(t.Rows.Count > 0 && t.Rows[0][0].ToString() == "TCR")
                    {
                        search.TotalCredit = rs["TCR"] != DBNull.Value ? Convert.ToDecimal(rs["TCR"]) : 0;
                        search.TotalDebit = rs["TDR"] != DBNull.Value ?  Convert.ToDecimal(rs["TDR"]) : 0;
                        search.ItemCount = rs["TDR"] != DBNull.Value ?  Convert.ToInt32(rs["Icount"]) : 0;
                    }

                   
                }
                rs.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }

            return transactions;
        }

        public static List<WalletTransaction> GetWalletTransactionPagingV2(User oUser, DateTime StartDate, DateTime EndDate, long BankID, long DealerID, long AgentID, SearchModel search)
        {

            IDictionary<String, Object> dbParameters = new  Dictionary<String, Object>();
            List<WalletTransaction> transactions = null;
            StringBuilder builder = new StringBuilder();

            StringBuilder builderCount = new StringBuilder();

            builder.Append("select a.* from tbl_setup_trans_history a");
            builder.Append(" where a.date_created >=@startdate AND a.date_created <=@enddate");



            if (DealerID > 0)
            {


                Dealers dealer = DealersProcessor.GetDealersByID(DealerID);
                List<String> dealerAccountInfo = new List<String>();
                if (!String.IsNullOrWhiteSpace(dealer.AccountNumber))
                {
                    dealerAccountInfo.Add(dealer.AccountNumber);
                }
                if (!String.IsNullOrWhiteSpace(dealer.SettleAccountNumber))
                {
                    dealerAccountInfo.Add(dealer.SettleAccountNumber);
                }
                if (dealerAccountInfo.Count > 0)
                {
                    String accountNumber = String.Join(",", dealerAccountInfo.Select(c => string.Format("'{0}'", c)).ToArray());

                    // builder.Append(String.Format(" and (b.Id =@dealer_id or a.acctnumber in(0))", accountNumber));
                    builder.Append(String.Format(" AND ( EXISTS(select 1 from tbl_setup_agent as g where g.dealer_id=@dealer_id AND  g.Id = a.agent_id ) OR (a.acctnumber in({0})))", accountNumber));
                }
                else
                {
                    builder.Append(" AND EXISTS(select 1 from tbl_setup_agent as g where g.dealer_id=@dealer_id AND  g.Id = a.agent_id )");
                }
                dbParameters.Add("@dealer_id", DealerID);
            }
            if(AgentID > 0)
            {
                builder.Append(" and a.agent_id =@agent_id");
                dbParameters.Add("@dealer_id", AgentID);
            }

            
            
            if (!search.TransactionType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append(" and a.trans_type =@trans_type");
                dbParameters.Add("@trans_type", search.TransactionType);
            }

            //  case "Terminal ID":
            //    command.AddParamWithValue(DbType.AnsiString, "@terminal_id", search.CustomSearchValue);
            //    break;
            //case "Customer Ref":
            //    command.AddParamWithValue(DbType.AnsiString, "@customer_ref", search.CustomSearchValue);
            //    break;
            //case "Reference":
            //    command.AddParamWithValue(DbType.AnsiString, "@trans_ref", search.CustomSearchValue);
            //    break;
                switch (search.CustomSearch)
            {
                case "Terminal ID":
                    
                    builder.Append(" and a.terminal_id=@terminal_id");
                    dbParameters.Add("@terminal_id", search.CustomSearchValue);

                    break;
                case "Customer Ref":
                    builder.Append(" and a.customerref=@customer_ref");
                    dbParameters.Add("@customer_ref", search.CustomSearchValue);
                    break;
                case "Reference":
                    builder.Append(" and a.trans_ref=@trans_ref");
                    dbParameters.Add("@trans_ref", search.CustomSearchValue);
                    break;

            }

            String QueryCount = builder.ToString().Replace("a.*", " SUM(CASE WHEN a.trans_type='CR' THEN a.trans_amt ELSE 0 END) AS 'TCR',SUM(Case WHEN a.trans_type='DR' THEN a.trans_amt ELSE 0 END) AS 'TDR',Count(*) as 'Icount' ");

            if (search.PageSize > 0)
            {
                builder.Append(String.Format(" limit {0},{1}", search.Skip, search.PageSize));
            }

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                IDbCommand commandCount = conn.CreateCommand();





                command.CommandText = builder.ToString();
                commandCount.CommandText = QueryCount;


                command.AddParamWithValue(DbType.DateTime, "@startdate", StartDate);
                command.AddParamWithValue(DbType.DateTime, "@enddate", EndDate);

               


                foreach(var parameter in dbParameters)
                {
                    command.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter(parameter.Key,parameter.Value));
                }



                IDataReader rs = command.ExecuteReader();
                transactions = GetWalletTransactionV2(rs);

                foreach (IDataParameter p in command.Parameters)
                {
                    commandCount.Parameters.Add(p);
                }

                rs = commandCount.ExecuteReader(CommandBehavior.CloseConnection);
                DataTable t = rs.GetSchemaTable();
                while (rs.Read())
                {
                    if (t.Rows.Count > 0 && t.Rows[0][0].ToString() == "TCR")
                    {
                        search.TotalCredit = rs["TCR"] != DBNull.Value ? Convert.ToDecimal(rs["TCR"]) : 0;
                        search.TotalDebit = rs["TDR"] != DBNull.Value ? Convert.ToDecimal(rs["TDR"]) : 0;
                        search.ItemCount = rs["TDR"] != DBNull.Value ? Convert.ToInt32(rs["Icount"]) : 0;
                    }


                }
                rs.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }

            return transactions;
        }


        public static List<WalletTransaction> GetWalletTransactionPagingArch(User oUser, DateTime StartDate, DateTime EndDate, long BankID, long DealerID, long AgentID, SearchModel search)
        {
            List<WalletTransaction> transactions = null;
            StringBuilder builder = new StringBuilder();

            StringBuilder builderCount = new StringBuilder();

            builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_setup_trans_history_arch a left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber left join tbl_setup_agent as c on a.agent_id = c.Id");
            builder.Append(" where date_created >=@startdate AND date_created <=@enddate");

            if (BankID > 0)
            {
                builder.Append(" and a.bank_id =@bank_id");
            }
            if (DealerID > 0)
            {
                Dealers dealer = DealersProcessor.GetDealersByID(DealerID);
                List<String> dealerAccountInfo = new List<String>();
                if (!String.IsNullOrWhiteSpace(dealer.AccountNumber))
                {
                    dealerAccountInfo.Add(dealer.AccountNumber);
                }
                if (!String.IsNullOrWhiteSpace(dealer.SettleAccountNumber))
                {
                    dealerAccountInfo.Add(dealer.SettleAccountNumber);
                }
                if (dealerAccountInfo.Count > 0)
                {
                    String accountNumber =String.Join(",",dealerAccountInfo.Select(c=>string.Format("'{0}'",c)).ToArray());

                    builder.Append(String.Format(" and (b.Id =@dealer_id or a.acctnumber in(0) or EXISTS(select 1 from tbl_setup_agent ag where a.agent_id = ag.Id  and ag.dealer_id=@dealer_id))", accountNumber));
                }
                else
                {
                    builder.Append(" and (b.Id =@dealer_id or EXISTS(select 1 from tbl_setup_agent ag where a.agent_id = ag.Id  and ag.dealer_id=@dealer_id))");
                }
               
            }
            if (AgentID > 0)
            {
                builder.Append(" and a.agent_id =@agent_id");
            }

            switch (search.CustomSearch)
            {
                case "Terminal ID":
                    ;
                    builder.Append(" and a.terminal_id=@terminal_id");
                    break;
                case "Customer Ref":
                    builder.Append(" and a.customerref=@customer_ref");
                    break;
                case "Reference":
                    builder.Append(" and a.trans_ref=@trans_ref");
                    break;

            }

            String QueryCount = builder.ToString().Replace("a.*,b.dealer_name,c.agent_name", " SUM(CASE WHEN a.trans_type='CR' THEN a.trans_amt ELSE 0 END) AS 'TCR',SUM(Case WHEN a.trans_type='DR' THEN a.trans_amt ELSE 0 END) AS 'TDR',Count(*) as 'Icount' ");

            if (search.PageSize > 0)
            {
                builder.Append(String.Format(" limit {0},{1}", search.Skip, search.PageSize));
            }

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandTimeout = 700;
                IDbCommand commandCount = conn.CreateCommand();
                commandCount.CommandTimeout = 700;





                command.CommandText = builder.ToString();
                commandCount.CommandText = QueryCount;


                command.AddParamWithValue(DbType.DateTime, "@startdate", StartDate);
                command.AddParamWithValue(DbType.DateTime, "@enddate", EndDate);

                if (BankID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@bank_id", BankID);
                }
                if (DealerID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@dealer_id", DealerID);
                }
                if (AgentID > 0)
                {
                    command.AddParamWithValue(DbType.Int64, "@agent_id", AgentID);
                }



                switch (search.CustomSearch)
                {
                    case "Terminal ID":
                        command.AddParamWithValue(DbType.AnsiString, "@terminal_id", search.CustomSearchValue);
                        break;
                    case "Customer Ref":
                        command.AddParamWithValue(DbType.AnsiString, "@customer_ref", search.CustomSearchValue);
                        break;
                    case "Reference":
                        command.AddParamWithValue(DbType.AnsiString, "@trans_ref", search.CustomSearchValue);
                        break;

                }



                IDataReader rs = command.ExecuteReader();
                transactions = GetWalletTransaction(rs);

                foreach (IDataParameter p in command.Parameters)
                {
                    commandCount.Parameters.Add(p);
                }

                rs = commandCount.ExecuteReader(CommandBehavior.CloseConnection);
                DataTable t = rs.GetSchemaTable();
                while (rs.Read())
                {
                    if (t.Rows.Count > 0 && t.Rows[0][0].ToString() == "TCR")
                    {
                        search.TotalCredit = rs["TCR"] != DBNull.Value ? Convert.ToDecimal(rs["TCR"]) : 0;
                        search.TotalDebit = rs["TDR"] != DBNull.Value ? Convert.ToDecimal(rs["TDR"]) : 0;
                        search.ItemCount = rs["TDR"] != DBNull.Value ? Convert.ToInt32(rs["Icount"]) : 0;
                    }


                }
                rs.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }

            return transactions;
        }

        public static List<WalletTransaction> GetWalletTransactionV2(IDataReader rs)
        {
            List<WalletTransaction> Transactions = new List<WalletTransaction>();

            try
            {
                while (rs.Read())
                {
                    WalletTransaction t = ReadWalletTransactionV2(rs);
                    Transactions.Add(t);
                }
                rs.Close();
            }
            catch (Exception ex)
            {
            }
            if (Transactions != null && Transactions.Count > 0)
            {
                var listOfAgents = AgenrtProcessor.GetALLAGent();
                var listOfDealers= DealersProcessor.GetAllDealers();

                long currentDealerId = 0;
                foreach (var tran in Transactions)
                {
                    currentDealerId = 0;
                    if (tran.AgentID.HasValue && tran.AgentID.Value > 0)
                    {
                        var agent = listOfAgents.Where(c => c.Id == tran.AgentID).SingleOrDefault();
                        if(agent != null)
                        {
                            tran.AgentName = agent.AgenName;
                            currentDealerId = agent.DealerId;
                        }
                    }
                    else
                    {
                        var agent =  listOfAgents.Where(c=>c.UBAAccountNo == tran.AccountNumber || c.AccountNo == tran.AccountNumber).FirstOrDefault();
                        if (agent != null)
                        {
                            tran.AgentName = agent.AgenName;
                            currentDealerId = agent.DealerId;
                        }
                    }
                    if(currentDealerId > 0)
                    {
                       var currentDealer =  listOfDealers.Where(c => c.Id == currentDealerId).FirstOrDefault();
                        if (currentDealer != null)
                        {
                            tran.DealerName = currentDealer.DealerName;
                        }
                    }
                    else
                    {
                        var dealer = listOfDealers.Where(c => c.AccountNumber == tran.AccountNumber || c.SettleAccountNumber == tran.AccountNumber || c.UBASettleAccountNumber == tran.AccountNumber).FirstOrDefault();
                        if (dealer != null)
                        {
                            tran.DealerName = dealer.DealerName;
                        }
                    }
                }
            }
           


            return Transactions;

        }

        public static List<WalletTransaction> GetWalletTransaction(IDataReader rs)
        {
            List<WalletTransaction> Transactions = new List<WalletTransaction>();

            try
            {
                while (rs.Read())
                {
                    WalletTransaction t = ReadWalletTransaction(rs);
                    Transactions.Add(t);
                }
                rs.Close();
            }
            catch (Exception ex)
            {
            }

            return Transactions;

        }

        public static WalletTransaction ReadWalletTransaction(IDataReader rs)
        {
            WalletTransaction t = new WalletTransaction();


            try
            {
                t.TranID = DataHelper.GetValue<String>(rs["tranid"]);
                t.Balance = 0;
                t.Description = DataHelper.GetValue<String>(rs["description"]);
                t.LastTransacDate = DataHelper.GetValue<DateTime>(rs["date_created"]);
                t.LastTransactionAmt = DataHelper.GetValue<decimal>(rs["trans_amt"]);
                t.DealerName = DataHelper.GetValue<String>(rs["dealer_name"]);
                t.TransactionType = DataHelper.GetValue<String>(rs["trans_type"]);
                t.AgentName = DataHelper.GetValue<String>(rs["agent_name"]);
                t.CustomerRef = DataHelper.GetValue<String>(rs["customerref"]);
                t.Reference = DataHelper.GetValue<String>(rs["trans_ref"]);
                t.TerminalID = DataHelper.GetValue<String>(rs["terminal_id"]);
                t.AccountNumber = DataHelper.GetValue<String>(rs["acctnumber"]);

                Int64 agentid = DataHelper.GetValue<Int64>(rs["agent_id"]);
                Int64 bank_id = DataHelper.GetValue<Int64>(rs["bank_id"]);
                if (agentid > 0)
                {
                    t.AgentID = agentid;
                }
                if (bank_id > 0)
                {
                    t.BankID = bank_id;
                }
                t.Amount = t.LastTransactionAmt;
               

                if(rs["reversed"] != DBNull.Value)
                {
                    if(rs["reversed"].ToString().Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        t.IsReversed = true;
                    }
                }

                if (t.TransactionType.Equals("CR", StringComparison.OrdinalIgnoreCase))
                {
                    t.CreditAmount = t.LastTransactionAmt;
                }
                else
                {
                    t.DebitAmount = t.LastTransactionAmt;
                    t.LastTransactionAmt = -1 * t.LastTransactionAmt;

                }
            }
            catch (Exception ex)
            {

            }

            return t;
        }

        public static WalletTransaction ReadWalletTransactionV2(IDataReader rs)
        {
            WalletTransaction t = new WalletTransaction();


            try
            {
                t.TranID = DataHelper.GetValue<String>(rs["tranid"]);
                t.Balance = 0;
                t.Description = DataHelper.GetValue<String>(rs["description"]);
                t.LastTransacDate = DataHelper.GetValue<DateTime>(rs["date_created"]);
                t.LastTransactionAmt = DataHelper.GetValue<decimal>(rs["trans_amt"]);
                //t.DealerName = DataHelper.GetValue<String>(rs["dealer_name"]);
                t.TransactionType = DataHelper.GetValue<String>(rs["trans_type"]);
               // t.AgentName = DataHelper.GetValue<String>(rs["agent_name"]);
                t.CustomerRef = DataHelper.GetValue<String>(rs["customerref"]);
                t.Reference = DataHelper.GetValue<String>(rs["trans_ref"]);
                t.TerminalID = DataHelper.GetValue<String>(rs["terminal_id"]);
                t.AccountNumber = DataHelper.GetValue<String>(rs["acctnumber"]);

                Int64 agentid = DataHelper.GetValue<Int64>(rs["agent_id"]);
                Int64 bank_id = DataHelper.GetValue<Int64>(rs["bank_id"]);
                if (agentid > 0)
                {
                    t.AgentID = agentid;
                }
                if (bank_id > 0)
                {
                    t.BankID = bank_id;
                }
                t.Amount = t.LastTransactionAmt;


                if (rs["reversed"] != DBNull.Value)
                {
                    if (rs["reversed"].ToString().Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        t.IsReversed = true;
                    }
                }

                if (t.TransactionType.Equals("CR", StringComparison.OrdinalIgnoreCase))
                {
                    t.CreditAmount = t.LastTransactionAmt;
                }
                else
                {
                    t.DebitAmount = t.LastTransactionAmt;
                    t.LastTransactionAmt = -1 * t.LastTransactionAmt;

                }
            }
            catch (Exception ex)
            {

            }

            return t;
        }
    }
}