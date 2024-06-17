# MyMovieLibrary - Developer Guide

## Introduction
This guide is intended for developers who are interested in understanding and contributing to the MyMovieLibrary project. It provides detailed information about the project's structure, database integration, key functionalities, and best practices for extending the application.

## Project Structure
- **`MainWindow.cs`**: Contains the main logic of the application, including event handlers, UI updates, and database interactions.
- **`Movie.cs`**, **`Actor.cs`**, **`Producer.cs`**: Data models representing movies, actors, and producers respectively.
- **`DatabaseContext.cs`**: Manages database connections and interactions.
- **`Resources/`**: Contains images for movies, actors, and producers.
- **`UI/`**: UI components such as XAML files and custom user controls.
- **`ViewModels/`**: Contains the view models used to bind data to the UI.

## Setting Up the Development Environment

### Prerequisites
- .NET SDK
- MySQL server
- Visual Studio or any C# compatible IDE

### Clone the Repository
```bash
git clone https://github.com/yourusername/MyMovieLibrary.git
cd MyMovieLibrary
```

### Database Setup
Ensure you have a MySQL server running and accessible. Create the required database and tables using the provided SQL script in `scripts/initialize_db.sql`.

### Connection String
Update the connection string in `DatabaseContext.cs` according to your database setup:
```csharp
private static readonly string ConnectionString = "server=localhost;user=root;password=yourpassword;database=MovieDB;";
```

## Database Integration

### DatabaseContext
```csharp
public static class DatabaseContext {
    private static readonly string ConnectionString = "your_connection_string";

    public static async Task<MySqlConnection> GetDBConnectionAsync() {
        var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
```
This static class handles the creation and management of the database connection, ensuring that connections are opened and closed properly to avoid leaks.

### Data Loading Methods
- **Movies:**
  ```csharp
  private async void LoadMoviesFromDb() {
      using (var connection = await DatabaseContext.GetDBConnectionAsync()) {
          // Database query and data processing logic
      }
  }
  ```
  Fetches movies from the database and updates the UI.

- **Actors:**
  ```csharp
  private async void LoadActorsFromDb() {
      using (var connection = await DatabaseContext.GetDBConnectionAsync()) {
          // Database query and data processing logic
      }
  }
  ```
  Fetches actors from the database and updates the UI.

- **Producers:**
  ```csharp
  private async void LoadProducersFromDb() {
      using (var connection = await DatabaseContext.GetDBConnectionAsync()) {
          // Database query and data processing logic
      }
  }
  ```
  Fetches producers from the database and updates the UI.

## UI Updates
- **UpdateMoviesUI:** Updates the list of movies displayed based on current filters and sorting criteria.
- **UpdateActorsUI:** Updates the list of actors displayed.
- **UpdateProducersUI:** Updates the list of producers displayed.

## Event Handlers
- **Genre Filtering:**
  ```csharp
  private void GenreButton_Click(object? sender, RoutedEventArgs e) {
      // Logic for filtering movies by genre
  }
  ```
  Filters the movies based on the selected genre.

- **Detail View:**
  ```csharp
  private void Image_PointerPressed(object? sender, PointerPressedEventArgs e) {
      // Logic for displaying detailed information about the selected movie
  }
  ```
  Displays detailed information about the selected movie.

### Adding Movies to Watch List
```csharp
private void AddToWatchButton_Click(object? sender, RoutedEventArgs e) {
    // Logic for adding the movie to the watch list and updating the UI
}
```
Adds the movie to the user's "Want to Watch" list and updates the UI with a confirmation message.

## Best Practices

### Code Organization
Maintain a clean and logical project structure. Group related files together and adhere to the MVC or MVVM pattern wherever applicable.

### Database Operations
Use asynchronous database operations to ensure the UI remains responsive. Wrap database calls in try-catch blocks to handle exceptions gracefully.

### UI Responsiveness
Use data binding and observable collections to ensure that UI updates are efficient and reflect the current state of the application's data.

### Extending the Application
When adding new features, follow the existing code patterns and structure. Add new database tables and update the data models as needed, ensuring that all database interactions are performed asynchronously.

## Running Tests
### Unit Tests
We use xUnit for unit testing. Test files are located in the `Tests/` directory and can be run using the following command:
```bash
dotnet test
```
### Integration Tests
Integration tests are also written using xUnit and typically involve testing database interactions and UI updates together.

## Contribution Guidelines
1. Fork the repository.
2. Create a new branch.
3. Make your changes.
4. Test your changes thoroughly.
5. Submit a pull request.

## Contact
For any questions or discussions, please open an issue on the GitHub repository.
