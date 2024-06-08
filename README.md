# Movie Library Project

## Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- [Docker (Optional)](https://www.docker.com) - Recommended for easier setup

## Installation

### Using Docker (Recommended):

1. **Clone the repository:**
   ```shell
   git clone <your-repository-url>
   cd <your-repository-dir>
   dotnet restore
UNIX: ./setup_database.sh
WINDOWS: mysql -u root -p MovieDB < ./database/database_dump.sql
dotnet run

