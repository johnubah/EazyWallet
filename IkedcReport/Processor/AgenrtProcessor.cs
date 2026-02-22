using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WalletReport.DBConnector;
using System.Data;

namespace WalletReport.Processor
{
    public class AgenrtProcessor
    {
        public static void Create(Models.AgentUser Agent, ref String ErrorMessage)
        {
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("Insert into tbl_setup_agent(agent_name,address_line_1,address_line_2,city,");
                builder.Append("state,country,contact_address,contact_email,contact_mobile,");
                builder.Append("contact_name,dealer_id,posting_mode");

                builder.Append(")");
                builder.Append("values(@agent_name,@address_line_1,@address_line_2,@city,");
                builder.Append("@state,@country,@contact_address,@contact_email,@contact_mobile,");
                builder.Append("@contact_name,@dealer_id,@posting_mode");
                builder.Append(")");

                command.CommandText = builder.ToString();
                SetBankParameter(command, Agent);
                int returnCode = command.ExecuteNonQuery();


                if (returnCode <= 0)
                {
                    throw new UpdateException("Could not insert Agent record");
                }
                command.Parameters.Clear();
                command.CommandText = "SELECT LAST_INSERT_ID()";
                object o = command.ExecuteScalar();
                Agent.Id = Convert.ToInt64(o);
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

        private static void SetBankParameter(IDbCommand command, Models.AgentUser Agent)
        {
            command.AddParamWithValue(DbType.AnsiString, "@agent_name", Agent.AgenName);
            command.AddParamWithValue(DbType.AnsiString, "@address_line_1", Agent.AddressLine1);
            command.AddParamWithValue(DbType.AnsiString, "@address_line_2", Agent.AddressLine2);
            command.AddParamWithValue(DbType.AnsiString, "@city", Agent.City);
            command.AddParamWithValue(DbType.AnsiString, "@state", Agent.State);
            command.AddParamWithValue(DbType.AnsiString, "@country", Agent.Country);
            command.AddParamWithValue(DbType.AnsiString, "@contact_address", Agent.ContactAddress);
            command.AddParamWithValue(DbType.AnsiString, "@contact_email", Agent.ContactEmail);
            command.AddParamWithValue(DbType.AnsiString, "@contact_mobile", Agent.ContactMobile);
            command.AddParamWithValue(DbType.AnsiString, "@contact_name", Agent.ContactName);
            command.AddParamWithValue(DbType.Int64, "@dealer_id", Agent.DealerId);
            command.AddParamWithValue(DbType.AnsiString, "@posting_mode", Agent.PostingMode);
            
            if (Agent.Id > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@Id", Agent.Id);
            }
        }
        public static void Update(Models.AgentUser Agent)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("UPDATE tbl_setup_agent SET agent_name=@agent_name,");
            builder.Append("address_line_1=@address_line_1,address_line_2=@address_line_2,");
            builder.Append("city=@city,state=@state,country=@country,contact_address=@contact_address,");
            builder.Append("contact_email=@contact_email,contact_mobile=@contact_mobile,");
            builder.Append("contact_name=@contact_name,dealer_id=@dealer_id,posting_mode=@posting_mode");
            builder.Append(" WHERE Id= @Id");

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

                SetBankParameter(command, Agent);
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
        public static List<Models.AgentUser> GetALLAGent()
        {
            List<Models.AgentUser> Agents = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT * from tbl_setup_agent";

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Agents = GetAgents(rs);



            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return Agents;
        }

        private static List<Models.AgentUser> GetAgents(IDataReader rs)
        {
            List<Models.AgentUser> Agents = new List<Models.AgentUser>();
            try
            {
                while (rs.Read())
                {
                    Models.AgentUser Agent = new Models.AgentUser();
                    Agent.Id = Convert.ToInt64(rs["Id"]);
                    Agent.AgenName = DataHelper.GetValue<String>(rs["agent_name"]);
                    Agent.AddressLine1 = DataHelper.GetValue<String>(rs["address_line_1"]);
                    Agent.AddressLine2 = DataHelper.GetValue<String>(rs["address_line_2"]);
                    Agent.City = DataHelper.GetValue<String>(rs["city"]);
                    Agent.State = DataHelper.GetValue<String>(rs["state"]);
                    Agent.Country = DataHelper.GetValue<String>(rs["country"]);
                    Agent.ContactAddress = DataHelper.GetValue<String>(rs["contact_address"]);
                    Agent.ContactEmail = DataHelper.GetValue<String>(rs["contact_email"]);
                    Agent.ContactMobile = DataHelper.GetValue<String>(rs["contact_mobile"]);
                    Agent.ContactName = DataHelper.GetValue<String>(rs["contact_name"]);
                    Agent.DealerId = DataHelper.GetValue<long>(rs["dealer_id"]);
                    Agent.PostingMode = DataHelper.GetValue<String>(rs["posting_mode"]);
                    Agent.Balance = DataHelper.GetValue<decimal>(rs["balance"]);
                    Agent.AccountNo = DataHelper.GetValue<String>(rs["acct_no"]);
                    Agent.UBAAccountNo = DataHelper.GetValue<String>(rs["uba_acct_no"]);

                    Agents.Add(Agent);
                }
                rs.Close();
            }
            catch (Exception ex)
            {
            }
            return Agents;
        }
        public static Models.AgentUser GetAgentBYID(long AgentID)
        {

            IDbConnection conn = null;
            List<Models.AgentUser> Agents = null;
            Models.AgentUser Agent = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();


                command.CommandText = "SELECT * from tbl_setup_agent where Id = @Id";
                command.AddParamWithValue(DbType.Int64, "@Id", AgentID);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Agents = GetAgents(rs);


                if (Agents != null && Agents.Count > 0)
                {
                    Agent = Agents[0];
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return Agent;
        }
        public static List<Models.AgentUser> GetAllAgent(Models.GeneralSearch generalSearch)
        {
            List<Models.AgentUser> Agents = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("SELECT a.* from tbl_setup_agent as a");


                Models.User currentUser = System.Web.HttpContext.Current.Session["CurrentUser"] as Models.User;


                if (currentUser == null)
                {
                    return new List<Models.AgentUser>();
                }
                if (currentUser.IsSuperUserOrAdmin || currentUser.IsEtop || currentUser.isBank)
                {
                    builder.Append(" WHERE a.Id > 0");
                }
                else if (currentUser.IsAgent)
                {
                    builder.Append(" WHERE a.Id = @Id");
                }
                else if (currentUser.IsDealer)
                {
                    builder.Append(" WHERE a.dealer_id = @dealer_id");
                }
                
                else
                {
                    return new List<Models.AgentUser>();
                }
                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Agent Name":
                        builder.Append(" AND agent_name REGEXP @agent_name");
                        break;
                    case "ContactName":
                        builder.Append(" AND contact_name REGEXP @contact_name");
                        break;
                    case "Contact Mobile":
                        builder.Append(" AND contact_mobile REGEXP @contact_mobile");
                        break;
                }

                IDbCommand commandCount = conn.CreateCommand();
                commandCount.CommandText = builder.ToString().Replace("a.*", "count(*) as ItemCount");



                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

                if (currentUser.IsAgent)
                {
                    command.AddParamWithValue(DbType.Int64, "@Id", currentUser.AgentID);
                }
                else if (currentUser.IsDealer)
                {
                    command.AddParamWithValue(DbType.Int64, "@dealer_id", currentUser.DealerID);
                }
                else if (currentUser.isBank)
                {
                    command.AddParamWithValue(DbType.Int64, "@dealer_id", currentUser.BankID);
                }



                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Agent Name":
                        command.AddParamWithValue(DbType.AnsiString, "@agent_name", generalSearch.SearchValue);
                        break;
                    case "ContactName":
                        command.AddParamWithValue(DbType.AnsiString, "@contact_name", generalSearch.SearchValue);
                        break;
                    case "Contact Mobile":
                        command.AddParamWithValue(DbType.AnsiString, "@contact_mobile", generalSearch.SearchValue);
                        break;
                }


                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                Agents = GetAgents(rs);

                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        foreach (IDataParameter param in command.Parameters)
                        {
                            commandCount.Parameters.Add(param);
                        }
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
            return Agents;

        }

