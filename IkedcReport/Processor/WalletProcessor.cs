using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Processor
{
    public class WalletProcessor
    {
        public static void Load(Models.WalletLoadRequest LoadRequest, ref String ErrorMessage,IDbCommand command)
        {
            try
            {


               

                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("INSERT INTO tbl_wallet_load_req");
                builder.Append("(dealer_id,amout,reference_no,description,create_date,bank_id,user_id,reference_id)");
                builder.Append("VALUES(@dealer_id,@amout,@reference_no,@description,@create_date,@bank_id,@user_id,@reference_id)");


                

                command.CommandText = builder.ToString();
                LoadRequest.Status = "Processed";
                command.Parameters.Clear();
                SetBankParameter(command, LoadRequest);

                int returnCode = command.ExecuteNonQuery();


                if (returnCode <= 0)
                {
                    throw new UpdateException("Could not insert Agent record");
                }
                command.Parameters.Clear();
                command.CommandText = "SELECT LAST_INSERT_ID()";
                object o = command.ExecuteScalar();
                LoadRequest.Id = Convert.ToInt64(o);
            }
            catch (OpenDBException ex)
            {
                ErrorMessage = "Error Opening Connection";
                ErrorMessage = ex.Message;
                throw ex;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error Creating Bank Information";
                ErrorMessage = ex.Message;
                throw ex;
            }
           
        }

        internal static bool Transfer(WalletLoadRequest walletLoad, User currentUser)
        {
            IDbConnection conn = null;
            IDbTransaction DbTransactManager = null;
            bool IsUpdated = false;
            try
            {
                //first we want to insert into wallet load history using transaction

               

           

                String message = String.Empty;


                Models.Dealers ODealer = Processor.DealersProcessor.GetDealersByID(currentUser.DealerID);
                if (ODealer != null)
                {
                    //insert into wallet history
                    //insert credit customer


                    Models.AgentUser agentUser = Processor.AgenrtProcessor.GetAgentBYID(walletLoad.AgentId);

                    if(agentUser == null)
                    {
                        return false;
                    }
                    conn = ConnectionManager.GetConnection();
                    conn.OpenIfClosed();
                    DbTransactManager = conn.BeginTransaction();

                    IDbCommand commandHistory = conn.CreateCommand();
                    commandHistory.Transaction = DbTransactManager;
                    Guid UniqueID = Guid.NewGuid();
                    String reference = String.Format("TRF-{0}", UniqueID.ToString().Replace("-", ""));
                    int AffectedRow = InsertHistory(commandHistory, UniqueID, UniqueID.ToString(), reference, walletLoad.Amount, ODealer.AccountNumber, "DR", String.Format("TRANSFER FROM {0} TO {1}",ODealer.DealerName,agentUser.AgenName), ODealer.AccountNumber);
                    if (AffectedRow > 0)
                    {
                        walletLoad.AccountNumber = ODealer.AccountNumber;
                        
                        IDbCommand commandBalance = conn.CreateCommand();
                        commandBalance.Transaction = DbTransactManager;
                        if (!UpdateDealerWalletBalance(commandBalance, ODealer.AccountNumber, walletLoad.Amount, walletLoad))
                        {
                            DbTransactManager.Rollback();
                            return false;
                        }

                        IDbCommand commandAgentCredit = conn.CreateCommand();
                        commandAgentCredit.Transaction = DbTransactManager;
                        String errorMessage = String.Empty;
                        bool isCredited = CreditAgentWallet(commandAgentCredit, walletLoad,agentUser);
                        if(!isCredited)
                        {
                            DbTransactManager.Rollback();
                            return false;
                        }
                        
                        int rowAffected = InsertHistoryAgent(conn, DbTransactManager, Guid.NewGuid(), reference, "TRF0001", walletLoad.Amount, String.Empty, "CR", String.Format("TRANSFER FROM {0} TO {1}", ODealer.DealerName, agentUser.AgenName), "TRF0001", walletLoad.AgentId, null);
                        if(rowAffected == 0)
                        {
                            DbTransactManager.Rollback();
                            return false;
                        }
                        DbTransactManager.Commit();
                        return true;
                    }
                    else
                    {
                        DbTransactManager.Rollback();
                    }

                }

            }
            catch (Exception ex)
            {
                if (DbTransactManager != null)
                    DbTransactManager.Rollback();
            }
            finally
            {
                if(conn != null)
                    ConnectionManager.Close(conn);
            }
            return IsUpdated;
        }

        public static bool DeleteClient(int id)
        {
            bool isUpdated = false;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = "delete from tbl_wallet_ip_whitelist where Id = @Id";
                command.AddParamWithValue(DbType.Int64, "@Id", id);
                isUpdated = command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return isUpdated;
        }

        public static bool CreateClientIPAddress(ClientIP clientIP)
        {
            bool isUpdated = false;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = "insert into tbl_wallet_ip_whitelist(ip_address,tran_type,description)values(@address,@tran_type,@description)";
                command.AddParamWithValue(DbType.AnsiString, "@address", clientIP.Address);
                command.AddParamWithValue(DbType.AnsiString, "@tran_type", clientIP.Trantype);
                command.AddParamWithValue(DbType.AnsiString, "@description", clientIP.Description);
                isUpdated = command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return isUpdated;
        }

        public static bool UpdateClientIPAddress(ClientIP clientIP)
        {
            bool isUpdated = false;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = "update tbl_wallet_ip_whitelist set ip_address=@address,tran_type=@tran_type,description=@description where Id = @Id";
                command.AddParamWithValue(DbType.AnsiString, "@address", clientIP.Address);
                command.AddParamWithValue(DbType.AnsiString, "@tran_type", clientIP.Trantype);
                command.AddParamWithValue(DbType.AnsiString, "@description", clientIP.Description);
                command.AddParamWithValue(DbType.Int64, "@Id", clientIP.Id);
                isUpdated = command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
            return isUpdated;
        }

        public static ClientIP GetClientIP(int Id)
        {
            List<ClientIP> listOfClientIP = new List<ClientIP>();
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "select * from tbl_wallet_ip_whitelist where Id=@Id";
                command.AddParamWithValue(DbType.Int64, "@Id", Id);
                var rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                LoadClientIP(listOfClientIP, rs);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            if (listOfClientIP != null && listOfClientIP.Count() > 0)
            {
                return listOfClientIP[0];
            }
            return null;
        }

        public static List<ClientIP> GetClientIP()
        {
            List<ClientIP> listOfClientIP = new List<ClientIP>();
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "select * from tbl_wallet_ip_whitelist";

                var rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                LoadClientIP(listOfClientIP, rs);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return listOfClientIP;
        }

        private static void LoadClientIP(List<ClientIP> listOfClientIP, IDataReader rs)
        {
            while (rs.Read())
            {
                ClientIP ip = new ClientIP();
                ip.Address = rs["ip_address"] == DBNull.Value ? String.Empty : rs["ip_address"].ToString();
                ip.Trantype = rs["tran_type"] == DBNull.Value ? String.Empty : rs["tran_type"].ToString();
                ip.Description = rs["description"] == DBNull.Value ? String.Empty : rs["description"].ToString();
                ip.Id = Convert.ToInt64(rs["Id"]);
                listOfClientIP.Add(ip);
            }
            rs.Close();
        }

        public static bool ISValidateLoadReference(String ReferenceID, Models.User CurrentUser)
        {
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command =  conn.CreateCommand();
                command.CommandText = "select count(*) from tbl_setup_load_ref where reference=@reference and UserID=@UserId";
                command.AddParamWithValue(DbType.AnsiString, "@reference", ReferenceID);
                command.AddParamWithValue(DbType.Int64, "@UserId", CurrentUser.Id);

                object o = command.ExecuteScalar();

                if (o != null && o != DBNull.Value)
                {
                    return Convert.ToInt32(o) > 0;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return false;
        }

        private static void SetBankParameter(IDbCommand command, Models.WalletLoadRequest LoadRequest)
        {
            //,@dealer_id,@amout,@reference_no,@description,@create_date,@bank_id,@user_id
            command.AddParamWithValue(DbType.Int64, "@dealer_id", LoadRequest.DealerId);
            command.AddParamWithValue(DbType.Decimal, "@amout", LoadRequest.Amount);
            command.AddParamWithValue(DbType.AnsiString, "@reference_no", LoadRequest.ReferenceNumber);
            command.AddParamWithValue(DbType.AnsiString, "@description", LoadRequest.Description);
            command.AddParamWithValue(DbType.DateTime, "@create_date", LoadRequest.CreateDate);
            command.AddParamWithValue(DbType.Int64, "@bank_id", LoadRequest.BankId);
            command.AddParamWithValue(DbType.Int64, "@user_id", LoadRequest.UserId);
            command.AddParamWithValue(DbType.AnsiString, "@status", LoadRequest.Status);
            command.AddParamWithValue(DbType.AnsiString, "@reference_id", LoadRequest.IDReference);
            if (LoadRequest.Id > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@Id", LoadRequest.Id);
            }
        }
        public static List<Models.WalletLoadRequestVM> GetALLLoadRequest()
        {

            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT a.* from tbl_wallet_load_req as a left join tbl_setup_dealers as b on a.dealer_id = b.id");
            builder.Append(" left join tbl_setup_banks as c on a.bank_id = c.id");

            List<Models.WalletLoadRequestVM> WalletLoadRequests = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                WalletLoadRequests = GetWallet(rs);


            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return WalletLoadRequests;
        }

        private static Random r = null;
        private static String[] characters = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9","A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z" };

        public static String GenerateReference()
        {

            return "06910220";
            //if (r == null)
            //    r = new Random();

            //System.Text.StringBuilder builder = new StringBuilder();

            //do
            //{
            //    try
            //    {
            //        int i = r.Next(70);
            //        if (i < characters.Length)
            //            builder.Append(characters[i]);
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}
            //while (builder.ToString().Length < 8);


            //return builder.ToString();
        }

        public static bool HasDuplicateTransaction(string iDReference)
        {
            //tbl_setup_load_duplicate
            bool hasDuplicate = false;
            IDbConnection conn = null;


            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command =  conn.CreateCommand();
                command.CommandText = "insert into tbl_setup_load_duplicate(reference)values(@reference)";
                command.AddParamWithValue(DbType.AnsiString, "@reference", iDReference);
                command.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                hasDuplicate = true;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return hasDuplicate;
        }

        internal static void DeleteDuplicateReference(string iDReference)
        {
            IDbConnection conn = null;


            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "delete from tbl_setup_load_duplicate where reference=@reference";
                command.AddParamWithValue(DbType.AnsiString, "@reference", iDReference);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
        }

        private static List<Models.WalletLoadRequestVM> GetWallet(IDataReader rs)
        {
            List<Models.WalletLoadRequestVM> WalletRequests = new List<Models.WalletLoadRequestVM>();
            try
            {

                IDictionary<Int64, Models.Dealers> DealerDictionary = Processor.DealersProcessor.GetAllDealersDictionary();
                while (rs.Read())
                {
                    Models.WalletLoadRequestVM WalletRequest = new Models.WalletLoadRequestVM();
                    WalletRequest.Id = Convert.ToInt64(rs["Id"]);
                    WalletRequest.Amount = DataHelper.GetValue<Decimal>(rs["amout"]);
                    WalletRequest.BankId = DataHelper.GetValue<Int64>(rs["bank_id"]);
                    WalletRequest.BankName = DataHelper.GetValue<String>(rs["bank_name"]);
                    WalletRequest.CreateDate = DataHelper.GetValue<DateTime>(rs["create_date"]);
                    WalletRequest.DealerId = DataHelper.GetValue<Int64>(rs["dealer_id"]);
                    WalletRequest.DealerName = DataHelper.GetValue<String>(rs["dealer_name"]);
                    WalletRequest.Description = DataHelper.GetValue<String>(rs["description"]);
                    WalletRequest.ReferenceNumber = DataHelper.GetValue<String>(rs["reference_no"]);
                    WalletRequest.Status = DataHelper.GetValue<String>(rs["status"]);
                    WalletRequest.UserId = DataHelper.GetValue<Int64>(rs["user_id"]);
                    WalletRequest.BalanceBefore = DataHelper.GetValue<Decimal>(rs["old_balance"]);
                    WalletRequest.BalanceAfter = DataHelper.GetValue<Decimal>(rs["new_balance"]);

                    Models.Dealers dealer = null;
                    if (DealerDictionary.TryGetValue(WalletRequest.DealerId, out  dealer))
                    {
                        WalletRequest.WalletBalance = dealer == null ? 0 : dealer.Balance;
                    }

                    WalletRequests.Add(WalletRequest);
                }
                rs.Close();
            }
            catch (Exception ex)
            {
            }
            return WalletRequests;
        }
        public static Models.WalletLoadRequest GetWalletRequestID(long Id)
        {


            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT a.*,b.dealer_name,c.bank_name from tbl_wallet_load_req as a left join tbl_setup_dealers as b on a.dealer_id = b.id");
            builder.Append(" left join tbl_setup_banks as c on a.bank_id = c.id  where a.Id = @Id");


            IDbConnection conn = null;
            List<Models.WalletLoadRequestVM> WalletRequests = null;
            Models.WalletLoadRequest WalletRequest = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();


                command.CommandText = builder.ToString();
                command.AddParamWithValue(DbType.Int64, "@Id", Id);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                WalletRequests = GetWallet(rs);


                if (WalletRequests != null && WalletRequests.Count > 0)
                {
                    WalletRequest = (Models.WalletLoadRequest)WalletRequests[0];
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return WalletRequest;
        }
        public static List<Models.WalletLoadRequestVM> GetAllWallet(Models.GeneralSearch generalSearch)
        {

            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT a.*,b.dealer_name,c.bank_name from tbl_wallet_load_req as a left join tbl_setup_dealers as b on a.dealer_id = b.id");
            builder.Append(" left join tbl_setup_banks as c on a.bank_id = c.id ");

            List<Models.WalletLoadRequestVM> WalletRequests = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Dealer Name":
                        builder.Append(" WHERE b.dealer_name REGEXP @dealername");
                        break;
                    case "Reference":
                        builder.Append(" WHERE a.reference_no REGEXP @reference_no");
                        break;
                    case "Description":
                        builder.Append(" WHERE a.description REGEXP @description");
                        break;
                }

                IDbCommand commandCount = conn.CreateCommand();
                commandCount.CommandText = builder.ToString().Replace("a.*,b.dealer_name,c.bank_name", "count(*) as ItemCount");



                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();



                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Dealer Name":
                        command.AddParamWithValue(DbType.AnsiString, "@dealername", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@dealername", generalSearch.SearchValue);
                        break;
                    case "Reference":
                        command.AddParamWithValue(DbType.AnsiString, "@reference_no", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@reference_no", generalSearch.SearchValue);
                        break;
                    case "Description":
                        command.AddParamWithValue(DbType.AnsiString, "@description", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@description", generalSearch.SearchValue);
                        break;
                }


                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                WalletRequests = GetWallet(rs);

                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        object o = commandCount.ExecuteScalar();

                        if (o != DBNull.Value && o != null)
                        {
                            generalSearch.ItemCount = Convert.ToInt32(o);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return WalletRequests;

        }


        internal static bool CreditAgentWallet(IDbCommand command,WalletLoadRequest walletLoad, Models.AgentUser oAgent)
        {
           
         
            //first we want to insert into wallet load history using transaction
                String Query = "update tbl_setup_agent set balance = balance + @tranamt where Id = @Id";

                try
                {
                    command.CommandText = Query;
                    command.AddParamWithValue(DbType.Decimal, "@tranamt", walletLoad.Amount);
                    command.AddParamWithValue(DbType.AnsiString, "@Id", oAgent.Id);

                    int RowAffected = command.ExecuteNonQuery();

                    if (RowAffected > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

           

        }


        internal static bool CreditAgentWallet(WalletLoadRequest WalletLoad,User currentUser, ref string ErrorMessage)
        {
            IDbConnection conn = null;
            IDbTransaction DbTransactManager = null;
            bool IsUpdated = false;
            try
            {
                //first we want to insert into wallet load history using transaction

                Models.AgentUser oAgent = Processor.AgenrtProcessor.GetAgentBYID(WalletLoad.AgentId);
                if (oAgent == null)
                {
                    ErrorMessage = "Agent does not exists";
                    return false;
                }
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                DbTransactManager = conn.BeginTransaction();

                String message = String.Empty;
               
              
                IDbCommand commandHistory = conn.CreateCommand();
                commandHistory.Transaction = DbTransactManager;
                Guid UniqueID = Guid.NewGuid();


                int AffectedRow = InsertHistoryAgent(conn, DbTransactManager, UniqueID, String.Format("WEB-{0}", WalletLoad.ReferenceNumber), "LOAD",WalletLoad.Amount, String.Empty, "CR", String.Format("LD WAL FROM {0}-{1}",currentUser.Firstname, oAgent.AgenName), String.Empty,oAgent.Id,null);
                if (AffectedRow > 0)
                {
                    IDbCommand commandBalance = conn.CreateCommand();
                    commandBalance.Transaction = DbTransactManager;
                    if (UpdateWalletAgentBalance(commandBalance, oAgent, WalletLoad.Amount, WalletLoad))
                    {
                        DbTransactManager.Commit();
                        IsUpdated = true;

                    }
                }
            }
            catch (Exception ex)
            {
                if (DbTransactManager != null)
                    DbTransactManager.Rollback();
                ErrorMessage = ex.Message;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return IsUpdated;
        }

        private static bool UpdateWalletAgentBalance(IDbCommand command, AgentUser oAgent, decimal amount, WalletLoadRequest walletLoad)
        {
            String Query = "update tbl_setup_agent set balance = balance + @tranamt where Id = @Id";

            try
            {
                command.CommandText = Query;
                command.AddParamWithValue(DbType.Decimal, "@tranamt", walletLoad.Amount);
                command.AddParamWithValue(DbType.AnsiString, "@Id", oAgent.Id);
             
                int RowAffected = command.ExecuteNonQuery();

                if (RowAffected > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static int InsertHistoryAgent(IDbConnection conn, IDbTransaction QueryTransactManager, Guid UniqueID, String TransReference, String TerminalID, Decimal TransactionAmt, String AccountNumber, String trans_type, String Description, String CustomerRef, Int64? agentID, Int64? bankId)
        {
            StringBuilder HistoryQueryBuilder = new StringBuilder();
            HistoryQueryBuilder.Append("Insert into tbl_setup_trans_history(tranid,trans_ref,terminal_id,acctnumber,trans_amt,trans_type,description,customerref,date_created,bank_id,agent_id,channel)");
            HistoryQueryBuilder.Append("Values(@tranid,@trans_ref,@terminal_id,@acct_number,@trans_amt,@trans_type,@description,@credit_acct_no,@date_created,@bank_id,@agent_id,@channel)");

            IDbCommand commandHistory = conn.CreateCommand();
            commandHistory.Transaction = QueryTransactManager;
            commandHistory.CommandText = HistoryQueryBuilder.ToString();

            commandHistory.AddParamWithValue(DbType.AnsiString, "@tranid", UniqueID.ToString());
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_ref", TransReference);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@terminal_id", TerminalID);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@acct_number", AccountNumber);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_amt", TransactionAmt);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_type", trans_type);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@description", Description);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@credit_acct_no", CustomerRef);
            commandHistory.AddParamWithValue(DbType.DateTime, "@date_created", DateTime.Now);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@channel", "POS");

            if (agentID.HasValue)
            {
                commandHistory.AddParamWithValue(DbType.Int64, "@agent_id", agentID.Value);
            }
            else
            {
                commandHistory.AddParamWithValue(DbType.Int64, "@agent_id", DBNull.Value);
            }
            if (bankId.HasValue)
            {
                commandHistory.AddParamWithValue(DbType.Int64, "@bank_id", bankId.Value);
            }
            else
            {
                commandHistory.AddParamWithValue(DbType.Int64, "@bank_id", DBNull.Value);
            }

            int RowAffected = commandHistory.ExecuteNonQuery();

            return RowAffected;

        }

        private static bool InsertIntoWalletAgent(IDbConnection conn, IDbTransaction dbTransactManager, WalletLoadRequest walletLoad, ref string message)
        {
            throw new NotImplementedException();
        }

        internal static bool ConfirmCode(Models.WalletLoadRequest WalletLoad,String code)
        {
            
            if(WalletLoad.ConfirmationCode == code)
                return true;
            return false;
        }


        private static bool InsertIntoWallet(IDbConnection conn, IDbTransaction dbTrasactManager, WalletReport.Models.WalletLoadRequest req,ref String ErrorMessage)
        {
            IDbCommand command = conn.CreateCommand();
            command.Transaction =  dbTrasactManager;

            bool IsSucceed = false;
            try
            {
                Load(req, ref ErrorMessage, command);
                IsSucceed = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                throw ex;
            }

            return IsSucceed;


        }
        public  static int InsertHistory(IDbCommand commandHistory, Guid UniqueID, String TransReference, String TerminalID, Decimal TransactionAmt, String AccountNumber, String trans_type, String Description, String CustomerRef)
        {

            StringBuilder HistoryQueryBuilder = new StringBuilder();
            HistoryQueryBuilder.Append("Insert into tbl_setup_trans_history(tranid,trans_ref,terminal_id,acctnumber,trans_amt,trans_type,description,customerref,date_created)");
            HistoryQueryBuilder.Append("Values(@tranid,@trans_ref,@terminal_id,@acct_number,@trans_amt,@trans_type,@description,@credit_acct_no,@date_created)");

            commandHistory.CommandText = HistoryQueryBuilder.ToString();

            commandHistory.AddParamWithValue(DbType.AnsiString, "@tranid", UniqueID.ToString());
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_ref", TransReference);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@terminal_id", TerminalID);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@acct_number", AccountNumber);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_amt", TransactionAmt);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_type", trans_type);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@description", Description);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@credit_acct_no", CustomerRef);
            commandHistory.AddParamWithValue(DbType.DateTime, "@date_created", DateTime.Now);
            int RowAffected = commandHistory.ExecuteNonQuery();
            return RowAffected;


        }
        public static bool UpdateWalletBalance(IDbCommand command,String AccountNumber, decimal Amount,Models.WalletLoadRequest WalletLoad)
        {
            //update set new_balance = 
            String Query = "update  tbl_wallet_load_req set old_balance = (select balance from tbl_setup_wallet_acct where acctnumber = @acct_no_2) where id = @wallet_id2;Update tbl_setup_wallet_acct set balance = balance + @tranamt where acctnumber = @acct and isactive=1;update  tbl_wallet_load_req set new_balance = (select balance from tbl_setup_wallet_acct where acctnumber = @acct_no_1) where id = @wallet_id";

            try
            {
                command.CommandText = Query;
                command.AddParamWithValue(DbType.Decimal, "@tranamt", Amount);
                command.AddParamWithValue(DbType.AnsiString, "@acct", AccountNumber);
                command.AddParamWithValue(DbType.AnsiString, "@acct_no_1", AccountNumber);
                command.AddParamWithValue(DbType.Int64, "@wallet_id", WalletLoad.Id);
                command.AddParamWithValue(DbType.AnsiString, "@acct_no_2", AccountNumber);
                command.AddParamWithValue(DbType.Int64, "@wallet_id2", WalletLoad.Id);

                int RowAffected = command.ExecuteNonQuery();

                if (RowAffected > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static bool UpdateDealerWalletBalance(IDbCommand command, String AccountNumber, decimal Amount, Models.WalletLoadRequest WalletLoad)
        {
            //update set new_balance = 
            String Query = "Update tbl_setup_wallet_acct set balance = balance - @tranamt where acctnumber = @acct and isactive=1";
            try
            {
                command.CommandText = Query;
                command.AddParamWithValue(DbType.Decimal, "@tranamt", Amount);
                command.AddParamWithValue(DbType.AnsiString, "@acct", AccountNumber);
                int RowAffected = command.ExecuteNonQuery();
                if (RowAffected > 0)
                {
                    command.Parameters.Clear();
                    command.CommandText = "select balance from  tbl_setup_wallet_acct where acctnumber = @acct";
                    command.AddParamWithValue(DbType.AnsiString, "@acct", AccountNumber);
                    object o = command.ExecuteScalar();
                    if(o != null && o != DBNull.Value)
                    {
                        if(Convert.ToDecimal(o) > 0)
                        {
                            return true;
                        }
                        else
                        {
                            throw new Exception("Insufficient Fund");
                        }
                    }
                    else
                    {
                        return false;
                    }
                    
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static bool CreditWallet(Models.WalletLoadRequest WalletLoad, User CurrentUser, ref String ErrorMessage)
        {
            IDbConnection conn = null;
            IDbTransaction DbTransactManager = null;
            bool IsUpdated = false;
            try
            {
                //first we want to insert into wallet load history using transaction

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                DbTransactManager = conn.BeginTransaction();

                String message = String.Empty;
              

               Models.Dealers ODealer = Processor.DealersProcessor.GetDealersByID(WalletLoad.DealerId);
               if (ODealer != null)
               {
                   //insert into wallet history
                   //insert credit customer

                   IDbCommand commandHistory = conn.CreateCommand();
                   commandHistory.Transaction = DbTransactManager;
                   Guid UniqueID = Guid.NewGuid();


                   int AffectedRow = InsertHistory(commandHistory, UniqueID, UniqueID.ToString(), String.Format("WEB-{0}", WalletLoad.ReferenceNumber), WalletLoad.Amount, ODealer.AccountNumber, "CR", String.Format("LD WAL FRM {0} TO {1}",CurrentUser.Firstname,ODealer.DealerName), ODealer.AccountNumber);
                   if (AffectedRow > 0)
                   {
                       WalletLoad.AccountNumber = ODealer.AccountNumber;
                       bool IsSucceed = InsertIntoWallet(conn, DbTransactManager, WalletLoad, ref message);
                       if (IsSucceed)
                       {
                           IDbCommand commandBalance = conn.CreateCommand();
                           commandBalance.Transaction = DbTransactManager;
                           if (UpdateWalletBalance(commandBalance, ODealer.AccountNumber, WalletLoad.Amount,WalletLoad))
                           {
                               DbTransactManager.Commit();
                               IsUpdated = true;

                           }
                       }
                       else
                       {
                           ErrorMessage = "Error while processing you request. Please try again";
                           DbTransactManager.Rollback();
                       }
                   }
                   else
                   {
                       ErrorMessage = "Dealer not found";
                       DbTransactManager.Rollback();
                   }

               }
               else
               {
                   ErrorMessage = "Error processing transaction";
                   DbTransactManager.Rollback();


               }

                



            }
            catch (Exception ex)
            {
                if (DbTransactManager != null)
                    DbTransactManager.Rollback();
                ErrorMessage = ex.Message;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return IsUpdated;
        }

        public static List<Models.WalletLoadRequest> GetLoad(IDataReader rs)
        {

            List<Models.WalletLoadRequest> WalletLoads = new List<Models.WalletLoadRequest>();

            while (rs.Read())
            {
                Models.WalletLoadRequest WL = new Models.WalletLoadRequest();
                try
                {
                    WL.DealerId = Convert.ToInt64(rs["dealer_id"]);
                    WL.Amount = Convert.ToDecimal(rs["amout"]);
                    WL.BankId = Convert.ToInt64(rs["bank_id"]);
                    WL.Description = DataHelper.GetValue<String>(rs["description"]);
                    WL.ReferenceNumber = DataHelper.GetValue<String>(rs["reference_no"]);
                    WL.Status = DataHelper.GetValue<String>(rs["status"]);


                    WalletLoads.Add(WL);


                }
                catch (Exception ex)
                {
                }
            }
            rs.Close();

            return WalletLoads;


        }

        internal static Models.WalletLoadRequest GetLoad(long id)
        {
            Models.WalletLoadRequest LoadReq = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command =  conn.CreateCommand();
                command.CommandText = "select * from tbl_wallet_load_req where Id=@Id";
                command.AddParamWithValue(DbType.Int64, "@Id", id);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

               List<Models.WalletLoadRequest> requests = GetLoad(rs);


               if (requests != null && requests.Count() > 0)
               {
                   LoadReq = requests[0];
               }

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return LoadReq;
        }

        internal static string FormatMobileNumber(string MobileNumber)
        {
            if (MobileNumber.StartsWith("234"))
                return MobileNumber;
            else if (MobileNumber.StartsWith("0"))
            {
                return string.Format("234{0}", MobileNumber.Substring(1));
            }
            else if(MobileNumber.StartsWith("+234"))
            {
                return MobileNumber.Substring(1);
            }
            return MobileNumber;
        }

        internal static bool InsertHistoryLoadReference(string Reference,long UserID)
        {
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = "insert into tbl_setup_load_ref(reference,UserID)values(@reference,@UserID)";


                command.AddParamWithValue(DbType.AnsiString, "@reference", Reference);
                command.AddParamWithValue(DbType.Int64, "@UserID", UserID);

                int RowAffected = command.ExecuteNonQuery();

                return RowAffected > 0;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return false;
        }
        public static int InsertHistory(IDbConnection conn, IDbTransaction QueryTransactManager, Guid UniqueID, String TransReference, String TerminalID, Decimal TransactionAmt, String AccountNumber, String trans_type, String Description, String CustomerRef, Int64? agentID, Int64? bankId)
        {

            StringBuilder HistoryQueryBuilder = new StringBuilder();
            HistoryQueryBuilder.Append("Insert into tbl_setup_trans_history(tranid,trans_ref,terminal_id,acctnumber,trans_amt,trans_type,description,customerref,date_created,bank_id,agent_id,channel)");
            HistoryQueryBuilder.Append("Values(@tranid,@trans_ref,@terminal_id,@acct_number,@trans_amt,@trans_type,@description,@credit_acct_no,@date_created,@bank_id,@agent_id,@channel)");

            IDbCommand commandHistory = conn.CreateCommand();
            commandHistory.Transaction = QueryTransactManager;
            commandHistory.CommandText = HistoryQueryBuilder.ToString();

            commandHistory.AddParamWithValue(DbType.AnsiString, "@tranid", UniqueID.ToString());
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_ref", TransReference);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@terminal_id", TerminalID);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@acct_number", AccountNumber);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_amt", TransactionAmt);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@trans_type", trans_type);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@description", Description);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@credit_acct_no", CustomerRef);
            commandHistory.AddParamWithValue(DbType.DateTime, "@date_created", DateTime.Now);
            commandHistory.AddParamWithValue(DbType.AnsiString, "@channel", "POS");

            if (agentID.HasValue)
            {
                commandHistory.AddParamWithValue(DbType.Int64, "@agent_id", agentID.Value);
            }
            else
            {
                commandHistory.AddParamWithValue(DbType.Int64, "@agent_id", DBNull.Value);
            }
            if (bankId.HasValue)
            {
                commandHistory.AddParamWithValue(DbType.Int64, "@bank_id", bankId.Value);
            }
            else
            {
                commandHistory.AddParamWithValue(DbType.Int64, "@bank_id", DBNull.Value);
            }

            int RowAffected = commandHistory.ExecuteNonQuery();

            return RowAffected;


        }

        public static Response ReverseTransaction(WalletTransaction OriginalTran)
        {
            Response resp = new Response();
            IDbConnection conn = null;
            IDbTransaction ReversalDBTransactionManager = null;
            try
            {


                //CustomerRe await GetCustomerInfo(new Transaction() { TerminalID = Trans.TerminalID, CustomerRef = Trans.CustomerRef, TransactionAmt = Trans.TransationAmt }, SQLParam);


                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();
                ReversalDBTransactionManager = conn.BeginTransaction();

                Guid UniqueID = Guid.NewGuid();



                IDbCommand command = conn.CreateCommand();
                command.CommandText = "update tbl_setup_trans_history set reversed = 'Y' where tranid=@tranID";
                command.Transaction = ReversalDBTransactionManager;
                command.AddParamWithValue(DbType.AnsiString, "@tranID", OriginalTran.TranID);

                int rowcount = command.ExecuteNonQuery();
                int rowInserted = 0;
                if (OriginalTran.TransactionType.Equals("CR", StringComparison.OrdinalIgnoreCase))
                {
                    rowInserted = InsertHistory(conn, ReversalDBTransactionManager, UniqueID, String.Format("{0}_RVL", OriginalTran.Reference), OriginalTran.TerminalID, OriginalTran.Amount, OriginalTran.AccountNumber, "DR", String.Format("{0} RSVL", OriginalTran.Description), OriginalTran.CustomerRef, OriginalTran.AgentID, OriginalTran.BankID);

                }
                else
                {
                    rowInserted = InsertHistory(conn, ReversalDBTransactionManager, UniqueID, String.Format("{0}_RVL", OriginalTran.Reference), OriginalTran.TerminalID, OriginalTran.Amount, OriginalTran.AccountNumber, "CR", String.Format("{0} RSVL", OriginalTran.Description), OriginalTran.CustomerRef, OriginalTran.AgentID, OriginalTran.BankID);

                }


                if (rowcount > 0 && rowInserted > 0)
                {


                    String Query = String.Empty;
                    if (OriginalTran.TransactionType.Equals("CR", StringComparison.OrdinalIgnoreCase))
                    {
                        Query = "Update tbl_setup_wallet_acct set balance = balance - @tranamt where acctnumber = @acct";

                    }
                    else
                    {
                        Query = "Update tbl_setup_wallet_acct set balance = balance + @tranamt where acctnumber = @acct";

                    }
                    command = conn.CreateCommand();
                    command.CommandText = Query;
                    command.Transaction = ReversalDBTransactionManager;

                    command.AddParamWithValue(DbType.Decimal, "@tranamt", OriginalTran.Amount);
                    command.AddParamWithValue(DbType.AnsiString, "@acct", OriginalTran.AccountNumber);

                    int walletRowCountAffected = command.ExecuteNonQuery();

                    if (walletRowCountAffected > 0)
                    {
                        ReversalDBTransactionManager.Commit();
                        resp.ResponseCode = "00";
                        resp.ResponseDescription = "SUCCESSFUL";


                        UpdateTerminalLimit(OriginalTran);

                    }
                    else
                    {
                        ReversalDBTransactionManager.Rollback();
                        resp.ResponseCode = "06";
                        resp.ResponseDescription = "Failed";
                    }
                }
                else
                {
                    ReversalDBTransactionManager.Rollback();
                    resp.ResponseCode = "06";
                    resp.ResponseDescription = "Failed";
                }
            }
            catch (Exception ex)
            {
                if (ReversalDBTransactionManager != null)
                {
                    ReversalDBTransactionManager.Rollback();
                }
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return resp;
        }



        private static void UpdateTerminalLimit(WalletTransaction OriginalTran)
        {
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                if (OriginalTran.TransactionType.Equals("CR", StringComparison.OrdinalIgnoreCase))
                {
                    command.CommandText = "update tbl_terminal_wal_map set debit_limit = debit_limit - @amt where terminal_id =@terminal_id and acctnumber = @acct_no and ignore_limit = 0";

                }
                else
                {
                    command.CommandText = "update tbl_terminal_wal_map set debit_limit = debit_limit + @amt where terminal_id =@terminal_id and acctnumber = @acct_no and ignore_limit = 0";

                }
                command.AddParamWithValue(DbType.Decimal, "@amt", (-1) * OriginalTran.LastTransactionAmt);
                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", OriginalTran.TerminalID);
                command.AddParamWithValue(DbType.AnsiString, "@acct_no", OriginalTran.AccountNumber);
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
        }
    }
}
