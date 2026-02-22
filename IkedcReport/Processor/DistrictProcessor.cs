using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WalletReport.DBConnector;

namespace WalletReport.Processor
{
    public class DistrictProcessor
    {

        public Models.District GetDistrict(String DistrictCode)
        {
            Models.District district = null;
            try
            {
                using (IDbConnection conn = WalletReport.DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * FROM tbl_SETUP_DISTRICT WHERE DistrictCode=@code";
                    command.AddParamWithValue(DbType.AnsiString, "@code", DistrictCode);

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    List <Models.District>  districts = GetDistricts(rs);
                    if (districts.Count() > 0)
                        district = districts[0];
                }
            }
            catch (Exception ex)
            {
            }
            return district;
        }
        public static void SetDistrictParameter(IDbCommand command, Models.District district)
        {
            command.AddParamWithValue(DbType.AnsiString, "@code", district.Code);
            command.AddParamWithValue(DbType.AnsiString, "@district_name", district.district_name);
            command.AddParamWithValue(DbType.AnsiString, "@address_line_1", district.AddressLine1);
            command.AddParamWithValue(DbType.AnsiString, "@address_line_2", district.AddressLine2);
            command.AddParamWithValue(DbType.AnsiString, "@city", district.City);
            command.AddParamWithValue(DbType.AnsiString, "@state", district.State);
            command.AddParamWithValue(DbType.AnsiString, "@country", district.Country);
            command.AddParamWithValue(DbType.AnsiString, "@contact_address", district.ContactAddress);
            command.AddParamWithValue(DbType.AnsiString, "@contact_email", district.ContactEmail);
            command.AddParamWithValue(DbType.AnsiString, "@contact_mobile", district.ContactMobile);
            command.AddParamWithValue(DbType.AnsiString, "@contact_name", district.ContactName);
            if (district.Id > 0)
            {
                command.AddParamWithValue(DbType.Int64, "@Id", district.Id);
            }
        }
        public static void Create(Models.District district)
        {
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();


                IDbCommand command = conn.CreateCommand();
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("Insert into tbl_setup_district(code,district_name,address_line_1,address_line_2,city,");
                builder.Append("state,country,contact_address,contact_email,contact_mobile,");
                builder.Append("contact_name");

                builder.Append(")");
                builder.Append("values(@code,@district_name,@address_line_1,@address_line_2,@city,");
                builder.Append("@state,@country,@contact_address,@contact_email,@contact_mobile,");
                builder.Append("@contact_name");
                builder.Append(")");

                command.CommandText = builder.ToString();
                SetDistrictParameter(command, district);

                int returnCode = command.ExecuteNonQuery();

                if (returnCode <= 0)
                {
                    throw new UpdateException("Could not insert dealer record");
                }

                command.Parameters.Clear();
                command.CommandText = "SELECT LAST_INSERT_ID()";
                object o = command.ExecuteScalar();

                district.Id = Convert.ToInt64(o);
            }
            catch (OpenDBException ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
        }

        public static List<Models.District> GetDistricts(IDataReader rs)
        {

            List<Models.District> districts = new List<Models.District>();

            while (rs.Read())
            {
                try
                {
                    Models.District district = new Models.District();
                    district.Id = Convert.ToInt64(rs["Id"]);
                    district.Code = DataHelper.GetValue<String>(rs["code"]);
                    district.district_name = DataHelper.GetValue<String>(rs["district_name"]);
                    district.AddressLine1 = DataHelper.GetValue<String>(rs["address_line_1"]);
                    district.AddressLine2 = DataHelper.GetValue<String>(rs["address_line_2"]);
                    district.City = DataHelper.GetValue<String>(rs["city"]);
                    district.State = DataHelper.GetValue<String>(rs["state"]);
                    district.Country = DataHelper.GetValue<String>(rs["country"]);
                    district.ContactAddress = DataHelper.GetValue<String>(rs["contact_address"]);
                    district.ContactEmail = DataHelper.GetValue<String>(rs["contact_email"]);
                    district.ContactMobile = DataHelper.GetValue<String>(rs["contact_mobile"]);
                    district.ContactName = DataHelper.GetValue<String>(rs["contact_name"]);
                    districts.Add(district);
                }
                catch (Exception ex)
                {
                }
            }
            rs.Close();


            return districts;
          
        }
        public static List<Models.District> GetAllDistrict()
        {
            List<Models.District> districts = null;
            try
            {
                using (IDbConnection conn = WalletReport.DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();
                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT * FROM tbl_setup_district";

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    districts = GetDistricts(rs);

                }
            }
            catch (Exception ex)
            {
            }

            if (districts == null)
                districts = new List<Models.District>();


            return districts;
        }

        
        internal static void Update(Models.District district)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("UPDATE tbl_setup_district SET code=@code,district_name=@district_name,");
            builder.Append("address_line_1=@address_line_1,address_line_2=@address_line_2,");
            builder.Append("city=@city,state=@state,country=@country,contact_address=@contact_address,");
            builder.Append("contact_email=@contact_email,contact_mobile=@contact_mobile,");
            builder.Append("contact_name=@contact_name");
            builder.Append(" WHERE Id= @Id");

            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = builder.ToString();

                SetDistrictParameter(command, district);
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

        internal static Models.District GetDistrictByID(long DistrictID)
        {
            Models.District district = null;
            try
            {
                using (IDbConnection conn = WalletReport.DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();

                    IDbCommand command = conn.CreateCommand();

                    command.CommandText = "SELECT * FROM tbl_setup_district WHERE Id=@Id";
                    command.AddParamWithValue(DbType.Int64, "@Id", DistrictID);

                    IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);
                    List<Models.District> districts = GetDistricts(rs);

                    if (districts.Count() > 0)
                        district = districts[0];

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
            }
            return district;
        }

        public static bool ISALLDistrict(Models.User user)
        {
            bool isAllDistrict = false;
            try
            {
                using (IDbConnection conn = WalletReport.DBConnector.ConnectionManager.GetConnection())
                {
                    conn.OpenIfClosed();

                    IDbCommand command = conn.CreateCommand();
                    command.CommandText = "SELECT count(*) FROM tbl_setup_user_district WHERE user_id=@Id";
                    command.AddParamWithValue(DbType.Int64,"@Id",user.Id);

                   object o = command.ExecuteScalar();
                    if(o != DBNull.Value)
                    {
                        isAllDistrict = Convert.ToInt32(o) == 0;
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
            }

            return isAllDistrict;
        }

        internal static List<Models.District> GetAllDistrict(Models.User oUser)
        {
            List<Models.District> Districts = null;
            IDbConnection conn = null;
            try
            {
                conn = ConnectionManager.GetConnection();
                conn.OpenIfClosed();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT * from tbl_setup_district  where exists(select * from tbl_setup_user_district where tbl_setup_user_district.district_id=tbl_setup_district.Id and tbl_setup_user_district.user_id=@user_id )";
                command.AddParamWithValue(DbType.Int64, "@user_id", oUser.Id);

                IDataReader rs = command.ExecuteReader(CommandBehavior.CloseConnection);

                Districts = GetDistricts(rs);


            }
            catch (Exception ex)
            {
            }
            finally
            {
                ConnectionManager.Close(conn);
            }
            return Districts;
        }
    }
}