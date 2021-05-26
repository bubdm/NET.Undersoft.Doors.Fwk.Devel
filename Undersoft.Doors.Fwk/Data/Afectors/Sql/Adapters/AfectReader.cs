using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Doors.Data;
using System.Doors;
using System.Globalization;
using System.Reflection;
using System.ComponentModel;

namespace System.Doors.Data.Afectors.Sql
{
    public interface IDataReader<T> where T : class
    {
        DataTrellis InjectRead(string tableName, bool isCube,
                       ICollection<string> keyNames = null, 
                       ICollection<string> indexNames = null,
                       DataDeposit deposit = null);
        DataTrellis InjectRead(DataTrellis table,
                       ICollection<string> keyNames = null, 
                       ICollection<string> indexNames = null);

        DataSphere UpdateRead(ICollection<DataTier>  toUpdateDataTiers);

        DataSphere InsertRead(ICollection<DataTier> toInsertDataTiers);
    }

    public class AfectReader<T> : IDataReader<T> where T : class
    {
        private IDataReader dr;

        public AfectReader(IDataReader _dr)
        {
            dr = _dr;
        }

        public DataTrellis InjectRead(string tableName, bool isCube,
                              ICollection<string> keyNames = null,
                              ICollection<string> sqinNames = null,
                              DataDeposit deposit = null)
        {
            DataTable schema = dr.GetSchemaTable();
            DataTrellis trell = new DataTrellis(tableName, true, isCube);
            bool IsCube = isCube;
            if (deposit != null)
            {
                trell.Deposit = deposit;
                deposit.Box.Prime = trell;
            }

            AggregateOperand parsedAggreg = new AggregateOperand();
            trell.TrellName = tableName;
            List<DataPylon> columns = new List<DataPylon>(schema.Rows.Cast<DataRow>().AsEnumerable().AsQueryable()
                                                .Where(n => n["ColumnName"].ToString() != "NOID").Select(c => 
                                                new DataPylon(Type.GetType(c["DataType"].ToString()), trell,
                                                                        c["ColumnName"].ToString(), c["ColumnName"].ToString())
                                                {
                                                    PylonSize = Convert.ToInt32(c["ColumnSize"]),
                                                    Ordinal = Convert.ToInt32(c["ColumnOrdinal"]),
                                                    isIdentity = Convert.ToBoolean(c["IsIdentity"]),
                                                    isAutoincrement = Convert.ToBoolean(c["IsAutoincrement"]),
                                                    isDBNull = Convert.ToBoolean(c["AllowDBNull"]),
                                                    JoinOperand = (Enum.TryParse<AggregateOperand>(c["ColumnName"].ToString()
                                                                       .Split('#')[0].ToUpper(), true, out parsedAggreg)) ?
                                                                        parsedAggreg : AggregateOperand.None,
                                                    TotalOperand = (Enum.TryParse<AggregateOperand>(c["ColumnName"].ToString()
                                                                        .Split('=')[0].ToUpper(), true, out parsedAggreg)) ?
                                                                         parsedAggreg : AggregateOperand.None,
                                                    isCube = IsCube
                                                }
                                                ).ToList());

            trell.Pylons.AddRange(columns);

            bool firstquid = true;
            if (sqinNames != null && sqinNames.Count > 0)
                foreach (string s in sqinNames)
                {
                    DataPylon dp = trell.Pylons.GetPylon(s);
                    if (dp != null)
                    {
                        if (!dp.isQuid)
                        {
                            if (firstquid)
                                dp.isIdentity = true;
                            dp.isQuid = true;
                        }
                        firstquid = false;
                    }
                }

            Type model = trell.Model;

            bool takeDbKeys = false;
            if (keyNames != null)
                if (keyNames.Count > 0)
                    trell.PrimeKey = trell.Pylons.GetPylons(keyNames);
                else
                    takeDbKeys = true;
            else
                takeDbKeys = true;

            if (takeDbKeys)
                if (DbHand.Schema != null)
                    if (DbHand.Schema.DataDbTables.List.Count > 0)
                    {
                        List<DbTable> dbtabs = DbHand.Schema.DataDbTables.List;
                        DataPylon[] pKeys = columns
                                .Where(c => dbtabs.SelectMany(t =>
                                 t.GetKeyForDataTable.Select(d => d.PylonName))
                                .Contains(c.PylonName)).ToArray();
                        if (pKeys.Length > 0)
                            trell.PrimeKey = pKeys;
                    }

            if (!IsCube)
            {
                bool firstLoop = true;
                int[] quidpyls = trell.QuidPylons;
                int columnsCount = 0;
                object[] itemArray = null;
                trell.Tiers.IsBatch = true;
                bool hasNoidColumn = false;
                int noidord = trell.NoidOrdinal;
                while (dr.Read())
                {
                    if (firstLoop)
                    {
                        columnsCount = dr.FieldCount;
                        itemArray = new object[columnsCount];
                        firstLoop = false;
                        hasNoidColumn = noidord < columnsCount;
                    }

                    DataTier tier = new DataTier(trell);
                    dr.GetValues(itemArray);
                    object iax = null;
                    foreach (int x in quidpyls)
                    {
                        iax = itemArray[x];
                        if (iax != DBNull.Value)
                            itemArray[x] = new Quid((long)iax);
                        else
                            itemArray[x] = Quid.Empty;
                    }

                    if (hasNoidColumn)
                    {
                        iax = itemArray[noidord];
                        if (iax != DBNull.Value)
                            itemArray[noidord] = new Noid((byte[])iax);
                    }
                    else
                    {
                        itemArray.Concat(new object[] { Noid.Empty });
                    }
                    
                    //itemArray.Select((a, y) => itemArray[y] = (a == DBNull.Value) ?  a.GetType().GetDefaultValue() : a).ToArray();

                    tier.PrimeArray = itemArray;
                    trell.Tiers.AddInner(tier);
                }
                itemArray = null;
                trell.Tiers.State.withPropagate = false;
                trell.Tiers.State.Saved = true;
                trell.Tiers.State.withPropagate = true;
            }
            dr.Dispose();
            return trell;
        }     

