﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Globalization;
using System.Doors;

namespace System.Doors.Data.Afectors.Sql
{
    public static class SqlNetType
    {
        public static Type SqlTypeToNet(string sqlType)
        {
            if (DbNetTypes.SqlNetTypes.ContainsKey(sqlType))
                return DbNetTypes.SqlNetTypes[sqlType];
            else
                return typeof(object);
        }
        public static object SqlNetVal(DataTier fieldRow, string fieldName, string prefix = "", string tableName = null)
        {
            object sqlNetVal = new object();
            try
            {
                CultureInfo cci = CultureInfo.CurrentCulture;
                string decRep = (cci.NumberFormat.NumberDecimalSeparator == ".") ? "," : ".";
                string decSep = cci.NumberFormat.NumberDecimalSeparator, _tableName = "";
                if (tableName != null) _tableName = tableName; else _tableName = fieldRow.Trell.TrellName;
                if (!DbHand.Schema.DataDbTables.Have(_tableName)) _tableName = prefix + _tableName;
                if (DbHand.Schema.DataDbTables.Have(_tableName))
                {
                    Type ft = DbHand.Schema.DataDbTables[_tableName].DataDbColumns[fieldName + "#"].DataType;

                    if (DBNull.Value != fieldRow[fieldName])
                    {
                        if (ft == typeof(decimal) || ft == typeof(float) || ft == typeof(double))
                            sqlNetVal = Convert.ChangeType(fieldRow[fieldName].ToString().Replace(decRep, decSep), ft);
                        else if (ft == typeof(string))
                        {
                            int maxLength = DbHand.Schema.DataDbTables[_tableName].DataDbColumns[fieldName + "#"].MaxLength;
                            if (fieldRow[fieldName].ToString().Length > maxLength)
                                sqlNetVal = Convert.ChangeType(fieldRow[fieldName].ToString().Substring(0, maxLength), ft);
                            else
                                sqlNetVal = Convert.ChangeType(fieldRow[fieldName], ft);
                        }
                        else if (ft == typeof(long) && fieldRow[fieldName] is Quid)
                            sqlNetVal = ((Quid)fieldRow[fieldName]).LongValue;
                        else if (ft == typeof(byte[]) && fieldRow[fieldName] is Noid)
                            sqlNetVal = ((Noid)fieldRow[fieldName]).GetBytes();
                        else
                            sqlNetVal = Convert.ChangeType(fieldRow[fieldName], ft);
                    }
                    else
                    {
                        fieldRow[fieldName] = DbNetTypes.SqlNetDefaults[ft];
                        sqlNetVal = Convert.ChangeType(fieldRow[fieldName], ft);
                    }
                }
                else
                {
                    sqlNetVal = fieldRow[fieldName];
                }
            }
            catch (Exception ex)
            {
              //  throw new Nnvalidnamespace System.Doors.Data.Afectors.Sql.Sq("\n" + ex + " - fieldName - " + fieldName);
            }
            return sqlNetVal;
        }
        public static string NetTypeToSql(Type netType)
        {
            if (DbNetTypes.SqlNetTypes.ContainsValue(netType))
                return DbNetTypes.SqlNetTypes.Where(p => p.Value == netType).Select(t => t.Key).First();
            else
                return "varchar";
        }
    }

    public static class DbNetTypes
    {        
        public static Dictionary<string, Type> SqlNetTypes { get { return sqlNetTypes; } }
        private static Dictionary<string, Type> sqlNetTypes = 
            new Dictionary<string, Type>()
            {
                {"int", typeof(int) },
                {"varchar", typeof(string)},
                {"nvarchar", typeof(string)},
                {"datetime", typeof(DateTime)},
                {"bit", typeof(bool)},
                {"float", typeof(double)},
                {"numeric", typeof(float) },
                {"decimal", typeof(decimal)},
                {"date", typeof(DateTime)},
                {"ntext", typeof(string)},
                {"uniqueidentifier", typeof(Guid)},
                {"bigint",  typeof(Quid) },
                {"varbinary", typeof(Noid) }
            };
       

        public static Dictionary<Type, object> SqlNetDefaults { get { return sqlNetDefaults; } }
        private static Dictionary<Type, object> sqlNetDefaults = 
            new Dictionary<Type, object>()
            {
                {typeof(int), 0},
                {typeof(string), ""},
                {typeof(DateTime), DateTime.Now},
                {typeof(bool), false},
                {typeof(float), 0},
                {typeof(decimal), 0},
                {typeof(Guid), Guid.Empty},
                {typeof(Quid), Quid.Empty},
                {typeof(Noid), Noid.Empty}
            };
       
    }
}
