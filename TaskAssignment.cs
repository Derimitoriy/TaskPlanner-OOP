using System;

namespace TaskPlanner.DAL.Entities
{

    [Serializable]
    public class TaskAssignment
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid MemberId { get; set; }
        public DateTime AssignedDate { get; set; }
        public aer Status { get; set; }
        public int CompletionPercentage { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; }

        public virtual ProjectTask Task { get; set; }
        public virtual TeamMember Member { get; set; }

        public TaskAssignment()
        {
            Id = Guid.NewGuid();
            AssignedDate = DateTime.Now;
            Status = aer.NotStarted;
            CompletionPercentage = 0;
        }

        public TaskAssignment(Guid taskId, Guid memberId) : this()
        {
            TaskId = taskId;
            MemberId = memberId;
        }

        public void UpdateProgress(int percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage),
                    "Відсоток виконання має бути від 0 до 100");

            CompletionPercentage = percentage;

            if (percentage == 0)
                Status = aer.NotStarted;
            else if (percentage < 100)
                Status = aer.InProgress;
            else
            {
                Status = aer.Completed;
                CompletedDate = DateTime.Now;
            }
        }

        public bool IsCompleted()
        {
            return Status == aer.Completed;
        }

        public bool IsOverdue()
        {
            return Task != null && !IsCompleted() && Task.IsOverdue();
        }

        public override string ToString()
        {
            return $"Завдання: {Task?.Title ?? "N/A"}, Виконавець: {Member?.GetFullName() ?? "N/A"}, " +
                   $"Статус: {Status}, Прогрес: {CompletionPercentage}%";
        }
    }

    [Serializable]
    public enum aer
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        OnHold = 3,
        Cancelled = 4
    }
}