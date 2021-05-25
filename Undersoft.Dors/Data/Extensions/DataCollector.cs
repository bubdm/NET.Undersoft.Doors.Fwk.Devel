using System.Collections.Generic;
using System.Linq;
using System.Dors;                            

namespace System.Dors.Data
{
    public interface IDataCollector
    {
        DataTiers Absorb(object source, bool addnew = true);
        DataTiers Collect(object source, string[] ColumnNames, List<object[]> Values);
    }

    public static class DataCollector
    {
        public static DataTiers Absorb(this DataTrellis ntab, DataTrellis fromTrellis, bool addnew = true)
        {
            if (fromTrellis.Tiers.Count > 0)
              return  ntab.Tiers.Absorb(fromTrellis.Tiers, addnew);
            return null;

        }
        public static DataTiers Absorb(this DataTrellis ntab, DataTiers fromTiers, bool addnew = true)
        {
            if (fromTiers.Count > 0)
               return ntab.Tiers.Absorb(fromTiers, addnew);
            return null;
        }
        public static DataTiers Absorb(this DataTrellis ntab, DataTier[] fromTiers, bool addnew = true)
        {
            if (fromTiers.Length > 0)
              return  ntab.Tiers.Absorb(fromTiers, addnew);
            return null;
        }
        public static DataTiers Absorb(this DataTrellis ntab, object[][] fromValues, bool addnew = true)
        {
            if (fromValues.Length > 0)
               return ntab.Tiers.Absorb(fromValues, addnew);
            return null;
        }
        public static DataTiers Absorb(this DataTrellis ntab, object[] fromValues, bool addnew = true)
        {
            if (fromValues.Length > 0)
                return ntab.Tiers.Absorb(new object[][] { fromValues }, addnew);
            return null;

        }
        public static DataTiers Absorb(this DataTrellis ntab, DataTier fromTier, bool addnew = true)
        {
            if (fromTier != null)
               return ntab.Tiers.Absorb(fromTier, addnew);
            return null;

        }   

        public static DataTier[] Collect(this DataDeposit trell, string ColumnName, object Value)
        {
            return trell.Box.Prime.TiersView.Collect(ColumnName, Value);
        }
        public static DataTier[] Collect(this DataDeposit trell, int ColumnOrdinal, object Value)
        {
            return trell.Box.Prime.TiersView.Collect(ColumnOrdinal, Value);
        }
        public static DataTier[] Collect(this DataDeposit trell, string[] ColumnNames, object[] Values)
        {
            return trell.Box.Prime.TiersView.Collect(ColumnNames, Values);
        }
        public static DataTier[] Collect(this DataDeposit trell, int[] ColumnOrdinals, object[] Values)
        {
            return trell.Box.Prime.TiersView.Collect(ColumnOrdinals, Values);
        }
        public static DataTiers Collect(this DataDeposit trell, string[] ColumnNames, List<object[]> Values)
        {
            return trell.Box.Prime.TiersView.Collect(ColumnNames, Values);
        }
        public static DataTiers Collect(this DataDeposit trell, int[] ColumnOrdinals, List<object[]> Values)
        {
            return trell.Box.Prime.TiersView.Collect(ColumnOrdinals, Values);
        }
        public static DataTier Collect(this DataDeposit trell, object[] KeyValues)
        {
            switch (trell.Box.Prime.Mode)
            {
                case DataModes.Sims:
                    return trell.Box.Prime.Sims.Collect(KeyValues);
                case DataModes.SimsView:
                    return trell.Box.Prime.SimsView.Collect(KeyValues);
                case DataModes.Tiers:
                    return trell.Box.Prime.Tiers.Collect(KeyValues);
                case DataModes.TiersView:
                    return trell.Box.Prime.TiersView.Collect(KeyValues);
                default:
                    return null;
            }
        }

        public static DataTier[] Collect(this DataTrellis trell, string ColumnName, object Value)
        {
            return trell.TiersView.Collect(ColumnName, Value);
        }
        public static DataTier[] Collect(this DataTrellis trell, int ColumnOrdinal, object Value)
        {
            return trell.TiersView.Collect(ColumnOrdinal, Value);
        }
        public static DataTier[] Collect(this DataTrellis trell, string[] ColumnNames, object[] Values)
        {
            return trell.TiersView.Collect(ColumnNames, Values);
        }
        public static DataTier[] Collect(this DataTrellis trell, int[] ColumnOrdinals, object[] Values)
        {
            return trell.TiersView.Collect(ColumnOrdinals, Values);
        }
        public static DataTiers Collect(this DataTrellis trell, string[] ColumnNames, List<object[]> Values)
        {
            return trell.TiersView.Collect(ColumnNames, Values);
        }
        public static DataTiers Collect(this DataTrellis trell, int[] ColumnOrdinals, List<object[]> Values)
        {
            return trell.TiersView.Collect(ColumnOrdinals, Values);
        }
        public static DataTier Collect(this DataTrellis trell, object[] KeyValues)
        {
            switch (trell.Mode)
            {
                case DataModes.Sims:
                    return trell.Sims.Collect(KeyValues);
                case DataModes.SimsView:
                    return trell.SimsView.Collect(KeyValues);
                case DataModes.Tiers:
                    return trell.Tiers.Collect(KeyValues);                    
                case DataModes.TiersView:
                    return trell.TiersView.Collect(KeyValues);                    
                default:
                    return null;
            }
        }

