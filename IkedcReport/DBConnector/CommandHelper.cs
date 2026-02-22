using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
namespace WalletReport.DBConnector
{
    public static class CommandHelper
    {
        public static bool HasColumn(this System.Data.IDataReader dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
        public static IDbDataParameter AddParamWithValue(this IDbCommand command, DbType type, String ParameterName,object value)
        {
            IDbDataParameter param = new MySql.Data.MySqlClient.MySqlParameter();
            param.Direction = ParameterDirection.Input;
            param.ParameterName = ParameterName;
            param.DbType = type;

            if (value == null)
                param.Value = DBNull.Value;
            else
            {
                param.Value = value;
            }
            command.Parameters.Add(param);
            return param;
        }
        public static IDbDataParameter AddOutputParam(this IDbCommand command, DbType type, String ParameterName,object value,int size)
        {
            IDbDataParameter param = new MySql.Data.MySqlClient.MySqlParameter();
            param.ParameterName = ParameterName;
            param.DbType = type;

            if (value == null)
                param.Value = DBNull.Value;
            else
            {
                param.Value = value;
            }
            if (size > 0)
            {
                param.Size = size;
            }
            param.Direction = ParameterDirection.Output;
            command.Parameters.Add(param);
            return param;
        }
        public static IDbDataParameter AddReturnParam(this IDbCommand command, DbType type, String ParameterName)
        {
            IDbDataParameter param = new MySql.Data.MySqlClient.MySqlParameter();
            param.ParameterName = ParameterName;
            param.DbType = type;

            param.Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(param);
            return param;
        }
    }
}