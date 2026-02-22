using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Processor
{
    public class PermissionProcessor
    {


        public List<Models.Permission> GetPermission(IDataReader rs)
        {
            List<Models.Permission> permissions = new List<Permission>();
            while (rs.Read())
            {
                try
                {
                    Permission p = new Permission();
                    p.Name = DataHelper.GetValue<String>(rs["PermissionName"]);
                    p.Description = DataHelper.GetValue<String>(rs["Description"]);
                    p.Id = Convert.ToInt32(rs["Id"]);
                    permissions.Add(p);
                }
                catch (Exception ex)
                {
                }
            }
            rs.Close();

            return permissions;
        }
        public List<Models.Permission> GetPermission()
        {
            List<Models.Permission> Permissions = null;
            try
            {
                using (IDbConnection conn = WalletReport.DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_SETUP_PERMISSION";

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    Permissions = GetPermission(rs);
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
            return Permissions;

        }
        public static void Create(Models.Permission permission)
        {
            try
            {
                using (IDbConnection conn = WalletReport.DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "INSERT INTO tbl_SETUP_PERMISSION(PermissionName,Description)values(@Name,@Description)";
                    command.AddParamWithValue(DbType.AnsiString, "@Name", permission.Name);
                    command.AddParamWithValue(DbType.AnsiString, "@Description", permission.Description);
                    int RowCountEffected = command.ExecuteNonQuery();

                    if (RowCountEffected <= 0)
                    {
                        throw new UpdateException("COULD NOT INSERT PERMISSION");
                    }
                }
            }
            catch (UpdateException ex)
            {
                throw ex;
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