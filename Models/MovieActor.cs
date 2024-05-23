using MySql.Data.MySqlClient;
using System.Threading.Tasks;

public class MovieActorService
{
    // Assuming DatabaseContext is a static class you've defined to get database connections
    public static async Task PopulateMovieActorsAsync()
    {
        using (var connection = DatabaseContext.GetDBConnection())
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand("CALL PopulateMovieActors();", connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
