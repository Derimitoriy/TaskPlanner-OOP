using System;
using System.Collections.Generic;

namespace TaskPlanner.DAL.Entities
{
 
    [Serializable]
    public class TeamMember
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public DateTime JoinDate { get; set; }

        public virtual List<TaskAssignment> TaskAssignments { get; set; }

        public TeamMember()
        {
            Id = Guid.NewGuid();
            TaskAssignments = new List<TaskAssignment>();
            JoinDate = DateTime.Now;
        }

        public TeamMember(string firstName, string lastName, string email, string position) : this()
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Position = position;
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }

        public override string ToString()
        {
            return $"{GetFullName()} ({Position}) - {Email}";
        }
    }
}