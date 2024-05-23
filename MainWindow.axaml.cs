using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MyMovieLibrary
{
    public partial class MainWindow : Window
    {
        private Movie? _currentMovie;

        public MainWindow()
        {
            InitializeComponent();
            LoadMoviesFromDb();
        }

        public static class DatabaseContext
        {
            private static readonly string ConnectionString = "server=localhost;user=root;password=Jurnalist1;database=MovieDB;";

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
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                const string query = "SELECT * FROM Movies";
                using var command = new MySqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var movie = new Movie
                    {
                        Id = Convert.ToInt32(reader["MovieID"]),
                        Title = reader["Title"].ToString() ?? string.Empty,
                        ReleaseYear = reader.GetInt32(reader.GetOrdinal("ReleaseYear")),
                        Genre = reader["Genre"].ToString() ?? string.Empty,
                        Storyline = reader["Storyline"].ToString(),
                        CountryOfOrigin = reader["CountryOfOrigin"].ToString(),
                        FilmingLocations = reader["FilmingLocations"].ToString(),
                        ProductionCompanies = reader["ProductionCompanies"].ToString(),
                        Category = reader["Category"].ToString(),
                        Producers = reader["Producers"].ToString(),
                    };
                    movies.Add(movie);
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

            moviesPanel.ItemsSource = null; // Ensure existing items are cleared

            var movieViews = new List<StackPanel>();

            foreach (var movie in movies)
            {
                var posterPath = $"mImages/{movie.Title.Replace(" ", string.Empty)}.jpg";
                if (!File.Exists(posterPath))
                {
                    continue; // Skip if image file does not exist
                }

                var image = new Image
                {
                    Source = new Bitmap(posterPath),
                    Stretch = Stretch.Uniform,
                    Tag = movie // Associate the movie object with the image for retrieval on click
                };

                // Centering and margin adjustments for spacing
                var titleTextBlock = new TextBlock
                {
                    Text = movie.Title,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var yearTextBlock = new TextBlock
                {
                    Text = $"({movie.ReleaseYear})",
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(10) // Add space around each movie poster block
                };

                stackPanel.Children.Add(image);
                stackPanel.Children.Add(titleTextBlock);
                stackPanel.Children.Add(yearTextBlock);

                image.PointerPressed += Image_PointerPressed; // Subscribe to the PointerPressed event

                movieViews.Add(stackPanel);
            }

            moviesPanel.ItemsSource = movieViews;

            // Update visibility based on the current window size
            UpdateMovieTitleVisibility(this.Width);
        }

        private async void LoadActorsFromDb()
        {
            var actors = new List<Actor>();
            try
            {
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                const string query = "SELECT * FROM Actors";
                using var command = new MySqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var actor = new Actor
                    {
                        Id = Convert.ToInt32(reader["ActorID"]),
                        Name = reader["Name"].ToString(),
                        Spouse = reader["Spouse"]?.ToString(),
                        Biography = reader["Biography"]?.ToString(),
                    };
                    actors.Add(actor);
                }
                UpdateActorsUI(actors);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void UpdateActorsUI(List<Actor> actors)
        {
            var actorsPanel = this.FindControl<ItemsControl>("ActorsPanel");

            actorsPanel.ItemsSource = null; // Ensure existing items are cleared

            var actorViews = new List<StackPanel>();

            foreach (var actor in actors)
            {
                var photoPath = $"aImages/{actor.Name?.Replace(" ", string.Empty)}.jpg";
                if (!File.Exists(photoPath))
                {
                    continue; // Skip if image file does not exist
                }

                var image = new Image
                {
                    Source = new Bitmap(photoPath),
                    Stretch = Stretch.Uniform,
                    Tag = actor // Associate the actor object with the image for retrieval on click
                };

                // Centering and margin adjustments for spacing
                var nameTextBlock = new TextBlock
                {
                    Text = actor.Name ?? string.Empty,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(10) // Add space around each actor photo block
                };

                stackPanel.Children.Add(image);
                stackPanel.Children.Add(nameTextBlock);

                image.PointerPressed += ActorImage_PointerPressed; // Subscribe to the PointerPressed event

                actorViews.Add(stackPanel);
            }

            actorsPanel.ItemsSource = actorViews;

            // Update visibility based on the current window size
            UpdateActorNameVisibility(this.Width);
        }

        private void Image_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Image image && image.Tag is Movie movie)
            {
                _currentMovie = movie; // Save the current movie
                ShowMovieDetails(movie);
            }
        }

        private void ActorImage_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Image image && image.Tag is Actor actor)
            {
                ShowActorDetails(actor);
            }
        }

        private void ShowMovieDetails(Movie movie)
        {
            var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");

            // Clear previous details
            detailsPanel.Children.Clear();

            // Add new details with TextWrapping property set to Wrap
            detailsPanel.Children.Add(new Image
            {
                Source = new Bitmap($"mImages/{movie.Title.Replace(" ", string.Empty)}.jpg"),
                Width = 200,
                Height = 300,
                Margin = new Thickness(0, 0, 0, 10) // Display poster
            });
            detailsPanel.Children.Add(new TextBlock { Text = $"Title: {movie.Title}", FontWeight = FontWeight.Bold, FontSize = 20, TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Release Year: {movie.ReleaseYear}", TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Genre: {movie.Genre}", TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Story Line: {movie.Storyline ?? "N/A"}", TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Country Of Origin: {movie.CountryOfOrigin ?? "N/A"}", TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Filming Locations: {movie.FilmingLocations ?? "N/A"}", TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Production Companies: {movie.ProductionCompanies ?? "N/A"}", TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Category: {movie.Category ?? "N/A"}", TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Producers: {movie.Producers ?? "N/A"}", TextWrapping = TextWrapping.Wrap });

            // Ensure the details panel is visible
            detailsPanel.IsVisible = true;
        }

        private void ShowActorDetails(Actor actor)
        {
            var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");

            // Clear previous details
            detailsPanel.Children.Clear();

            // Add new details with TextWrapping property set to Wrap
            detailsPanel.Children.Add(new Image
            {
                Source = new Bitmap($"aImages/{actor.Name?.Replace(" ", string.Empty)}.jpg"),
                Width = 200,
                Height = 300,
                Margin = new Thickness(0, 0, 0, 10) // Display photo
            });
            detailsPanel.Children.Add(new TextBlock { Text = $"Name: {actor.Name}", FontWeight = FontWeight.Bold, FontSize = 20, TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Spouse: {actor.Spouse ?? "N/A"}", TextWrapping = TextWrapping.Wrap });
            detailsPanel.Children.Add(new TextBlock { Text = $"Biography: {actor.Biography ?? "N/A"}", TextWrapping = TextWrapping.Wrap });

            // Ensure the details panel is visible
            detailsPanel.IsVisible = true;
        }

        private void UpdateMovieTitleVisibility(double windowWidth)
        {
            var moviesPanel = this.FindControl<ItemsControl>("MoviesPanel");
            if (moviesPanel.ItemsSource is IEnumerable<StackPanel> moviePanels)
            {
                foreach (var moviePanel in moviePanels)
                {
                    foreach (var child in moviePanel.Children)
                    {
                        if (child is TextBlock textBlock)
                        {
                            textBlock.IsVisible = windowWidth >= 600;
                        }
                    }
                }
            }
        }

        private void UpdateActorNameVisibility(double windowWidth)
        {
            var actorsPanel = this.FindControl<ItemsControl>("ActorsPanel");
            if (actorsPanel.ItemsSource is IEnumerable<StackPanel> actorPanels)
            {
                foreach (var actorPanel in actorPanels)
                {
                    foreach (var child in actorPanel.Children)
                    {
                        if (child is TextBlock textBlock)
                        {
                            textBlock.IsVisible = windowWidth >= 600;
                        }
                    }
                }
            }
        }

        private void Window_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateMovieTitleVisibility(e.NewSize.Width);
            UpdateActorNameVisibility(e.NewSize.Width);
        }

        private void MoviesButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;

            if (_currentMovie != null)
            {
                ShowMovieDetails(_currentMovie); // Show previously selected movie details
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = true; // Ensure details are visible
            }
            else
            {
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = false; // Hide details if no movie is selected
            }

            LoadMoviesFromDb();
        }

        private void ActorsButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = true;
            this.FindControl<StackPanel>("DetailsPanel").IsVisible = false; // Hide details when switching to actors
            LoadActorsFromDb();
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
    }

    public class Actor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Spouse { get; set; }
        public string? Biography { get; set; }
    }
}
