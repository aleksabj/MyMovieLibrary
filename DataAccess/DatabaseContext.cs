using MySql.Data.MySqlClient;

public static class DatabaseContext
{
    public static MySqlConnection GetDBConnection()
    {
        string connectionString = "server=127.0.0.1;port=3306;database=MovieDB;user=root;password=Jurnalist1";
        return new MySqlConnection(connectionString);
    }
}