        public static DataTier Collect(this DataTiers tiers, object[] KeyValues)
        {
            object o = tiers.Registry[new Noid(KeyValues.GetShah())];
            if (o != null)
                return (DataTier)o;
            return null;
        }
        public static DataTier[] Collect(this DataTiers tiers, string ColumnName, object Value)
        {
            DataTrellis trell = tiers.Trell;
            DataTier tier = new DataTier(trell);
            int colord = -1;
            Dictionary<IDataCells, DataTier[]> keysDict = PrepareRegistry(tiers, ColumnName, out colord);
            ((IDataNative)tier.n)[colord] = Value;
            IDataCells indata = tier.Cells[colord];
            DataTier[] result = null;
            keysDict.TryGetValue(indata, out result);
            return result;
        }
        public static DataTier[] Collect(this DataTiers tiers, int ColumnOrdinal, object Value)
        {
            DataTrellis trell = tiers.Trell;
            DataTier tier = new DataTier(trell);
            int colord = -1;
            Dictionary<IDataCells, DataTier[]> keysDict = PrepareRegistry(tiers, ColumnOrdinal, out colord);
            ((IDataNative)tier.n)[colord] = Value;
            IDataCells indata = tier.Cells[colord];
            DataTier[] result = null;
            keysDict.TryGetValue(indata, out result);
            return result;
        }
        public static DataTier[] Collect(this DataTiers tiers, string[] ColumnNames, object[] Values)
        {
            DataTrellis trell = tiers.Trell;
            DataTier tier = new DataTier(trell);
            int[] colord = null;
            Dictionary<IDataCells[], DataTier[]> keysDict = PrepareRegistry(tiers, ColumnNames, out colord);
            colord.Select((x, y) => ((IDataNative)tier.n)[x] = Values[y]).ToArray();
            IDataCells[] indata = colord.Select(x => tier.Cells[x]).ToArray();
            DataTier[] result = null;
            keysDict.TryGetValue(colord.Select(x => tier.Cells[x]).ToArray(), out result);
            return result;
        }
        public static DataTier[] Collect(this DataTiers tiers, int[] ColumnOrdinals, object[] Values)
        {          
            DataTrellis trell = tiers.Trell;
            DataTier tier = new DataTier(trell);
            int[] colord = null;
            Dictionary<IDataCells[], DataTier[]> keysDict = PrepareRegistry(tiers, ColumnOrdinals, out colord);
            colord.Select((x, y) => tier[x] = Values[y]).ToArray();
            DataTier[] result = null;
            keysDict.TryGetValue(colord.Select(x => tier.Cells[x]).ToArray(), out result);
            return result;
        }
        public static DataTiers Collect(this DataTiers tiers, string[] ColumnNames, List<object[]> Values)
        {
            int[] colord = null;
            DataTrellis trell = tiers.Trell;
            Dictionary<IDataCells[], DataTier[]> keysDict = PrepareRegistry(tiers, ColumnNames, out colord);           
            DataTiers _tiers = new DataTiers(trell, tiers.Mode);
            
            for(int i = 0; i < Values.Count; i++)
            {
                DataTier[] result = null;
                DataTier t = new DataTier(trell);
                if(keysDict.TryGetValue(colord.Select((x, y) => t[x] = Values[i][y]).Select((u, f) => t.Cells[colord[f]]).ToArray(), out result))
                    _tiers.AppendView(result);
            }

            return _tiers;
        }
        public static DataTiers Collect(this DataTiers tiers, int[] ColumnOrdinals, List<object[]> Values)
        {
            int[] colord = null;
            DataTrellis trell = tiers.Trell;
            Dictionary<IDataCells[], DataTier[]> keysDict = PrepareRegistry(tiers, ColumnOrdinals, out colord);           
            DataTiers _tiers = new DataTiers(trell, tiers.Mode);

            for (int i = 0; i < Values.Count; i++)
            {
                DataTier[] result = null;
                DataTier t = new DataTier(trell);
                if (keysDict.TryGetValue(colord.Select((x, y) => t[x] = Values[i][y]).Select((u, f) => t.Cells[colord[f]]).ToArray(), out result))
                    _tiers.AppendView(result);
            }

            return _tiers;
        }

