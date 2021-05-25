using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Dors.Data;

namespace System.Dors.Data.Afectors.Sql
{
    
    public class AfectMapper
    {
        public DataTier[] TiersMapped { get; set; }
        public AfectMapper(DataTier[] DataTrellissFromDataTiers, bool KeysFromTrellis = false, string[] dbTableNames = null, string tablePrefix = "")
        {

            DataTier[] irs = DataTrellissFromDataTiers;
            if (irs.Length <= 0)
                return;
            try
            {
                bool mixedMode = false;
                string tName = "", dbtName = "", prefix = tablePrefix;
                List<string> dbtNameMixList = new List<string>();
                if (dbTableNames != null)
                {
                    foreach (string dbTableName in dbTableNames)
                        if (DbHand.Schema.DataDbTables.Have(dbTableName))
                            dbtNameMixList.Add(dbTableName);
                    if (dbtNameMixList.Count > 0)
                        mixedMode = true;
                }

                DataTrellis t = new DataTrellis();
                foreach (DataTier tier in irs)
                {
                    if (t.TrellName != tier.Trell.TrellName)
                    {
                        t = tier.Trell;
                        tName = t.TrellName;
                        if (!mixedMode)
                        {
                            if (!DbHand.Schema.DataDbTables.Have(tName))
                            {
                                if (DbHand.Schema.DataDbTables.Have(prefix + tName))
                                    dbtName = prefix + tName;
                            }
                            else
                                dbtName = tName;
                            if (!string.IsNullOrEmpty(dbtName))
                            {
                                if (!KeysFromTrellis)
                                {
                                    HashSet<int> colOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                            !DbHand.Schema.DataDbTables[dbtName].DbPrimaryKey.Select(pk => pk.ColumnName)
                                                                                                                 .Contains(c.PylonName)).Select(o => o.Ordinal));
                                    HashSet<int> keyOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DbPrimaryKey.Select(pk => pk.ColumnName)
                                                                                                                 .Contains(c.PylonName)).Select(o => o.Ordinal));
                                    AfectMapping iAfectMap = new AfectMapping(dbtName, keyOrdinal, colOrdinal);
                                    t.AddMap(iAfectMap);
                                }
                                else
                                {
                                    HashSet<int> colOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                                  !c.isKey).Select(o => o.Ordinal));
                                    HashSet<int> keyOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                                  c.isKey).Select(o => o.Ordinal));
                                    AfectMapping iAfectMap = new AfectMapping(dbtName, keyOrdinal, colOrdinal);
                                    t.AddMap(iAfectMap);
                                }
                            }
                        }
                        else
                        {
                            if (!KeysFromTrellis)
                            {
                                foreach (string dbtNameMix in dbtNameMixList)
                                {
                                    dbtName = dbtNameMix;
                                    HashSet<int> colOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                                  !DbHand.Schema.DataDbTables[dbtName].DbPrimaryKey.Select(pk => pk.ColumnName)
                                                                                                                .Contains(c.PylonName)).Select(o => o.Ordinal));
                                    HashSet<int> keyOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DbPrimaryKey.Select(pk => pk.ColumnName)
                                                                                                                 .Contains(c.PylonName)).Select(o => o.Ordinal));
                                    if (keyOrdinal.Count == 0)
                                        keyOrdinal = new HashSet<int>(t.PrimeKey.Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName)).Select(o => o.Ordinal));
                                    AfectMapping iAfectMap = new AfectMapping(dbtName, keyOrdinal, colOrdinal);
                                    t.AddMap(iAfectMap);
                                }
                            }
                            else
                            {
                                foreach (string dbtNameMix in dbtNameMixList)
                                {
                                    dbtName = dbtNameMix;
                                    HashSet<int> colOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                                  !c.isKey).Select(o => o.Ordinal));
                                    HashSet<int> keyOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                                  c.isKey).Select(o => o.Ordinal));
                                    if (keyOrdinal.Count == 0)
                                        keyOrdinal = new HashSet<int>(t.PrimeKey.Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName)).Select(o => o.Ordinal));
                                    AfectMapping iAfectMap = new AfectMapping(dbtName, keyOrdinal, colOrdinal);
                                    t.AddMap(iAfectMap);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new AfectMapperException(ex.ToString());
            }
            TiersMapped = irs;
        }
        public AfectMapper(DataTrellis trell, bool KeysFromTrellis = false, string[] dbTableNames = null, string tablePrefix = "")
        {
            try
            {
                bool mixedMode = false;
                string tName = "", dbtName = "", prefix = tablePrefix;
                List<string> dbtNameMixList = new List<string>();
                if (dbTableNames != null)
                {
                    foreach (string dbTableName in dbTableNames)
                        if (DbHand.Schema.DataDbTables.Have(dbTableName))
                            dbtNameMixList.Add(dbTableName);
                    if (dbtNameMixList.Count > 0)
                        mixedMode = true;
                }
                DataTrellis t = trell;
                tName = t.TrellName;
                if (!mixedMode)
                {
                    if (!DbHand.Schema.DataDbTables.Have(tName))
                    {
                        if (DbHand.Schema.DataDbTables.Have(prefix + tName))
                            dbtName = prefix + tName;
                    }
                    else
                        dbtName = tName;
                    if (!string.IsNullOrEmpty(dbtName))
                    {
                        if (!KeysFromTrellis)
                        {
                            HashSet<int> colOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                    !DbHand.Schema.DataDbTables[dbtName].DbPrimaryKey.Select(pk => pk.ColumnName)
                                                                                                         .Contains(c.PylonName)).Select(o => o.Ordinal));
                            HashSet<int> keyOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DbPrimaryKey.Select(pk => pk.ColumnName)
                                                                                                         .Contains(c.PylonName)).Select(o => o.Ordinal));
                            AfectMapping iAfectMap = new AfectMapping(dbtName, keyOrdinal, colOrdinal);
                            t.AddMap(iAfectMap);
                        }
                        else
                        {
                            HashSet<int> colOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                          !c.isKey).Select(o => o.Ordinal));
                            HashSet<int> keyOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                          c.isKey).Select(o => o.Ordinal));
                            AfectMapping iAfectMap = new AfectMapping(dbtName, keyOrdinal, colOrdinal);
                            t.AddMap(iAfectMap);
                        }
                    }
                }
                else
                {
                    if (!KeysFromTrellis)
                    {
                        foreach (string dbtNameMix in dbtNameMixList)
                        {
                            dbtName = dbtNameMix;
                            HashSet<int> colOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                          !DbHand.Schema.DataDbTables[dbtName].DbPrimaryKey.Select(pk => pk.ColumnName)
                                                                                                        .Contains(c.PylonName)).Select(o => o.Ordinal));
                            HashSet<int> keyOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DbPrimaryKey.Select(pk => pk.ColumnName)
                                                                                                         .Contains(c.PylonName)).Select(o => o.Ordinal));
                            if (keyOrdinal.Count == 0)
                                keyOrdinal = new HashSet<int>(t.PrimeKey.Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName)).Select(o => o.Ordinal));
                            AfectMapping iAfectMap = new AfectMapping(dbtName, keyOrdinal, colOrdinal);
                            t.AddMap(iAfectMap);
                        }
                    }
                    else
                    {
                        foreach (string dbtNameMix in dbtNameMixList)
                        {
                            dbtName = dbtNameMix;
                            HashSet<int> colOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                          !c.isKey).Select(o => o.Ordinal));
                            HashSet<int> keyOrdinal = new HashSet<int>(t.Pylons.AsEnumerable().Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName) &&
                                                                                                          c.isKey).Select(o => o.Ordinal));
                            if (keyOrdinal.Count == 0)
                                keyOrdinal = new HashSet<int>(t.PrimeKey.Where(c => DbHand.Schema.DataDbTables[dbtName].DataDbColumns.Have(c.PylonName)).Select(o => o.Ordinal));
                            AfectMapping iAfectMap = new AfectMapping(dbtName, keyOrdinal, colOrdinal);
                            t.AddMap(iAfectMap);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new AfectMapperException(ex.ToString());
            }
            TiersMapped = trell.Tiers.AsArray();

        }

        public class AfectMapperException : Exception
        {
            public AfectMapperException(string message)
                : base(message)
            {

            }
        }

    }
}
