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
    // Main class representing the window of the application
    public partial class MainWindow : Window {
        // Fields to store current selected movie and producer
        private Movie? _currentMovie;
        private Producer? _currentProducer;
        // List to store all movies loaded from the database
        private List<Movie> _allMovies = new List<Movie>();
        // Dictionary to categorize movies by their genres
        private Dictionary<string, List<Movie>> _moviesByGenre = new Dictionary<string, List<Movie>>();
        // List to store movies that the user wants to watch
        private List<Movie> _wantToWatchMovies = new List<Movie>();

        // Constructor initializing components and loading movies from the database
        public MainWindow() {
            InitializeComponent();
            LoadMoviesFromDb(); // Load movies when the window is initialized
        }

        // Static nested class providing database connection handling
        public static class DatabaseContext {
            // Connection string for accessing the database
            private static readonly string ConnectionString = "server=localhost;user=root;password=Jurnalist1;database=MovieDB;";

            // Method to asynchronously get a database connection
            public static async Task<MySqlConnection> GetDBConnectionAsync() {
                var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();
                return connection; // Return an open connection
            }
        }

        // Asynchronously load movies from the database
        private async void LoadMoviesFromDb() {
            var movies = new List<Movie>();
            var genresSet = new HashSet<string>(); // Set to store unique genres

            try {
                // Get the database connection
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                // Query to fetch all movies
                const string query = "SELECT * FROM Movies";
                using var command = new MySqlCommand(query, connection);

                // Execute the query and read the results
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    // Read movie properties from the database
                    var movieId = Convert.ToInt32(reader["MovieID"]);
                    var title = reader["Title"].ToString() ?? string.Empty;

                    Console.WriteLine($"MovieID: {movieId}, Title: {title}"); // Log the movieID and title

                    // Create a new Movie object and populate its properties
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

                    Console.WriteLine($"Assigned {movie.Actors.Count} actors to movie: {movie.Title}"); // Log the number of actors assigned to the movie
                    movies.Add(movie); // Add the movie to the local list

                    // Process each genre the movie belongs to
                    foreach (var genre in movie.Genre.Split(',').Select(g => g.Trim())) {
                        genresSet.Add(genre); // Add the genre to the genre set

                        // Initialize genre list if it doesn't exist
                        if (!_moviesByGenre.ContainsKey(genre))
                            _moviesByGenre[genre] = new List<Movie>();
                        
                        // Add the movie to the genre list if it's not already present
                        if (!_moviesByGenre[genre].Any(m => m.Id == movie.Id))
                            _moviesByGenre[genre].Add(movie);
                    }
                }

                _allMovies = movies; // Update class-level list of all movies
                CreateGenreFilterButtons(genresSet); // Create filter buttons based on genres
                UpdateMoviesUI(movies); // Update the UI with the loaded movies
            }
            catch (Exception ex) {
                // Log any errors that occur during the process
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }


private void CreateGenreFilterButtons(IEnumerable<string> genres) {
    // Find the StackPanel in the UI where the genre filter buttons will be added
    var genreFilterPanel = this.FindControl<StackPanel>("GenreFilterPanel");

    // Clear any existing buttons to start fresh
    genreFilterPanel.Children.Clear();

    // Create and configure the "All" button, which will display all movies when clicked
    var allButton = new Button { Content = "All", Tag = "All", Margin = new Thickness(5)};
    allButton.Click += GenreButton_Click; // Attach the click event handler to the "All" button
    genreFilterPanel.Children.Add(allButton); // Add the "All" button to the genre filter panel

    // Iterate over each genre, ordered alphabetically
    foreach (var genre in genres.OrderBy(g => g)) {
        // Create and configure a button for each genre
        var genreButton = new Button { Content = genre, Tag = genre, Margin = new Thickness(5) };
        genreButton.Click += GenreButton_Click; // Attach the click event handler to the genre button
        genreFilterPanel.Children.Add(genreButton); // Add the genre button to the genre filter panel
    }
}


// Method responsible for updating the UI with a list of movies
private void UpdateMoviesUI(List<Movie> movies) {
    // Find the ItemsControl within the UI, where the movies will be displayed
    var moviesPanel = this.FindControl<ItemsControl>("MoviesPanel");
    
    // Clear any existing items in the moviesPanel
    moviesPanel.ItemsSource = null;

    // Create a list to hold the visual representations (views) of each movie
    var movieViews = new List<StackPanel>();

    // Loop through each movie provided in the list
    foreach (var movie in movies) {
        // Construct the expected path for the movie's poster image using the title
        var posterPath = $"mImages/{movie.Title.Replace(" ", string.Empty)}.jpg";
        
        // Skip this movie if the poster image does not exist
        if (!File.Exists(posterPath))
            continue;

        // Create an Image control to display the movie's poster
        var image = new Image {
            Source = new Bitmap(posterPath), // Set the image source to the poster path
            Stretch = Stretch.Uniform,      // Ensure the image maintains its aspect ratio
            Tag = movie                     // Store the movie object in the Tag property for later reference
        };

        // Create a TextBlock to display the movie's title
        var titleTextBlock = new TextBlock {
            Text = movie.Title,
            TextAlignment = TextAlignment.Center,  // Center-align the title text
            TextWrapping = TextWrapping.Wrap       // Wrap the text if it's too long
        };

        // Create a TextBlock to display the movie's release year
        var yearTextBlock = new TextBlock {
            Text = $"({movie.ReleaseYear})",
            TextAlignment = TextAlignment.Center,  // Center-align the year text
            TextWrapping = TextWrapping.Wrap       // Wrap the text if it's too long
        };

        // Create a StackPanel to hold the image and text controls for the movie
        var stackPanel = new StackPanel {
            Margin = new Thickness(10)  // Add some margin around the stack panel
        };

        // Add the image and text blocks to the StackPanel
        stackPanel.Children.Add(image);
        stackPanel.Children.Add(titleTextBlock);
        stackPanel.Children.Add(yearTextBlock);

        // Add an event handler to handle pointer press events on the image
        image.PointerPressed += Image_PointerPressed;

        // Add the StackPanel to the list of movie views
        movieViews.Add(stackPanel);
    }

    // Set the ItemsSource of moviesPanel to the list of movie views
    moviesPanel.ItemsSource = movieViews;

    // Update the visibility of movie titles based on the current width of the UI
    UpdateMovieTitleVisibility(this.Width);
}

// Asynchronous method for loading actors from the database
private async void LoadActorsFromDb() {
    var actors = new List<Actor>(); // List to hold the actor data

    try {
        // Obtain a database connection asynchronously
        using var connection = await DatabaseContext.GetDBConnectionAsync();
        
        // SQL query to select all actors from the Actors table
        const string query = "SELECT * FROM Actors";
        using var command = new MySqlCommand(query, connection);

        // Execute the query and obtain a data reader for the results
        using var reader = await command.ExecuteReaderAsync();

        // Read each row of the result set asynchronously
        while (await reader.ReadAsync()) {
            // Create an Actor object and populate it with data from the current row
            var actor = new Actor {
                Id = Convert.ToInt32(reader["ActorID"]),  // Convert ActorID to an integer and set it
                Name = reader["Name"].ToString(),         // Convert the Name field to a string and set it
                Spouse = reader["Spouse"]?.ToString(),    // Convert the Spouse field to a string if it's not null
                Biography = reader["Biography"]?.ToString() // Convert the Biography field to a string if it's not null
            };

            // Add the populated Actor object to the list of actors
            actors.Add(actor);
        }

        // Update the UI with the list of actors
        UpdateActorsUI(actors);
    }
    // Catch any exceptions that occur during the database operations
    catch (Exception ex) {
        // Log the exception message to the console
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}


private void UpdateActorsUI(List<Actor> actors) {
    // Find the ItemsControl named "ActorsPanel" in the UI and clear its item source.
    var actorsPanel = this.FindControl<ItemsControl>("ActorsPanel");
    actorsPanel.ItemsSource = null;

    // Create a list to hold StackPanel elements for each actor.
    var actorViews = new List<StackPanel>();

    foreach (var actor in actors) {
        // Construct the path to the actor's photo by replacing spaces in the actor's name.
        var photoPath = $"aImages/{actor.Name?.Replace(" ", string.Empty)}.jpg";
        // Skip the actor if the photo does not exist.
        if (!File.Exists(photoPath))
            continue;

        // Create an Image control to display the actor's photo.
        var image = new Image {
            Source = new Bitmap(photoPath),
            Stretch = Stretch.Uniform, // Maintain the aspect ratio while scaling.
            Tag = actor // Store the actor object in the Tag property for reference.
        };

        // Create a TextBlock to display the actor's name.
        var nameTextBlock = new TextBlock {
            Text = actor.Name ?? string.Empty,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };

        // Create a StackPanel to hold the Image and TextBlock.
        var stackPanel = new StackPanel {
            Margin = new Thickness(10) // Add some margin around the panel.
        };

        // Add the Image and TextBlock to the StackPanel.
        stackPanel.Children.Add(image);
        stackPanel.Children.Add(nameTextBlock);

        // Attach an event handler to the Image's PointerPressed event.
        image.PointerPressed += ActorImage_PointerPressed;
        
        // Add the StackPanel to the list of actor views.
        actorViews.Add(stackPanel);
    }

    // Set the ItemsControl's item source to the list of StackPanels.
    actorsPanel.ItemsSource = actorViews;

    // Update the visibility of the actor names based on the current width of the control.
    UpdateActorNameVisibility(this.Width);
}

private async void LoadProducersFromDb() {
    var producers = new List<Producer>();
    try {
        // Obtain a database connection asynchronously.
        using var connection = await DatabaseContext.GetDBConnectionAsync();
        
        // Define a query to select all records from the Producers table.
        const string query = "SELECT * FROM Producers";
        
        // Execute the query using a MySqlCommand.
        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        // Read each producer record from the database.
        while (await reader.ReadAsync()) {
            // Create a new Producer object and populate its properties from the database record.
            var producer = new Producer {
                Id = Convert.ToInt32(reader["id"]),
                Name = reader["name"].ToString(),
                YearOfBirth = reader["year_of_birth"]?.ToString(),
                MostFamousMovies = reader["most_famous_movies"]?.ToString(),
                CountryOfOrigin = reader["country_of_origin"]?.ToString(),
            };
            // Add the producer to the list.
            producers.Add(producer);
        }
        // Update the UI with the list of producers.
        UpdateProducersUI(producers);
    }
    catch (Exception ex) {
        // Log any exception that occurs during data retrieval.
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}


// Updates the UI with a list of producers by creating visual elements for each.
private void UpdateProducersUI(List<Producer> producers) {
    // Finds the ItemsControl named "ProducersPanel" in the UI to update.
    var producersPanel = this.FindControl<ItemsControl>("ProducersPanel");

    // Clears the existing items in the ItemsControl.
    producersPanel.ItemsSource = null;

    // A list to hold UI elements (StackPanels) for each producer.
    var producerViews = new List<StackPanel>();

    // Iterate through the list of producers.
    foreach (var producer in producers) {
        // Constructs the file path for the producer's image.
        var photoPath = $"pImages/{producer.Name?.Replace(" ", string.Empty)}.jpg";
        if (!File.Exists(photoPath))
            continue; // Skips to the next producer if the image file does not exist.

        // Creates an Image control for the producer's photo.
        var image = new Image {
            Source = new Bitmap(photoPath),
            Stretch = Stretch.Uniform,
            Tag = producer // Stores the producer object in the Tag property.
        };

        // Creates a TextBlock to display the producer's name.
        var nameTextBlock = new TextBlock {
            Text = producer.Name ?? string.Empty,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };

        // Creates a StackPanel to hold the image and name TextBlock.
        var stackPanel = new StackPanel {
            Margin = new Thickness(10)
        };

        stackPanel.Children.Add(image); // Adds the Image to the StackPanel.
        stackPanel.Children.Add(nameTextBlock); // Adds the TextBlock to the StackPanel.

        // Adds event handler for clicking on the producer's image.
        image.PointerPressed += ProducerImage_PointerPressed;

        // Adds the StackPanel to the list of producer views.
        producerViews.Add(stackPanel);
    }

    // Sets the ItemsSource of the ItemsControl to the list of StackPanels.
    producersPanel.ItemsSource = producerViews;

    // Updates the visibility of the producer names based on the width of the control.
    UpdateProducerNameVisibility(this.Width);
}

// Event handler for when a producer image is clicked.
private void Image_PointerPressed(object? sender, PointerPressedEventArgs e) {
    if (sender is Image image && image.Tag is Producer producer) {
        // Retrieves the producer from the Tag property and shows their details.
        _currentProducer = producer;
        ShowProducerDetails(producer);
    }
}

// Asynchronously shows the details of a movie in the UI.
private async Task ShowMovieDetails(Movie movie) {
    // Finds the StackPanel named "DetailsPanel" in the UI to update.
    var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");

    // Clears any existing children from the DetailsPanel.
    detailsPanel.Children.Clear();

    // Creates a button to add the movie to a watch list.
    var addToWatchButton = new Button {
        Content = "Add to the list",
        Tag = movie, // Stores the movie object in the Tag property.
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
    addToWatchButton.Click += AddToWatchButton_Click; // Adds event handler for button click.

    detailsPanel.Children.Add(addToWatchButton); // Adds the button to the DetailsPanel.

    // Creates and adds an Image control to display the movie poster.
    detailsPanel.Children.Add(new Image {
        Source = new Bitmap($"mImages/{movie.Title.Replace(" ", string.Empty)}.jpg"),
        Width = 200,
        Height = 300,
        Margin = new Thickness(0, 0, 0, 10)
    });

    // Creates and adds TextBlocks to display various movie details.
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
        Margin = new Thickness(0, 5) // Increases spacing between lines
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
    detailsPanel.Children.Add(new TextBlock {
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

    // Adds a bold heading for producers.
    detailsPanel.Children.Add(new TextBlock {
        Text = "Producers:",
        FontWeight = FontWeight.Bold,
        FontSize = 20,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 5)
    });

    // Creates a StackPanel to hold the list of producers.
    var producersPanel = new StackPanel { Orientation = Orientation.Vertical };

    // Splits the producer names by comma and trims any extra spaces.
    var producerNames = movie.Producers?.Split(',') ?? Array.Empty<string>();
    foreach (var producerName in producerNames) {
        var producerNameTrimmed = producerName.Trim();
        if (await ProducerExists(producerNameTrimmed)) {
            // Creates a clickable TextBlock for existing producers.
            var link = new TextBlock {
                Text = producerNameTrimmed,
                Foreground = Brushes.Blue,
                Cursor = new Cursor(StandardCursorType.Hand),
                TextWrapping = TextWrapping.Wrap
            };
            link.PointerPressed += (s, e) => ShowProducerDetailsByName(producerNameTrimmed); // Adds event handler.
            producersPanel.Children.Add(link);
        } else {
            // Adds a non-clickable TextBlock for producers not in the system.
            producersPanel.Children.Add(new TextBlock { Text = producerNameTrimmed, TextWrapping = TextWrapping.Wrap });
        }
    }

    detailsPanel.Children.Add(producersPanel); // Adds the producers panel to the DetailsPanel.

    // Adds additional movie details (actors).
    detailsPanel.Children.Add(new TextBlock {
        Text = "", // Placeholder for possible additional data.
        FontWeight = FontWeight.Bold,
        FontSize = 18,
        TextWrapping = TextWrapping.Wrap
    });

    // Creates a StackPanel to hold the list of actors.
    var actorsPanel = new StackPanel { Orientation = Orientation.Vertical };

    // Iterates through the list of actors in the movie.
    foreach (var actor in movie.Actors) {
        Console.WriteLine($"Displaying actor: {actor.Name}"); // Debugging output.

        // Creates an Image control to display the actor's photo.
        var actorImage = new Image {
            Source = new Bitmap($"aImages/{actor.Name?.Replace(" ", string.Empty)}.jpg"),
            Width = 100,
            Height = 150,
            Margin = new Thickness(5),
            Tag = actor
        };

        actorImage.PointerPressed += ActorImage_PointerPressed; // Adds event handler for clicking the actor's image.

        // Creates a StackPanel to hold the actor's image and name.
        var actorInfoPanel = new StackPanel {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        actorInfoPanel.Children.Add(actorImage); // Adds the Image to the actor info panel.
        actorInfoPanel.Children.Add(new TextBlock {
            Text = actor.Name ?? string.Empty,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        });

        actorsPanel.Children.Add(actorInfoPanel); // Adds the actor info panel to the actors panel.
    }

    detailsPanel.Children.Add(actorsPanel); // Adds the actors panel to the DetailsPanel.

    // Makes the details panel visible.
    detailsPanel.IsVisible = true;
}


        // Method to check if a producer exists in the database based on their name
private async Task<bool> ProducerExists(string producerName) {
    try {
        using var connection = await DatabaseContext.GetDBConnectionAsync(); // Get a database connection
        const string query = "SELECT COUNT(*) FROM Producers WHERE name = @ProducerName"; // SQL query to count rows with the specified producer name
        using var command = new MySqlCommand(query, connection); // Create a new SQL command with the query and connection
        command.Parameters.AddWithValue("@ProducerName", producerName); // Add the producer name as a parameter to avoid SQL injection

        var count = Convert.ToInt32(await command.ExecuteScalarAsync()); // Execute the query and get the number of rows
        return count > 0; // Return true if count is greater than 0, indicating the producer exists
    }
    catch (Exception ex) {
        Console.WriteLine($"An error occurred: {ex.Message}"); // Log any exceptions that occur
        return false; // Return false if an exception is caught
    }
}
        //A function to show producer details by name
        private async void ShowProducerDetailsByName(string producerName) {
            try {
                // Get a database connection asynchronously
                using var connection = await DatabaseContext.GetDBConnectionAsync();
                const string query = "SELECT * FROM Producers WHERE name = @ProducerName";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProducerName", producerName);
                // Execute the query and read the results
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
                    // Show the producer details in the UI
                    this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
                    this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
                    this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = true;
                    this.FindControl<StackPanel>("DetailsPanel").IsVisible = true;
                }
            }
            //  Log any errors that occur during the process
            catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        // Event handler for when an actor image is clicked
        private void ActorImage_PointerPressed(object? sender, PointerPressedEventArgs e) {
            if (sender is Image image && image.Tag is Actor actor)  {
                ShowActorDetails(actor);
                // Show the actor details in the UI
                this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
                this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = true;
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = true;
            }
        }
        // Method to show actor details in the UI
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
        // Method to update the visibility of movie titles based on the window width
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
        // Method to update the visibility of actor names based on the window width
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
        // Method to update the visibility of producer names based on the window width
        private void UpdateProducerNameVisibility(double windowWidth) {
            // Find the ItemsControl named "ProducersPanel" in the UI
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
        // Event handler for when the window size changes
        private void Window_SizeChanged(object? sender, SizeChangedEventArgs e) {
            UpdateMovieTitleVisibility(e.NewSize.Width);
            UpdateActorNameVisibility(e.NewSize.Width);
            UpdateProducerNameVisibility(e.NewSize.Width);
        }   

        // Event handler for when the Movies button is clicked
        private void MoviesButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false;
 
            // Show the movie details if a movie is currently selected
            if (_currentMovie != null) {
                ShowMovieDetails(_currentMovie);
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = true;
            } else {
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            }

            UpdateMoviesUI(_allMovies);
        }
        // Event handler for when the Actors button is clicked
        private void ActorsButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;
            this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            LoadActorsFromDb();
        }
        // Event handler for when the Producers button is clicked
        private void ProducersButton_Click(object? sender, RoutedEventArgs e){
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;
            this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
            LoadProducersFromDb();
        }
        // Event handler for when the 'Want to Watch' button is clicked
        private void WantToWatchButton_Click(object? sender, RoutedEventArgs e) {
            this.FindControl<ScrollViewer>("MoviesScrollViewer").IsVisible = true;
            this.FindControl<ScrollViewer>("ActorsScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("ProducersScrollViewer").IsVisible = false;
            this.FindControl<ScrollViewer>("GenreFilterScrollViewer").IsVisible = false;
            // Show the movies in the 'Want to Watch' list
            if (_wantToWatchMovies.Count > 0) {
                this.FindControl<StackPanel>("DetailsPanel").IsVisible = false;
                UpdateMoviesUI(_wantToWatchMovies);
            // Show a message if the list is empty        
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
        // Event handler for when a genre filter button is clicked
        private void GenreButton_Click(object? sender, RoutedEventArgs e) {
            if (sender is Button button && button.Tag is string genre)
                FilterMoviesByGenre(genre);
        }
        // Method to filter movies by genre and update the UI
        private void FilterMoviesByGenre(string genre) {
            var filteredMovies = genre == "All" ? _allMovies : _moviesByGenre.GetValueOrDefault(genre, new List<Movie>());
            UpdateMoviesUI(filteredMovies);
        }
        // Event handler for when a movie image is clicked
        private void ProducerImage_PointerPressed(object? sender, PointerPressedEventArgs e) {
            if (sender is Image image && image.Tag is Producer producer) { // Check if the sender is an Image and the Tag is a Producer
                _currentProducer = producer;
                ShowProducerDetails(producer);
            }
        }
        // Method to show producer details in the UI
        private void ShowProducerDetails(Producer producer) {
            var detailsPanel = this.FindControl<StackPanel>("DetailsPanel");
            // Clear any existing children from the DetailsPanel
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
        // Event handler for when the 'Add to Watch' button is clicked
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
    
// Movie, Actor, and Producer classes represent the data model for the application
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
