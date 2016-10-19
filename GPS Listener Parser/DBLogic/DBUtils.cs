using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace GPSParser.DBLogic
{   
        public class DBUtils
        {
            private string connectionString;
            public DBUtils()
            {
                connectionString = GetConnectionString();
            }
            public string GetConnectionString()
            {               
                return GPSParser.Properties.Settings.Default.GPS_TrackingConnectionString;
            }        
          
            //----------------------------------------------------------------------------------//
            //**********************************************************************************//
            //----------------------------------------------------------------------------------//
            public SqlCommand InitSP(string spname)
            {
                SqlCommand command = new SqlCommand();
                command.Connection = new SqlConnection(GetConnectionString());
                command.Connection.Open();
                command.CommandText = spname;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("Result", System.Data.SqlDbType.Int)).Direction = System.Data.ParameterDirection.ReturnValue;
                return command;
            }
            public SqlCommand InitQuery(string sql)
            {
                SqlCommand command = new SqlCommand();
                command.Connection = new SqlConnection(GetConnectionString());
                command.Connection.Open();
                command.CommandText = sql;
                command.CommandType = System.Data.CommandType.Text;
                return command;
            }         

            public void ExecSP(SqlCommand sp)
            {
                sp.ExecuteNonQuery();

                if (!sp.Parameters[0].Value.Equals(0))
                    throw new Exception("Stored	procedure (" + sp.CommandText + ") result error: " + sp.Parameters[0].Value.ToString());
            }

            public object ExecFunction(SqlCommand sp)
            {
                sp.ExecuteNonQuery();
                return sp.Parameters[0].Value;
            }

            public void ExecQuery(SqlCommand InitQuery)
            {
                try
                {
                    InitQuery.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Exception while store in db: " + ex.ToString());
                }
                finally
                {                    
                    FreeSP(InitQuery);
                }
            }

            public SqlDataReader OpenSP(SqlCommand sp)
            {
                return sp.ExecuteReader(CommandBehavior.CloseConnection);
            }

            public void FreeSP(SqlCommand sp)
            {
                if (sp == null) return;
                sp.Connection.Close();             
                sp.Dispose();
                sp = null;
            }
        }
    

}
