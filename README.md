# MyMovieLibrary

Welcome to MyMovieLibrary! A dynamic, user-friendly movie library application crafted with C#, leveraging the robust Avalonia framework for the interface, and powered by Azure Database. This guide will help you set up and run the project seamlessly on different systems.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

- .NET 6.0 SDK
- Docker
- Git (for cloning the repository)

### Installing

1. **Clone the repository**

```bash
git clone https://github.com/aleksabj/MyMovieLibrary.git
cd MyMovieLibrary
```

2. **Build the project**

Using .NET CLI, you can build the project as follows:

```bash
dotnet build
```

### Setting up Azure Database

My application uses Azure Database for MySQL. The good news is, you don't need to configure the database connection manually. The `appsettings.json` file is pre-configured to connect to the existing Azure-hosted database:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "You will find the entire information in my project"
  }
}
```

### Running with Docker

This application is Docker-ready. Make sure Docker is installed and running on your system. Then, execute the following command in the project root directory:

```bash
docker-compose up --build
```

The command above builds the Docker image for the application and starts it, along with any required services.

### Running the Application

After setting up the environment, run the application using:

```bash
dotnet run 
```

The application should now be running and accessible.

## Project Structure

Here's a brief overview of the main directories and files in the project:

- `DataAccess/`: Contains the Entity Framework Core context and data access layer.
- `src/`: Source code for the application, including the Avalonia XAML interface.
- `MovieDB_dump.sql`: SQL dump file for the database.
- `docker-compose.yml` & `Dockerfile`: Docker configuration files for containerization.
- `appsettings.json`: Configuration file, including the database connection string.
- `README.md`: This file, which provides setup and running instructions.

## Contributing

Feel free to fork the repository and submit pull requests. Your contributions are greatly appreciated!


## Acknowledgments

- The Avalonia team for the fantastic framework.
- Azure for the managed database services.
- Docker for simplifying deployment.

I hope you enjoy using MyMovieLibrary as much as I enjoyed building it!
