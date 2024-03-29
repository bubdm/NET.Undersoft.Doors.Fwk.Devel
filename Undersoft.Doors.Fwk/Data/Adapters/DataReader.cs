﻿using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Globalization;
using System.Doors;

namespace System.Doors.Data
{
    public class DataReader : IDataReader
    {
        // The DataCellsReader should always be open when returned to the user.
        private bool m_fOpen = true;

        private DataPylons m_resultset;
        private object[][] m_values;
        private int y_size;
        private int x_size;
        private static int m_STARTPOS = -1;
        private int m_nPos = m_STARTPOS;

        public DataReader(DataTrellis resultset)
        {
            m_resultset = resultset.Pylons;
            x_size = resultset.Pylons.Count;
            m_values = resultset.Tiers.AsEnumerable().Select(p => p.PrimeArray).ToArray();
            y_size = m_values.Length;
        }

        public DataReader(DataTiers resultset)
        {
            if (resultset.Count > 0)
            {
                m_resultset = resultset.Trell.Pylons;
                x_size = m_resultset.Count;
                m_values = resultset.AsEnumerable().Select(p => p.PrimeArray).ToArray();
                y_size = m_values.Length;
            }
        }

        public DataReader(ICollection<DataTier> resultset)
        {
            if (resultset.Count > 0)
            {
                m_resultset = resultset.First().Trell.Pylons;
                x_size = m_resultset.Count;
                m_values = resultset.AsEnumerable().Select(p => p.PrimeArray).ToArray();
                y_size = m_values.Length;
            }
        }

        /****
         * METHODS / PROPERTIES FROM IDataReader.
         ****/
        public int Depth
        {
            get { return 0; }
        }

        public bool IsClosed
        {

            get { return !m_fOpen; }
        }

        public int RecordsAffected
        {

            get { return -1; }
        }

        public void Close()
        {

            m_fOpen = false;
        }

        public bool NextResult()
        {

            return false;
        }

        public bool Read()
        {

            if (++m_nPos >= y_size)
                return false;
            else
                return true;
        }

        public DataTable GetSchemaTable()
        {
            //$
            throw new NotSupportedException();
        }

        /****
         * METHODS / PROPERTIES FROM IDataRecord.
         ****/
        public int FieldCount
        {

            get { return x_size; }
        }

        public string GetName(int i)
        {
            return m_resultset[i].PylonName;
        }

        public string GetDataTypeName(int i)
        {
            return m_resultset[i].DataType.Name;
        }

        public Type GetFieldType(int i)
        {
            // Return the actual Type class for the data type.
            return m_resultset[i].DataType;
        }

        public Object GetValue(int i)
        {
            object o = m_values[m_nPos][i];
            if (o is Quid)
                return ((Quid)o).LongValue;
            else if (o is Noid)
                return ((Noid)o).GetBytes();
            else if (o is DateTime)
                if ((DateTime)o == DateTime.MinValue)
                    o = DateTime.FromBinary(599581440000000000);
            return o;
        }

        public int GetValues(object[] values)
        {
            int i = 0, j = 0;
            for (; i < values.Length && j < x_size; i++, j++)
            {
                object o = m_values[m_nPos][j];
                if (o is Quid)
                    values[i] = ((Quid)o).LongValue;
                else if (o is Noid)
                    values[i] = ((Noid)o).GetBytes();
                else
                    values[i] =  o;
            }
            return i;
        }

        public int GetOrdinal(string name)
        {
            // Look for the ordinal of the column with the same name and return it.
            for (int i = 0; i < x_size; i++)
            {
                if (0 == _cultureAwareCompare(name, m_resultset[i].PylonName))
                {
                    return i;
                }
            }

            // Throw an exception if the ordinal cannot be found.
            throw new IndexOutOfRangeException("Could not find specified column in results");
        }

        public object this[int i]
        {
            get
            {
                object o = m_values[m_nPos][i];
                if (o is Quid)
                    return ((Quid)o).LongValue;
                else
                    return o;
            }
        }

        public object this[string name]
        {
            get
            {
                object o = this[GetOrdinal(name)];
                if (o is Quid)
                    return ((Quid)o).LongValue;
                else
                    return o;
            }
        }

        public bool GetBoolean(int i)
        {
            return (bool)m_values[m_nPos][i];
        }

        public byte GetByte(int i)
        { 
            return (byte)m_values[m_nPos][i];
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException("GetBytes not supported.");
        }

        public char GetChar(int i)
        {
            return (char)m_values[m_nPos][i];
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException("GetChars not supported.");
        }

        public Guid GetGuid(int i)
        { 
            return (Guid)m_values[m_nPos][i];
        }

        public Int16 GetInt16(int i)
        {
            return (Int16)m_values[m_nPos][i];
        }

        public Int32 GetInt32(int i)
        {
            return (Int32)m_values[m_nPos][i];
        }

        public Int64 GetInt64(int i)
        {
            return (Int64)m_values[m_nPos][i];
        }

        public float GetFloat(int i)
        {

            return (float)m_values[m_nPos][i];
        }

        public double GetDouble(int i)
        {

            return (double)m_values[m_nPos][i];
        }

        public string GetString(int i)
        {

            return (string)m_values[m_nPos][i];
        }

        public Decimal GetDecimal(int i)
        {

            return (decimal)m_values[m_nPos][i];
        }

        public DateTime GetDateTime(int i)
        {

            return (DateTime)m_values[m_nPos][i];
        }

        public IDataReader GetData(int i)
        {

            throw new NotSupportedException("GetData not supported.");
        }

        public bool IsDBNull(int i)
        {
            return m_values[m_nPos][i] == DBNull.Value;
        }

        private int _cultureAwareCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.Close();
                }
                catch (Exception e)
                {
                    throw new SystemException("An exception of type " + e.GetType() +
                                              " was encountered while closing the TemplateDataReader.");
                }
            }
        }

    }
}
