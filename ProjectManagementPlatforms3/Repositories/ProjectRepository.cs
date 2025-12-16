using ProjectManagementPlatform.Data;
using ProjectManagementPlatform.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ProjectManagementPlatform.Repositories
{
    public class ProjectRepository
    {
        private readonly DatabaseManager _dbManager;

        public ProjectRepository()
        {
            _dbManager = new DatabaseManager();
            _dbManager.InitializeDatabase();
        }

        public List<Project> GetAllProjects()
        {
            var projects = new List<Project>();

            _dbManager.ExecuteWithConnection(connection =>
            {
                string sql = "SELECT * FROM Projects ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        projects.Add(new Project
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            StartDate = Convert.ToDateTime(reader["StartDate"]),
                            Deadline = Convert.ToDateTime(reader["Deadline"]),
                            Budget = Convert.ToDecimal(reader["Budget"]),
                            Status = reader["Status"].ToString(),
                            ManagerId = Convert.ToInt32(reader["ManagerId"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            });

            return projects;
        }

        public bool AddProject(Project project)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = @"
                    INSERT INTO Projects (Name, Description, StartDate, Deadline, Budget, Status, ManagerId, CreatedAt) 
                    VALUES (@Name, @Description, @StartDate, @Deadline, @Budget, @Status, @ManagerId, datetime('now'))";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", project.Name);
                        command.Parameters.AddWithValue("@Description", project.Description ?? "");
                        command.Parameters.AddWithValue("@StartDate", project.StartDate);
                        command.Parameters.AddWithValue("@Deadline", project.Deadline);
                        command.Parameters.AddWithValue("@Budget", project.Budget);
                        command.Parameters.AddWithValue("@Status", project.Status);
                        command.Parameters.AddWithValue("@ManagerId", project.ManagerId);

                        command.ExecuteNonQuery();
                    }
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateProject(Project project)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = @"
                    UPDATE Projects 
                    SET Name = @Name, 
                        Description = @Description, 
                        Deadline = @Deadline, 
                        Budget = @Budget, 
                        Status = @Status 
                    WHERE Id = @Id";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", project.Id);
                        command.Parameters.AddWithValue("@Name", project.Name);
                        command.Parameters.AddWithValue("@Description", project.Description ?? "");
                        command.Parameters.AddWithValue("@Deadline", project.Deadline);
                        command.Parameters.AddWithValue("@Budget", project.Budget);
                        command.Parameters.AddWithValue("@Status", project.Status);

                        command.ExecuteNonQuery();
                    }
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteProject(int id)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    // Сначала удаляем задачи проекта
                    string deleteTasksSql = "DELETE FROM Tasks WHERE ProjectId = @ProjectId";
                    using (var tasksCommand = new SQLiteCommand(deleteTasksSql, connection))
                    {
                        tasksCommand.Parameters.AddWithValue("@ProjectId", id);
                        tasksCommand.ExecuteNonQuery();
                    }

                    // Затем удаляем проект
                    string deleteProjectSql = "DELETE FROM Projects WHERE Id = @Id";
                    using (var projectCommand = new SQLiteCommand(deleteProjectSql, connection))
                    {
                        projectCommand.Parameters.AddWithValue("@Id", id);
                        projectCommand.ExecuteNonQuery();
                    }
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<Project> SearchProjects(string searchTerm)
        {
            var projects = new List<Project>();

            _dbManager.ExecuteWithConnection(connection =>
            {
                string sql = "SELECT * FROM Projects WHERE Name LIKE @SearchTerm OR Description LIKE @SearchTerm ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            projects.Add(new Project
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                StartDate = Convert.ToDateTime(reader["StartDate"]),
                                Deadline = Convert.ToDateTime(reader["Deadline"]),
                                Budget = Convert.ToDecimal(reader["Budget"]),
                                Status = reader["Status"].ToString(),
                                ManagerId = Convert.ToInt32(reader["ManagerId"]),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            });

            return projects;
        }
    }
}