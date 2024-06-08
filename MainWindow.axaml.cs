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
        private Movie? _currentMovie;
        private Producer? _currentProducer;
        private List<Movie> _allMovies = new List<Movie>();

        private Dictionary<string, List<Movie>> _moviesByGenre = new Dictionary<string, List<Movie>>();

        private List<Movie> _wantToWatchMovies = new List<Movie>();

        public MainWindow() {
            InitializeComponent();
            LoadMoviesFromDb();
        }

        public static class DatabaseContext {
            private static readonly string ConnectionString = "server=localhost;user=root;password=Jurnalist1;database=MovieDB;";

            public static async Task<MySqlConnection> GetDBConnectionAsync() {
                var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();
                return connection;
            }
        }

        private async void LoadMoviesFromDb() {
            var movies = new List<Movie>();
            var genresSet = new HashSet<string>();

            try {
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                const string query = "SELECT * FROM Movies";
                using var command = new MySqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    var movieId = Convert.ToInt32(reader["MovieID"]);
                    var title = reader["Title"].ToString() ?? string.Empty;

                    Console.WriteLine($"MovieID: {movieId}, Title: {title}");

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

                    Console.WriteLine($"Assigned {movie.Actors.Count} actors to movie: {movie.Title}");
                    movies.Add(movie);

                    foreach (var genre in movie.Genre.Split(',').Select(g => g.Trim())) {
                        genresSet.Add(genre);

                        if (!_moviesByGenre.ContainsKey(genre))
                            _moviesByGenre[genre] = new List<Movie>();
                        

                        if (!_moviesByGenre[genre].Any(m => m.Id == movie.Id))
                            _moviesByGenre[genre].Add(movie);
                    }
                }

                _allMovies = movies;
                CreateGenreFilterButtons(genresSet);
                UpdateMoviesUI(movies);
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void CreateGenreFilterButtons(IEnumerable<string> genres) {
            var genreFilterPanel = this.FindControl<StackPanel>("GenreFilterPanel");

            genreFilterPanel.Children.Clear();

            var allButton = new Button { Content = "All", Tag = "All", Margin = new Thickness(5)};
            allButton.Click += GenreButton_Click;
            genreFilterPanel.Children.Add(allButton);

            foreach (var genre in genres.OrderBy(g => g)) {
                var genreButton = new Button { Content = genre, Tag = genre, Margin = new Thickness(5) };
                genreButton.Click += GenreButton_Click;
                genreFilterPanel.Children.Add(genreButton);
            }
        }

        private void UpdateMoviesUI(List<Movie> movies) {
            var moviesPanel = this.FindControl<ItemsControl>("MoviesPanel");

            moviesPanel.ItemsSource = null;

            var movieViews = new List<StackPanel>();

            foreach (var movie in movies) {
                var posterPath = $"mImages/{movie.Title.Replace(" ", string.Empty)}.jpg";
                if (!File.Exists(posterPath))
                    continue;
                var image = new Image {
                    Source = new Bitmap(posterPath),
                    Stretch = Stretch.Uniform,
                    Tag = movie
                };

                var titleTextBlock = new TextBlock {
                    Text = movie.Title,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var yearTextBlock = new TextBlock {
                    Text = $"({movie.ReleaseYear})",
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var stackPanel = new StackPanel{
                    Margin = new Thickness(10)
                };

                stackPanel.Children.Add(image);
                stackPanel.Children.Add(titleTextBlock);
                stackPanel.Children.Add(yearTextBlock);

                image.PointerPressed += Image_PointerPressed;

                movieViews.Add(stackPanel);
            }

            moviesPanel.ItemsSource = movieViews;
            UpdateMovieTitleVisibility(this.Width);
        }

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

        private void Image_PointerPressed(object? sender, PointerPressedEventArgs e) {
            if (sender is Image image && image.Tag is Movie movie)  {
                _currentMovie = movie;
                ShowMovieDetails(movie);
            }
        }

        private async Task ShowMovieDetails(Movie movie) {
            var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");

            detailsPanel.Children.Clear();

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

            var producerNames = movie.Producers?.Split(',') ?? Array.Empty<string>();
            foreach (var producerName in producerNames) {
                var producerNameTrimmed = producerName.Trim();
                if (await ProducerExists(producerNameTrimmed)) {
                    var link = new TextBlock  {
                        Text = producerNameTrimmed,
                        Foreground = Brushes.Blue,
                        Cursor = new Cursor(StandardCursorType.Hand),
                        TextWrapping = TextWrapping.Wrap
                    };
                    link.PointerPressed += (s, e) => ShowProducerDetailsByName(producerNameTrimmed);
                    producersPanel.Children.Add(link);
                }
                else {
                    producersPanel.Children.Add(new TextBlock { Text = producerNameTrimmed, TextWrapping = TextWrapping.Wrap });
                }
            }

            detailsPanel.Children.Add(producersPanel);

            detailsPanel.Children.Add(new TextBlock {
                Text = "",///////
                FontWeight = FontWeight.Bold,
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap
            });

            var actorsPanel = new StackPanel { Orientation = Orientation.Vertical };

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

        private void ActorImage_PointerPressed(object? sender, PointerPressedEventArgs e) {
            if (sender is Image image && image.Tag is Actor actor)  {
                ShowActorDetails(actor);

                this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
                this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = true;
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = true;
            }
        }

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

        private void UpdateMovieTitleVisibility(double windowWidth) {
            var moviesPanel = this.FindControl<ItemsControl>("MoviesPanel");
            if (moviesPanel.ItemsSource is IEnumerable<StackPanel> moviePanels) {
                foreach (var moviePanel in moviePanels) {
                    foreach (var child in moviePanel.Children) {
                        if (child is TextBlock textBlock)
                            textBlock.IsVisible = windowWidth >= 600;
                    }
                }
            }
        }

        private void UpdateActorNameVisibility(double windowWidth) {
            var actorsPanel = this.FindControl<ItemsControl>("ActorsPanel");
            if (actorsPanel.ItemsSource is IEnumerable<StackPanel> actorPanels) {
                foreach (var actorPanel in actorPanels) {
                    foreach (var child in actorPanel.Children) {
                        if (child is TextBlock textBlock)
                            textBlock.IsVisible = windowWidth >= 600;
                    }
                }
            }
        }

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

        private void Window_SizeChanged(object? sender, SizeChangedEventArgs e) {
            UpdateMovieTitleVisibility(e.NewSize.Width);
            UpdateActorNameVisibility(e.NewSize.Width);
            UpdateProducerNameVisibility(e.NewSize.Width);
        }

        private void MoviesButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false;
 

            if (_currentMovie != null) {
                ShowMovieDetails(_currentMovie);
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = true;
            } else {
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            }

            UpdateMoviesUI(_allMovies);
        }

        private void ActorsButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;
            this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            LoadActorsFromDb();
        }

        private void ProducersButton_Click(object? sender, RoutedEventArgs e){
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;
            this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            LoadProducersFromDb();
        }

        private void WantToWatchButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;

            if (_wantToWatchMovies.Count > 0) {
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
                UpdateMoviesUI(_wantToWatchMovies);
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

        private void GenreButton_Click(object? sender, RoutedEventArgs e) {
            if (sender is Button button && button.Tag is string genre)
                FilterMoviesByGenre(genre);
        }

        private void FilterMoviesByGenre(string genre) {
            var filteredMovies = genre == "All" ? _allMovies : _moviesByGenre.GetValueOrDefault(genre, new List<Movie>());
            UpdateMoviesUI(filteredMovies);
        }

        private void ProducerImage_PointerPressed(object? sender, PointerPressedEventArgs e) {
            if (sender is Image image && image.Tag is Producer producer)
            {
                _currentProducer = producer;
                ShowProducerDetails(producer);
            }
        }

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

    public class Actor {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Spouse { get; set; }
        public string? Biography { get; set; }
    }

    public class Producer{
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? YearOfBirth { get; set; }
        public string? MostFamousMovies { get; set; }
        public string? CountryOfOrigin { get; set; }
    }
}
