using System;
using System.Collections.Generic;
using System.Data;
using WalletReport.DBConnector;

namespace WalletReport.Processor
{
    public class BankProcessor
    {
        public static void Create(Models.Bank Bank,ref String ErrorMessage)
        {
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("Insert into tbl_setup_banks(bank_name,address_line_1,address_line_2,city,");
                builder.Append("state,country,contact_address,contact_email,contact_mobile,");
                builder.Append("contact_name,terminal_prefix");

                builder.Append(")");
                builder.Append("values(@bank_name,@address_line_1,@address_line_2,@city,");
                builder.Append("@state,@country,@contact_address,@contact_email,@contact_mobile,");
                builder.Append("@contact_name,@terminal_prefix");
                builder.Append(")");

                command.CommandText = builder.ToString();
                SetBankParameter(command, Bank);

                int returnCode = command.ExecuteNonQuery();

                if (returnCode <= 0)
                {
                    throw new UpdateException("Could not insert dealer record");
                }

                command.Parameters.Clear();
                command.CommandText = "SELECT LAST_INSERT_ID()";
                object o = command.ExecuteScalar();

                Bank.Id = Convert.ToInt64(o);
            }
            catch (OpenDBException ex)
            {
                ErrorMessage = "Error Opening Connection";
                throw ex;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error Creating Bank Information";

                throw ex;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
        }

        private static void SetBankParameter(IDbCommand command, Models.Bank _Bank)
        {
            command.AddParamWithValue(DbType.AnsiString, "@bank_name", _Bank.BankName);
            command.AddParamWithValue(DbType.AnsiString, "@address_line_1", _Bank.AddressLine1);
            command.AddParamWithValue(DbType.AnsiString, "@address_line_2", _Bank.AddressLine2);
            command.AddParamWithValue(DbType.AnsiString, "@city", _Bank.City);
            command.AddParamWithValue(DbType.AnsiString, "@state", _Bank.State);
            command.AddParamWithValue(DbType.AnsiString, "@country", _Bank.Country);
            command.AddParamWithValue(DbType.AnsiString, "@contact_address", _Bank.ContactAddress);
            command.AddParamWithValue(DbType.AnsiString, "@contact_email", _Bank.ContactEmail);
            command.AddParamWithValue(DbType.AnsiString, "@contact_mobile", _Bank.ContactMobile);
            command.AddParamWithValue(DbType.AnsiString, "@contact_name", _Bank.ContactName);
            command.AddParamWithValue(DbType.AnsiString, "@terminal_prefix", _Bank.TerminalPrefix);
            if (_Bank.Id > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@Id", _Bank.Id);
            }
        }
        public static void Update(Models.Bank _Bank)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("UPDATE tbl_setup_banks SET bank_name=@bank_name,");
            builder.Append("address_line_1=@address_line_1,address_line_2=@address_line_2,");
            builder.Append("city=@city,state=@state,country=@country,contact_address=@contact_address,");
            builder.Append("contact_email=@contact_email,contact_mobile=@contact_mobile,");
            builder.Append("contact_name=@contact_name,terminal_prefix=@terminal_prefix");
            builder.Append(" WHERE Id= @Id");

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

                SetBankParameter(command, _Bank);
                int returnCode = command.ExecuteNonQuery();
                if (returnCode <= 0)
                {
                    throw new UpdateException("Could not insert bank record");
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
        internal static List<Models.Bank> GetALLBanks(Models.User CurrentUser)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("SELECT a.* from tbl_setup_banks as a where Id > 0");

            List<Models.Bank> Banks = null;
            IDbConnection conn = null;
            try
            {

                if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank))
                {
                    if (CurrentUser.IsDealer)
                    {
                        builder.Append(" and EXISTS(SELECT * from tbl_setup_dealers as c where c.Id=@Id and c.bank_id = a.Id )");
                    }
                    else
                    {
                        builder.Append(" and EXISTS(SELECT * from tbl_setup_agent as b left join tbl_setup_dealers as c on b.dealer_id = c.Id where b.Id =@Id and c.bank_id = a.Id");
                    }
                }
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();


                if (!(CurrentUser.IsSuperUserOrAdmin || CurrentUser.IsEtop || CurrentUser.isBank))
                {
                   if (CurrentUser.IsDealer)
                    {
                        command.AddParamWithValue(DbType.Int64, "@Id", CurrentUser.DealerID);
                    }
                    else
                    {
                        command.AddParamWithValue(DbType.Int64, "@Id", CurrentUser.AgentID);
                    }
                }



                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Banks = GetBanks(rs);



            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return Banks;
        }