        public DataTrellis InjectRead(DataTrellis table,
                              ICollection<string> keyNames = null, 
                              ICollection<string> sqinNames = null)
        {
            DataTable schema = dr.GetSchemaTable();
            DataTrellis trell = table;
            bool IsCube = table.IsCube;
            AggregateOperand parsedAggreg = new AggregateOperand();
            List<DataPylon> columns = new List<DataPylon>(schema.Rows.Cast<DataRow>().AsEnumerable().AsQueryable()
                                                .Where(n => n["ColumnName"].ToString() != "NOID").Select(c =>
                                                new DataPylon(Type.GetType(c["DataType"].ToString()), trell,
                                                                        c["ColumnName"].ToString(), c["ColumnName"].ToString())
                                                {
                                                    PylonSize = Convert.ToInt32(c["ColumnSize"]),
                                                    Ordinal = Convert.ToInt32(c["ColumnOrdinal"]),
                                                    isIdentity = Convert.ToBoolean(c["IsIdentity"]),
                                                    isAutoincrement = Convert.ToBoolean(c["IsAutoincrement"]),
                                                    isDBNull = Convert.ToBoolean(c["AllowDBNull"]),
                                                    JoinOperand = (Enum.TryParse<AggregateOperand>(c["ColumnName"].ToString()
                                                                       .Split('#')[0].ToUpper(), true, out parsedAggreg)) ?
                                                                        parsedAggreg : AggregateOperand.None,
                                                    TotalOperand = (Enum.TryParse<AggregateOperand>(c["ColumnName"].ToString()
                                                                        .Split('=')[0].ToUpper(), true, out parsedAggreg)) ?
                                                                         parsedAggreg : AggregateOperand.None,
                                                    isCube = IsCube
                                                }
                                                ).ToList());

            trell.Pylons.AddRange(columns);

            bool firstquid = true;
            if (sqinNames != null && sqinNames.Count > 0)
                foreach (string s in sqinNames)
                {
                    DataPylon dp = trell.Pylons.GetPylon(s);
                    if (dp != null)
                    {
                        if (!dp.isQuid)
                        {
                            if (firstquid)
                                dp.isIdentity = true;
                            dp.isQuid = true;
                        }
                        firstquid = false;
                    }
                }

            Type model = trell.Model;

            bool takeDbKeys = false;
            if (keyNames != null)
                if (keyNames.Count > 0)
                    trell.PrimeKey = trell.Pylons.GetPylons(keyNames);
                else
                    takeDbKeys = true;
            else
                takeDbKeys = true;

            if (takeDbKeys)
                if (DbHand.Schema != null)
                    if (DbHand.Schema.DataDbTables.List.Count > 0)
                    {
                        List<DbTable> dbtabs = DbHand.Schema.DataDbTables.List;
                        DataPylon[] pKeys = columns
                                .Where(c => dbtabs.SelectMany(t =>
                                 t.GetKeyForDataTable.Select(d => d.PylonName))
                                .Contains(c.PylonName)).ToArray();
                        if (pKeys.Length > 0)
                            trell.PrimeKey = pKeys;
                    }

            trell.Group = DataGroup.None;

            if (!IsCube)
            {
                bool firstLoop = true;
                int[] quidpyls = trell.QuidPylons;
                int columnsCount = 0;
                object[] itemArray = null;
                while (dr.Read())
                {
                    if (firstLoop)
                    {
                        columnsCount = dr.FieldCount;
                        itemArray = new object[columnsCount];
                        firstLoop = false;
                    }

                    DataTier tier = new DataTier(trell);
                    dr.GetValues(itemArray);
                    object iax = null;
                    foreach (int x in quidpyls)
                    {
                        iax = itemArray[x];
                        if (iax != DBNull.Value)
                            itemArray[x] = new Quid((long)iax);
                        else
                            itemArray[x] = Quid.Empty;
                    }

                    iax = itemArray[trell.NoidOrdinal];
                    if (iax != DBNull.Value)
                        itemArray[trell.NoidOrdinal] = new Noid((byte[])iax);

                    tier.PrimeArray = itemArray;
                    trell.Tiers.PutInner(tier, false);
                }
                itemArray = null;
                trell.Tiers.State.withPropagate = false;
                trell.Tiers.State.Saved = true;
                trell.Tiers.State.withPropagate = true;
            }
            dr.Dispose();
            return trell;
        }

