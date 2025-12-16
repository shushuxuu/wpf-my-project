using ProjectManagementPlatform.Data;
using ProjectManagementPlatform.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ProjectManagementPlatform.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseManager _dbManager;

        public UserRepository()
        {
            _dbManager = new DatabaseManager();
            _dbManager.InitializeDatabase();
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            _dbManager.ExecuteWithConnection(connection =>
            {
                string sql = "SELECT Id, Username, Email, FullName, Role, IsActive FROM Users WHERE IsActive = 1";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Username = reader["Username"].ToString(),
                            Email = reader["Email"].ToString(),
                            FullName = reader["FullName"].ToString(),
                            Role = reader["Role"].ToString(),
                            IsActive = Convert.ToBoolean(reader["IsActive"])
                        });
                    }
                }
            });

            return users;
        }

        public User GetUserById(int id)
        {
            User user = null;

            _dbManager.ExecuteWithConnection(connection =>
            {
                string sql = "SELECT Id, Username, Email, FullName, Role, IsActive FROM Users WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Username = reader["Username"].ToString(),
                                Email = reader["Email"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Role = reader["Role"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
            });

            return user;
        }
    }
}