        public static Dictionary<IDataCells, DataTier[]> PrepareRegistry(DataTiers tiers, string ColumnName, out int colord)
        {
            int _colord = tiers.Trell.Pylons.AsEnumerable()
                     .Where(c => ColumnName == c.PylonName)
                     .Select(o => o.Ordinal).First();
            colord = _colord;
            return new Dictionary<IDataCells, DataTier[]>(tiers.AsArray().GroupBy(x => x.Cells[_colord], new DataCellsComparer(colord))
                                                 .Select(z => new KeyValuePair<IDataCells, DataTier[]>(z.Key, z.Select(t => t).ToArray()))
                                                  .ToDictionary(k => k.Key, v => v.Value), new DataCellsComparer(colord));

        }
        public static Dictionary<IDataCells, DataTier[]> PrepareRegistry(DataTiers tiers, int ColumnOrdinal, out int colord)
        {
            int _colord = ColumnOrdinal;
            colord = _colord;
            return new Dictionary<IDataCells, DataTier[]>(tiers.AsArray().GroupBy(x => x.Cells[_colord], new DataCellsComparer(colord))
                                               .Select(z => new KeyValuePair<IDataCells, DataTier[]>(z.Key, z.Select(t => t).ToArray()))
                                                .ToDictionary(k => k.Key, v => v.Value), new DataCellsComparer(colord));

        }
        public static Dictionary<IDataCells[], DataTier[]> PrepareRegistry(DataTiers tiers, string[] ColumnNames, out int[] colord)
        {
            int[] _colord = tiers.Trell.Pylons.AsEnumerable()
                     .Where(c => ColumnNames.Contains(c.PylonName))
                     .Select(o => o.Ordinal).ToArray();
            colord = _colord;
            return new Dictionary<IDataCells[], DataTier[]>(tiers.AsArray().GroupBy(x => _colord.Select(b => x.Cells[b]).ToArray(), new DataCellsArrayComparer(colord))
                                                 .Select(z => new KeyValuePair<IDataCells[], DataTier[]>(z.Key, z.Select(t => t).ToArray()))
                                                  .ToDictionary(k => k.Key, v => v.Value), new DataCellsArrayComparer(colord));

        }
        public static Dictionary<IDataCells[], DataTier[]> PrepareRegistry(DataTiers tiers, int[] ColumnOrdinals, out int[] colord)
        {
            int[] _colord = tiers.Trell.Pylons.AsEnumerable()
                   .Where(c => ColumnOrdinals.Contains(c.Ordinal))
                   .Select(o => o.Ordinal).ToArray();
            colord = _colord;
            return new Dictionary<IDataCells[], DataTier[]>(tiers.Tiers.AsArray()
                                                .GroupBy(x => _colord
                                                    .Select(b => x.Cells[b]).ToArray(), 
                                                        new DataCellsArrayComparer(colord))
                                                            .Select(z => new KeyValuePair<IDataCells[], DataTier[]>(
                                                                z.Key, z.Select(t => t).ToArray()))
                                                                    .ToDictionary(k => k.Key, v => v.Value), 
                                                                        new DataCellsArrayComparer(colord));

        }
        public static Dictionary<IDataCells[], DataTier[]> PrepareRegistry(DataTrellis trell, string[] ColumnNames, out int[] colord)
        {
            return PrepareRegistry(trell.TiersView, ColumnNames, out colord);          
        }
        public static Dictionary<IDataCells[], DataTier[]> PrepareRegistry(DataTrellis trell, int[] ColumnOrdinals, out int[] colord)
        {
            return PrepareRegistry(trell.TiersView, ColumnOrdinals, out colord);
        }       

        public static DataTiers Absorb(this DataTiers tiers, List<DataTier> data, bool addnew = false)
        {
            
            foreach (DataTier tier in data)
                tiers.Put(tier, false);
            return tiers;
        }
        public static DataTiers Absorb(this DataTiers tiers, DataTier[] data, bool addnew = false)
        {
            
            foreach (DataTier tier in data)
                tiers.Put(tier, false);
            return tiers;
        }
        public static DataTiers Absorb(this DataTiers tiers, object[][] data, bool addnew = false)
        {
            
            foreach (object[] dat in data)
            {
                DataTier tier = new DataTier(tiers.Trell);
                tier.DataArray = dat;
                tiers.Put(tier, false);
            }
            return tiers;
        }
        public static DataTiers Absorb(this DataTiers tiers, DataTiers data, bool addnew = false)
        {
            
            foreach (DataTier tier in data)
                tiers.Put(tier, false);
            return tiers;
        }
        public static DataTiers Absorb(this DataTiers tiers, DataTier data, bool addnew = false)
        {
            
            tiers.Put(data, false);
            return tiers;
        }       
      
   

    }
}
