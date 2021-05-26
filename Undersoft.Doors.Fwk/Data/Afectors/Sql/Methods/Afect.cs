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
    public class Afect
    {
        private SqlConnection _cn;

        public Afect(SqlConnection cn)
        {
            _cn = cn;
        }
        public Afect(string cnstring)
        {
            _cn = new SqlConnection(cnstring);
        }      

        public class AfectException : Exception
        {
            public AfectException(string message)
                : base(message)
            {

            }
        }

    }
}
