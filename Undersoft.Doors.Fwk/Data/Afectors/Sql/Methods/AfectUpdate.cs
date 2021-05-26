using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace System.Doors.Data.Afectors.Sql
{
    public class AfectUpdate
    {
        private SqlConnection _cn;

        public AfectUpdate(SqlConnection cn)
        {
            _cn = cn;
        } 
        public AfectUpdate(string cnstring)
        {
            _cn = new SqlConnection(cnstring);
        }

        public DataSphere Update(DataTier[] tiers, bool keysFromTrellis = false, bool buildMapping = false, bool updateKeys = false, string[] updateExcept = null, BulkPrepareType tempType = BulkPrepareType.Trunc)
        {
            try
            {
                DataTrellis it = null;
                if (tiers.Any())
                {
                    it = tiers[0].Trell;
                    IList<AfectMapping> nMaps = new List<AfectMapping>();
                    if (buildMapping)
                    {
                        AfectMapper imapper = new AfectMapper(it, keysFromTrellis);
                    }
                    nMaps = it.AfectMap;
                    string dbName = _cn.Database;
                    AfectAdapter afad = new AfectAdapter(_cn);
                    afad.DataBulk(tiers, it.TrellName, tempType, BulkDbType.TempDB);
                    _cn.ChangeDatabase(dbName);
                    DataSphere nSet = new DataSphere("AfectUpdate");

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(@"  /* ----  SQL AFECTOR START CMD ------ */  ");                
                    foreach (AfectMapping nMap in nMaps)
                    {
                        sb.AppendLine(@"  /* ----  TABLE BULK START CMD ------ */  ");

                        DataPylon[] ic = it.Pylons.AsEnumerable().Where(c => nMap.ColumnOrdinal.Contains(c.Ordinal)).ToArray();
                        DataPylon[] ik = it.Pylons.AsEnumerable().Where(c => nMap.KeyOrdinal.Contains(c.Ordinal)).ToArray();

                        if (updateExcept != null)
                        {
                            ic = ic.Where(c => !updateExcept.Contains(c.PylonName)).ToArray();
                            ik = ik.Where(c => !updateExcept.Contains(c.PylonName)).ToArray();
                        }

                        string qry = BulkUpdateQuery(dbName, it.TrellName, nMap.DbTableName, ic, ik, updateKeys).ToString();
                        sb.Append(qry);
                        sb.AppendLine(@"  /* ----  TABLE BULK EDataD CMD ------ */  ");
                    }
                    sb.AppendLine(@"  /* ----  SQL AFECTOR EDataD CMD ------ */  ");

                    DataSphere bDataSphere = afad.ExecuteUpdate(sb.ToString(), tiers, true);

                    if (nSet.Trells.Count == 0)
                        nSet = bDataSphere;
                    else
                        foreach (DataTrellis its in bDataSphere.Trells)
                        {
                            if (nSet.Trells.Have(its.TrellName))
                                nSet.Trells[its.TrellName].Absorb(its);
                            else
                                nSet.Trells.Add(its);
                        }
                    sb.Clear();                 

                    return nSet;
                }
                else
                    return null;
            }
            catch (SqlException ex)
            {
                _cn.Close();
                throw new AfectUpdateException(ex.ToString());
            }
        }
        public DataSphere BatchUpdate(DataTier[] tiers, bool keysFromTrellis = false, bool buildMapping = false, bool updateKeys = false, string[] updateExcept = null, int batchSize = 250)
        {
            try
            {
                DataTrellis it = new DataTrellis();
                IList<AfectMapping> nMaps = new List<AfectMapping>();
                AfectAdapter afad = new AfectAdapter(_cn);
                StringBuilder sb = new StringBuilder();
                DataSphere iSet = new DataSphere("AfectUpdate");
                sb.AppendLine(@"  /* ----  SQL AFECTOR START CMD ------ */  ");
                int count = 0;
                foreach (DataTier ir in tiers)
                {
                    if (ir.Trell.TrellName != it.TrellName)
                    {
                        it = ir.Trell;
                        if (buildMapping)
                        {
                            AfectMapper imapper = new AfectMapper(it, keysFromTrellis);
                        }
                        nMaps = it.AfectMap;
                    }

                    foreach (AfectMapping nMap in nMaps)
                    {
                        HashSet<int> co = nMap.ColumnOrdinal;
                        HashSet<int> ko = nMap.KeyOrdinal;
                        DataPylon[] ic = it.Pylons.AsEnumerable().Where(c => nMap.ColumnOrdinal.Contains(c.Ordinal)).ToArray();
                        DataPylon[] ik = it.Pylons.AsEnumerable().Where(c => nMap.KeyOrdinal.Contains(c.Ordinal)).ToArray();
                        if (updateExcept != null)
                        {
                            ic = ic.Where(c => !updateExcept.Contains(c.PylonName)).ToArray();
                            ik = ik.Where(c => !updateExcept.Contains(c.PylonName)).ToArray();
                        }

                        string qry = BatchUpdateQuery(ir, nMap.DbTableName, ic, ik, updateKeys).ToString();
                        sb.Append(qry);
                        count++;
                    }
                    if (count >= batchSize)
                    {
                        sb.AppendLine(@"  /* ----  DATA BATCH END CMD ------ */  ");
                        DataSphere bDataSphere = afad.ExecuteUpdate(sb.ToString(), tiers);
                        if (iSet.Trells.Count == 0)
                            iSet = bDataSphere;
                        else
                            foreach (DataTrellis its in bDataSphere.Trells)
                            {
                                if (iSet.Trells.Have(its.TrellName))
                                    iSet.Trells[its.TrellName].Absorb(its);
                                else
                                    iSet.Trells.Add(its);
                            }
                        sb.Clear();
                        sb.AppendLine(@"  /* ----  SQL AFECTOR START CMD ------ */  ");
                        count = 0;
                    }
                }
                sb.AppendLine(@"  /* ----  DATA AFECTOR END CMD ------ */  ");

                DataSphere rDataSphere = afad.ExecuteUpdate(sb.ToString(), tiers, true);

                if (iSet.Trells.Count == 0)
                    iSet = rDataSphere;
                else
                    foreach (DataTrellis its in rDataSphere.Trells)
                    {
                        if (iSet.Trells.Have(its.TrellName))
                            iSet.Trells[its.TrellName].Absorb(its);
                        else
                            iSet.Trells.Add(its);
                    }

                return iSet;
            }
            catch (SqlException ex)
            {
                _cn.Close();
                throw new AfectUpdateException(ex.ToString());
            }
        }
        public int  SimpleUpdate(DataTier[] tiers, bool buildMapping = false, bool updateKeys = false, string[] updateExcept = null, int batchSize = 500)
        {
            try
            {
                DataTrellis it = new DataTrellis();
                IList<AfectMapping> nMaps = new List<AfectMapping>();
                AfectAdapter afad = new AfectAdapter(_cn);
                StringBuilder sb = new StringBuilder();
                int intAfect = 0;
                sb.AppendLine(@"  /* ----  SQL AFECTOR START CMD ------ */  ");
                int count = 0;
                foreach (DataTier ir in tiers)
                {
                    if (ir.Trell.TrellName != it.TrellName)
                    {
                        it = ir.Trell;
                        if (buildMapping)
                        {
                            AfectMapper imapper = new AfectMapper(it);
                        }
                        nMaps = it.AfectMap;
                    }

                    foreach (AfectMapping nMap in nMaps)
                    {
                        HashSet<int> co = nMap.ColumnOrdinal;
                        HashSet<int> ko = nMap.KeyOrdinal;
                        DataPylon[] ic = it.Pylons.AsEnumerable().Where(c => nMap.ColumnOrdinal.Contains(c.Ordinal)).ToArray();
                        DataPylon[] ik = it.Pylons.AsEnumerable().Where(c => nMap.KeyOrdinal.Contains(c.Ordinal)).ToArray();
                        if (updateExcept != null)
                        {
                            ic = ic.Where(c => !updateExcept.Contains(c.PylonName)).ToArray();
                            ik = ik.Where(c => !updateExcept.Contains(c.PylonName)).ToArray();
                        }

                        string qry = BatchUpdateQuery(ir, nMap.DbTableName, ic, ik, updateKeys).ToString();
                        sb.Append(qry);
                        count++;
                    }
                    if (count >= batchSize)
                    {
                        sb.AppendLine(@"  /* ----  DATA BATCH EDataD CMD ------ */  ");
                        intAfect += afad.ExecuteUpdate(sb.ToString());
                        sb.Clear();
                        sb.AppendLine(@"  /* ----  SQL AFECTOR START CMD ------ */  ");
                        count = 0;
                    }
                }
                sb.AppendLine(@"  /* ----  DATA AFECTOR EDataD CMD ------ */  ");

                intAfect += afad.ExecuteUpdate(sb.ToString());
                return intAfect;
            }
            catch (SqlException ex)
            {
                _cn.Close();
                throw new AfectUpdateException(ex.ToString());
            }
        }
   
        public StringBuilder BatchUpdateQuery(DataTier tier, string tableName, DataPylon[] columns, DataPylon[] keys, bool updateKeys = true)
        {
            StringBuilder sb = new StringBuilder();
            string tName = tableName;
            DataTier ir = tier;
            object[] ia = ir.DataArray;
            DataPylon[] ic = columns;
            DataPylon[] ik = keys;

            sb.AppendLine(@"  /* ---- DATA AFECTOR START DataTEM CMD ------ */  ");
            sb.Append("UPDATE " + tName + " SET ");
            bool isUpdateCol = false;
            string delim = "";
            int c = 0;
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].PylonName.ToLower() == "updated")
                    isUpdateCol = true;
                if (ia[columns[i].Ordinal] != DBNull.Value)
                {
                    if (c > 0)
                        delim = ",";
                    sb.AppendFormat(CultureInfo.InvariantCulture, @"{0}[{1}] = {2}{3}{2}", delim,
                                                                                          columns[i].PylonName,
                                                                                          (columns[i].DataType == typeof(string) ||
                                                                                           columns[i].DataType == typeof(DateTime)) ? "'" : "",
                                                                                          (columns[i].DataType != typeof(string)) ?
                                                                                          Convert.ChangeType(ia[columns[i].Ordinal], columns[i].DataType) :
                                                                                           ia[columns[i].Ordinal].ToString().Replace("'","''")
                                                                                          );
                    c++;
                }
            }
          
            if (DbHand.Schema.DataDbTables[tableName].DataDbColumns.Have("updated") && !isUpdateCol)
                sb.AppendFormat(CultureInfo.InvariantCulture, ", [updated] = '{0}'", DateTime.Now);

            if (updateKeys)
            {
                if (columns.Length > 0)
                    delim = ",";
                else
                    delim = "";
                c = 0;
                for (int i = 0; i < keys.Length; i++)
                {
                    
                    if (ia[keys[i].Ordinal] != DBNull.Value)
                    {
                        if (c > 0)
                            delim = ",";
                        sb.AppendFormat(CultureInfo.InvariantCulture, @"{0}[{1}] = {2}{3}{2}", delim,
                                                                                          keys[i].PylonName,
                                                                                          (keys[i].DataType == typeof(string) ||
                                                                                           keys[i].DataType == typeof(DateTime)) ? "'" : "",
                                                                                             (keys[i].DataType != typeof(string)) ?
                                                                                          Convert.ChangeType(ia[keys[i].Ordinal], keys[i].DataType) :
                                                                                           ia[keys[i].Ordinal].ToString().Replace("'", "''")
                                                                                          );
                        c++;
                    }
                }
            }
            sb.AppendLine(" OUTPUT inserted.*, deleted.* ");
            delim = "";
            c = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (i > 0)
                    delim = " AND ";
                else
                    delim = " WHERE ";
                if (ia[keys[i].Ordinal] != DBNull.Value)
                {
                    if (c > 0)
                        delim = " AND ";
                    else
                        delim = " WHERE ";
                    sb.AppendFormat(CultureInfo.InvariantCulture, @"{0} [{1}] = {2}{3}{2}", delim,
                                                                                      keys[i].PylonName,
                                                                                      (keys[i].DataType == typeof(string) ||
                                                                                       keys[i].DataType == typeof(DateTime)) ? "'" : "", 
                                                                                      (ia[keys[i].Ordinal] != DBNull.Value) ?
                                                                                         (keys[i].DataType != typeof(string)) ?
                                                                                          Convert.ChangeType(ia[keys[i].Ordinal], keys[i].DataType) :
                                                                                           ia[keys[i].Ordinal].ToString().Replace("'", "''") : ""
                                                                                      );
                    c++;
                }
            }
            sb.AppendLine("");
            sb.AppendLine(@"  /* ----  SQL AFECTOR EDataD DataTEM CMD ------ */  ");
            return sb;
        }
        public StringBuilder BulkUpdateQuery(string DBName, string buforName, string tableName, DataPylon[] columns, DataPylon[] keys, bool updateKeys = true)
        {
            StringBuilder sb = new StringBuilder();
            string bName = buforName;
            string tName = tableName;
            DataPylon[] ic = columns;
            DataPylon[] ik = keys;
            string dbName = DBName; 
            sb.AppendLine(@"  /* ---- DATA AFECTOR START DataTEM CMD ------ */  ");
            sb.AppendFormat(@"UPDATE [{0}].[dbo].[" + tName + "] SET ", dbName);
            bool isUpdateCol = false;
            string delim = "";
            int c = 0;
            for (int i = 0; i < columns.Length; i++)
            {

                if (columns[i].PylonName.ToLower() == "updated")
                    isUpdateCol = true;

                if (c > 0)
                    delim = ",";
                sb.AppendFormat(CultureInfo.InvariantCulture, @"{0}[{1}] =[S].[{2}]", delim,
                                                                                      columns[i].PylonName,
                                                                                      columns[i].PylonName
                                                                                      );
                c++;
            }       

            if (updateKeys)
            {
                if (columns.Length > 0)
                    delim = ",";
                else
                    delim = "";
                c = 0;
                for (int i = 0; i < keys.Length; i++)
                {
                        if (c > 0)
                            delim = ",";
                        sb.AppendFormat(CultureInfo.InvariantCulture, @"{0}[{1}] = [S].[{2}]", delim,
                                                                                          keys[i].PylonName,
                                                                                          keys[i].PylonName
                                                                                          );
                        c++;
                }
            }
            sb.AppendLine(" OUTPUT inserted.*, deleted.* ");
            sb.AppendFormat(" FROM [tempdb].[dbo].[{0}] AS S INNER JOIN [{1}].[dbo].[{2}] AS T ", bName, dbName, tName );
            delim = "";
            c = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (i > 0)
                    delim = " AND ";
                else
                    delim = " ON ";

                sb.AppendFormat(CultureInfo.InvariantCulture, @"{0} [T].[{1}] = [S].[{2}]", delim,
                                                                                          keys[i].PylonName,
                                                                                          keys[i].PylonName
                                                                                          );
                c++;
            }
            sb.AppendLine("");
            sb.AppendLine(@"  /* ----  SQL AFECTOR EDataD DataTEM CMD ------ */  ");
            return sb;
        }

        public class AfectUpdateException : Exception
        {
            public AfectUpdateException(string message)
                : base(message)
            {

            }
        }

    }
}