        public static List<Models.AgentUser> GetAgentByUser(Models.User oUser)
        {

            List<Models.AgentUser> AgentUsers = null;
            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT a.* from tbl_setup_agent as a left join tbl_setup_dealers b on a.dealer_id = b.Id");
            IDbConnection conn = null;
            try
            {


                if (!(oUser.IsSuperUserOrAdmin || oUser.IsEtop))
                {
                    if (oUser.isBank)
                    {
                        builder.Append(" where b.bank_id = @bank_id");
                    }
                    else if (oUser.IsDealer)
                    {
                        builder.Append(" where a.dealer_id = @DealerID");
                    }
                    else if (oUser.IsAgent)
                    {
                        builder.Append(" where a.Id = @agent_id");
                    }
                }
                conn = ConnectionManager.GetConnection();
                conn.Open();

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

                AgentUsers = GetAgents(rs);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return AgentUsers;
        }

        public static List<Models.AgentUser> GetFullAgentByDealer(long Id)
        {
            List<Models.AgentUser> agentVM = new List<Models.AgentUser>();
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "select Id,agent_name from tbl_setup_agent where dealer_id = @dealer_id";
                command.AddParamWithValue(DbType.Int64, "@dealer_id", Id);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

                agentVM = GetAgents(rs);


            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            if (agentVM == null)
                agentVM = new List<Models.AgentUser>();
            return agentVM;
        }
        internal static List<Models.AgentDisplayVM> GetAgentByDealer(long Id)
        {
            List<Models.AgentDisplayVM> agentVM = new List<Models.AgentDisplayVM>();
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "select Id,agent_name from tbl_setup_agent where dealer_id = @dealer_id";
                command.AddParamWithValue(DbType.Int64, "@dealer_id", Id);

                IDataReader rs =  command.ExecuteReader(CommandBehavior.CloseConnection);

                while (rs.Read())
                {
                    Models.AgentDisplayVM vm = new Models.AgentDisplayVM();
                    vm.AgenName = rs["agent_name"].ToString();
                    vm.Id = Convert.ToInt64(rs["Id"]);
                    agentVM.Add(vm);
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
            return agentVM;
        }
    }
}
