using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Layout;


namespace MyMovieLibrary {
    public partial class MainWindow : Window {
        // Current movie being processed or viewed
        private Movie? _currentMovie;
        // Current producer being processed or viewed
        private Producer? _currentProducer;
        // List of all movies
        private List<Movie> _allMovies = new List<Movie>();

        // Dictionary mapping genres to lists of movies in that genre
        private Dictionary<string, List<Movie>> _moviesByGenre = new Dictionary<string, List<Movie>>();

        // List of movies that the user wants to watch
        private List<Movie> _wantToWatchMovies = new List<Movie>();

        // Constructor for the MainWindow class
        public MainWindow() {
            // Initialize the components of the window
            InitializeComponent();
            // Load the movies from the database
            LoadMoviesFromDb();
        }

        // Static class for database context
        public static class DatabaseContext {
            // Connection string for the database
            private static readonly string ConnectionString = "server=localhost;user=root;password=Jurnalist1;database=MovieDB;";

            // Method to get a connection to the database
            public static async Task<MySqlConnection> GetDBConnectionAsync() {
                // Create a new connection with the connection string
                var connection = new MySqlConnection(ConnectionString);
                // Open the connection asynchronously
                await connection.OpenAsync();
                // Return the connection
                return connection;
            }
        }

        // Method to load movies from the database
        private async void LoadMoviesFromDb() { 
            var movies = new List<Movie>(); // List of movies
            var genresSet = new HashSet<string>(); // Set of genres
            // Try to get a connection to the database
            try {
                using var connection = await DatabaseContext.GetDBConnectionAsync(); //  Get a connection to the database
                const string query = "SELECT * FROM Movies"; // SQL query to get all movies
                using var command = new MySqlCommand(query, connection); // Create a new command with the query and connection

                using var reader = await command.ExecuteReaderAsync(); // Execute the command and get the reader
                // Read the results from the reader
                while (await reader.ReadAsync()) {
                    var movieId = Convert.ToInt32(reader["MovieID"]);  // Get the movie ID
                    var title = reader["Title"].ToString() ?? string.Empty; // Get the movie title
                    // Print the movie ID and title to the console
                    Console.WriteLine($"MovieID: {movieId}, Title: {title}");
                    // Create a new movie object with the movie ID and title
                    var movie = new Movie {
                        Id = movieId,
                        Title = title,
                        ReleaseYear = reader.GetInt32(reader.GetOrdinal("ReleaseYear")),
                        Genre = reader["Genre"].ToString() ?? string.Empty,
                        Storyline = reader["Storyline"].ToString(),
                        CountryOfOrigin = reader["CountryOfOrigin"].ToString(),
                        FilmingLocations = reader["FilmingLocations"].ToString(),
                        ProductionCompanies = reader["ProductionCompanies"].ToString(),
                        Category = reader["Category"].ToString(),
                        Producers = reader["Producers"].ToString(),
                    };
                    // Print the movie details to the console
                    Console.WriteLine($"Assigned {movie.Actors.Count} actors to movie: {movie.Title}");
                    movies.Add(movie);
                    // Add the genres to the genres set
                    foreach (var genre in movie.Genre.Split(',').Select(g => g.Trim())) {
                        genresSet.Add(genre);
                        // Add the movie to the dictionary of movies by genre
                        if (!_moviesByGenre.ContainsKey(genre))
                            _moviesByGenre[genre] = new List<Movie>();
                        
                        // Add the movie to the list of movies by genre
                        if (!_moviesByGenre[genre].Any(m => m.Id == movie.Id))
                            _moviesByGenre[genre].Add(movie);
                    }
                }
                // Set the list of all movies to the movies list
                _allMovies = movies;
                CreateGenreFilterButtons(genresSet);
                UpdateMoviesUI(movies);
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        // Method to create genre filter buttons
        private void CreateGenreFilterButtons(IEnumerable<string> genres) {
            var genreFilterPanel = this.FindControl<StackPanel>("GenreFilterPanel"); // Get the genre filter panel

            genreFilterPanel.Children.Clear(); // Clear the children of the genre filter panel

            var allButton = new Button { Content = "All", Tag = "All", Margin = new Thickness(5)}; // Create a new button for all genres
            allButton.Click += GenreButton_Click; // Add a click event handler for the button
            genreFilterPanel.Children.Add(allButton); // Add the button to the genre filter panel
            // Loop through the genres and create a button for each genre
            foreach (var genre in  genres.OrderBy(g => g)) {
                var genreButton = new Button { Content = genre, Tag = genre, Margin = new Thickness(5) };
                genreButton.Click += GenreButton_Click;
                genreFilterPanel.Children.Add(genreButton);
            }
        }
        // Method to update the movies UI
        private void UpdateMoviesUI(List<Movie> movies) {
            var moviesPanel = this.FindControl<ItemsControl>("MoviesPanel");
            moviesPanel.ItemsSource = null; // Set the items source of the movies panel to null
            var movieViews = new List<StackPanel>(); // Create a new list of stack panels for the movies
            // Loop through the movies and create a stack panel for each movie
            foreach (var movie in movies) {
                var posterPath = $"mImages/{movie.Title.Replace(" ", string.Empty)}.jpg";
                if (!File.Exists(posterPath))
                    continue;
                var image = new Image {
                    Source = new Bitmap(posterPath), // Set the source of the image to the poster path
                    Stretch = Stretch.Uniform,
                    Tag = movie
                };

                var titleTextBlock = new TextBlock {
                    Text = movie.Title,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap // Wrap the text
                };

                var yearTextBlock = new TextBlock {
                    Text = $"({movie.ReleaseYear})",
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var stackPanel = new StackPanel{
                    Margin = new Thickness(10)
                };
                // Add the image, title text block, and year text block to the stack panel
                stackPanel.Children.Add(image);
                stackPanel.Children.Add(titleTextBlock);
                stackPanel.Children.Add(yearTextBlock);

                image.PointerPressed += Image_PointerPressed;

                movieViews.Add(stackPanel);
            }

            moviesPanel.ItemsSource = movieViews;
            UpdateMovieTitleVisibility(this.Width);
        }
        // Method to load actors from the database
        private async void LoadActorsFromDb() {
            var actors = new List<Actor>();
            try {
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                const string query = "SELECT * FROM Actors";
                using var command = new MySqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    var actor = new Actor {
                        Id = Convert.ToInt32(reader["ActorID"]),
                        Name = reader["Name"].ToString(),
                        Spouse = reader["Spouse"]?.ToString(),
                        Biography = reader["Biography"]?.ToString(),
                    };
                    actors.Add(actor);
                }
                UpdateActorsUI(actors);
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void UpdateActorsUI(List<Actor> actors) {
            var actorsPanel = this.FindControl<ItemsControl>("ActorsPanel");
            actorsPanel.ItemsSource = null;
            var actorViews = new List<StackPanel>();

            foreach (var actor in actors) {
                var photoPath = $"aImages/{actor.Name?.Replace(" ", string.Empty)}.jpg";
                if (!File.Exists(photoPath))
                    continue;

                var image = new Image {
                    Source = new Bitmap(photoPath),
                    Stretch = Stretch.Uniform,
                    Tag = actor
                };

                var nameTextBlock = new TextBlock {
                    Text = actor.Name ?? string.Empty,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var stackPanel = new StackPanel {
                    Margin = new Thickness(10)
                };

                stackPanel.Children.Add(image);
                stackPanel.Children.Add(nameTextBlock);

                image.PointerPressed += ActorImage_PointerPressed;
                actorViews.Add(stackPanel);
            }

            actorsPanel.ItemsSource = actorViews;

            UpdateActorNameVisibility(this.Width);
        }
        // Method to load producers from the database
        private async void LoadProducersFromDb() {
            var producers = new List<Producer>();
            try {
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                const string query = "SELECT * FROM Producers";
                using var command = new MySqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    var producer = new Producer {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                        YearOfBirth = reader["year_of_birth"]?.ToString(),
                        MostFamousMovies = reader["most_famous_movies"]?.ToString(),
                        CountryOfOrigin = reader["country_of_origin"]?.ToString(),
                    };
                    producers.Add(producer);
                }
                UpdateProducersUI(producers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void UpdateProducersUI(List<Producer> producers) {
            var producersPanel = this.FindControl<ItemsControl>("ProducersPanel");

            producersPanel.ItemsSource = null;

            var producerViews = new List<StackPanel>();

            foreach (var producer in producers) {
                var photoPath = $"pImages/{producer.Name?.Replace(" ", string.Empty)}.jpg";
                if (!File.Exists(photoPath))
                    continue;

                var image = new Image {
                    Source = new Bitmap(photoPath),
                    Stretch = Stretch.Uniform,
                    Tag = producer
                };

                var nameTextBlock = new TextBlock {
                    Text = producer.Name ?? string.Empty,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var stackPanel = new StackPanel {
                    Margin = new Thickness(10)
                };

                stackPanel.Children.Add(image);
                stackPanel.Children.Add(nameTextBlock);

                image.PointerPressed += ProducerImage_PointerPressed;

                producerViews.Add(stackPanel);
            }

            producersPanel.ItemsSource = producerViews;

            UpdateProducerNameVisibility(this.Width);
        }
        // Method to handle the pointer pressed event for an image
        private void Image_PointerPressed(object? sender, PointerPressedEventArgs e) {
            if (sender is Image image && image.Tag is Movie movie)  {
                _currentMovie = movie;
                ShowMovieDetails(movie);
            }
        }
        // Method to show movie details
        private async Task ShowMovieDetails(Movie movie) {
            var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");

            detailsPanel.Children.Clear();
            // Create a new button to add the movie to the watch list
            var addToWatchButton = new Button {
                Content = "Add to the list",
                Tag = movie,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 10),
                Background = Brushes.Gray,
                Foreground = Brushes.White,
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(20),
                FontSize = 15
            };
            addToWatchButton.Click += AddToWatchButton_Click;

            detailsPanel.Children.Add(addToWatchButton);
            // Create a new button to show the movie details
            detailsPanel.Children.Add(new Image {
                Source = new Bitmap($"mImages/{movie.Title.Replace(" ", string.Empty)}.jpg"),
                Width = 200,
                Height = 300,
                Margin = new Thickness(0, 0, 0, 10)
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Title: {movie.Title}",
                FontWeight = FontWeight.Bold,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Release Year: {movie.ReleaseYear}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) //increase spacing between lines
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Genre: {movie.Genre}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
                
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Story Line: {movie.Storyline ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Country Of Origin: {movie.CountryOfOrigin ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Filming Locations: {movie.FilmingLocations ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Production Companies: {movie.ProductionCompanies ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Category: {movie.Category ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });

            detailsPanel.Children.Add(new TextBlock  {
                Text = "Producers:",
                FontWeight = FontWeight.Bold,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });

            var producersPanel = new StackPanel { Orientation = Orientation.Vertical };
            // Split the producers by comma and trim the names
            var producerNames = movie.Producers?.Split(',') ?? Array.Empty<string>();
            foreach (var producerName in producerNames) { // Loop through the producer names
                var producerNameTrimmed = producerName.Trim();
                if (await ProducerExists(producerNameTrimmed)) { // Check if the producer exists
                    var link = new TextBlock  {
                        Text = producerNameTrimmed,
                        Foreground = Brushes.Blue,
                        Cursor = new Cursor(StandardCursorType.Hand),
                        TextWrapping = TextWrapping.Wrap
                    }; 
                    // Add a pointer pressed event handler to show the producer details
                    link.PointerPressed += (s, e) => ShowProducerDetailsByName(producerNameTrimmed);
                    producersPanel.Children.Add(link);
                }
                else { 
                    producersPanel.Children.Add(new TextBlock { Text = producerNameTrimmed, TextWrapping = TextWrapping.Wrap });
                }
            }
            // Add the producers panel to the details panel
            detailsPanel.Children.Add(producersPanel);
            detailsPanel.Children.Add(new TextBlock {
                Text = "",//add a blank line
                FontWeight = FontWeight.Bold,
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap
            });

            var actorsPanel = new StackPanel { Orientation = Orientation.Vertical };
            // Loop through the actors in the movie
            foreach (var actor in movie.Actors) {
                Console.WriteLine($"Displaying actor: {actor.Name}");

                var actorImage = new Image {
                    Source = new Bitmap($"aImages/{actor.Name?.Replace(" ", string.Empty)}.jpg"),
                    Width = 100,
                    Height = 150,
                    Margin = new Thickness(5),
                    Tag = actor
                };

                actorImage.PointerPressed += ActorImage_PointerPressed;

                var actorInfoPanel = new StackPanel {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                actorInfoPanel.Children.Add(actorImage);
                actorInfoPanel.Children.Add(new TextBlock
                {
                    Text = actor.Name ?? string.Empty,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });

                actorsPanel.Children.Add(actorInfoPanel);
            }

            detailsPanel.Children.Add(actorsPanel);

            detailsPanel.IsVisible = true;
        }
        // Method to check if a producer exists
        private async Task<bool> ProducerExists(string producerName) {
            try
            {
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                const string query = "SELECT COUNT(*) FROM Producers WHERE name = @ProducerName";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProducerName", producerName);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
        // Method to show producer details by name
        private async void ShowProducerDetailsByName(string producerName) {
            try {
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                const string query = "SELECT * FROM Producers WHERE name = @ProducerName";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProducerName", producerName);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())  {
                    var producer = new Producer {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                        YearOfBirth = reader["year_of_birth"]?.ToString(),
                        MostFamousMovies = reader["most_famous_movies"]?.ToString(),
                        CountryOfOrigin = reader["country_of_origin"]?.ToString(),
                    };

                    _currentProducer = producer;
                    ShowProducerDetails(producer);
                    // Hide the movies and actors scroll viewers
                    this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
                    this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
                    this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = true;
                    this.FindControl<StackPanel>("DetailsPanel").IsVisible = true;
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        // Method to show actor details
        private void ActorImage_PointerPressed(object? sender, PointerPressedEventArgs e) {
            if (sender is Image image && image.Tag is Actor actor)  {
                ShowActorDetails(actor);
                // Hide the movies and producers scroll viewers
                this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
                this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = true;
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = true;
            }
        }
        // Method to show actor details
        private void ShowActorDetails(Actor actor) {
            var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");

            detailsPanel.Children.Clear();

            detailsPanel.Children.Add(new Image {
                Source = new Bitmap($"aImages/{actor.Name?.Replace(" ", string.Empty)}.jpg"),
                Width = 200,    
                Height = 300,
                Margin = new Thickness(0, 0, 0, 10)
            });
                        detailsPanel.Children.Add(new TextBlock {
                Text = $"Name: {actor.Name}",
                FontWeight = FontWeight.Bold,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock {
                  TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Biography: {actor.Biography ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });

            detailsPanel.IsVisible = true;
        }
        // Method to update the visibility of the movie title
        private void UpdateMovieTitleVisibility(double windowWidth) {
            var moviesPanel = this.FindControl<ItemsControl>("MoviesPanel");
            if (moviesPanel.ItemsSource is IEnumerable<StackPanel> moviePanels) {
                foreach (var moviePanel in moviePanels) { // Loop through the movie panels
                    foreach (var child in moviePanel.Children) {
                        if (child is TextBlock textBlock)
                            textBlock.IsVisible = windowWidth >= 600;
                    }
                }
            }
        }
        // Method to update the visibility of the actor name
        private void UpdateActorNameVisibility(double windowWidth) {
            var actorsPanel = this.FindControl<ItemsControl>("ActorsPanel"); // Get the actors panel
            if (actorsPanel.ItemsSource is IEnumerable<StackPanel> actorPanels) {
                foreach (var actorPanel in actorPanels) {
                    foreach (var child in actorPanel.Children) {
                        if (child is TextBlock textBlock)
                            textBlock.IsVisible = windowWidth >= 600;
                    }
                }
            }
        }
        // Method to update the visibility of the producer name
        private void UpdateProducerNameVisibility(double windowWidth) {
            var producersPanel = this.FindControl<ItemsControl>("ProducersPanel");
            if (producersPanel.ItemsSource is IEnumerable<StackPanel> producerPanels) {
                foreach (var producerPanel in producerPanels) {
                    foreach (var child in producerPanel.Children){
                        if (child is TextBlock textBlock)
                            textBlock.IsVisible = windowWidth >= 600;
                    }
                }
            }
        }
        // Method to handle the size changed event for the window
        private void Window_SizeChanged(object? sender, SizeChangedEventArgs e) {
            UpdateMovieTitleVisibility(e.NewSize.Width); // Update the visibility of the movie title
            UpdateActorNameVisibility(e.NewSize.Width); // Update the visibility of the actor name
            UpdateProducerNameVisibility(e.NewSize.Width); //   Update the visibility of the producer name
        }
        // Method to handle the movies button click event
        private void MoviesButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = true; // Show the movies scroll viewer
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = true; // Show the genre filter scroll viewer
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false; // Hide the actors scroll viewer
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false; // Hide the producers scroll viewer
 
            // Check if the current movie is not null
            if (_currentMovie != null) {
                ShowMovieDetails(_currentMovie);
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = true;
            } else {
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            }

            UpdateMoviesUI(_allMovies);
        }
        // Method to handle the actors button click event
        private void ActorsButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;
            this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            LoadActorsFromDb();
        }
        // Method to handle the producers button click event
        private void ProducersButton_Click(object? sender, RoutedEventArgs e){
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;
            this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            LoadProducersFromDb();
        }
        // Method to handle the want to watch button click event
        private void WantToWatchButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;
            // Check if there are movies in the want to watch list
            if (_wantToWatchMovies.Count > 0) {
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
                UpdateMoviesUI(_wantToWatchMovies);
            // Show a message if there are no movies in the want to watch list  
            } else {
                var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");
                detailsPanel.Children.Clear();
                detailsPanel.Children.Add(new TextBlock {
                    Text = "No movies in your 'Want to Watch' list.",
                    TextAlignment = TextAlignment.Center,
                    FontSize = 18,
                    FontWeight = FontWeight.Bold,
                    Margin = new Thickness(0, 10)

                });
                detailsPanel.IsVisible = true;
            }
        }
        // Method to handle the genre button click event
        private void GenreButton_Click(object? sender, RoutedEventArgs e) {
            // Check if the sender is a button and the tag is a string genre
            if (sender is Button button && button.Tag is string genre)
                FilterMoviesByGenre(genre);
        }
        // Method to filter movies by genre
        private void FilterMoviesByGenre(string genre) {
            // Get the filtered movies by genre
            var filteredMovies = genre == "All" ? _allMovies : _moviesByGenre.GetValueOrDefault(genre, new List<Movie>());
            UpdateMoviesUI(filteredMovies);
        }
        // Method to handle the producer image pointer pressed event
        private void ProducerImage_PointerPressed(object? sender, PointerPressedEventArgs e) {
            if (sender is Image image && image.Tag is Producer producer) {
                _currentProducer = producer;
                ShowProducerDetails(producer);
            }
        }
        // Method to show producer details
        private void ShowProducerDetails(Producer producer) {
            var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");

            detailsPanel.Children.Clear();

            detailsPanel.Children.Add(new Image{
                Source = new Bitmap($"pImages/{producer.Name?.Replace(" ", string.Empty)}.jpg"),
                Width = 200,
                Height = 300,
                Margin = new Thickness(0, 0, 0, 10)
            });
            detailsPanel.Children.Add(new TextBlock{
                Text = $"Name: {producer.Name}",
                FontWeight = FontWeight.Bold,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock  {
                Text = $"Year of Birth: {producer.YearOfBirth ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5) 
            });
            detailsPanel.Children.Add(new TextBlock{
                Text = $"Most Famous Movies: {producer.MostFamousMovies ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5)
            });
            detailsPanel.Children.Add(new TextBlock {
                Text = $"Country of Origin: {producer.CountryOfOrigin ?? "N/A"}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5)
            });

            detailsPanel.IsVisible = true;
        }
        // Method to handle the add to watch button click event
        private void AddToWatchButton_Click(object? sender, RoutedEventArgs e) {
            if (sender is Button button && button.Tag is Movie movie){
                if (!_wantToWatchMovies.Any(m => m.Id == movie.Id)) {
                    _wantToWatchMovies.Add(movie);

                    var message = new TextBlock {
                        Text = "Successfully added",
                        TextAlignment = TextAlignment.Center,
                        Background = Brushes.LightGreen,
                        Foreground = new SolidColorBrush(Color.Parse("#006400")), 
                        FontSize = 16,
                        Margin = new Thickness(0, 10, 0, 0) 
                    };

                    button.Background = Brushes.Blue;
                    this.FindControl<StackPanel>("DetailsPanel").Children.Add(message);

                    //hide the message after 3 seconds
                    Task.Delay(3000).ContinueWith(_ =>
                        Dispatcher.UIThread.InvokeAsync(() =>
                            this.FindControl<StackPanel>("DetailsPanel").Children.Remove(message)));
                }
            }
        }
    }
    // Class for the movie
    public class Movie{
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
        public List<Actor> Actors { get; set; } = new List<Actor>();
    }
    // Class for the actor
    public class Actor {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Spouse { get; set; }
        public string? Biography { get; set; }
    }
    // Class for the producer
    public class Producer{
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? YearOfBirth { get; set; }
        public string? MostFamousMovies { get; set; }
        public string? CountryOfOrigin { get; set; }
    }
}