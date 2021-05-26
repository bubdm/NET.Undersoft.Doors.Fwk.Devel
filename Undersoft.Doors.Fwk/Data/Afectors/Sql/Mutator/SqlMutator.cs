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
    public class SqlMutator
    {
        private SqlAfector sqaf;

        public SqlMutator()
        {
        }

        public DataSphere SetSqlTiers(string SqlConnectString, DataTier[] tiers, bool Renew, bool IsCube)
        {
            try
            {
                if(sqaf == null)
                    sqaf = new SqlAfector(SqlConnectString);

                try
                {
                    bool buildmap = true;
                    if (tiers.Length > 0)
                    {
                        if (IsCube)
                        {
                            AfectMapper am = new AfectMapper(tiers[0].Trell, Renew, new string[]
                                                                                   { tiers[0].Trell.Cube.SubTrell.TrellName }
                                                                                   .Concat(tiers[0].Trell.Cube.CubeRelays
                                                                                   .Select(r => r.ChildName).ToArray()).ToArray());
                            buildmap = false;
                        }
                        BulkPrepareType prepareType = BulkPrepareType.Drop;

                        if (Renew)
                            prepareType = BulkPrepareType.Trunc;

                        DataSphere ds = sqaf.Update(tiers[0].Trell.Tiers.AsArray(), Renew, buildmap, true, null, prepareType);
                        if (ds != null)
                            return sqaf.Insert(ds.Trells["Failed"].Tiers.AsArray(), Renew, false, prepareType);
                        else
                            return null;
                    }
                    return null;
                }
                catch (SqlAfectException ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }        
    }
}
