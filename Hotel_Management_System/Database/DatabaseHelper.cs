using MySqlConnector;

namespace Dashboard_WPF.Database
{
    public static class DatabaseHelper
    {
        private const string ConnectionString =
            "Server=127.0.0.1;Port=3306;Database=hotel_management;User ID=root;Password=;";

        public static async Task<MySqlConnection> OpenConnectionAsync()
        {
            MySqlConnection connection = new(ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
