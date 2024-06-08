using System.IO;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration; 

public static class DatabaseContext
{
    public static MySqlConnection GetDBConnection()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection");
        return new MySqlConnection(connectionString);
    }
}
