using System;
using System.Collections.Generic;
using System.Text;
using WalletReport.DBConnector;
using System.Data;
using System.Web;
using WalletReport.Models;
using com.ujc.StringHelper;

namespace WalletReport.Processor
{
    public class TerminalProcessor
    {

        public static void Create(Models.TerminalAssignment Terminal, ref String ErrorMessage)
        {
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("INSERT INTO tbl_terminal_wal_map(terminal_id,acctnumber,debit_limit,ignore_limit,isactive,dealer_id,agent_id)");
                builder.Append("VALUES(@terminal_id,@acctnumber,@debit_limit,@ignore_limit,@isactive,@dealer_id,@agent_id)");

                command.CommandText = builder.ToString();
                SetBankParameter(command, Terminal);
                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", Terminal.TerminalID);
                int returnCode = command.ExecuteNonQuery();


                if (returnCode <= 0)
                {
                    throw new UpdateException("Could not insert Agent record");
                }
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

        public static bool DeActivateTerminal(string id)
        {
            IDbConnection conn = null;
            bool isUpdated = false;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("update tbl_terminal_wal_map set isactive = 0 where terminal_id =@terminal_id");

                command.CommandText = builder.ToString();
                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", id);
                isUpdated = command.ExecuteNonQuery() > 0;
            }
            catch (OpenDBException ex)
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
            return isUpdated;
        }

        internal static bool ActivateTerminal(string id)
        {
            IDbConnection conn = null;
            bool isUpdated = false;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("update tbl_terminal_wal_map set isactive = 1 where terminal_id =@terminal_id");

                command.CommandText = builder.ToString();
                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", id);
                isUpdated = command.ExecuteNonQuery() > 0;
            }
            catch (OpenDBException ex)
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
            return isUpdated;
        }

        private static void SetBankParameter(IDbCommand command, Models.TerminalAssignment Terminal)
        {


            Models.Dealers dealer = DealersProcessor.GetDealersByID(Terminal.DealerID);

            
            command.AddParamWithValue(DbType.AnsiString, "@acctnumber", dealer.AccountNumber);
            command.AddParamWithValue(DbType.Decimal, "@debit_limit", Terminal.DebitLimit);
            command.AddParamWithValue(DbType.Boolean, "@ignore_limit", Terminal.IgnoreLimit);
            command.AddParamWithValue(DbType.Boolean, "@isactive", Terminal.IsActive);
            command.AddParamWithValue(DbType.Int64, "@dealer_id", Terminal.DealerID);
            if (Terminal.AgentID > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@agent_id", Terminal.AgentID);
            }
            else
            {
                command.AddParamWithValue(DbType.Int64, "@agent_id", DBNull.Value);
            }
            

        }

        internal static bool HasTerminalPIN(string id)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("select count(*) from tbl_terminal_pin where terminal_id = @terminal_id");

            bool hasPIN = false;


            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

               
                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", id);

