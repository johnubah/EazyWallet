using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Processor
{
    public class DealersProcessor
    {

        public static List<Models.Dealers> GetDealers(IDataReader rs)
        {

            List<Models.Dealers> dealers = new List<Models.Dealers>();



            bool hasColumn = rs.HasColumn("balance");

           

            while (rs.Read())
            {
                try
                {
                    Models.Dealers dealer = new Models.Dealers();
                    dealer.Id = Convert.ToInt64(rs["Id"]);
                    dealer.DealerName = DataHelper.GetValue<String>(rs["dealer_name"]);
                    dealer.AddressLine1 = DataHelper.GetValue<String>(rs["address_line_1"]);
                    dealer.AddressLine2 = DataHelper.GetValue<String>(rs["address_line_2"]);
                    dealer.City = DataHelper.GetValue<String>(rs["city"]);
                    dealer.State = DataHelper.GetValue<String>(rs["state"]);
                    dealer.Country = DataHelper.GetValue<String>(rs["country"]);
                    dealer.ContactAddress = DataHelper.GetValue<String>(rs["contact_address"]);
                    dealer.ContactEmail = DataHelper.GetValue<String>(rs["contact_email"]);
                    dealer.ContactMobile = DataHelper.GetValue<String>(rs["contact_mobile"]);
                    dealer.ContactName = DataHelper.GetValue<String>(rs["contact_name"]);
                    dealer.BankId = DataHelper.GetValue<Int64>(rs["bank_id"]);
                    if(hasColumn)
                    {
                        dealer.Balance = DataHelper.GetValue<Decimal>(rs["balance"]);
                    }
                    dealer.SettleAccountNumber = DataHelper.GetValue<String>(rs["settl_acct_no"]);
                    dealer.AccountNumber = DataHelper.GetValue<String>(rs["acctnumber"]);
                    dealer.MerchantId = DataHelper.GetValue<int>(rs["merchant_id"]);
                    dealer.UBASettleAccountNumber = DataHelper.GetValue<String>(rs["uba_settl_acct_no"]);
                    dealers.Add(dealer);
                }
                catch (Exception ex)
                {
                }
            }
            rs.Close();

            return dealers;

        }

       

        public static void SetDealersParameter(IDbCommand command, Models.Dealers dealers)
        {
            command.AddParamWithValue(DbType.AnsiString, "@dealer_name", dealers.DealerName);
            command.AddParamWithValue(DbType.AnsiString, "@address_line_1", dealers.AddressLine1);
            command.AddParamWithValue(DbType.AnsiString, "@address_line_2", dealers.AddressLine2);
            command.AddParamWithValue(DbType.AnsiString, "@city", dealers.City);
            command.AddParamWithValue(DbType.AnsiString, "@state", dealers.State);
            command.AddParamWithValue(DbType.AnsiString, "@country", dealers.Country);
            command.AddParamWithValue(DbType.AnsiString, "@contact_address", dealers.ContactAddress);
            command.AddParamWithValue(DbType.AnsiString, "@contact_email", dealers.ContactEmail);
            command.AddParamWithValue(DbType.AnsiString, "@contact_mobile", dealers.ContactMobile);
            command.AddParamWithValue(DbType.AnsiString, "@contact_name", dealers.ContactName);
            command.AddParamWithValue(DbType.Int64, "@bank_id",dealers.BankId);
            command.AddParamWithValue(DbType.Int32, "@merchant_id", dealers.MerchantId);
            if (dealers.Id > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@Id", dealers.Id);
            }
        }

        private static void CreateDealer(IDbConnection conn,IDbTransaction DbTransaction,Guid UniqueID)
        {
            
            try
            {
                IDbCommand command = conn.CreateCommand();
                command.CommandText = "insert into tbl_setup_wallet_acct(acctnumber,balance,isactive)values(@acctnumber,@balance,@isactive)";
                command.Transaction = DbTransaction;

                command.AddParamWithValue(DbType.AnsiString, "@acctnumber", UniqueID.ToString());
                command.AddParamWithValue(DbType.Decimal, "@balance", 0);
                command.AddParamWithValue(DbType.Boolean, "@isactive", true);


                int RowAffected = command.ExecuteNonQuery();
                if (RowAffected <= 0)
                {
                    throw new Exception("Could not update record");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static void Create(Models.Dealers dealers)
        {

            Guid UniqueAccount = Guid.NewGuid();

            IDbTransaction Bbtransaction = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                Bbtransaction = conn.BeginTransaction();
                CreateDealer(conn, Bbtransaction, UniqueAccount);


                IDbCommand command = conn.CreateCommand();
                command.Transaction = Bbtransaction;

                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("Insert into tbl_setup_dealers(dealer_name,address_line_1,address_line_2,city,");
                builder.Append("state,country,contact_address,contact_email,contact_mobile,");
                builder.Append("contact_name,bank_id,acctnumber,merchant_id");

                builder.Append(")");
                builder.Append("values(@dealer_name,@address_line_1,@address_line_2,@city,");
                builder.Append("@state,@country,@contact_address,@contact_email,@contact_mobile,");
                builder.Append("@contact_name,@bank_id,@acctno,@merchant_id");
                builder.Append(")");

                command.CommandText = builder.ToString();
                SetDealersParameter(command, dealers);
                command.AddParamWithValue(DbType.AnsiString, "@acctno", UniqueAccount.ToString());
              

                int returnCode = command.ExecuteNonQuery();

                if (returnCode <= 0)
                {
                    throw new UpdateException("Could not insert dealer record");
                }
                Bbtransaction.Commit();

                command.Parameters.Clear();
                command.CommandText = "SELECT LAST_INSERT_ID()";
                object o = command.ExecuteScalar();

                dealers.Id = Convert.ToInt64(o);
            }
            catch (OpenDBException ex)
            {
                if (Bbtransaction != null)
                    Bbtransaction.Rollback();
                throw ex;
            }
            catch (Exception ex)
            {
                if (Bbtransaction != null)
                    Bbtransaction.Rollback();
                throw ex;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
        }

        internal static bool SettlementAccountExistsForOtherDealer(Dealers dealers)
        {
            IDbConnection conn = null;
            bool isExists = false;
            try
            {
                conn = DBConnector.ConnectionManager.GetConnection();
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "select count(*) from tbl_setup_dealers where settl_acct_no = @acct_no and Id !=@Id";
                command.AddParamWithValue(DbType.AnsiString, "@acct_no", dealers.SettleAccountNumber);
                command.AddParamWithValue(DbType.Int64, "@Id", dealers.Id);

                object o = command.ExecuteScalar();
                isExists = Convert.ToInt32(o) > 0;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                DBConnector.ConnectionManager.Close(conn);
            }
            return isExists;
        }

        public static bool SettlementAccountExists(Dealers dealers)
        {
            IDbConnection conn = null;
            bool isExists = false;
            try
            {
                conn =  DBConnector.ConnectionManager.GetConnection();
                if(conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "select count(*) from tbl_setup_dealers where settl_acct_no = @acct_no";
                command.AddParamWithValue(DbType.AnsiString, "@acct_no", dealers.SettleAccountNumber);
                object o = command.ExecuteScalar();
                isExists = Convert.ToInt32(o) > 0;
            }
            catch(Exception ex)
            {

            }
            finally
            {
                DBConnector.ConnectionManager.Close(conn);
            }
            return isExists;
        }

        public static void Update(Models.Dealers dealers)
        {

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("update tbl_setup_dealers set dealer_name=@dealer_name,address_line_1=@address_line_1,address_line_2=@address_line_2,city=@city,");
            builder.Append("state = @state,country=@country,contact_address=@contact_address,contact_email=@contact_email,contact_mobile=@contact_mobile,");
            builder.Append("contact_name = @contact_name,bank_id=@bank_id,merchant_id=@merchant_id where Id=@Id");

           

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

                SetDealersParameter(command, dealers);
                //command.AddParamWithValue(DbType.AnsiString, "@settl_acct_no", dealers.SettleAccountNumber);
                int returnCode = command.ExecuteNonQuery();
                if (returnCode <= 0)
                {
                    throw new UpdateException("Could not insert dealer record");
                }
            }
            catch (UpdateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
        }
        public static List<Models.Dealers> GetAllDealers(Models.GeneralSearch generalSearch)
        {
            List<Models.Dealers> dealers = null;
            IDbConnection conn = null;
            try
            {


                Models.User CurrentUser = HttpContext.Current.Session["CurrentUser"] as Models.User;

                if (CurrentUser == null)
                {
                    return new List<Models.Dealers>();
                }

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("SELECT a.*,b.balance from tbl_setup_dealers as a left join tbl_setup_wallet_acct as b on a.acctnumber = b.acctnumber where a.Id>0");

                if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank || CurrentUser.IsManager))
                {
                    if (CurrentUser.isBank && CurrentUser.BankID > 0)
                    {
                        builder.Append(" and a.bank_id = @bank_id");
                    }
                    if (CurrentUser.IsDealer)
                    {
                        builder.Append(" and a.Id = @Id");
                    }
                }

                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Dealer Name":
                        builder.Append(" and a.dealer_name REGEXP @dealer_name");
                        break;
                    case "ContactName":
                        builder.Append(" and a.contact_name REGEXP @contact_name");
                        break;
                    case "Contact Mobile":
                        builder.Append(" and a.contact_mobile REGEXP @contact_mobile");
                        break;
                }

                IDbCommand commandCount = conn.CreateCommand();
                commandCount.CommandText = builder.ToString().Replace("*", "count(*) as ItemCount");

                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();






                if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank || CurrentUser.IsManager))
                {
                    
                    if (CurrentUser.isBank && CurrentUser.BankID > 0)
                    {
                        command.AddParamWithValue(DbType.Int64, "@bank_id", CurrentUser.BankID);
                    }
                    if (CurrentUser.IsDealer)
                    {
                        command.AddParamWithValue(DbType.Int64, "@Id", CurrentUser.DealerID);
                    }
                }
                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Dealer Name":
                        command.AddParamWithValue(DbType.AnsiString, "@dealer_name", generalSearch.SearchValue);
                        break;
                    case "ContactName":
                        command.AddParamWithValue(DbType.AnsiString, "@contact_name", generalSearch.SearchValue);
                        break;
                    case "Contact Mobile":
                        command.AddParamWithValue(DbType.AnsiString, "@contact_mobile", generalSearch.SearchValue);
                        break;
                }


                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                dealers = GetDealers(rs);

                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        object o = commandCount.ExecuteScalar();

                        foreach (IDataParameter param in command.Parameters)
                        {
                            commandCount.Parameters.Add(param);
                        }

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
            return dealers;

        }
        internal static List<Models.Dealers> GetAllDealers(Models.User CurrentUser)
        {
            List<Models.Dealers> dealers = null;
            IDbConnection conn = null;

            System.Text.StringBuilder builderQuery = new System.Text.StringBuilder();
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                builderQuery.Append("SELECT a.*,b.balance from tbl_setup_dealers a left join tbl_setup_wallet_acct as b on a.acctnumber=b.acctnumber where a.Id > 0");

              

                if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank))
                {
                   
                    if (CurrentUser.IsDealer)
                    {
                        builderQuery.Append(" and a.Id = @Id");
                    }
                    else if(CurrentUser.IsAgent)
                    {
                        builderQuery.Append(" and exists( select * from tbl_setup_agent as c where c.Id = @id and a.Id = c.dealer_id ");
                    }
                }
                IDbCommand command = conn.CreateCommand();
                command.CommandText = builderQuery.ToString();

                if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank))
                {
                   
                    if (CurrentUser.IsDealer)
                    {
                        command.AddParamWithValue(DbType.Int64, "@Id", CurrentUser.DealerID);
                    }
                    else
                    {
                        command.AddParamWithValue(DbType.Int64, "@id", CurrentUser.AgentID);
                    }
                }

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                dealers = GetDealers(rs);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return dealers;
        }

        internal static List<Models.Dealers> GetAllDealers()
        {
            List<Models.Dealers> dealers = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT a.*,b.balance from tbl_setup_dealers a left join tbl_setup_wallet_acct as b on a.acctnumber=b.acctnumber";

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                dealers = GetDealers(rs);



            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return dealers;
        }

        internal static Models.Dealers GetDealersByID(long Id)
        {
            Models.Dealers dealer = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT a.*,a.merchant_id,b.balance from tbl_setup_dealers a left join tbl_setup_wallet_acct as b on a.acctnumber = b.acctnumber where a.Id=@Id";


                command.AddParamWithValue(DbType.Int64, "@Id", Id);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                List<Models.Dealers> dealers = GetDealers(rs);
                if (dealers != null && dealers.Count() > 0)
                {
                    dealer = dealers[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return dealer;
        }



        



        internal static IDictionary<Int64, Models.Dealers> GetAllDealersDictionary()
        {
           List<Models.Dealers> dealerList =  GetAllDealers();

           IDictionary<Int64, Models.Dealers> DealerDictionary = new Dictionary<Int64, Models.Dealers>();

           if (dealerList != null)
           {
               foreach (Models.Dealers d in dealerList)
               {
                   DealerDictionary.Add(d.Id, d);
               }
           }
           return DealerDictionary;

        }

        internal static List<Models.Dealers> GetDealersByUser(Models.User oUser)
        {
            List<Models.Dealers> Dealers = null;
            IDbConnection conn = null;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            try
            {
                builder.Append("select a.* from tbl_setup_dealers as a ");
                builder.Append(" left join tbl_setup_agent as b on a.Id = b.dealer_id");
                if (!(oUser.IsSuperUserOrAdmin || oUser.IsEtop))
                {
                    if (oUser.isBank)
                    {
                        builder.Append(" where a.bank_id = @bank_id");
                    }
                    else if (oUser.IsDealer)
                    {
                        builder.Append(" where a.Id = @DealerID");
                    }
                    else if (oUser.IsAgent)
                    {
                        builder.Append(" where c.Id = @agent_id");
                    }
                }

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();
                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();


                if (!(oUser.IsSuperUserOrAdmin || oUser.IsEtop))
                {
                    if (oUser.isBank)
                    {
                        command.AddParamWithValue(DbType.Int64, "@bank_id", oUser.BankID);
                    }
                    else if (oUser.IsDealer)
                    {
                        command.AddParamWithValue(DbType.Int64, "@DealerID", oUser.DealerID);
                    }
                    else if (oUser.IsAgent)
                    {
                        command.AddParamWithValue(DbType.Int64, "@agent_id", oUser.AgentID);
                    }
                }

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Dealers = GetDealers(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }

            return Dealers;
        }

        public static List<Models.Dealers> GetFullDealerByBank(long BankID)
        {
            List<Models.Dealers> Dealers = null;// new List<Models.Dealers>();


            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT * from tbl_setup_dealers where bank_id = @bankid";

                command.AddParamWithValue(DbType.Int64, "@bankid", BankID);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Dealers = GetDealers(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            if (Dealers == null)
                Dealers = new List<Models.Dealers>();
            return Dealers;
        }


        public static List<Models.DealerVM> GetDealerByBank(long BankID)
        {
            List<Models.DealerVM> Dealers = new List<Models.DealerVM>();


            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

               IDbCommand command =  conn.CreateCommand();
               command.CommandText = "SELECT Id,dealer_name from tbl_setup_dealers";

              // command.AddParamWithValue(DbType.Int64, "@bankid", BankID);
               IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
               while (rs.Read())
               {
                   Models.DealerVM dealer = new Models.DealerVM();
                   dealer.Id = Convert.ToInt64(rs["Id"]);
                   dealer.DealerName = rs["dealer_name"].ToString();
                   Dealers.Add(dealer);

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
            return Dealers;
        }

        
    }
}