using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;

namespace VideoConverter.DAL
{
    public class DBUtils
    {
        public static MySqlConnection GetDBConnection(IConfiguration configuration)
        {
            string host = configuration.GetValue<string>("MySqlConnection:Host");
            int port = Convert.ToInt32(configuration.GetValue<string>("MySqlConnection:Port"));
            string database = configuration.GetValue<string>("MySqlConnection:Database");
            string username = configuration.GetValue<string>("MySqlConnection:Username");
            string password = configuration.GetValue<string>("MySqlConnection:Password");

            return GetDBConnection(host, port, database, username, password);
        }

        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            // Connection String.
            String connString = "Server=" + host + ";Database=" + database
                + ";port=" + port + ";User Id=" + username + ";password=" + password;

            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }

    }
}
