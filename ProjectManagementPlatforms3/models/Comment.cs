using System;

namespace ProjectManagementPlatform.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? TaskId { get; set; }
        public int? ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CreatedAtDisplay => CreatedAt.ToString("dd.MM.yyyy HH:mm");
        public bool CanDelete { get; set; } // Для проверки прав на удаление
    }
}