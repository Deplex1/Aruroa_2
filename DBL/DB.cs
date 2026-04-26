using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DBL
{
    public abstract class DB
    {
        // Connection string is now loaded from appsettings.json
        // This is more secure than hardcoding credentials
        private static string? connectionString;

        protected DbConnection conn;
        protected DbCommand cmd;
        protected DbDataReader reader;

        protected DB()
        {
            // Get connection string from configuration
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to hardcoded for backwards compatibility
                // TODO: Remove this fallback after updating all deployments
                connectionString = @"server=localhost;
                                    user id=root;
                                    password=josh17rog;
                                    persistsecurityinfo=True;
                                    database=auroradb";
            }

            if (conn == null)
            {
                conn = new MySqlConnection(connectionString);
            }
            cmd = new MySqlCommand();
            cmd.Connection = conn;
            reader = null;
        }

        /// <summary>
        /// Set connection string from configuration (called from Program.cs)
        /// </summary>
        public static void SetConnectionString(string connStr)
        {
            connectionString = connStr;
        }
    }
}