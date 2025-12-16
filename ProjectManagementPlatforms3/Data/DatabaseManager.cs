using System;
using System.Data.SQLite;
using System.IO;

namespace ProjectManagementPlatform.Data
{
    public class DatabaseManager
    {
        private string _databasePath;

        public DatabaseManager()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            _databasePath = Path.Combine(appPath, "ProjectManagement.db");
        }

        public void InitializeDatabase()
        {
            if (!File.Exists(_databasePath))
            {
                SQLiteConnection.CreateFile(_databasePath);

                using (var connection = GetConnection())
                {
                    connection.Open();
                    CreateTables(connection);
                    InsertSampleData(connection);
                    // Не закрываем здесь - using сам закроет
                }
            }
        }

        private void CreateTables(SQLiteConnection connection)
        {
            // Таблица пользователей
            string createUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Email TEXT NOT NULL,
                FullName TEXT NOT NULL,
                Role TEXT NOT NULL,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                IsActive BOOLEAN DEFAULT 1
            )";

            // Таблица проектов
            string createProjectsTable = @"
            CREATE TABLE IF NOT EXISTS Projects (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                StartDate DATETIME NOT NULL,
                Deadline DATETIME NOT NULL,
                Budget REAL,
                Status TEXT NOT NULL,
                ManagerId INTEGER,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            )";

            // Таблица задач
            string createTasksTable = @"
            CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT,
                ProjectId INTEGER NOT NULL,
                AssignedTo TEXT,
                DueDate DATETIME NOT NULL,
                Priority TEXT NOT NULL,
                Status TEXT NOT NULL,
                EstimatedHours REAL,
                ActualHours REAL,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            )";
            string createCommentsTable = @"
    CREATE TABLE IF NOT EXISTS Comments (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Content TEXT NOT NULL,
        AuthorId INTEGER NOT NULL,
        TaskId INTEGER,
        ProjectId INTEGER,
        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (AuthorId) REFERENCES Users(Id),
        FOREIGN KEY (TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE,
        FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
             )";

            ExecuteNonQuery(createCommentsTable, connection);
            ExecuteNonQuery(createUsersTable, connection);
            ExecuteNonQuery(createProjectsTable, connection);
            ExecuteNonQuery(createTasksTable, connection);
        }

        private void InsertSampleData(SQLiteConnection connection)
        {
            // Проверяем, есть ли уже данные
            string checkAdmin = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
            using (var cmd = new SQLiteCommand(checkAdmin, connection))
            {
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    // Добавляем администратора
                    string insertAdmin = @"
                    INSERT INTO Users (Username, PasswordHash, Email, FullName, Role) 
                    VALUES ('admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'admin@company.com', 'Администратор', 'Admin')";

                    // Добавляем тестовый проект
                    string insertProject = @"
                    INSERT INTO Projects (Name, Description, StartDate, Deadline, Budget, Status, ManagerId) 
                    VALUES ('Разработка сайта', 'Создание корпоративного сайта', 
                            datetime('now'), datetime('now', '+2 months'), 100000, 'InProgress', 1)";

                    ExecuteNonQuery(insertAdmin, connection);
                    ExecuteNonQuery(insertProject, connection);
                }
            }
        }

        private void ExecuteNonQuery(string sql, SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;");
            return connection;
        }

      
        public void ExecuteWithConnection(Action<SQLiteConnection> action)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                action(connection);
              
            }
        }
    }
}