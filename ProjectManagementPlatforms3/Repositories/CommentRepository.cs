using ProjectManagementPlatform.Data;
using ProjectManagementPlatform.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Xml.Linq;

namespace ProjectManagementPlatform.Repositories
{
    public class CommentRepository
    {
        private readonly DatabaseManager _dbManager;

        public CommentRepository()
        {
            _dbManager = new DatabaseManager();
            _dbManager.InitializeDatabase();
        }

        public bool AddComment(Comment comment)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = @"
                    INSERT INTO Comments (Content, AuthorId, TaskId, ProjectId, CreatedAt) 
                    VALUES (@Content, @AuthorId, @TaskId, @ProjectId, datetime('now'))";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Content", comment.Content ?? "");
                        command.Parameters.AddWithValue("@AuthorId", comment.AuthorId);
                        command.Parameters.AddWithValue("@TaskId", comment.TaskId);
                        command.Parameters.AddWithValue("@ProjectId", comment.ProjectId);

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

        public List<Comment> GetCommentsByTask(int taskId)
        {
            var comments = new List<Comment>();

            _dbManager.ExecuteWithConnection(connection =>
            {
                string sql = @"
                SELECT c.*, u.FullName as AuthorName 
                FROM Comments c 
                LEFT JOIN Users u ON c.AuthorId = u.Id 
                WHERE c.TaskId = @TaskId 
                ORDER BY c.CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comments.Add(new Comment
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Content = reader["Content"].ToString(),
                                AuthorId = Convert.ToInt32(reader["AuthorId"]),
                                AuthorName = reader["AuthorName"].ToString(),
                                TaskId = taskId,
                                ProjectId = reader["ProjectId"] != DBNull.Value ?
                                          Convert.ToInt32(reader["ProjectId"]) : (int?)null,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            });

            return comments;
        }

        public List<Comment> GetCommentsByProject(int projectId)
        {
            var comments = new List<Comment>();

            _dbManager.ExecuteWithConnection(connection =>
            {
                string sql = @"
                SELECT c.*, u.FullName as AuthorName 
                FROM Comments c 
                LEFT JOIN Users u ON c.AuthorId = u.Id 
                WHERE c.ProjectId = @ProjectId 
                ORDER BY c.CreatedAt DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ProjectId", projectId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comments.Add(new Comment
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Content = reader["Content"].ToString(),
                                AuthorId = Convert.ToInt32(reader["AuthorId"]),
                                AuthorName = reader["AuthorName"].ToString(),
                                TaskId = reader["TaskId"] != DBNull.Value ?
                                        Convert.ToInt32(reader["TaskId"]) : (int?)null,
                                ProjectId = projectId,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            });

            return comments;
        }

        public bool DeleteComment(int commentId)
        {
            try
            {
                _dbManager.ExecuteWithConnection(connection =>
                {
                    string sql = "DELETE FROM Comments WHERE Id = @Id";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", commentId);
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
    }
}