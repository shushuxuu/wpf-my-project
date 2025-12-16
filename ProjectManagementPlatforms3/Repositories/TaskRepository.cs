using ProjectManagementPlatform.Data;
using ProjectManagementPlatform.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ProjectManagementPlatform.Repositories
{
    public class TaskRepository
    {
        private readonly DatabaseManager _dbManager;

        public TaskRepository()
        {
            _dbManager = new DatabaseManager();
            _dbManager.InitializeDatabase();
        }

        public List<string> GetAvailableAssignees()
        {
            return new List<string>
            {
                "Не назначено",
                "Иванов И.И.",
                "Петров П.П.",
                "Сидоров С.С.",
                "Кузнецов К.К.",
                "Смирнов С.С."
            };
        }

        // Упрощенный метод изменения приоритета
        public bool UpdateTaskPriority(int taskId, string priority)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = "UPDATE Tasks SET Priority = @Priority WHERE Id = @Id";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", taskId);
                        command.Parameters.AddWithValue("@Priority", priority);
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

        // Упрощенный метод для назначения исполнителя
        public bool AssignTaskTo(int taskId, string assignee)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = "UPDATE Tasks SET AssignedTo = @AssignedTo WHERE Id = @Id";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", taskId);
                        command.Parameters.AddWithValue("@AssignedTo", assignee);
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

        // Обновленный метод UpdateTask
        public bool UpdateTask(Task task)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = @"
                    UPDATE Tasks 
                    SET Title = @Title,
                        Description = @Description,
                        AssignedTo = @AssignedTo,
                        DueDate = @DueDate,
                        Priority = @Priority,
                        Status = @Status,
                        EstimatedHours = @EstimatedHours,
                        ActualHours = @ActualHours
                    WHERE Id = @Id";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", task.Id);
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@Description", task.Description ?? "");
                        command.Parameters.AddWithValue("@AssignedTo", task.AssignedTo ?? "");
                        command.Parameters.AddWithValue("@DueDate", task.DueDate);
                        command.Parameters.AddWithValue("@Priority", task.Priority);
                        command.Parameters.AddWithValue("@Status", task.Status);
                        command.Parameters.AddWithValue("@EstimatedHours", task.EstimatedHours);
                        command.Parameters.AddWithValue("@ActualHours", task.ActualHours);

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

        // Обновленный метод GetTasksByProject
        public List<Task> GetTasksByProject(int projectId)
        {
            var tasks = new List<Task>();

            _dbManager.ExecuteWithConnection(connection =>
            {
                string sql = "SELECT * FROM Tasks WHERE ProjectId = @ProjectId ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ProjectId", projectId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new Task
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                ProjectId = projectId,
                                AssignedTo = reader["AssignedTo"].ToString(),
                                DueDate = Convert.ToDateTime(reader["DueDate"]),
                                Priority = reader["Priority"].ToString(),
                                Status = reader["Status"].ToString(),
                                EstimatedHours = reader["EstimatedHours"] != DBNull.Value ?
                                                Convert.ToDecimal(reader["EstimatedHours"]) : 0,
                                ActualHours = reader["ActualHours"] != DBNull.Value ?
                                             Convert.ToDecimal(reader["ActualHours"]) : 0,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            });

            return tasks;
        }

        // Обновленный метод AddTask
        public bool AddTask(Task task)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = @"
                    INSERT INTO Tasks (Title, Description, ProjectId, AssignedTo, DueDate, Priority, Status, 
                                       EstimatedHours, ActualHours, CreatedAt) 
                    VALUES (@Title, @Description, @ProjectId, @AssignedTo, @DueDate, @Priority, @Status, 
                            @EstimatedHours, @ActualHours, datetime('now'))";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@Description", task.Description ?? "");
                        command.Parameters.AddWithValue("@ProjectId", task.ProjectId);
                        command.Parameters.AddWithValue("@AssignedTo", task.AssignedTo ?? "Не назначено");
                        command.Parameters.AddWithValue("@DueDate", task.DueDate);
                        command.Parameters.AddWithValue("@Priority", task.Priority ?? "Medium");
                        command.Parameters.AddWithValue("@Status", task.Status ?? "ToDo");
                        command.Parameters.AddWithValue("@EstimatedHours", task.EstimatedHours);
                        command.Parameters.AddWithValue("@ActualHours", task.ActualHours);

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

        // Остальные методы аналогично...
        public bool DeleteTask(int taskId)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = "DELETE FROM Tasks WHERE Id = @Id";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", taskId);
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

        public Task GetTaskById(int taskId)
        {
            Task task = null;

            _dbManager.ExecuteWithConnection(connection =>
            {
                string sql = "SELECT * FROM Tasks WHERE Id = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", taskId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            task = new Task
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                ProjectId = Convert.ToInt32(reader["ProjectId"]),
                                AssignedTo = reader["AssignedTo"].ToString(),
                                DueDate = Convert.ToDateTime(reader["DueDate"]),
                                Priority = reader["Priority"].ToString(),
                                Status = reader["Status"].ToString(),
                                EstimatedHours = reader["EstimatedHours"] != DBNull.Value ?
                                                Convert.ToDecimal(reader["EstimatedHours"]) : 0,
                                ActualHours = reader["ActualHours"] != DBNull.Value ?
                                             Convert.ToDecimal(reader["ActualHours"]) : 0,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            };
                        }
                    }
                }
            });

            return task;
        }
    }
}