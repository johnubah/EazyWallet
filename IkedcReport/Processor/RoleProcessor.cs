using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using WalletReport.DBConnector;
using System.Text;

namespace WalletReport.Processor
{
    public class RoleProcessor
    {

        public static Models.Role GetRoleById(int Id)
        {
            Models.Role Role = null;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();

                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_SETUP_ROLE WHERE Id=@Id";
                    command.AddParamWithValue(DbType.Int32, "@Id", Id);

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    List<Models.Role> Roles = GetRoles(rs);
                    if (Roles.Count() > 0)
                    {
                        Role = Roles[0];
                    }
                }
            }
            catch (OpenDBException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Role;
        }
        public static Models.Role GetRoleByName(String RoleName)
        {
            Models.Role Role = null;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();

                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_setup_role WHERE rolename=@RoleName";
                    command.AddParamWithValue(DbType.AnsiString, "@RoleName", RoleName);
                    
                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    List<Models.Role> Roles = GetRoles(rs);
                    if (Roles.Count() > 0)
                    {
                        Role = Roles[0];
                    }
                }
            }
            catch (OpenDBException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Role;
        }
        public static List<Models.Role> GetAllRoles()
        {
            List<Models.Role> Roles = null;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();


                    IDbCommand command =  conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_SETUP_ROLE";


                    IDataReader rs =  command.ExecuteReader(CommandBehavior.CloseConnection);
                    Roles = GetRoles(rs);
                }
            }
            catch (Exception ex)
            {
            }
            return Roles;
        }

        public static List<Models.Role> GetRoles(IDataReader rs)
        {
            List<Models.Role> roles = new List<Models.Role>();
           
            while (rs.Read())
            {
                Models.Role role = new Models.Role();
                role.RoleName = DataHelper.GetValue<String>(rs["rolename"]);
                role.Description = DataHelper.GetValue<String>(rs["description"]);
                role.Id = Convert.ToInt32(rs["Id"]);


                roles.Add(role);
            }
            rs.Close();
            return roles;
        }
        public static void Create(Models.Role role)
        {
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();

                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "INSERT INTO tbl_SETUP_ROLE(RoleName,Description,DateCreated)values(@RoleName,@Description,@DateCreated)";

                    command.AddParamWithValue(DbType.AnsiString, "@RoleName", role.RoleName);
                    command.AddParamWithValue(DbType.AnsiString, "@Description", role.Description);
                    command.AddParamWithValue(DbType.DateTime, "@DateCreated", role.DateCreated);

                    int RowCountEffected = command.ExecuteNonQuery();

                    if (RowCountEffected <= 0)
                    {
                        throw new UpdateException("COULD NOT INSERT DISTRICT");
                    }
                }
            }
            catch (OpenDBException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void Update(Models.Role role)
        {
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();


                    StringBuilder builder = new StringBuilder();
                    builder.Append("UPDATE tbl_SETUP_ROLE set RoleName=@RoleName");
                    builder.Append(",Description=@Description");
                    builder.Append(",DateLastModified=@DateLastModified");
                    builder.Append(" WHERE Id=@Id");


                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = builder.ToString();

                    command.AddParamWithValue(DbType.AnsiString, "@RoleName", role.RoleName);
                    command.AddParamWithValue(DbType.AnsiString, "@Description", role.Description);
                    command.AddParamWithValue(DbType.DateTime, "@DateLastModified", role.DateLastModified);
                    command.AddParamWithValue(DbType.Int32, "@Id", role.Id);

                    int RowCountEffected = command.ExecuteNonQuery();

                    if (RowCountEffected <= 0)
                    {
                        throw new UpdateException("COULD NOT UPDATE ROLE");
                    }
                }
            }
            catch (OpenDBException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}