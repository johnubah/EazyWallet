using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using WalletReport.DBConnector;
using WalletReport.Models;

namespace WalletReport.Processor
{
    public class RolePermissionProcessor
    {
        public static void Create(RolePermission permission)
        {
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();

                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "INSERT INTO tbl_ROLE_PERMISSION(PermissionId,RoleId)values(@PermissionId,@RoleId)";
                    
                    command.AddParamWithValue(DbType.Int32, "@PermissionId", permission.PermissionId);
                    command.AddParamWithValue(DbType.Int32, "@RoleId", permission.RoleId);


                    int RowCountEffected = command.ExecuteNonQuery();

                    if (RowCountEffected <= 0)
                    {
                        throw new UpdateException("COULD NOT INSERT ROLE PERMISSION");
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