        public DataSphere UpdateRead(ICollection<DataTier> toUpdateDataTiers)
        {
            DataCreator ic = new DataCreator();

            DataTrellis trell = new DataTrellis("Update");
            HashSet<int> ko = new HashSet<int>();
            if (toUpdateDataTiers.Count > 0)
            {
                trell.Pylons = toUpdateDataTiers.First().Trell.Pylons;
                trell.KeyId = toUpdateDataTiers.First().Trell.KeyId;
                trell.PrimeKey = toUpdateDataTiers.First().Trell.PrimeKey;
                ko = new HashSet<int>(toUpdateDataTiers.First().Trell.KeyId);
            }
            ICollection<DataTier> toUpdateArray = toUpdateDataTiers;
            List<DataTier> updatedList = new List<DataTier>();
            List<DataTier> toInsertList = new List<DataTier>();

            int i = 0;
            do
            {
                int columnsCount = trell.Pylons.Count;
                int[] quidpyls = trell.QuidPylons;

                if (i == 0 && columnsCount == 0)
                {
                    DataTable schema = dr.GetSchemaTable();
                    trell = DataTrellisFromSchema(schema, trell.PrimeKey, true);
                    columnsCount = trell.Pylons.Count;
                }
                object[] itemArray = new object[columnsCount];
                while (dr.Read())
                {
                    if (columnsCount != (int)(dr.FieldCount / 2))
                    {
                        DataTable schema = dr.GetSchemaTable();
                        trell = DataTrellisFromSchema(schema, trell.PrimeKey, true);
                        columnsCount = trell.Pylons.Count;
                        itemArray = new object[columnsCount];
                    }
                    object iax = null;
                    dr.GetValues(itemArray);
                    foreach (int x in quidpyls)
                    {
                        iax = itemArray[x];
                        if (iax != DBNull.Value)
                            itemArray[x] = new Quid((long)iax);
                        else
                            itemArray[x] = Quid.Empty;
                    }

                    iax = itemArray[trell.NoidOrdinal];
                    if (iax != DBNull.Value)
                        itemArray[trell.NoidOrdinal] = new Noid((byte[])iax);

                    DataTier row = new DataTier(trell);
                    row.PrimeArray = itemArray;
                    updatedList.Add(row);
                }
                HashSet<int> updka = new HashSet<int>(updatedList.Select(k => 
                                                      k.Keys.GetShahCode32()).ToArray(), 
                                                      new HashComparer());
                foreach (DataTier ir in toUpdateArray)
                    if (!updka.Contains(ir.Keys.GetShahCode32()))
                        toInsertList.Add(ir);

            } while (dr.NextResult());

            DataSphere iSet = new DataSphere("UpdateResult");

            DataTrellis toins = new DataTrellis("Failed");
            toins.State.Expeled = true;
            toins.Tiers.AddViewRange(toInsertList.ToArray());
            iSet.Trells.Add(toins);

            DataTrellis upd = new DataTrellis("Updated");
            upd.State.Expeled = true;
            upd.Tiers.AddViewRange(updatedList.ToArray());
            iSet.Trells.Add(upd);

            return iSet;
        }

