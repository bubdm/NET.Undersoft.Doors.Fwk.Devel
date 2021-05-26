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
    public class AfectInsert
    {
        private SqlConnection _cn;

        public AfectInsert(SqlConnection cn)
        {
            _cn = cn;
        }
        public AfectInsert(string cnstring)
        {
            _cn = new SqlConnection(cnstring);
        }

        public DataSphere Insert(DataTier[] tiers, bool keysFromTrellis = false, bool buildMapping = false, bool updateKeys = false, string[] updateExcept = null, BulkPrepareType tempType = BulkPrepareType.Trunc)
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
                    DataSphere nSet = new DataSphere("AfectInsert");

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

                        string qry = BulkInsertQuery(dbName, it.TrellName, nMap.DbTableName, ic, ik, updateKeys).ToString();
                        sb.Append(qry);
                        sb.AppendLine(@"  /* ----  TABLE BULK END CMD ------ */  ");
                    }
                    sb.AppendLine(@"  /* ----  SQL AFECTOR END CMD ------ */  ");

                    DataSphere bDataSphere = afad.ExecuteInsert(sb.ToString(), tiers, true);

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
                throw new AfectInsertException(ex.ToString());
            }
        }

        public DataSphere BatchInsert(DataTier[] tiers, int batchSize = 1000)
        {
            try
            {
                DataTrellis it = new DataTrellis();
                IList<AfectMapping> nMaps = new List<AfectMapping>();
                AfectAdapter afad = new AfectAdapter(_cn);
                StringBuilder sb = new StringBuilder();
                DataSphere iSet = new DataSphere("AfectInsert");
                sb.AppendLine(@"  /* ----  SQL AFECTOR START CMD ------ */  ");
                int count = 0;
                foreach (DataTier ir in tiers)
                {
                    if (ir.Trell.TrellName != it.TrellName)
                    {
                        it = ir.Trell;
                        nMaps = it.AfectMap;
                    }

                    foreach (AfectMapping nMap in nMaps)
                    {
                        DataPylon[] ic = it.Pylons.AsEnumerable().Where(c => nMap.ColumnOrdinal.Contains(c.Ordinal)).ToArray();
                        DataPylon[] ik = it.Pylons.AsEnumerable().Where(c => nMap.KeyOrdinal.Contains(c.Ordinal)).ToArray();                   

                        string qry = BatchInsertQuery(ir, nMap.DbTableName, ic, ik).ToString();
                        sb.Append(qry);
                        count++;
                    }
                    if (count >= batchSize)
                    {
                        sb.AppendLine(@"  /* ----  DATA BATCH END CMD ------ */  ");
                        DataSphere bDataSphere = afad.ExecuteInsert(sb.ToString(), tiers);
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

                DataSphere rDataSphere = afad.ExecuteInsert(sb.ToString(), tiers);

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
                throw new AfectInsertException(ex.ToString());
            }
        }
        public DataSphere BatchInsert(DataTier[] tiers, bool buildMapping, int batchSize = 1000)
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
                        AfectMapper imapper = new AfectMapper(it);
                        nMaps = it.AfectMap;
                    }

                    foreach (AfectMapping nMap in nMaps)
                    {
                        DataPylon[] ic = it.Pylons.AsEnumerable().Where(c => nMap.ColumnOrdinal.Contains(c.Ordinal)).ToArray();
                        DataPylon[] ik = it.Pylons.AsEnumerable().Where(c => nMap.KeyOrdinal.Contains(c.Ordinal)).ToArray();

                        string qry = BatchInsertQuery(ir, nMap.DbTableName, ic, ik).ToString();
                        sb.Append(qry);
                        count++;
                    }
                    if (count >= batchSize)
                    {
                        sb.AppendLine(@"  /* ----  DATA BATCH END CMD ------ */  ");
                        DataSphere bDataSphere = afad.ExecuteInsert(sb.ToString(), tiers);
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

                DataSphere rDataSphere = afad.ExecuteInsert(sb.ToString(), tiers);

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
                throw new AfectInsertException(ex.ToString());
            }
        }

        public int SimpleInsert(DataTier[] tiers, int batchSize = 1000)
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
                        nMaps = it.AfectMap;
                    }

                    foreach (AfectMapping nMap in nMaps)
                    {
                        DataPylon[] ic = it.Pylons.AsEnumerable().Where(c => nMap.ColumnOrdinal.Contains(c.Ordinal)).ToArray();
                        DataPylon[] ik = it.Pylons.AsEnumerable().Where(c => nMap.KeyOrdinal.Contains(c.Ordinal)).ToArray();                    

                        string qry = BatchInsertQuery(ir, nMap.DbTableName, ic, ik).ToString();
                        sb.Append(qry);
                        count++;
                    }
                    if (count >= batchSize)
                    {
                        sb.AppendLine(@"  /* ----  DATA BATCH END CMD ------ */  ");
                        intAfect += afad.ExecuteInsert(sb.ToString());
                      
                        sb.Clear();
                        sb.AppendLine(@"  /* ----  SQL AFECTOR START CMD ------ */  ");
                        count = 0;
                    }
                }
                sb.AppendLine(@"  /* ----  DATA AFECTOR END CMD ------ */  ");

                intAfect += afad.ExecuteInsert(sb.ToString());
                             
                return intAfect;
            }
            catch (SqlException ex)
            {
                _cn.Close();
                throw new AfectInsertException(ex.ToString());
            }
        }
        public int SimpleInsert(DataTier[] tiers, bool buildMapping, int batchSize = 1000)
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
                        AfectMapper imapper = new AfectMapper(it);
                        nMaps = it.AfectMap;
                    }

                    foreach (AfectMapping nMap in nMaps)
                    {;
                        DataPylon[] ic = it.Pylons.AsEnumerable().Where(c => nMap.ColumnOrdinal.Contains(c.Ordinal)).ToArray();
                        DataPylon[] ik = it.Pylons.AsEnumerable().Where(c => nMap.KeyOrdinal.Contains(c.Ordinal)).ToArray();

                        string qry = BatchInsertQuery(ir, nMap.DbTableName, ic, ik).ToString();
                        sb.Append(qry);
                        count++;
                    }
                    if (count >= batchSize)
                    {
                        sb.AppendLine(@"  /* ----  DATA BATCH END CMD ------ */  ");
                      
                        intAfect += afad.ExecuteInsert(sb.ToString());

                        sb.Clear();
                        sb.AppendLine(@"  /* ----  SQL AFECTOR START CMD ------ */  ");
                        count = 0;
                    }
                }
                sb.AppendLine(@"  /* ----  DATA AFECTOR END CMD ------ */  ");

                intAfect += afad.ExecuteInsert(sb.ToString());
                return intAfect;
            }
            catch (SqlException ex)
            {
                _cn.Close();
                throw new AfectInsertException(ex.ToString());
            }
        }

        public StringBuilder BatchInsertQuery(DataTier tier, string tableName, DataPylon[] columns, DataPylon[] keys, bool updateKeys = true)
        {
            StringBuilder sbCols = new StringBuilder(), sbVals = new StringBuilder(), sbQry = new StringBuilder();
            string tName = tableName;
            DataTier ir = tier;
            object[] ia = ir.DataArray;
            DataPylon[] ic = columns;
            DataPylon[] ik = keys;

            sbCols.AppendLine(@"  /* ---- DATA AFECTOR START DataTEM CMD ------ */  ");
            sbCols.Append("INSERT INTO " + tableName + " (");
            sbVals.Append(@") OUTPUT inserted.* VALUES (");
            bool isUpdateCol = false;
            string delim = "";
            int c = 0;
            for (int i = 0; i < columns.Length; i++)
            {


                if (columns[i].PylonName.ToLower() == "updated")
                    isUpdateCol = true;
                if (ia[columns[i].Ordinal] != DBNull.Value && !columns[i].isIdentity)
                {
                    if (c > 0)
                        delim = ",";
                    sbCols.AppendFormat(CultureInfo.InvariantCulture, @"{0}[{1}]", delim,
                                                                                   columns[i].PylonName                                                                                         
                                                                                   );
                    sbVals.AppendFormat(CultureInfo.InvariantCulture, @"{0} {1}{2}{1}", delim,
                                                                                        (columns[i].DataType == typeof(string) ||
                                                                                        columns[i].DataType == typeof(DateTime)) ? "'" : "",
                                                                                        (columns[i].DataType != typeof(string)) ?
                                                                                        Convert.ChangeType(ia[columns[i].Ordinal], columns[i].DataType) :
                                                                                        ia[columns[i].Ordinal].ToString().Replace("'", "''")
                                                                                        );
                    c++;
                }
            }

            if (DbHand.Schema.DataDbTables[tableName].DataDbColumns.Have("updated") && !isUpdateCol)
            {
                sbCols.AppendFormat(CultureInfo.InvariantCulture, ", [updated]", DateTime.Now);
                sbVals.AppendFormat(CultureInfo.InvariantCulture, ", '{0}'", DateTime.Now);
            }          
                if (columns.Length > 0)
                    delim = ",";
                else
                    delim = "";
                c = 0;
                for (int i = 0; i < keys.Length; i++)
                {

                    if (ia[keys[i].Ordinal] != DBNull.Value && !keys[i].isIdentity)
                    {
                        if (c > 0)
                            delim = ",";
                    sbCols.AppendFormat(CultureInfo.InvariantCulture, @"{0}[{1}]",  delim,
                                                                                    keys[i].PylonName
                                                                                    );
                    sbVals.AppendFormat(CultureInfo.InvariantCulture, @"{0} {1}{2}{1}", delim,
                                                                                        (keys[i].DataType == typeof(string) ||
                                                                                        keys[i].DataType == typeof(DateTime)) ? "'" : "",
                                                                                        (keys[i].DataType != typeof(string)) ?
                                                                                        Convert.ChangeType(ia[keys[i].Ordinal], keys[i].DataType) :
                                                                                        ia[keys[i].Ordinal].ToString().Replace("'", "''")
                                                                                        );
                    c++;
                    }
                }
            sbQry.Append(sbCols.ToString() + sbVals.ToString() + ") ");
            sbQry.AppendLine(@"  /* ----  SQL AFECTOR END ITEM CMD ------ */  ");
            return sbQry;
        }
        public StringBuilder BulkInsertQuery(string DBName, string buforName, string tableName, DataPylon[] columns, DataPylon[] keys, bool updateKeys = true)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbv = new StringBuilder();
            string bName = buforName;
            string tName = tableName;
            DataPylon[] pylons = keys.Concat(columns).ToArray();           
            string dbName = DBName;
            sb.AppendLine(@"  /* ---- DATA AFECTOR START ITEM CMD ------ */");
            sb.AppendFormat(@"INSERT INTO [{0}].[dbo].[" + tName + "] (", dbName);
            sbv.Append(@"SELECT ");
            bool isUpdateCol = false;
            string delim = "";
            int c = 0;
            for (int i = 0; i < pylons.Length; i++)
            {

                if (pylons[i].PylonName.ToLower() == "updated")
                    isUpdateCol = true;

                if (c > 0)
                    delim = ",";
                sb.AppendFormat(CultureInfo.InvariantCulture, @"{0}[{1}]", delim, pylons[i].PylonName);
                sbv.AppendFormat(CultureInfo.InvariantCulture, @"{0}[S].[{1}]", delim, pylons[i].PylonName);
                c++;
            }
            sb.AppendFormat(CultureInfo.InvariantCulture, @") OUTPUT inserted.* {0}", sbv.ToString());
            sb.AppendFormat(" FROM [tempdb].[dbo].[{0}] AS S ", bName, dbName, tName);
            sb.AppendLine("");
            sb.AppendLine(@"  /* ----  SQL AFECTOR END ITEM CMD ------ */  ");
            sbv.Clear();
            return sb;
        }
    }

    public class AfectInsertException : Exception
    {
        public AfectInsertException(string message)
            : base(message)
        {

        }
    }
}
