using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WalletReport.DBConnector;
using WalletReport.Models;
using System.Data;
using System.Text;

namespace WalletReport.Processor
{
    public class UserInRoleProcessor
    {
        public static void AddRoleToUser(Models.User oUser, Models.Role role)
        {
           // AddRoleToUser(oUser.Id, role.Id);
        }

        public static void RemoveUserFromRole(int UserID, int RoleID)
        {
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "DELETE FROM tbl_USER_IN_ROLE WHERE UserID = @UserID AND RoleID = @RoleID ";

                    command.AddParamWithValue(DbType.Int32, "@UserID", UserID);
                    command.AddParamWithValue(DbType.Int32, "@RoleID", RoleID);

                    int RowCountEffected = command.ExecuteNonQuery();
                    if (RowCountEffected <= 0)
                    {
                        throw new UpdateException("COULD NOT DELETE USER FROM ROLE");
                    }
                }
            }
            catch (OpenDBException ex)
            {
                throw ex;
            }
            catch (UpdateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsUserInRole(Int64 UserId, int RoleId)
        {
            bool UserIsInRole = false;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT count(*) from tbl_USER_IN_ROLE WHERE UserID = @UserID AND RoleID = @RoleID ";

                    command.AddParamWithValue(DbType.Int64, "@UserID", UserId);
                    command.AddParamWithValue(DbType.Int32, "@RoleID", RoleId);
                    object RowCount = command.ExecuteScalar();
                    if (RowCount != null && RowCount != DBNull.Value)
                    {
                        if (Convert.ToInt32(RowCount) > 0)
                        {
                            UserIsInRole = true;
                        }
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

            return UserIsInRole;
        }
        public static List<Role> GetRoleInUser(User ouser)
        {
            return GetRoleInUser(ouser.Id);
        }
        public static List<Models.User> GetUserInRole(Int64 roleID)
        {
            List<Models.User> Users = new List<User>();

            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();

                    StringBuilder builder = new StringBuilder();

                    builder.Append("SELECT b.* from tbl_USER_IN_ROLE a,tbl_SETUP_USER b");
                    builder.Append(" WHERE a.UserID = b.UserID AND a.RoleId=@RoleId");
                    command.AddParamWithValue(DbType.Int32, "@RoleId", roleID);
                    command.CommandText = builder.ToString();

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    Users = UserProcessor.GetUserList(rs);
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

            return Users;
        }
        public static List<Role> GetRoleInUser(Int64 UserID)
        {
            List<Models.Role> Roles = new List<Role>();

            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();

                    StringBuilder builder = new StringBuilder();

                    builder.Append("SELECT b.* from tbl_user_role a,tbl_setup_role b");
                    builder.Append(" WHERE a.RoleId = b.Id AND a.UserId=@UserId");
                    command.CommandText = builder.ToString();
                    command.AddParamWithValue(DbType.Int64, "@UserId", UserID);
                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    Roles = RoleProcessor.GetRoles(rs);
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


            return Roles;

        }

        private static void AddRoleToUser(int UserId, int RoleId)
        {
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "INSERT INTO tbl_USER_IN_ROLE(UserID,RoleID)values(@UserId,@RoleId)";
                    command.AddParamWithValue(DbType.Int32, "@UserId", UserId);
                    command.AddParamWithValue(DbType.Int32, "@RoleId", RoleId);
                    command.ExecuteNonQuery();

                    int RowCountEffected = command.ExecuteNonQuery();

                    if (RowCountEffected <= 0)
                    {
                        throw new UpdateException("COULD ADD ROLE TO USER PROFILE");
                    }

                }
            }
            catch (OpenDBException ex)
            {
                throw ex;
            }
            catch (UpdateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void AddRoleToUser(User oUser, string Rolename)
        {
            Role role = RoleProcessor.GetRoleByName(Rolename);
            if (role == null)
            {
                return;
            }

            List<Role> Roles = GetRoleInUser(oUser.Id);

            if (Roles != null && Roles.Count(c => c.Id == role.Id) > 0)
            {
                return;
            }
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command =  conn.CreateCommand();
                command.CommandText = "insert into tbl_user_role(UserId,RoleId)values(@UserId,@RoleId)";
                command.AddParamWithValue(DbType.Int64, "@UserId", oUser.Id);
                command.AddParamWithValue(DbType.Int64, "@RoleId", role.Id);
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

        internal static void DeleteUserRoleNotInRole(User oUser, string UserRole)
        {
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "delete from tbl_user_role where UserId = @UserId and RoleId  in(select Id from tbl_setup_role where rolename != @rolename)";
                command.AddParamWithValue(DbType.Int64, "@UserId", oUser.Id);
                command.AddParamWithValue(DbType.AnsiString, "@rolename", UserRole);
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