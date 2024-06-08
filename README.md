# **Movie Library Project**

Welcome to the **Movie Library Project** repository. This application helps you manage and browse a library of movies, including details about each film and its actors and producers.

## **Prerequisites**

Before you begin, make sure you have the following software installed:

- [.NET SDK](https://dotnet.microsoft.com/download) - **Required** to build and run the project.
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) - For the database.
- [Docker (Optional)](https://www.docker.com) - **Recommended** for easier setup and deployment.

## **Installation**

### **Using Docker (Recommended)**

Docker provides an easy and consistent setup for running the application. Follow these steps:

1. **Clone the Repository:**
   ```shell
   git clone https://github.com/aleksabj/MyMovieLibrary.git
   cd MyMovieLibrary
   ```

2. **Build and Run the Application with Docker Compose:**
   ```shell
   docker-compose up --build
   ```

   This command will build the Docker images and start the application. The application should be accessible at `http://localhost:8080`.

### **Manual Setup**

If you prefer setting up the project manually, follow these steps:

1. **Clone the Repository:**
   ```shell
   git clone https://github.com/aleksabj/MyMovieLibrary.git
   cd MyMovieLibrary
   ```

2. **Install .NET Dependencies:**
   ```shell
   dotnet restore
   ```

3. **Set Up the Database:**

   #### On _Unix-based Systems_:
   ```shell
   ./setup_database.sh
   ```

   #### On _Windows_:
   Open a command prompt and run:
   ```shell
   mysql -u root -p MovieDB < ./database/database_dump.sql
   ```

4. **Run the Application:**
   ```shell
   dotnet run
   ```

   The application will start and should be accessible at `http://localhost:5000`.


### **Project Structure**

- `database/`: Contains SQL dump files for setting up the database.
- `src/`: Contains the main application code.
- `Dockerfile`: Defines the Docker image for the application.
- `docker-compose.yml`: Configures Docker services for the application.
- `setup_database.sh`: Script to set up the database on Unix-based systems.
