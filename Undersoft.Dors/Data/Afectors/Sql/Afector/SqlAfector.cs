using System.Linq;
using System.Data.SqlClient;

namespace System.Dors.Data.Afectors.Sql
{
    public class SqlAfector
    {
        private string cnString;
        private SqlConnection sqlcn;
        private Afect afect;
        private AfectUpdate nUpdate;
        private AfectInsert nInsert;
        private AfectMapper nMapper;

        public SqlAfector(string SqlConnectionString)
        {
            cnString = SqlConnectionString;
            sqlcn = new SqlConnection(cnString);

            string dbName = sqlcn.Database;
            DataSchemaBuild SchemaBuild = new DataSchemaBuild(sqlcn);
            SchemaBuild.SchemaPrepare(BuildDbSchemaType.Schema);
            sqlcn.ChangeDatabase("tempdb");
            SchemaBuild.SchemaPrepare(BuildDbSchemaType.Temp);
            sqlcn.ChangeDatabase(dbName);
        }
        public SqlAfector(SqlConnection SqlDbConnection)
        {
            sqlcn = SqlDbConnection;
            cnString = sqlcn.ConnectionString;

            string dbName = sqlcn.Database;
            DataSchemaBuild SchemaBuild = new DataSchemaBuild(sqlcn);
            SchemaBuild.SchemaPrepare(BuildDbSchemaType.Schema);
            sqlcn.ChangeDatabase("tempdb");
            SchemaBuild.SchemaPrepare(BuildDbSchemaType.Temp);
            sqlcn.ChangeDatabase(dbName);
        }
      
        public DataSphere Update(DataTier[] tiers, bool keysFromTrellis = false, bool buildMapping = false, bool updateKeys = false, string[] updateExcept = null, BulkPrepareType tempType = BulkPrepareType.Trunc)
        {
            if(nUpdate == null)
                nUpdate = new AfectUpdate(sqlcn);
            return nUpdate.Update(tiers, keysFromTrellis, buildMapping, updateKeys, updateExcept, tempType);
        }

        public DataSphere BatchUpdate(DataTier[] tiers, bool keysFromTrellis = false, bool buildMapping = false, bool updateKeys = false, string[] updateExcept = null)
        {
            if (nUpdate == null)
                nUpdate = new AfectUpdate(sqlcn);
            return nUpdate.BatchUpdate(tiers, keysFromTrellis, buildMapping, updateKeys, updateExcept);
        }

        public int SimpleUpdate(DataTier[] tiers, bool buildMapping = false, bool updateKeys = false, string[] updateExcept = null)
        {
            if (nUpdate == null)
                nUpdate = new AfectUpdate(sqlcn);
            return nUpdate.SimpleUpdate(tiers, buildMapping, updateKeys, updateExcept);
        }

        public DataSphere Insert(DataTier[] tiers, bool keysFromTrellis = false, bool buildMapping = false, BulkPrepareType tempType = BulkPrepareType.Trunc)
        {
            if (nInsert == null)
                nInsert = new AfectInsert(sqlcn);
            return nInsert.Insert(tiers, keysFromTrellis, buildMapping, false, null, tempType);
        }

        public DataSphere BatchInsert(DataTier[] tiers, bool buildMapping)
        {
            if (nInsert == null)
                nInsert = new AfectInsert(sqlcn);
            return nInsert.BatchInsert(tiers, buildMapping);
        }

        public int SimpleInsert(DataTier[] tiers, bool buildMapping)
        {
            if (nInsert == null)
                nInsert = new AfectInsert(sqlcn);
            return nInsert.SimpleInsert(tiers, buildMapping);
        }
        public int SimpleInsert(DataTier[] tiers)
        {
            if (nInsert == null)
                nInsert = new AfectInsert(sqlcn);
            return nInsert.SimpleInsert(tiers.ToArray());
        }

        public int Execute(string query)
        {
            SqlCommand cmd = sqlcn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = query;
            return cmd.ExecuteNonQuery();
        }

        public DataTier[] Mapper(DataTier[] DataTablesFromDataTiers, bool keysFromTrellis = false, string[] dbTableNames = null, string tablePrefix = "")
        {
            nMapper = new AfectMapper(DataTablesFromDataTiers, keysFromTrellis, dbTableNames, tablePrefix);
            return nMapper.TiersMapped;
        }
        public DataTier[] Mapper(DataTrellis trell, bool keysFromTrellis = false, string[] dbTableNames = null, string tablePrefix = "")
        {
            nMapper = new AfectMapper(trell, keysFromTrellis, dbTableNames, tablePrefix);
            return nMapper.TiersMapped;
        }

    }
 
    public class SqlAfectException : Exception
    {
        public SqlAfectException(string message)
            : base(message)
        {

        }
    }
}
