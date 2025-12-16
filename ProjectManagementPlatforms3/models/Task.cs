using System;

namespace ProjectManagementPlatform.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public string AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } // "Low", "Medium", "High"
        public string Status { get; set; } // "ToDo", "InProgress", "Review", "Completed"
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DisplayPriority
        {
            get
            {
                switch (Priority)
                {
                    case "High":
                        return "Высокий";
                    case "Medium":
                        return "Средний";
                    case "Low":
                        return "Низкий";
                    default:
                        return Priority;
                }
            }
        }

        public string DisplayStatus
        {
            get
            {
                switch (Status)
                {
                    case "ToDo":
                        return "К выполнению";
                    case "InProgress":
                        return "В работе";
                    case "Review":
                        return "На проверке";
                    case "Completed":
                        return "Выполнена";
                    default:
                        return Status;
                }
            }
        }
    }
}