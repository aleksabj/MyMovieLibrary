using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Layout;
using Avalonia.Media;

namespace MyMovieLibrary
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadMoviesFromDb();
        }

        public static class DatabaseContext
        {
            private static string ConnectionString = "server=localhost;user=root;password=Jurnalist1;database=MovieDB;";

            public static async Task<MySqlConnection> GetDBConnectionAsync()
            {
                var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();
                return connection;
            }
        }

        private async void LoadMoviesFromDb()
        {
            var movies = new List<Movie>();
            try
            {
                using (var connection = await DatabaseContext.GetDBConnectionAsync())
                {
                    const string query = "SELECT * FROM Movies";
                    using var command = new MySqlCommand(query, connection);
                    
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var movie = new Movie
                        {
                            Id = Convert.ToInt32(reader["MovieID"]),
                            Title = reader["Title"].ToString(),
                            ReleaseYear = reader.GetInt32(reader.GetOrdinal("ReleaseYear")),
                            Genre = reader["Genre"].ToString(),
                            // Continue mapping all necessary fields...
                            Storyline = reader["Storyline"].ToString(),
                            CountryOfOrigin = reader["CountryOfOrigin"].ToString(),
                            FilmingLocations = reader["FilmingLocations"].ToString(),
                            ProductionCompanies = reader["ProductionCompanies"].ToString(),
                            Category = reader["Category"].ToString(),
                            Producers = reader["Producers"].ToString(),
                        };
                        movies.Add(movie);
                    }
                }

                UpdateMoviesUI(movies);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void UpdateMoviesUI(List<Movie> movies)
        {
            var moviesPanel = this.FindControl<ItemsControl>("MoviesPanel");

            moviesPanel.Items.Clear(); // Clear existing items

            foreach (var movie in movies)
            {
                var posterPath = $"mImages/{movie.Title.Replace(" ", string.Empty)}.jpg";
                var image = new Image
                {
                    Source = new Bitmap(posterPath),
                    Width = 200,
                    Height = 300,
                    Stretch = Stretch.Uniform,
                    Tag = movie // Associate the movie object with the image for retrieval on click
                };

                image.PointerPressed += Image_PointerPressed; // Subscribe to the PointerPressed event

                moviesPanel.Items.Add(image); // Add the image directly to MoviesPanel
            }
        }

        private void Image_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Image image && image.Tag is Movie movie)
            {
                ShowMovieDetails(movie);
            }
        }

        private void ShowMovieDetails(Movie movie)
        {
            var detailsPanel = this.FindControl<StackPanel>("MovieDetailsPanel"); // Assuming this is correctly named as per your XAML

            detailsPanel.Children.Clear(); // Clear existing details

            // Populate the details panel
            detailsPanel.Children.Add(new TextBlock { Text = $"Title: {movie.Title}", FontWeight = FontWeight.Bold, FontSize = 20 });
            detailsPanel.Children.Add(new TextBlock { Text = $"Release Year: {movie.ReleaseYear}" });
            detailsPanel.Children.Add(new TextBlock { Text = $"Genre: {movie.Genre}" });
            // Add more details... Make sure to check for nulls or use ?. operator for nullable strings
            
            detailsPanel.Children.Add(new TextBlock { Text = $"Story Line: {movie.Storyline ?? "N/A"}" });
            // Continue adding details similarly
            detailsPanel.Children.Add(new TextBlock { Text = $"Country Of Origin: {movie.CountryOfOrigin ?? "N/A"}" });
            detailsPanel.Children.Add(new TextBlock { Text = $"Filming Locations: {movie.FilmingLocations ?? "N/A"}" });
            detailsPanel.Children.Add(new TextBlock { Text = $"Production Companies: {movie.ProductionCompanies ?? "N/A"}" });
             detailsPanel.Children.Add(new TextBlock { Text = $"Category: {movie.Category ?? "N/A"}" });
              detailsPanel.Children.Add(new TextBlock { Text = $"Producers: {movie.Producers ?? "N/A"}" });
        }
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string? Storyline { get; set; }
        public string? CountryOfOrigin { get; set; }
        public string? FilmingLocations { get; set; }
        public string? ProductionCompanies { get; set; }
        public string? Category { get; set; }
        public string? Producers { get; set; }
        // Add other properties here as needed.
    }
}
