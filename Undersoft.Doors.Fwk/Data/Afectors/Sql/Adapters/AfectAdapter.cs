using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace System.Doors.Data.Afectors.Sql
{
    public class AfectAdapter
    {
        private SqlConnection _cn;
        private SqlCommand _cmd;    

        public AfectAdapter(SqlConnection cn)
        {
            _cn = cn;        
        }
        public AfectAdapter(string cnstring)
        {
            _cn = new SqlConnection(cnstring);         
        }

        public DataTrellis ExecuteInject(string sqlqry, string tableName, 
                                            bool isCube = false, 
                                            ICollection<string> keyNames = null, 
                                            ICollection<string> indexNames = null, 
                                            DataDeposit deposit = null)
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand(sqlqry, _cn);
                            cmd.Prepare();
                            if (_cn.State == ConnectionState.Closed)
                                _cn.Open();
                            IDataReader sdr = cmd.ExecuteReader();
                            AfectReader<DataTrellis> dr = new AfectReader<DataTrellis>(sdr);
                            DataTrellis it = dr.InjectRead(tableName, isCube, keyNames, indexNames, deposit);
                            sdr.Dispose();
                            cmd.Dispose();
                            return it;
                        }
                        catch (Exception ex)
                        {
                            throw new SqlAfectException(ex.ToString());
                        }           
                    }

        public DataTrellis ExecuteInject(string sqlqry, string tableName = null)
        {
            SqlCommand cmd = new SqlCommand(sqlqry, _cn);          
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            IDataReader sdr = cmd.ExecuteReader();
            AfectReader<DataTrellis> dr = new AfectReader<DataTrellis>(sdr);
            DataTrellis it = dr.InjectRead(tableName,true);
            sdr.Dispose();
            cmd.Dispose();
            return it;
        }

        public DataTrellis ExecuteInject(string sqlqry, DataTrellis it)
        {
            SqlCommand cmd = new SqlCommand(sqlqry, _cn);
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            IDataReader sdr = cmd.ExecuteReader();
            AfectReader<DataTrellis> dr = new AfectReader<DataTrellis>(sdr);
            DataTrellis _it = dr.InjectRead(it);
            sdr.Dispose();
            cmd.Dispose();
            return _it;
        }

        public DataSphere ExecuteUpdate(string sqlqry, DataTier[] tiers, bool disposeCmd = false)
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;                 
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            IDataReader sdr = cmd.ExecuteReader();        
            AfectReader<DataTrellis> dr = new AfectReader<DataTrellis>(sdr);
            DataSphere _is = dr.UpdateRead(tiers);
            sdr.Dispose();
            if(disposeCmd)
                cmd.Dispose();
            return _is;
        }
        public int ExecuteUpdate(string sqlqry, bool disposeCmd = false)
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;
            SqlTransaction tr = _cn.BeginTransaction();
            cmd.Transaction = tr;
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            int i = cmd.ExecuteNonQuery();
            tr.Commit();
            if (disposeCmd)
                cmd.Dispose();            
            return i;          
        }

        public DataSphere ExecuteInsert(string sqlqry, DataTier[] tiers,  bool disposeCmd = false)
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;           
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();         
            IDataReader sdr = cmd.ExecuteReader();           
            AfectReader<DataTrellis> dr = new AfectReader<DataTrellis>(sdr);
            DataSphere _is = dr.InsertRead(tiers);
            sdr.Dispose();
            if (disposeCmd)
                cmd.Dispose();
            return _is;
        }
        public int ExecuteInsert(string sqlqry, bool disposeCmd = false)
        {
             if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;
            SqlTransaction tr = _cn.BeginTransaction();
            cmd.Transaction = tr;
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            int i = cmd.ExecuteNonQuery();
            tr.Commit();
            if (disposeCmd)
                cmd.Dispose();
            return i;
        }

        public bool DataBulk(DataTrellis trell, string buforTable, BulkPrepareType prepareType = BulkPrepareType.None, BulkDbType dbType = BulkDbType.TempDB)
        {
            try
            {               
                if (_cn.State == ConnectionState.Closed)
                    _cn.Open();
                try
                {
                    if (dbType == BulkDbType.TempDB) _cn.ChangeDatabase("tempdb");
                    if (!DbHand.Schema.DataDbTables.Have(buforTable) || prepareType == BulkPrepareType.Drop)
                    {
                        string createTable = "";
                        if (prepareType == BulkPrepareType.Drop)
                           createTable += "Drop table if exists [" + buforTable + "] \n";
                        createTable += "Create Table [" + buforTable + "] ( ";
                        foreach (DataPylon column in trell.Pylons)
                        {
                            string sqlTypeString = "varchar(200)";
                            List<string> defineOne = new List<string>() { "varchar", "nvarchar", "ntext", "varbinary" };
                            List<string> defineDec = new List<string>() { "decimal", "numeric" };                          
                            int colLenght = column.PylonSize;                       
                            sqlTypeString = SqlNetType.NetTypeToSql(column.DataType);
                            string addSize = (colLenght > 0) ? (defineOne.Contains(sqlTypeString)) ? (string.Format(@"({0})", colLenght)) :
                                                               (defineDec.Contains(sqlTypeString)) ? (string.Format(@"({0}, {1})", colLenght - 6, 6)) : "" : "";
                            sqlTypeString += addSize;   
                            createTable += " [" + column.PylonName + "] " + sqlTypeString + ",";
                        }
                        createTable = createTable.TrimEnd(new char[] { ',' }) + " ) ";
                        SqlCommand createcmd = new SqlCommand(createTable, _cn);
                        createcmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    throw new AfectInsertException(ex.ToString());
                }
                if (prepareType == BulkPrepareType.Trunc)
                {
                    string deleteData = "Truncate Table [" + buforTable + "]";
                    SqlCommand delcmd = new SqlCommand(deleteData, _cn);
                    delcmd.ExecuteNonQuery();
                }

                try
                {
                    DataReader ndr = new DataReader(trell);
                    SqlBulkCopy bulkcopy = new SqlBulkCopy(_cn);
                    bulkcopy.DestinationTableName = "[" + buforTable + "]";
                    bulkcopy.WriteToServer(ndr);
                }
                catch (SqlException ex)
                {
                    throw new AfectInsertException(ex.ToString());
                }             
                return true;
            }
            catch (SqlException ex)
            {
                throw new AfectInsertException(ex.ToString());
            }
        }
        public bool DataBulk(DataTier[] tiers, string buforTable, BulkPrepareType prepareType = BulkPrepareType.None, BulkDbType dbType = BulkDbType.TempDB)
        {
            try
            {
                DataTrellis trell = null;
                if (tiers.Any())
                {
                    trell = tiers.ElementAt(0).Trell;
                    if (_cn.State == ConnectionState.Closed)
                        _cn.Open();
                    try
                    {
                        if (dbType == BulkDbType.TempDB) _cn.ChangeDatabase("tempdb");
                        if (!DbHand.Temp.DataDbTables.Have(buforTable) || prepareType == BulkPrepareType.Drop)
                        {
                            string createTable = "";
                            if (prepareType == BulkPrepareType.Drop)
                                createTable += "Drop table if exists [" + buforTable + "] \n";
                            createTable += "Create Table [" + buforTable + "] ( ";
                            foreach (DataPylon column in trell.Pylons)
                            {
                                    string sqlTypeString = "varchar(200)";
                                    List<string> defineStr = new List<string>() { "varchar", "nvarchar", "ntext", "varbinary" };
                                    List<string> defineDec = new List<string>() { "decimal", "numeric" };
                                    int colLenght = column.PylonSize;
                                    sqlTypeString = SqlNetType.NetTypeToSql(column.DataType);
                                    string addSize = (colLenght > 0) ? (defineStr.Contains(sqlTypeString)) ? (string.Format(@"({0})", colLenght)) :
                                                                       (defineDec.Contains(sqlTypeString)) ? (string.Format(@"({0}, {1})", colLenght - 6, 6)) : "" : "";
                                    sqlTypeString += addSize;
                                    createTable += " [" + column.PylonName + "] " + sqlTypeString + ",";
                            }
                            createTable = createTable.TrimEnd(new char[] { ',' }) + " ) ";
                            SqlCommand createcmd = new SqlCommand(createTable, _cn);
                            createcmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new AfectInsertException(ex.ToString());
                    }
                    if (prepareType == BulkPrepareType.Trunc)
                    {
                        string deleteData = "Truncate Table [" + buforTable + "]";
                        SqlCommand delcmd = new SqlCommand(deleteData, _cn);
                        delcmd.ExecuteNonQuery();
                    }

                    try
                    {
                        DataReader ndr = new DataReader(tiers);
                        SqlBulkCopy bulkcopy = new SqlBulkCopy(_cn);
                        bulkcopy.DestinationTableName = "[" + buforTable + "]";
                        bulkcopy.WriteToServer(ndr);
                    }
                    catch (SqlException ex)
                    {
                        throw new AfectInsertException(ex.ToString());
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (SqlException ex)
            {
                throw new AfectInsertException(ex.ToString());               
            }
        }
    }

    public enum BulkPrepareType
    {
        Trunc,
        Drop,
        None
    }

    public enum BulkDbType
    {
        TempDB,
        Origin,
        None
    }
}
