using System;

namespace TaskPlanner.BLL.Exceptions
{
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException(string message) : base(message) { }
        public BusinessLogicException(string message, Exception innerException)
            : base(message, innerException) { }
    }
    public class MemberOverloadedException : BusinessLogicException
    {
        public int CurrentTaskCount { get; }
        public int MaxAllowedTasks { get; }

        public MemberOverloadedException(string memberName, int currentCount, int maxAllowed)
            : base($"Виконавець {memberName} перевантажений. " +
                   $"Поточна кількість завдань: {currentCount}, максимально дозволено: {maxAllowed}")
        {
            CurrentTaskCount = currentCount;
            MaxAllowedTasks = maxAllowed;
        }
    }

    public class OverdueTaskAssignmentException : BusinessLogicException
    {
        public DateTime Deadline { get; }

        public OverdueTaskAssignmentException(string taskTitle, DateTime deadline)
            : base($"Неможливо призначити завдання '{taskTitle}'. " +
                   $"Термін виконання ({deadline:dd.MM.yyyy}) вже минув")
        {
            Deadline = deadline;
        }
    }
    public class MemberHasActiveTasksException : BusinessLogicException
    {
        public int ActiveTasksCount { get; }

        public MemberHasActiveTasksException(string memberName, int activeTasksCount)
            : base($"Неможливо видалити виконавця {memberName}. " +
                   $"У нього є {activeTasksCount} активних завдань")
        {
            ActiveTasksCount = activeTasksCount;
        }
    }

    public class DuplicateEmailException : BusinessLogicException
    {
        public string Email { get; }

        public DuplicateEmailException(string email)
            : base($"Виконавець з email '{email}' вже існує в системі")
        {
            Email = email;
        }
    }

    public class DuplicateAssignmentException : BusinessLogicException
    {
        public DuplicateAssignmentException(string taskTitle, string memberName)
            : base($"Завдання '{taskTitle}' вже призначено виконавцю {memberName}")
        {
        }
    }

    public class ValidationException : BusinessLogicException
    {
        public ValidationException(string message) : base(message) { }
    }
}