using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Processor
{
    public class ClientMerchantInformationService
    {


        public static List<Models.ClientBankInformation> GetList(GeneralSearch generalSearch)
        {
            IDictionary<String,String> parameters = new Dictionary<String,String>();
            List<Models.ClientBankInformation> clients = new List<Models.ClientBankInformation>();
            StringBuilder builder = new StringBuilder();
            builder.Append("select * from tbl_setup_client_mapping");
            try
            {
                switch (generalSearch.SearchCriteria)
                {
                    case "ALL":
                        break;
                    case "Client Id":
                        parameters.Add("@client_id", generalSearch.SearchValue);
                        builder.Append(" WHERE client_id REGEXP @client_id");
                        break;
                    case "Client Name":
                        parameters.Add("@client_name", generalSearch.SearchValue);
                        builder.Append(" WHERE client_name REGEXP @client_name");
                        break;
                   
                }
                using (var conn = ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    var command = conn.CreateCommand();
                    command.CommandText = builder.ToString();
                    foreach(var items in parameters)
                    {
                        command.AddParamWithValue(DbType.String, items.Key,items.Value);

                    }
                    using (var rs = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (rs.Read())
                        {
                            Models.ClientBankInformation clientBankInformation = new Models.ClientBankInformation();

                            clientBankInformation.Id = Convert.ToInt32(rs["id"]);
                            clientBankInformation.Name = rs["client_name"].ToString();
                            clientBankInformation.ClientId = rs["client_id"].ToString();
                            clientBankInformation.SecretKey = rs["client_secret"].ToString();
                            clients.Add(clientBankInformation);
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return clients;
          
        }
        public static bool SaveChanges(Models.ClientBankInformation clientInformation)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                if (clientInformation.Id == 0)
                {

                    builder.Append("Insert into tbl_setup_client_mapping(client_name,client_id,client_secret)");
                    builder.Append("values(@client_name,@client_id,@client_secret)");

                }
                else
                {
                    
                    builder.Append("update tbl_setup_client_mapping set client_name=@client_name,");
                    builder.Append("client_id = @client_id,client_secret = @client_secret where id = @id");

                    
                }

                using (var conn = ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    var command = conn.CreateCommand();
                    command.CommandText = builder.ToString();
                    command.AddParamWithValue(DbType.AnsiString, "@client_name", clientInformation.Name);
                    command.AddParamWithValue(DbType.AnsiString, "@client_id", clientInformation.ClientId);
                    command.AddParamWithValue(DbType.AnsiString, "@client_secret", clientInformation.SecretKey);
                    if(clientInformation.Id > 0)
                    {
                        command.AddParamWithValue(DbType.Int32, "@id", clientInformation.Id);
                    }
                    int rowCount = command.ExecuteNonQuery();
                    return rowCount > 0;
                }
                   
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

        internal static ClientBankInformation GetClientById(long id)
        {
            Models.ClientBankInformation clientBankInformation = null;
            try
            {
                using (var conn = ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    var command = conn.CreateCommand();
                    command.CommandText = "select * from tbl_setup_client_mapping where id=@id";
                    command.AddParamWithValue(DbType.AnsiString, "@id", id);
                    using (var rs = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (rs.Read())
                        {
                            clientBankInformation = new Models.ClientBankInformation();

                            clientBankInformation.Id = Convert.ToInt32(rs["id"]);
                            clientBankInformation.Name = rs["client_name"].ToString();
                            clientBankInformation.ClientId = rs["client_id"].ToString();
                            clientBankInformation.SecretKey = rs["client_secret"].ToString();
                          
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return clientBankInformation;
        }
    }
}