        public static List<Models.Bank> GetALLBanks()
        {
            List<Models.Bank> Banks = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT * from tbl_setup_banks";

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Banks = GetBanks(rs);



            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return Banks;
        }

        private static List<Models.Bank> GetBanks(IDataReader rs)
        {
            List<Models.Bank> banks = new List<Models.Bank>();
            try
            {
                while (rs.Read())
                {
                    Models.Bank bank = new Models.Bank();

                    bank.Id = Convert.ToInt64(rs["Id"]);
                    bank.BankName = DataHelper.GetValue<String>(rs["bank_name"]);
                    bank.AddressLine1 = DataHelper.GetValue<String>(rs["address_line_1"]);
                    bank.AddressLine2 = DataHelper.GetValue<String>(rs["address_line_2"]);
                    bank.City = DataHelper.GetValue<String>(rs["city"]);
                    bank.State = DataHelper.GetValue<String>(rs["state"]);
                    bank.Country = DataHelper.GetValue<String>(rs["country"]);
                    bank.ContactAddress = DataHelper.GetValue<String>(rs["contact_address"]);
                    bank.ContactEmail = DataHelper.GetValue<String>(rs["contact_email"]);
                    bank.ContactMobile = DataHelper.GetValue<String>(rs["contact_mobile"]);
                    bank.ContactName = DataHelper.GetValue<String>(rs["contact_name"]);
                    bank.TerminalPrefix = DataHelper.GetValue<String>(rs["terminal_prefix"]);
                    banks.Add(bank);
                }
                rs.Close();
            }
            catch (Exception ex)
            {
            }
            return banks;
        }
        public static Models.Bank GetBanksByID(long BankID)
        {

            IDbConnection conn = null;
            List<Models.Bank> Banks = null;
            Models.Bank bank = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();


                command.CommandText = "SELECT * from tbl_setup_banks where Id = @Id";
                command.AddParamWithValue(DbType.Int64, "@Id", BankID);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Banks = GetBanks(rs);
              

                if (Banks != null && Banks.Count > 0)
                {
                    bank =  Banks[0];
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return bank;
        }
        public static List<Models.Bank> GetAllBanks(Models.GeneralSearch generalSearch)
        {
            List<Models.Bank> Banks = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("SELECT * from tbl_setup_banks");
                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Bank Name":
                        builder.Append(" WHERE bank_name REGEXP @bank_name");
                        break;
                    case "ContactName":
                        builder.Append(" WHERE contact_name REGEXP @contact_name");
                        break;
                    case "Contact Mobile":
                        builder.Append(" WHERE contact_mobile REGEXP @contact_mobile");
                        break;
                }

                IDbCommand commandCount = conn.CreateCommand();
                commandCount.CommandText = builder.ToString().Replace("*", "count(*) as ItemCount");



                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();



                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Bank Name":
                        command.AddParamWithValue(DbType.AnsiString, "@bank_name", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@bank_name", generalSearch.SearchValue);
                        break;
                    case "ContactName":
                        command.AddParamWithValue(DbType.AnsiString, "@contact_name", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@contact_name", generalSearch.SearchValue);
                        break;
                    case "Contact Mobile":
                        command.AddParamWithValue(DbType.AnsiString, "@contact_mobile", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@contact_mobile", generalSearch.SearchValue);
                        break;
                }


                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Banks = GetBanks(rs);

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
            return Banks;

        }


        internal static Models.Bank GetBanksByDealerID(int p)
        {
            throw new NotImplementedException();
        }

        internal static List<Models.Bank> GetBankByUser(Models.User oUser)
        {
            List<Models.Bank> Banks = null;
            IDbConnection conn = null;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            try
            {
                builder.Append("select distinct a.* from tbl_setup_banks as a left join tbl_setup_dealers as b on a.Id = b.bank_id");
                builder.Append(" left join tbl_setup_agent as c on b.Id = c.dealer_id");
                if (!(oUser.IsSuperUserOrAdmin || oUser.IsEtop))
                {
                    if (oUser.isBank)
                    {
                        builder.Append(" where a.Id = @bank_id");
                    }
                    else if (oUser.IsDealer)
                    {
                        builder.Append(" where b.Id = @DealerID");
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
                Banks = GetBanks(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }

            return Banks;
        }

       
    }
}
