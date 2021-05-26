using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Doors.Data;
using System.Globalization;

namespace System.Doors.Data.Afectors.Sql
{
    public class SqlAccessor
    {
        public SqlAccessor()
        {
        }

        public DataTrellis GetSqlTrellis(string sqlConnectString, 
                                    string sqlQry, 
                                    string tableName, 
                                    DataDeposit deposit, 
                                    List<string> keyNames, 
                                    List<string> indexNames, 
                                    bool renew, 
                                    bool isCube)
        {
            try
            {
                if (DbHand.Schema == null|| DbHand.Schema.DbTables.Count == 0)
                {
                    SqlAfector sqaf = new SqlAfector(sqlConnectString);
                }
                AfectAdapter afad = new AfectAdapter(sqlConnectString);

                try
                {
                    if (!renew)
                        return afad.ExecuteInject(sqlQry, tableName, isCube, keyNames, indexNames, deposit);
                    else
                        return afad.ExecuteInject(sqlQry, deposit.Box.Prime);
                }
                catch(Exception ex)
                {
                    throw new SqlAfectException(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object GetSqlDataTable(object parameters)
        {
            try
            {
                Dictionary<string, object> param = new Dictionary<string, object>((Dictionary<string, object>)parameters);
                string sqlQry = param["SqlQuery"].ToString();
                string sqlConnectString = param["ConnectionString"].ToString();

                DataTable Table = new DataTable();
                SqlConnection sqlcn = new SqlConnection(sqlConnectString);
                sqlcn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlQry, sqlcn);
                adapter.Fill(Table);
                return Table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetSqlDataTable(SqlCommand cmd)
        {
            try
            {

                DataTable Table = new DataTable();
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(Table);
                return Table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DataTable GetSqlDataTable(string qry, SqlConnection cn)
        {
            try
            {
                DataTable Table = new DataTable();
                if (cn.State == ConnectionState.Closed)
                    cn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(qry, cn);
                adapter.Fill(Table);
                return Table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }       
    }
}
