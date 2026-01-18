using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DBL
{
    public abstract class DB
    {

        private const string MySqlConnSTR = @"server=localhost;
                                    user id=root;
                                    password=<<password>>;
                                    persistsecurityinfo=True;
                                    database=mystore";

        protected DbConnection conn;
        protected DbCommand cmd;
        protected DbDataReader reader;

        protected DB()
        {
            if (conn == null)
            {
                conn = new MySqlConnection(MySqlConnSTR);
            }
            cmd = new MySqlCommand();
            cmd.Connection = conn;
            reader = null;
        }
    }
}