                int returnCode = Convert.ToInt32( command.ExecuteScalar());
                if (returnCode > 0)
                {
                    hasPIN = true;
                }
            }
            catch (UpdateException ex)
            {
                
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return hasPIN;
        }

        public static bool CreateTerminalPIN(TerminalPIN terminalPIN)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("insert into tbl_terminal_pin(terminal_id,pin)values(@terminal_id,@pin)");

            bool hasUpdated = false;


            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();


                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", terminalPIN.TerminalID);
                command.AddParamWithValue(DbType.AnsiString, "@pin",  terminalPIN.Pin.Hash(HashType.SHA512,Encoding.ASCII));

                int returnCode = command.ExecuteNonQuery();
                if (returnCode > 0)
                {
                    hasUpdated = true;
                }
            }
            catch (UpdateException ex)
            {

            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return hasUpdated;
        }

        internal static bool UpdateTerminalPIN(TerminalPIN terminalPIN)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("update tbl_terminal_pin set pin=@pin where terminal_id=@terminal_id");

            bool hasUpdated = false;


            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();


                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", terminalPIN.TerminalID);
                command.AddParamWithValue(DbType.AnsiString, "@pin", terminalPIN.Pin.Hash(HashType.SHA512, Encoding.ASCII));

                int returnCode = command.ExecuteNonQuery();
                if (returnCode > 0)
                {
                    hasUpdated = true;
                }
            }
            catch (UpdateException ex)
            {

            }
            catch (Exception ex)
            {

            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return hasUpdated;
        }

        public static void Update(Models.TerminalAssignment Terminal)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("UPDATE  tbl_terminal_wal_map SET acctnumber = @acctnumber,debit_limit=@debit_limit,");
            builder.Append("ignore_limit=@ignore_limit,isactive = @isactive,dealer_id=@dealer_id,agent_id=@agent_id");
            builder.Append(" where terminal_id = @terminal_id");




            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

                SetBankParameter(command, Terminal);
                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", Terminal.OldTerminalID);

                int returnCode = command.ExecuteNonQuery();
                if (returnCode <= 0)
                {
                    throw new UpdateException("Failed to Update record");
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
        public static List<Models.TerminalAssignmentVM> GetALLAGent()
        {
            List<Models.TerminalAssignmentVM> TerminalList = null;
            IDbConnection conn = null;

            System.Text.StringBuilder Builder = new StringBuilder();
            Builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_terminal_wal_map as a left join tbl_setup_dealers as b on a.dealer_id = b.Id");
            Builder.Append(" left join tbl_setup_agent as c on a.agent_id = c.id");
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = Builder.ToString();

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                TerminalList = GetTerminal(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return TerminalList;
        }

        private static List<Models.TerminalAssignmentVM> GetTerminal(IDataReader rs)
        {
            List<Models.TerminalAssignmentVM> Terminals = new List<Models.TerminalAssignmentVM>();
            try
            {
                while (rs.Read())
                {
                    Models.TerminalAssignmentVM _Terminal = new Models.TerminalAssignmentVM();
                    _Terminal.TerminalID = DataHelper.GetValue<String>(rs["terminal_id"]);
                    _Terminal.AccountNumber = DataHelper.GetValue<String>(rs["acctnumber"]);
                    _Terminal.DebitLimit = DataHelper.GetValue<Decimal>(rs["debit_limit"]);
                    _Terminal.IgnoreLimit = DataHelper.GetValue<Boolean>(rs["ignore_limit"]);
                    _Terminal.IsActive = DataHelper.GetValue<Boolean>(rs["isactive"]);
                    _Terminal.DealerID = DataHelper.GetValue<Int64>(rs["dealer_id"]);
                    _Terminal.AgentID = DataHelper.GetValue<Int64>(rs["agent_id"]);
                    _Terminal.DealerName = DataHelper.GetValue<String>(rs["dealer_name"]);
                    _Terminal.AgentName = DataHelper.GetValue<String>(rs["agent_name"]);
                    Terminals.Add(_Terminal);
                }
                rs.Close();
            }
            catch (Exception ex)
            {
            }
            return Terminals;
        }
        public static Models.TerminalAssignment GetTerminalBYID(String TerminalID)
        {

            System.Text.StringBuilder Builder = new StringBuilder();
            Builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_terminal_wal_map as a left join tbl_setup_dealers as b on a.dealer_id = b.Id");
            Builder.Append(" left join tbl_setup_agent as c on a.agent_id = c.id where a.terminal_id = @terminal_id");

            IDbConnection conn = null;
            Models.TerminalAssignment Terminal = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();


                command.CommandText = Builder.ToString();
                command.AddParamWithValue(DbType.AnsiString, "@terminal_id", TerminalID);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                List<Models.TerminalAssignmentVM> terminals = GetTerminal(rs);


                if (terminals != null && terminals.Count > 0)
                {
                    Terminal = (Models.TerminalAssignment)terminals[0];
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return Terminal;
        }
        public static List<Models.TerminalAssignmentVM> GetAllTerminal(Models.GeneralSearch generalSearch)
        {
            User CurrentUser = HttpContext.Current.Session["CurrentUser"] as User;
            System.Text.StringBuilder builder = new StringBuilder();
            builder.Append("select a.*,b.dealer_name,c.agent_name from tbl_terminal_wal_map as a left join tbl_setup_dealers as b on a.dealer_id = b.Id");
            builder.Append(" left join tbl_setup_agent as c on a.agent_id = c.id ");

            if(CurrentUser.IsDealer)
            {
                builder.Append(String.Format(" WHERE b.Id = {0}", CurrentUser.DealerID));
            }
            else if(CurrentUser.IsAgent)
            {
                builder.Append(String.Format(" WHERE c.id = {0}", CurrentUser.AgentID));
            }
            else
            {
                builder.Append(" WHERE  terminal_id is not null ");
            }
            List<Models.TerminalAssignmentVM> Terminals = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Agent Name":
                        builder.Append(" AND c.agent_name REGEXP @agent_name");
                        break;
                    case "Dealer Name":
                        builder.Append(" AND b.dealer_name REGEXP @dealer_name");
                        break;
                    case "Terminal ID":
                        builder.Append(" AND a.terminal_id REGEXP @terminal_id");
                        break;
                }

                IDbCommand commandCount = conn.CreateCommand();
                commandCount.CommandText = builder.ToString().Replace("a.*,b.dealer_name,c.agent_name from tbl_terminal_wal_map", "count(*) as ItemCount");



                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();



                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Agent Name":
                        command.AddParamWithValue(DbType.AnsiString, "@agent_name", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@agent_name", generalSearch.SearchValue);
                        break;
                    case "Dealer Name":
                        command.AddParamWithValue(DbType.AnsiString, "@dealer_name", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@dealer_name", generalSearch.SearchValue);
                        break;
                    case "Terminal ID":
                        command.AddParamWithValue(DbType.AnsiString, "@terminal_id", generalSearch.SearchValue);
                        commandCount.AddParamWithValue(DbType.AnsiString, "@terminal_id", generalSearch.SearchValue);
                        break;
                }


                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Terminals = GetTerminal(rs);

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
            return Terminals;

        }
    }
}
