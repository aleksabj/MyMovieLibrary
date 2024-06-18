# MyMovieLibrary - Developer Guide

## Introduction

This guide is for developers interested in understanding and contributing to the MyMovieLibrary project. The application, built with C# and the Avalonia framework, offers a dynamic and user-friendly way to manage a movie library, utilizing Azure Database for backend storage. This document covers the project's structure, database integration, key functionalities, and extension best practices.

## Project Structure

- `DataAccess/`: Contains Entity Framework Core context for database interactions.
- `src/`: Source code of the application, including Avalonia XAML UI definitions.
    - `App.axaml`: The entry point for Avalonia application.
    - `App.axaml.cs`: Code-behind for the application startup.
    - `MainWindow.axaml.cs`: Contains main logic such as event handlers, UI updates, and database interactions.
- `aImages/`, `mImages/`, `pImages/`: These folders contain images for actors, movies, and producers respectively.
- `database/`: Includes scripts like `setup_database.sh` and `MovieDB_dump.sql` for database setup.
- `docker-compose.yml` & `Dockerfile`: Docker configurations for building and running the application in containers.
- `appsettings.json`: Configuration file containing database connection strings.

## Setting Up the Development Environment

### Prerequisites

- .NET 6.0 SDK
- Docker (for containerization)
- Git (for cloning the repository)

For the MyMovieLibrary project, as the developer, I've opted for a mix of robust technologies and frameworks. The core development is carried out in C#, with Avalonia as the choice framework for the user interface, ensuring a smooth, cross-platform experience. On the backend, I leverage Azure Database for MySQL, providing a secure and scalable database solution. The whole project is structured around the .NET 6.0 SDK, which supports its cross-platform capabilities.

**Installation and Setup Steps:**

1. **Clone the Repository:**
Start by cloning the project to your local system using:
```bash
git clone https://github.com/aleksabj/MyMovieLibrary.git
cd MyMovieLibrary
```

2. **Install Dependencies:**
Ensure that the .NET 6.0 SDK and Docker are installed on your system. Docker is utilized for containerization, simplifying the setup and deployment process.

3. **Build the Project:**
With dependencies in place, you can easily build the project by executing:
```bash
dotnet build
```

4. **Database Configuration:**
The project interfaces with an Azure-hosted MySQL database. Thankfully, there's no need for manual setup here, as the `appsettings.json` file contains the pre-configured connection string.

5. **Run the Application:**
You have the choice of running the application directly or using Docker. For a Docker run, use:
```bash
docker-compose up --build
```
Alternatively, for a more straightforward execution:
```bash
dotnet run
```

**Cross-Platform Compatibility:**

Given the project's foundation on .NET 6.0 SDK and the Avalonia framework, it's inherently cross-platform. This means seamless operation across Windows, macOS, and Linux. As someone who primarily operates on macOS, I found Avalonia to be exceptionally accommodating. It provides excellent integration and performance on macOS, which, in my experience, aligns it as a prime choice for developers on this platform.


## Contribution Guidelines

1. Fork the repository.
2. Create a feature branch.
3. Implement your feature or bug fix.
4. Add, commit, and push your changes.
5. Submit a pull request.

## Contact

Please open an issue on the GitHub repository for any questions or discussions related to contributions.
