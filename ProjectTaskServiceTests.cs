using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TaskPlanner.BLL.Services;
using TaskPlanner.DAL.Entities;
using TaskPlanner.DAL.Repositories;
using TaskPlanner.BLL.Exceptions;

namespace TaskPlanner.Tests
{
    [TestClass]
    public class ProjectTaskServiceTests
    {
        private ProjectTaskService _service;
        private IRepository<ProjectTask> _taskRepository;
        private IRepository<TaskAssignment> _assignmentRepository;

        [TestInitialize]
        public void Setup()
        {
            _taskRepository = new JsonRepository<ProjectTask>("test_tasks.json");
            _assignmentRepository = new JsonRepository<TaskAssignment>("test_assignments.json");
            _service = new ProjectTaskService(_taskRepository, _assignmentRepository);

            foreach (var task in _taskRepository.GetAll().ToList())
            {
                _taskRepository.Delete(task);
            }
            _taskRepository.SaveChanges();
        }

        [TestMethod]
        public void AddTask_ValidData_ShouldAddSuccessfully()
        {
            string title = "Нове завдання";
            string description = "Опис завдання";
            DateTime deadline = DateTime.Now.AddDays(7);
            zdfzfhd priority = zdfzfhd.High;
            int estimatedHours = 10;

            var task = _service.AddTask(title, description, deadline, priority, estimatedHours);

            Assert.IsNotNull(task);
            Assert.AreEqual(title, task.Title);
            Assert.AreEqual(description, task.Description);
            Assert.AreEqual(deadline.Date, task.Deadline.Date);
            Assert.AreEqual(priority, task.Priority);
            Assert.AreEqual(estimatedHours, task.EstimatedHours);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void AddTask_EmptyTitle_ShouldThrowException()
        {
            string title = "";
            string description = "Опис завдання";
            DateTime deadline = DateTime.Now.AddDays(7);
            zdfzfhd priority = zdfzfhd.High;
            int estimatedHours = 10;

            _service.AddTask(title, description, deadline, priority, estimatedHours);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void AddTask_PastDeadline_ShouldThrowException()
        {
            string title = "Завдання";
            string description = "Опис";
            DateTime deadline = DateTime.Now.AddDays(-1); 
            zdfzfhd priority = zdfzfhd.Medium;
            int estimatedHours = 5;

            _service.AddTask(title, description, deadline, priority, estimatedHours);
        }

        [TestMethod]
        public void GetAllTasks_AfterAddingTwo_ShouldReturnTwo()
        {
            _service.AddTask("Завдання 1", "Опис 1", DateTime.Now.AddDays(5), zdfzfhd.Medium, 8);
            _service.AddTask("Завдання 2", "Опис 2", DateTime.Now.AddDays(10), zdfzfhd.High, 12);

            var tasks = _service.GetAllTasks().ToList();

            Assert.AreEqual(2, tasks.Count);
        }

        [TestMethod]
        public void SearchTasks_ByTitle_ShouldReturnMatching()
        {
            _service.AddTask("Розробити API", "Розробити REST API", DateTime.Now.AddDays(7), zdfzfhd.High, 16);
            _service.AddTask("Тестування", "Протестувати функціонал", DateTime.Now.AddDays(5), zdfzfhd.Medium, 8);
            _service.AddTask("API документація", "Написати документацію", DateTime.Now.AddDays(3), zdfzfhd.Low, 4);

            
            var results = _service.SearchTasks("API").ToList();

            Assert.AreEqual(2, results.Count);
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var task in _taskRepository.GetAll().ToList())
            {
                _taskRepository.Delete(task);
            }
            _taskRepository.SaveChanges();
        }
    }
}