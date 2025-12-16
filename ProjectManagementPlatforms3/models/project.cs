using System;

namespace ProjectManagementPlatform.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Deadline { get; set; }
        public decimal Budget { get; set; }
        public string Status { get; set; }
        public int ManagerId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DisplayStatus
        {
            get
            {
                switch (Status)
                {
                    case "Planning":
                        return "Планирование";
                    case "InProgress":
                        return "В работе";
                    case "Completed":
                        return "Завершен";
                    case "OnHold":
                        return "Приостановлен";
                    default:
                        return Status;
                }
            }
        }
    }
}