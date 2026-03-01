using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Processor
{
    public class IndexProcessor
    {

        public static Models.Summary GetTotalTransaction(DateTime startDate, DateTime endDate,Models.User OUser)
        {

            IDictionary<String, Object> dbParameters = new Dictionary<string, object>();
            Models.Summary summary = new Models.Summary();

            IDbConnection conn = null;

            StringBuilder builder = new StringBuilder();
            try
            {
                builder.Append("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;Select SUM(CASE WHEN a.trans_type='CR' THEN a.trans_amt ELSE 0 END) AS 'TCR',SUM(Case WHEN a.trans_type='DR' THEN a.trans_amt ELSE 0 END) AS 'TDR',Count(*) as 'Icount' ");
                builder.Append(" from tbl_setup_trans_history as a");
                builder.Append(" where a.date_created between @startDate and @endDate");

                if (!(OUser.IsSuperUserOrAdmin || OUser.IsEtop))
                {
                    if (OUser.isBank)
                    {
                        builder.Append(" and a.bank_id=@bank_id");
                        dbParameters.Add("@bank_id", OUser.BankID);
                    }
                    else if (OUser.IsDealer)
                    {
                        Dealers dealer = DealersProcessor.GetDealersByID(OUser.DealerID);
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
                        dbParameters.Add("@dealer_id", OUser.DealerID);
                    }
                    else if(OUser.IsAgent)
                    {
                        builder.Append(" and a.agent_id = @agent_id");
                        dbParameters.Add("@agent_id", OUser.AgentID);
                    }
                }

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandTimeout = 90;
                command.CommandText = builder.ToString();
                command.AddParamWithValue(DbType.DateTime, "@startDate", startDate);
                command.AddParamWithValue(DbType.DateTime, "@endDate", endDate);

                foreach (var param in dbParameters)
                {
                    command.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter(param.Key,param.Value));
                }
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (rs.Read())
                {
                    summary.TotalCredit =  rs["TCR"] == DBNull.Value ? 0 :  Convert.ToDecimal(rs["TCR"]);
                    summary.TotalDebit = rs["TDR"] == DBNull.Value ? 0 : Convert.ToDecimal(rs["TDR"]);
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
            return summary;
        }

        public static TransactionDetail GetTransactionDetail(string id)
        {
            TransactionDetail detail = null;
            MySql.Data.MySqlClient.MySqlConnection conn = null;
            try
            {
                System.Text.StringBuilder builder = new StringBuilder();
                builder.Append("select a.*,b.dealer_name,c.agent_name,b.settl_acct_no from tbl_setup_trans_history a left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber left join tbl_setup_agent as c on a.agent_id = c.Id");
                builder.Append(" where a.tranid = @tranID; SELECT * FROM tbl_notify_log where notify_id =(select notify_id from tbl_setup_trans_history where tranid=@tranID)");

                conn = (MySql.Data.MySqlClient.MySqlConnection)ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                MySql.Data.MySqlClient.MySqlCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();
                command.Parameters.AddWithValue("@tranID", id);
                using (var rs = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    WalletTransaction t = null;
                    if (rs.Read())
                    {
                        t = TransactionQueryProcessor.ReadWalletTransaction(rs);
                        if(rs["settl_acct_no"] != DBNull.Value)
                        {
                            t.SettlementAccount = rs["settl_acct_no"].ToString();
                        }
                        detail = new TransactionDetail();
                        detail.Transaction = t;
                    }
                    rs.NextResult();
                    if (rs.Read())
                    {
                        detail.RequestXml = rs["request_msg"] == DBNull.Value ? String.Empty : rs["request_msg"].ToString();
                        detail.ResponseXml = rs["response_msg"] == DBNull.Value ? String.Empty : rs["response_msg"].ToString();

                    }
                    rs.Close();
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (conn != null) conn.Close();
            }
            return detail;
        }

        internal static Models.TotalCreditDebit GetTotalDebitCreditTransaction(User currentUser)
        {
            Models.TotalCreditDebit totalCredit = new TotalCreditDebit();

            MySql.Data.MySqlClient.MySqlConnection conn = null;
            try
            {
                
                conn = (MySql.Data.MySqlClient.MySqlConnection)ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                String query = "select a.trans_type, sum(a.trans_amt) as trans_amt  from tbl_setup_trans_history as a ";
                var command =  conn.CreateCommand();
               
                if(!(currentUser.IsSuperUserOrAdmin || currentUser.IsEtop))
                {
                    if(currentUser.IsDealer)
                    {
                        query = query + "  left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber where b.Id=@Id";
                    }
                    else if(currentUser.IsAgent)
                    {
                        query = query + " left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber left join tbl_setup_agent as c on a.agent_id = c.Id where c.Id = @Id";
                    }
                }
                command.CommandText = String.Format("{0} group by a.trans_type ", query);
                //command.CommandText = "select trans_type, sum(trans_amt) as trans_amt  from tbl_setup_trans_history group by trans_type";
                if (!(currentUser.IsSuperUserOrAdmin || currentUser.IsEtop))
                {
                    if (currentUser.IsDealer)
                    {
                        command.AddParamWithValue(DbType.Int64, "@Id", currentUser.DealerID);
                    }
                    else if(currentUser.IsAgent)
                    {
                        command.AddParamWithValue(DbType.Int64, "@Id", currentUser.AgentID);
                    }
                }

                var rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (rs.Read())
                {
                    if(rs["trans_type"] != DBNull.Value && rs["trans_type"].ToString().Trim().Equals("CR", StringComparison.OrdinalIgnoreCase))
                    {
                        totalCredit.CreditTotal = Convert.ToDecimal(rs["trans_amt"]);
                    }
                    if (rs["trans_type"] != DBNull.Value && rs["trans_type"].ToString().Trim().Equals("DR", StringComparison.OrdinalIgnoreCase))
                    {
                        totalCredit.DebitTotal = Convert.ToDecimal(rs["trans_amt"]);
                    }

                }
                rs.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (conn != null) conn.Close();
            }
            return totalCredit;
        }



        public static List<Models.WalletTransaction> GetTransactionHistory(Models.User Ouser)
        {

            return TransactionQueryProcessor.GetWalletTransactionForDashboard(Ouser, DateTime.Now.AddDays(-15), DateTime.Now.AddDays(1));

            //List<Models.WalletTransaction> Transactions = null;
            //System.Text.StringBuilder builder = new StringBuilder();
            //builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_setup_trans_history a left join tbl_setup_dealers as b on a.acctnumber = b.acctnumber left join tbl_setup_agent as c on a.agent_id = c.Id");
            //if (!(Ouser.IsSuperUserOrAdmin || Ouser.IsEtop))
            //{
            //    if (Ouser.isBank)
            //    {
            //        builder.Append(" where a.bank_id=@bank_id");
            //    }
            //    else if (Ouser.IsDealer)
            //    {
            //        builder.Append(" where b.Id=@Id");
            //    }
            //    else
            //    {
            //        builder.Append(" where c.Id=@Id");
            //    }

            //}
            //builder.Append(" order by date_created desc limit 0,15");
            //IDbConnection conn = null;

            //try
            //{
            //    conn = ConnectionManager.GetConnection();
            //    conn.OpenIfClosed();

            //    IDbCommand command = conn.CreateCommand();
            //    command.CommandTimeout = 90;
            //    command.CommandText = builder.ToString();
            //    if (!(Ouser.IsSuperUserOrAdmin || Ouser.IsEtop))
            //    {
            //        if (Ouser.isBank)
            //        {
            //            command.AddParamWithValue(DbType.Int64, "@bank_id", Ouser.BankID);
            //        }
            //        else if (Ouser.IsDealer)
            //        {
            //            command.AddParamWithValue(DbType.Int64, "@Id", Ouser.DealerID);
            //        }
            //        else
            //        {
            //            command.AddParamWithValue(DbType.Int64, "@Id", Ouser.AgentID);
            //        }

            //    }
            //    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

            //    Transactions = WalletReport.Processor.TransactionQueryProcessor.GetWalletTransaction(rs);
            //}
            //catch (Exception ex)
            //{
            //}
            //finally
            //{
            //    ConnectionManager.Close(conn);
            //}
            //if (Transactions == null)
            //    Transactions = new List<Models.WalletTransaction>();

            //return Transactions;

        }
    }
}