        public DataSphere InsertRead(ICollection<DataTier> toInsertDataTiers)
        {
            DataCreator ic = new DataCreator();

            DataTrellis trell = new DataTrellis("Insert");
            HashSet<int> ko = new HashSet<int>();
            if (toInsertDataTiers.Count > 0)
            {
                trell.Pylons = toInsertDataTiers.First().Trell.Pylons;
                trell.KeyId = toInsertDataTiers.First().Trell.KeyId;
                trell.PrimeKey = toInsertDataTiers.First().Trell.PrimeKey;
                ko = new HashSet<int>(toInsertDataTiers.First().Trell.KeyId);
            }
            ICollection<DataTier> toInsertArray = toInsertDataTiers;
            List<DataTier> insertedList = new List<DataTier>();
            List<DataTier> brokenList = new List<DataTier>();

            int i = 0;
            do
            {
                int columnsCount = trell.Pylons.Count;
                int[] quidpyls = trell.QuidPylons;

                if (i == 0 && trell.Pylons.Count == 0)
                {
                    trell = DataTrellisFromSchema(dr.GetSchemaTable(), trell.PrimeKey);
                    columnsCount = trell.Pylons.Count;
                }
                object[] itemArray = new object[columnsCount];

                while (dr.Read())
                {
                    if (columnsCount != dr.FieldCount)
                    {
                        trell = DataTrellisFromSchema(dr.GetSchemaTable(), trell.PrimeKey);
                        columnsCount = trell.Pylons.Count;
                        itemArray = new object[columnsCount];
                    }
                    object iax = null;
                    dr.GetValues(itemArray);
                    foreach (int x in quidpyls)
                    {
                        iax = itemArray[x];
                        if (iax != DBNull.Value)
                            itemArray[x] = new Quid((long)iax);
                        else
                            itemArray[x] = Quid.Empty;
                    }

                    iax = itemArray[trell.NoidOrdinal];
                    if (iax != DBNull.Value)
                        itemArray[trell.NoidOrdinal] = new Noid((byte[])iax);

                    DataTier row = new DataTier(trell);
                    row.PrimeArray = itemArray;
                    insertedList.Add(row);
                }
                HashSet<int> updka = new HashSet<int>(insertedList.Select(k =>
                                                      k.Keys.GetShahCode32()).ToArray(),
                                                      new HashComparer());

                foreach (DataTier ir in toInsertDataTiers)
                    if (!updka.Contains(ir.Keys.GetShahCode32()))
                        brokenList.Add(ir);

            } while (dr.NextResult());

            DataSphere iSet = new DataSphere("InsertResult");

            DataTrellis toins = new DataTrellis("Failed");
            toins.State.Expeled = true;
            toins.Tiers.AddViewRange(brokenList.ToArray());
            iSet.Trells.Add(toins);

            DataTrellis upd = new DataTrellis("Inserted");
            upd.State.Expeled = true;
            upd.Tiers.AddViewRange(insertedList.ToArray());
            iSet.Trells.Add(upd);

            return iSet;
        }    

        public DataTrellis DataTrellisFromSchema(DataTable schema, 
                                           ICollection<DataPylon> operColumns, 
                                           bool insAndDel = false)
        {
          
            DataTrellis trell = new DataTrellis("Schema");
            List<DataPylon> columns = new List<DataPylon>(schema.Rows.Cast<DataRow>().AsEnumerable().AsQueryable()
                                                .Select(c => new DataPylon(Type.GetType(c["DataType"].ToString()), trell, c["ColumnName"].ToString(), c["ColumnName"].ToString())
                                                {
                                                    Ordinal = Convert.ToInt32(c["ColumnOrdinal"]),
                                                    isIdentity = Convert.ToBoolean(c["IsIdentity"]),
                                                }).ToList());
            if(insAndDel)
            for (int i = 0; i < (int)(columns.Count / 2); i++)
                trell.Pylons.Add(columns[i]);
            else
                trell.Pylons.AddRange(columns);
            List<DbTable> dbtabs = DbHand.Schema.DataDbTables.List;
            DataPylon[] pKeys = columns.Where(c => dbtabs.SelectMany(t => t.GetKeyForDataTable.Select(d => d.PylonName)).Contains(c.PylonName) && operColumns.Select(o => o.PylonName).Contains(c.PylonName)).ToArray();
            if (pKeys.Length > 0)
                trell.PrimeKey = pKeys;

            return trell;
        }

    }
   
}



