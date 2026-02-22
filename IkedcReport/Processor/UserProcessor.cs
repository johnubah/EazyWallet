using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WalletReport.Models;
using WalletReport.DBConnector;
using System.Data;
using System.Text;
using com.ujc.StringHelper;
using System.Net.Mail;
using System.Threading;

namespace WalletReport.Processor
{
    public class UserProcessor
    {

        private static String[] alphabet = new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private static String[] Numbers = new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        private static Random Rndalphabet = new Random();
        private static Random RndNumbers = new Random();


        public static String GetRandomPassword()
        {

            StringBuilder builder = new StringBuilder();

            while (builder.ToString().Length <= 5)
            {
                int count = Rndalphabet.Next(25);
                builder.Append(alphabet[count]);
            }
            while (builder.ToString().Length <= 8)
            {
                int count = RndNumbers.Next(9);
                builder.Append(Numbers[count]);
            }

            return builder.ToString();
        }
        public static User AuthenticateUser(String Username, String Password,ref String ErrorMessage)
        {
            User oUser = null;

            if (IsValidateUser(Username, Password, ref ErrorMessage))
            {
                oUser = GetUserByUserName(Username);
            }

            return oUser;

        }

        private static bool IsValidateUser(string Usernaname, string Password, ref String ErrorMessage)
        {
            bool isValidated = false;
            try
            {
               
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();

                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_setup_user where Username = @Username";

                    command.AddParamWithValue(DbType.AnsiString, "@Username", Usernaname);

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    String _StoredPassword = String.Empty;
                    bool IsLockOut = false;
                    bool Isfound = false;
                    bool ActiveStatus = false;
                    Nullable<DateTime> ExpiryDate = null;

                    if (rs.Read())
                    {
                        Isfound = true;
                        _StoredPassword = DataHelper.GetValue<String>(rs["password"]);
                        IsLockOut = DataHelper.GetValue<bool>(rs["IsLockOut"]);
                        ActiveStatus = DataHelper.GetValue<bool>(rs["activeStatus"]);
                        if (rs["ExpiryDate"] != DBNull.Value)
                        {
                            ExpiryDate = Convert.ToDateTime(rs["ExpiryDate"]);
                        }
                    }
                    rs.Close();

                    if (Isfound)
                    {
                        if (IsLockOut)
                        {
                            ErrorMessage = String.Format("{0} is Lock", Usernaname);
                            throw new UserLockOutException(ErrorMessage);
                        }
                        if (!ActiveStatus)
                        {
                            ErrorMessage = String.Format("{0} is not activate", Usernaname);
                            throw new UserLockOutException(ErrorMessage);
                        }


                        if (Password.Hash(HashType.SHA512, Encoding.ASCII) == _StoredPassword)
                        {
                            isValidated = true;

                        }
                        else
                        {
                            ErrorMessage = "Wrong credential supplied. ";
                            isValidated = false;
                        }
                    }
                    else
                    {
                        ErrorMessage = "Wrong credential supplied. ";
                        isValidated = false;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isValidated;
        }
        public static List<User> GetUsers()
        {
            List<User> UserList = null;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_setup_user";

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    UserList = GetUserList(rs);
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
            return UserList;

        }
        public static User GetUserByUserByID(int Id)
        {
            User oUser = null;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_SETUP_USER where Id = @Id";
                    command.AddParamWithValue(DbType.Int32, "@Id", Id);

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

                    List<User> UserList = GetUserList(rs);
                    if (UserList.Count() > 0)
                    {
                        oUser = UserList[0];
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

            return oUser;
        }
        public static User GetUserByUserName(String Username)
        {
            User oUser = null;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_SETUP_USER where Username = @Username";
                    command.AddParamWithValue(DbType.AnsiString, "@Username", Username);

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

                    List<User> UserList = GetUserList(rs);
                    if (UserList.Count() > 0)
                    {
                        oUser = UserList[0];
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

            return oUser;
        }

        public static List<User> GetUserList(IDataReader rs)
        {

  //`Othernames` varchar(80) DEFAULT NULL,
  //`DateCreated` datetime NOT NULL,
  //`DateModified` datetime DEFAULT NULL,
  //`CreatedByUserID` int(11) DEFAULT NULL,
  //`IsLockOut` bit(1) DEFAULT NULL,
  //`Password` varchar(250) DEFAULT NULL,
  //`ExpiryDate` datetime DEFAULT NULL,
  //`gender` varchar(5) DEFAULT NULL,



            List<User> userList = new List<User>();

            while (rs.Read())
            {
                try
                {
                    User oUser = new User();

                    oUser.Username = DataHelper.GetValue<String>(rs["Username"]);
                    oUser.UserNote = DataHelper.GetValue<String>(rs["UserNote"]);
                    oUser.Firstname = DataHelper.GetValue<String>(rs["Firstname"]);
                    oUser.LastName = DataHelper.GetValue<String>(rs["LastName"]);
                    oUser.Othernames = DataHelper.GetValue<String>(rs["Othernames"]);
                    oUser.EmailAddress = DataHelper.GetValue<String>(rs["EmailAddress"]);
                    oUser.DisplayName = DataHelper.GetValue<String>(rs["DisplayName"]);
                    oUser.Mobile = DataHelper.GetValue<String>(rs["mobile"]);
                    oUser.Id = Convert.ToInt64(rs["Id"]);
                    oUser.DateCreated = DataHelper.GetValue<DateTime>(rs["DateCreated"]);
                    oUser.IsEtop = rs["ISEtop"] == DBNull.Value ? false : Convert.ToBoolean(rs["ISEtop"]);
                    oUser.IsAgent = rs["IsAgent"] == DBNull.Value ? false : Convert.ToBoolean(rs["IsAgent"]);
                    oUser.IsDealer = rs["IsDealer"] == DBNull.Value ? false : Convert.ToBoolean(rs["IsDealer"]);
                    if (rs["IsBank"] != DBNull.Value)
                    {

                        String reference = rs["IsBank"].ToString();
                       // bool isbank = Convert.ToBoolean(rs["IsBank"]);
                        oUser.isBank = Convert.ToBoolean(rs["IsBank"]);

                    }
                   
                    
                    if (rs["user_type_id"] != DBNull.Value)
                    {
                        oUser.UserTypeID = DataHelper.GetValue<Int32>(rs["user_type_id"]);
                    }
                    oUser.Country = DataHelper.GetValue<String>(rs["country"]);
                    oUser.City = DataHelper.GetValue<String>(rs["city"]);
                    if(rs["dealer_id"] != DBNull.Value)
                    {
                        oUser.DealerID = Convert.ToInt32(rs["dealer_id"]);  
                    }
                    if (rs["bank_id"] != DBNull.Value)
                    {
                        oUser.BankID = Convert.ToInt32(rs["bank_id"]);
                    }
                    if (rs["agent_id"] != DBNull.Value)
                    {
                        oUser.AgentID = Convert.ToInt32(rs["agent_id"]);
                    }
                    if (rs["force_passw_change"] != DBNull.Value)
                    {
                        oUser.ForceChangePassword = Convert.ToBoolean(rs["force_passw_change"]);
                    }
                  

                    oUser.ActiveStatus =  DataHelper.GetValue<bool>(rs["activestatus"]);
                    oUser.IsActive = oUser.ActiveStatus;
                    try { oUser.AllowTerminalConfig = rs["allow_terminal_config"] == DBNull.Value ? String.Empty: rs["allow_terminal_config"].ToString(); } catch (Exception exp) { }
                    userList.Add(oUser);

                    }
                    catch (Exception ex)
                    {
                    }

            }
            rs.Close();

            return userList;
        }
        public static void Update(User oUser)
        {
            try
            {

                
                StringBuilder builder = new StringBuilder();
                builder.Append("update tbl_setup_user set ");
                builder.Append("Username=@Username,Firstname=@Firstname,Lastname=@Lastname,DisplayName=@DisplayName,");
                builder.Append("Othernames=@Othernames,EmailAddress=@EmailAddress,UserNote=@UserNote,");
                builder.Append("user_type_id=@user_type_id,dealer_id=@dealer_id,activestatus=@ActiveStatus,");
                builder.Append("city=@city,country=@country,IsBank=@IsBank,IsAgent=@IsAgent,IsDealer=@IsDealer,IsEtop=@IsEtop,bank_id=@bank_id,agent_id=@agent_id,mobile=@mobile,allow_terminal_config=@allow_terminal_config");
                builder.Append(" WHERE Id=@Id");

                String UserRole = String.Empty;

                switch (oUser.UserTypeID)
                {
                    case 1:
                        UserRole = "Bank";
                        break;
                    case 2:
                        UserRole = "Dealer";
                        break;
                    case 3:
                        UserRole = "Agent";

                        break;
                    case 4:
                        UserRole = "Etop";
                        break;
                }

                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();

                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = builder.ToString();

                    SetUserParameter(command, oUser);

                    int RowEffected = command.ExecuteNonQuery();

                    if (RowEffected <= 0)
                    {
                        throw new UpdateException("COULD NOT CREATE USER");
                    }

                    
                }
                Processor.UserInRoleProcessor.DeleteUserRoleNotInRole(oUser, UserRole);
                Processor.UserInRoleProcessor.AddRoleToUser(oUser, UserRole);
            }
            catch (UpdateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new UpdateException(ex.Message, ex);
            }
        }
        public static bool IsUserExists(String Username)
        {
            bool IsUserExist = false;
            IDbConnection conn = null;

            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT 1 from tbl_setup_user WHERE Username = @username";
                command.AddParamWithValue(DbType.AnsiString, "@username", Username);

                object o = command.ExecuteScalar();
                if (o != null)
                {
                    Int64 i = Convert.ToInt64(o);
                    IsUserExist = i > 0;
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
            return IsUserExist;
        }
        public static void SetUserParameter(IDbCommand command,User oUser)
        {
            //@IsAgent,@IsDealer,,@,@
            if (oUser.BankID > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@bank_id", oUser.BankID);
            }
            else
            {
                command.AddParamWithValue(DbType.Int64, "@bank_id", DBNull.Value);
            }
            if (oUser.AgentID > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@agent_id", oUser.AgentID);
            }
            else
            {
                command.AddParamWithValue(DbType.Int64, "@agent_id", DBNull.Value);
            }
            if (oUser.DealerID > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@dealer_id", oUser.DealerID);
            }
            else
            {
                command.AddParamWithValue(DbType.Int64, "@dealer_id", DBNull.Value);
            }
            //
            command.AddParamWithValue(DbType.Boolean, "@IsBank", (oUser.UserTypeID == 1));
            command.AddParamWithValue(DbType.Boolean, "@IsEtop", (oUser.UserTypeID == 4));
            command.AddParamWithValue(DbType.Boolean, "@IsDealer", (oUser.UserTypeID == 2));
            command.AddParamWithValue(DbType.Boolean, "@IsAgent", (oUser.UserTypeID == 3));
            command.AddParamWithValue(DbType.AnsiString, "@Username", oUser.Username);
            command.AddParamWithValue(DbType.AnsiString, "@Firstname", oUser.Firstname);
            command.AddParamWithValue(DbType.AnsiString, "@Lastname", oUser.LastName);
            command.AddParamWithValue(DbType.AnsiString, "@DisplayName", oUser.DisplayName);
            command.AddParamWithValue(DbType.AnsiString, "@Othernames", null);
            command.AddParamWithValue(DbType.AnsiString, "@EmailAddress", oUser.EmailAddress);
            command.AddParamWithValue(DbType.AnsiString, "@UserNote", null);
            command.AddParamWithValue(DbType.Boolean, "@ActiveStatus", oUser.ActiveStatus);
            command.AddParamWithValue(DbType.AnsiString, "@city", oUser.City);
            command.AddParamWithValue(DbType.AnsiString, "@country", oUser.Country);
            command.AddParamWithValue(DbType.Int32, "@user_type_id", oUser.UserTypeID);
            command.AddParamWithValue(DbType.AnsiString, "@mobile", oUser.Mobile);
            command.AddParamWithValue(DbType.AnsiString, "@allow_terminal_config", oUser.AllowTerminalConfig);
            //


            if (oUser.Id > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@Id", oUser.Id);
            }
            

           
              //          Id` bigint(20) NOT NULL AUTO_INCREMENT,
              //`DateCreated` datetime NOT NULL,
              //`DateModified` datetime DEFAULT NULL,
              //`CreatedByUserID` int(11) DEFAULT NULL,
              //`IsLockOut` bit(1) DEFAULT NULL,
              //`Password` varchar(250) DEFAULT NULL,
              //`ExpiryDate` datetime DEFAULT NULL,
              //`user_type_id` int(11) DEFAULT NULL,
              //`force_passw_change` bit(1) DEFAULT NULL,


              //`dealer_id` bigint(20) DEFAULT NULL,
              //`gender` varchar(5) DEFAULT NULL,
              //`city` varchar(50) DEFAULT NULL,
              //`country` varchar(60) DEFAULT NULL,



        }
        public  static void Create(User oUser)
        {
            IDbConnection conn = null;
            try
            {

                if (IsUserExists(oUser.Username))
                {
                    throw new UpdateException("This User already exists");
                }

                StringBuilder builder = new StringBuilder();
                builder.Append("insert into tbl_setup_user(");
                builder.Append("Username,Firstname,Lastname,DisplayName,");
                builder.Append("Othernames,EmailAddress,datecreated,UserNote,");
                builder.Append("CreatedByUserID,ActiveStatus,Password,");
                builder.Append("user_type_id,force_passw_change,dealer_id,");
                builder.Append("city,country,IsBank,IsAgent,IsDealer,IsEtop,bank_id,agent_id,mobile,allow_terminal_config");
                builder.Append(")");

                builder.Append("values(");
                builder.Append("@Username,@Firstname,@Lastname,@DisplayName,");
                builder.Append("@Othernames,@EmailAddress,@DateCreated,@UserNote,");
                builder.Append("@CreatedByUserID,@ActiveStatus,@Password,");
                builder.Append("@user_type_id,@force_passw_change,@dealer_id,");
                builder.Append("@city,@country,@IsBank,@IsAgent,@IsDealer,@IsEtop,@bank_id,@agent_id,@mobile,@allow_terminal_config");
                builder.Append(")");

                 conn = DBConnector.ConnectionManager.GetConnection();
                
                 conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();
                oUser.ActiveStatus = true;
                oUser.CreatedByUserID = null;
                oUser.DateCreated = DateTime.Now;

                SetUserParameter(command, oUser);
                String Password = oUser.Password.Hash(HashType.SHA512, Encoding.ASCII);

                command.AddParamWithValue(DbType.AnsiString, "@Password", Password);
                command.AddParamWithValue(DbType.Boolean, "@force_passw_change", true);
                command.AddParamWithValue(DbType.Int64, "@CreatedByUserID", null);
                command.AddParamWithValue(DbType.DateTime, "@DateCreated", oUser.DateCreated);

                int RowEffected = command.ExecuteNonQuery();
                if (RowEffected <= 0)
                {
                    throw new UpdateException("COULD NOT CREATE USER");
                }
                command.Parameters.Clear();

                command.CommandText = "SELECT LAST_INSERT_ID()";

                object o = command.ExecuteScalar();
                if (o != null)
                {
                    oUser.Id = Convert.ToInt64(o);
                    //we need to create role for user
                    if (oUser.UserTypeID == 1)
                    {
                        UserInRoleProcessor.AddRoleToUser(oUser, "Bank");
                    }
                    else if (oUser.UserTypeID == 2)
                    {
                        UserInRoleProcessor.AddRoleToUser(oUser, "Dealer");
                    }
                    else if (oUser.UserTypeID == 3)
                    {
                        UserInRoleProcessor.AddRoleToUser(oUser, "Agent");
                    }
                    else if(oUser.UserTypeID == 4)
                    {
                        //etop
                        UserInRoleProcessor.AddRoleToUser(oUser, "Etop");

                    }
                    
                }

                
            }
            catch (UpdateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new UpdateException(ex.Message, ex);
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
        }
        public static List<Int64> GetUserDistrict(Int64 UserID)
        {

            List<Int64> UserDistrict = new List<long>();
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();
                IDbCommand command = conn.CreateCommand();
                command.CommandText = "select district_id from tbl_setup_user_district where user_id=@user_id";
                command.AddParamWithValue(DbType.Int64, "@user_id", UserID);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

                while (rs.Read())
                {
                    UserDistrict.Add(Convert.ToInt64(rs["district_id"]));
                }
                rs.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return UserDistrict;
        }

        private static void SaveUserDistrict(User oUser)
        {
            IDbConnection conn = null;

            try
            {

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                List<Int64> CurrentDistrict = GetUserDistrict(oUser.Id);

                var query = (from c in oUser.District
                             where !CurrentDistrict.Contains(c) && c !=0
                             select c).ToList();

                if (query.Count() > 0)
                {
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "insert into tbl_setup_user_district(user_id,district_id)values(@user_id,@district_id)";
                    foreach (Int64 districtid in query)
                    {
                        try
                        {
                            command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
                            command.AddParamWithValue(DbType.Int64, "@district_id", districtid);
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                        }
                        command.Parameters.Clear();
                    }

                }

                var query1 = (from c in CurrentDistrict
                             where !oUser.District.Contains(c)
                             select c).ToList();

                if (query1 != null && query1.Count() > 0)
                {
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "delete from tbl_setup_user_district where user_id=@user_id,district_id=@district_id";
                    foreach (Int64 districtid in query)
                    {
                        try
                        {
                            command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);
                            command.AddParamWithValue(DbType.Int64, "@district_id", districtid);
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                        }
                        command.Parameters.Clear();
                    }
                }


            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
        }

        internal static void SetUserRoles(User oUser)
        {

            IDbConnection conn = null;

            try
            {

                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

               IDbCommand command =  conn.CreateCommand();
               command.CommandText = "select rolename from tbl_setup_role where exists(select Id from tbl_user_role where tbl_user_role.RoleId=tbl_setup_role.Id and tbl_user_role.UserId = @user_id)" ;


               command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);

               IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
               while (rs.Read())
               {
                   oUser.Roles.Add(rs["rolename"].ToString());

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
        }

        internal static List<User> GetUsers(User oUser)
        {
            List<User> UserList = null;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();

                    if (oUser.IsSuperUserOrAdmin )
                    {

                        StringBuilder builder = new StringBuilder();


                        builder.Append("SELECT * from tbl_setup_user WHERE Id not in(SELECT user_id from tbl_user_in_role,tbl_setup_roles");
                        builder.Append(" WHERE tbl_user_in_role.role_id =tbl_setup_roles.Id");
                        builder.Append(" AND tbl_setup_roles.rolename  IN('Admistrator','Super'))");
                        command.CommandText = builder.ToString();


                    }
                    else if (oUser.IsManager)
                    {
                        StringBuilder builder = new StringBuilder();


                        builder.Append("SELECT * from tbl_setup_user WHERE Id not in(SELECT user_id from tbl_user_in_role,tbl_setup_roles");
                        builder.Append(" WHERE tbl_user_in_role.role_id =tbl_setup_roles.Id");
                        builder.Append(" AND tbl_setup_roles.rolename  IN('Admistrator','Super')) AND (IsManager IS NULL or IsManager ='0')");
                        command.CommandText = builder.ToString();
                    }
                    else if (oUser.IsDistrictUser)
                    {
                        command.CommandText = "SELECT * from tbl_setup_user where dealer_id=@dealer_id";
                        command.AddParamWithValue(DbType.Int64, "@dealer_id", oUser.DealerID);

                    }
                    else if (oUser.IsDealer)
                    {
                        command.CommandText = "SELECT * from tbl_setup_user where IsDistrictUser=1";
                    }
                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    UserList = GetUserList(rs);

                    conn.Close();
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
            return UserList;


        }

        internal static void DeactivateUser(long Id)
        {
            IDbConnection conn = null;

            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command =  conn.CreateCommand();
                command.CommandText = "Update tbl_setup_user set ActiveStatus=@active where Id = @Id";
                command.AddParamWithValue(DbType.Int64, "@Id", Id);
                command.AddParamWithValue(DbType.Boolean, "@active", false);
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

        public static User GetUsersByID(long Id)
        {
            Models.User oUser = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT * from tbl_setup_user where Id=@Id ";

                command.AddParamWithValue(DbType.Int64, "@Id", Id);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

                List<Models.User> oUsers = GetUserList(rs);

                if (oUsers != null && oUsers.Count() > 0)
                {
                    oUser = oUsers[0];
                }



            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return oUser;
        }

        public static string GetOldPassword(long Id)
        {
            IDbConnection conn = null;

            String password = String.Empty;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "select Password from tbl_setup_user where Id = @Id";
                command.AddParamWithValue(DbType.Int64, "@Id", Id);
                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (rs.Read())
                {
                    if (rs["Password"] != DBNull.Value)
                    {
                        password = rs["Password"].ToString();
                    }
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
            return password;

        }
        //internal static List<Models.AgentUser> GetAllAgents()
        //{
        //    List<Models.AgentUser> Users = new List<AgentUser>();
        //    IDbConnection conn = null;
        //    try
        //    {
        //        conn = ConnectionManager.GetConnection();
        //        conn.OpenIfClosed();

        //        IDbCommand command = conn.CreateCommand();
        //        command.CommandText = "SELECT * from tbl_setup_user where IsAgent=@IsAgent ";

        //        command.AddParamWithValue(DbType.Boolean, "@IsAgent", true);
        //        IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
        //        while (rs.Read())
        //        {
        //            Models.AgentUser oUser = new AgentUser();

        //            oUser.AgentID = Convert.ToInt64(rs["Id"]); 
        //            oUser.FirstName = DataHelper.GetValue<String>(rs["Firstname"]);
        //            oUser.LastName = DataHelper.GetValue<String>(rs["LastName"]);
        //            oUser.DisplayName = DataHelper.GetValue<String>(rs["DisplayName"]);
        //            Users.Add(oUser);
        //        }
        //        rs.Close();


        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    finally
        //    {
        //        ConnectionManager.Close(conn);
        //    }
        //    return Users;
        //}
        public static bool ChangePassword(User Ouser, string newPassword,bool isResetPassword = false)
        {
            IDbConnection conn = null;
            bool IsChangepassword = false;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "Update tbl_setup_user set Password=@password,force_passw_change=@force_passw_change,ActiveStatus=@ActiveStatus  where Id = @Id";
                command.AddParamWithValue(DbType.Int64, "@Id", Ouser.Id);
                newPassword = newPassword.Hash(HashType.SHA512,Encoding.ASCII);
                command.AddParamWithValue(DbType.AnsiString, "@password", newPassword);
                if (isResetPassword)
                {
                    command.AddParamWithValue(DbType.Boolean, "@force_passw_change", true);
                }
                else
                {
                    command.AddParamWithValue(DbType.Boolean, "@force_passw_change", false);
                }
                command.AddParamWithValue(DbType.Boolean, "@ActiveStatus", true);
                int i = command.ExecuteNonQuery();

                if (i > 0)
                {
                    IsChangepassword = true;
                }

            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return IsChangepassword;
        }

        //internal static List<AgentUser> GetAllAgentsBYDealer(long Dealer_id)
        //{
        //    List<Models.AgentUser> Users = new List<AgentUser>();
        //    IDbConnection conn = null;
        //    try
        //    {
        //        conn = ConnectionManager.GetConnection();
        //        conn.OpenIfClosed();

        //        IDbCommand command = conn.CreateCommand();
        //        command.CommandText = "SELECT * from tbl_setup_user where dealer_id=@dealer_id AND IsAgent=@IsAgent ";

        //        command.AddParamWithValue(DbType.Boolean, "@IsAgent", true);
        //        command.AddParamWithValue(DbType.Int64, "@dealer_id ", Dealer_id);

        //        IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
        //        while (rs.Read())
        //        {
        //            Models.AgentUser oUser = new AgentUser();

        //            oUser.AgentID = Convert.ToInt64(rs["Id"]);
        //            oUser.FirstName = DataHelper.GetValue<String>(rs["Firstname"]);
        //            oUser.LastName = DataHelper.GetValue<String>(rs["Lastname"]);
        //            oUser.DisplayName = DataHelper.GetValue<String>(rs["DisplayName"]);
        //            Users.Add(oUser);
        //        }
        //        rs.Close();


        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    finally
        //    {
        //        ConnectionManager.Close(conn);
        //    }
        //    return Users;
        //}

        internal static List<User> GetUsers(User oUser, GeneralSearch search)
        {

            List<User> UserList = null;
            IDbConnection conn = null;
            try
            {
                    conn = DBConnector.ConnectionManager.GetConnection();
                
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();

                    StringBuilder builder = new StringBuilder();

                    if (oUser.IsSuperUserOrAdmin )
                    {

                        builder.Append("SELECT * from tbl_setup_user WHERE Id not in(SELECT UserId from tbl_user_role,tbl_setup_role");
                        builder.Append(" WHERE tbl_user_role.RoleId =tbl_setup_role.Id");
                        builder.Append(" AND tbl_setup_role.rolename  IN('Admistrator','Super','Posters'))");

                    }
                    else if (oUser.IsEtop)
                    {

                        builder.Append("SELECT * from tbl_setup_user WHERE Id not in(SELECT UserId from tbl_user_role,tbl_setup_role");
                        builder.Append(" WHERE tbl_user_role.RoleId =tbl_setup_role.Id");
                        builder.Append(" AND tbl_setup_role.rolename  IN('Admistrator','Super','Posters'))");
                    }
                    else if (oUser.IsDealer)
                    {
                        builder.Append("SELECT * from tbl_setup_user WHERE Id not in(SELECT UserId from tbl_user_role,tbl_setup_role");
                        builder.Append(" WHERE tbl_user_role.RoleId =tbl_setup_role.Id");
                        builder.Append(" AND tbl_setup_role.rolename  IN('Admistrator','Super','Posters')) and a.dealer_id=@dealer_id" );

                        
                    }
                    else if (oUser.IsAgent)
                    {
                        builder.Append("SELECT * from tbl_setup_user WHERE Id not in(SELECT UserId from tbl_user_role,tbl_setup_role");
                        builder.Append(" WHERE tbl_user_role.RoleId =tbl_setup_role.Id");
                        builder.Append(" AND tbl_setup_role.rolename  IN('Admistrator','Super','Posters')) and a.agent_id=@agent_id");
                    }
                    else if (oUser.isBank)
                    {
                        builder.Append("SELECT * from tbl_setup_user WHERE Id not in(SELECT UserId from tbl_user_role,tbl_setup_role");
                        builder.Append(" WHERE tbl_user_role.RoleId =tbl_setup_role.Id");
                        builder.Append(" AND tbl_setup_role.rolename  IN('Admistrator','Super','Posters')) and a.bank_id=@bank_id");
                    }

                    //SearchCriteria.Add("Firstname");
                    //SearchCriteria.Add("Lastname");
                    //SearchCriteria.Add("Email");
                    switch (search.SearchCriteria)
                    {
                        case "ALL":
                            break;
                        case "Firstname":
                            builder.Append(" AND Firstname REGEXP @Criteria");
                            break;
                        case "Lastname":
                            builder.Append(" AND Lastname REGEXP @Criteria");
                            break;
                        case "Email":
                            builder.Append(" AND EmailAddress REGEXP @Criteria");
                            break;
                    }

                    IDbCommand commandCount = conn.CreateCommand();

                    commandCount.CommandText = builder.ToString().Replace("*", "count(*) as ItemCount");


                    if (search.PageSize > 0)
                    {
                        builder.Append(" Limit ").Append(search.Skip).Append(",").Append(search.PageSize);

                    }
                    command.CommandText = builder.ToString();




                    if (oUser.IsDealer)
                    {
                        command.AddParamWithValue(DbType.Int64, "@dealer_id", oUser.DealerID);
                        commandCount.AddParamWithValue(DbType.Int64, "@dealer_id", oUser.DealerID);
                    }
                    else if (oUser.IsAgent)
                    {
                        command.AddParamWithValue(DbType.Int64, "@agent_id", oUser.AgentID);
                        commandCount.AddParamWithValue(DbType.Int64, "@agent_id", oUser.AgentID);
                    }
                    else if (oUser.isBank)
                    {
                        command.AddParamWithValue(DbType.Int64, "@bank_id", oUser.BankID);
                        commandCount.AddParamWithValue(DbType.Int64, "@bank_id", oUser.BankID);
                    }

                    switch (search.SearchCriteria)
                    {
                        case "ALL":
                            break;
                        default:
                            command.AddParamWithValue(DbType.AnsiString, "@Criteria", search.SearchValue);
                            commandCount.AddParamWithValue(DbType.AnsiString, "@Criteria", search.SearchValue);
                            break;
                    }

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    UserList = GetUserList(rs);


                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        object o = commandCount.ExecuteScalar();

                        if (o != null && o != DBNull.Value)
                        {
                            search.ItemCount = Convert.ToInt32(o);
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
            finally
            {
                ConnectionManager.Close(conn);
            }
            return UserList;


        }

        internal static void ActivateUser(long Id)
        {
               IDbConnection conn = null;
               try
               {
                   conn = DBConnector.ConnectionManager.GetConnection();

                   conn.OpenIfClosed();
                   IDbCommand command = conn.CreateCommand();

                   command.CommandText = "update tbl_setup_user set ActiveStatus=@active where Id=@Id";
                   command.AddParamWithValue(DbType.Boolean, "@active", true);
                   command.AddParamWithValue(DbType.Int64, "@Id", Id);

                   command.ExecuteNonQuery();

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

        public static User GetUserByUserNameOrEmail(string emailAddress)
        {
            User oUser = null;
            try
            {
                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * from tbl_setup_user where username =@username or emailaddress = @username";
                    command.AddParamWithValue(DbType.AnsiString, "@username", emailAddress);

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

                    List<User> UserList = GetUserList(rs);
                    if (UserList.Count() > 0)
                    {
                        oUser = UserList[0];
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

            return oUser;
        }

        public static string InsertResetPassword(string username, string emailAddress)
        {
            try
            {
                String id = String.Format("{0},{1}", Guid.NewGuid().ToString().Replace("-", ""), GetRandomPassword());

                using (IDbConnection conn = DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "insert into tbl_password_reset(id,username,emailaddress)values(@id,@username, @emailaddress)";
                    command.AddParamWithValue(DbType.AnsiString, "@username", username);
                    command.AddParamWithValue(DbType.AnsiString, "@emailaddress", emailAddress);
                    command.AddParamWithValue(DbType.AnsiString, "@id", id);

                    int rowAffected = command.ExecuteNonQuery();
                    if (rowAffected > 0)
                    {
                        return id;
                    }
                    throw new Exception("System unable to inititate change password");

                   
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