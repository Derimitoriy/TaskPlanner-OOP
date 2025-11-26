using System;
using System.Collections.Generic;
using System.Linq;
using TaskPlanner.BLL.Exceptions;
using TaskPlanner.DAL.Entities;
using TaskPlanner.DAL.Repositories;
using aer = TaskPlanner.DAL.Entities.aer;

namespace TaskPlanner.BLL.Services
{
    public class TaskAssignmentService
    {
        private readonly IRepository<TaskAssignment> _assignmentRepository;
        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<TeamMember> _memberRepository;

        public TaskAssignmentService(
            IRepository<TaskAssignment> assignmentRepository,
            IRepository<ProjectTask> taskRepository,
            IRepository<TeamMember> memberRepository)
        {
            _assignmentRepository = assignmentRepository;
            _taskRepository = taskRepository;
            _memberRepository = memberRepository;
        }

        public TaskAssignment AssignTask(Guid taskId, Guid memberId)
        {
            var task = _taskRepository.GetById(taskId);
            if (task == null)
                throw new ValidationException("Завдання не знайдено");

            var member = _memberRepository.GetById(memberId);
            if (member == null)
                throw new ValidationException("Виконавця не знайдено");

            var existingAssignment = _assignmentRepository
                .Find(a => a.TaskId == taskId && a.MemberId == memberId)
                .FirstOrDefault();
            if (existingAssignment != null)
                throw new DuplicateAssignmentException(task.Title, member.GetFullName());

            if (task.IsOverdue())
                throw new OverdueTaskAssignmentException(task.Title, task.Deadline);

            var memberAssignments = _assignmentRepository
                .Find(a => a.MemberId == memberId &&
                          a.Status != aer.Completed &&
                          a.Status != aer.Cancelled)
                .Count();
            const int maxTasks = 5;
            if (memberAssignments >= maxTasks)
                throw new MemberOverloadedException(member.GetFullName(), memberAssignments, maxTasks);

            var assignment = new TaskAssignment(taskId, memberId);
            _assignmentRepository.Add(assignment);
            _assignmentRepository.SaveChanges();

            return assignment;
        }

        public void UpdateProgress(Guid assignmentId, int percentage)
        {
            var assignment = _assignmentRepository.GetById(assignmentId);
            if (assignment == null)
                throw new ValidationException("Призначення не знайдено");

            assignment.UpdateProgress(percentage);
            _assignmentRepository.Update(assignment);
            _assignmentRepository.SaveChanges();
        }

        public void UnassignTask(Guid assignmentId)
        {
            var assignment = _assignmentRepository.GetById(assignmentId);
            if (assignment == null)
                throw new ValidationException("Призначення не знайдено");

            _assignmentRepository.Delete(assignment);
            _assignmentRepository.SaveChanges();
        }

        public IEnumerable<TaskAssignment> GetAllAssignments()
        {
            return _assignmentRepository.GetAll();
        }

        public TaskAssignment GetAssignmentById(Guid assignmentId)
        {
            return _assignmentRepository.GetById(assignmentId);
        }

        public IEnumerable<TaskAssignment> GetMemberAssignments(Guid memberId)
        {
            return _assignmentRepository.Find(a => a.MemberId == memberId);
        }

        public IEnumerable<TaskAssignment> GetTaskAssignments(Guid taskId)
        {
            return _assignmentRepository.Find(a => a.TaskId == taskId);
        }

        public ProjectStatistics GetProjectStatistics()
        {
            var tasks = _taskRepository.GetAll().Count();
            var members = _memberRepository.GetAll().Count();
            var assignments = _assignmentRepository.GetAll().ToList();

            var completedTasks = assignments.Count(a => a.Status == aer.Completed);
            var inProgressTasks = assignments.Count(a => a.Status == aer.InProgress);
            var overdueTasks = assignments.Count(a => a.IsOverdue());

            double completionRate = tasks > 0 ? (double)completedTasks / tasks * 100 : 0;

            return new ProjectStatistics
            {
                TotalTasks = tasks,
                TotalMembers = members,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                OverdueTasks = overdueTasks,
                CompletionRate = completionRate
            };
        }
    }

    public class ProjectStatistics
    {
        public int TotalTasks { get; set; }
        public int TotalMembers { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionRate { get; set; }
    }
}