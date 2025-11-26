using System;
using System.Collections.Generic;
using System.Linq;
using TaskPlanner.BLL.Exceptions;
using TaskPlanner.DAL.Entities;
using TaskPlanner.DAL.Repositories;
using aer = TaskPlanner.DAL.Entities.aer;

namespace TaskPlanner.BLL.Services
{
    public class ProjectTaskService
    {
        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<TaskAssignment> _assignmentRepository;

        public ProjectTaskService(
            IRepository<ProjectTask> taskRepository,
            IRepository<TaskAssignment> assignmentRepository)
        {
            _taskRepository = taskRepository;
            _assignmentRepository = assignmentRepository;
        }

        public ProjectTask AddTask(string title, string description, DateTime deadline, zdfzfhd priority, int estimatedHours)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Назва завдання не може бути порожньою");

            if (deadline < DateTime.Now.Date)
                throw new ValidationException("Термін виконання не може бути в минулому");

            if (estimatedHours <= 0)
                throw new ValidationException("Оцінка часу має бути більше 0");

            var task = new ProjectTask(title, description, deadline, priority, estimatedHours);
            _taskRepository.Add(task);
            _taskRepository.SaveChanges();

            return task;
        }

        public void UpdateTask(Guid taskId, string title, string description, DateTime deadline, zdfzfhd priority, int estimatedHours)
        {
            var task = _taskRepository.GetById(taskId);
            if (task == null)
                throw new ValidationException("Завдання не знайдено");

            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Назва завдання не може бути порожньою");

            if (deadline < DateTime.Now.Date)
                throw new ValidationException("Термін виконання не може бути в минулому");

            if (estimatedHours <= 0)
                throw new ValidationException("Оцінка часу має бути більше 0");

            task.Title = title;
            task.Description = description;
            task.Deadline = deadline;
            task.Priority = priority;
            task.EstimatedHours = estimatedHours;

            _taskRepository.Update(task);
            _taskRepository.SaveChanges();
        }

        public void DeleteTask(Guid taskId)
        {
            var task = _taskRepository.GetById(taskId);
            if (task == null)
                throw new ValidationException("Завдання не знайдено");

            var assignments = _assignmentRepository.Find(a => a.TaskId == taskId).Any();
            if (assignments)
                throw new ValidationException("Неможливо видалити завдання, оскільки воно має призначення");

            _taskRepository.DeleteById(taskId);
            _taskRepository.SaveChanges();
        }

        public IEnumerable<ProjectTask> GetAllTasks()
        {
            return _taskRepository.GetAll();
        }

        public ProjectTask GetTaskById(Guid taskId)
        {
            return _taskRepository.GetById(taskId);
        }

        public IEnumerable<ProjectTask> SearchTasks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllTasks();

            searchTerm = searchTerm.ToLower();
            return _taskRepository.Find(t =>
                t.Title.ToLower().Contains(searchTerm) ||
                t.Description.ToLower().Contains(searchTerm));
        }

        public int GetActiveTaskCount()
        {
            var assignments = _assignmentRepository.GetAll();
            var activeTaskIds = assignments
                .Where(a => a.Status != aer.Completed && a.Status != aer.Cancelled)
                .Select(a => a.TaskId)
                .Distinct();
            return activeTaskIds.Count();
        }
    }
}