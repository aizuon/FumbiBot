using Dapper.FastCrud;
using MySql.Data.MySqlClient;
using Serilog;
using Serilog.Core;
using System.Data;

namespace DiscordBot
{
    public static class Database
    {
        private static IDbConnection _connection;
        private static string s_connectionString;

        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Database));

        private static void Initialize()
        {
            string server = Config.Instance.Database.Host;
            string database = Config.Instance.Database.Database;
            string uid = Config.Instance.Database.Username;
            string password = Config.Instance.Database.Password;
            s_connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            OrmConfiguration.DefaultDialect = SqlDialect.MySql;

            _connection = new MySqlConnection(s_connectionString);
        }

        public static IDbConnection Open()
        {
            Logger.Information("Connecting to the database...");

            if (_connection == null)
                Initialize();

            try
            {
                _connection.Open();
                Logger.Information("Successfully connected to the database!");
                return _connection;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Logger.Error("Cannot connect to the database.");
                        break;

                    case 1045:
                        Logger.Error("Invalid username/password.");
                        break;

                    default:
                        Logger.Error("Unhandled exception -> " + ex.Message);
                        break;
                }
                return null;
            }
        }

        public static IDbConnection GetCurrentConnection() => _connection;

        public static bool Close()
        {
            try
            {
                _connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Logger.Error(ex.Message);
                return false;
            }
        }
    }
}
