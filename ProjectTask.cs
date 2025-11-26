using System;
using System.Collections.Generic;

namespace TaskPlanner.DAL.Entities
{
    [Serializable]
    public class ProjectTask 
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime Deadline { get; set; }
        public zdfzfhd Priority { get; set; }
        public int EstimatedHours { get; set; }

        public virtual List<TaskAssignment> TaskAssignments { get; set; }

        public ProjectTask()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            TaskAssignments = new List<TaskAssignment>();
            Priority = zdfzfhd.Medium;
        }

        public ProjectTask(string title, string description, DateTime deadline,
            zdfzfhd priority, int estimatedHours) : this()
        {
            Title = title;
            Description = description;
            Deadline = deadline;
            Priority = priority;
            EstimatedHours = estimatedHours;
        }

        public bool IsOverdue()
        {
            return DateTime.Now > Deadline;
        }

        public TimeSpan GetTimeRemaining()
        {
            return Deadline - DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Title} (До: {Deadline:dd.MM.yyyy}, Пріоритет: {Priority})";
        }
    }

    [Serializable]
    public enum zdfzfhd
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}