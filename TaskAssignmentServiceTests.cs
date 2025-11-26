using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TaskPlanner.BLL.Exceptions;
using TaskPlanner.BLL.Services;
using TaskPlanner.DAL.Entities;
using TaskPlanner.DAL.Repositories;
using aer = TaskPlanner.DAL.Entities.aer;

namespace TaskPlanner.Tests
{
    [TestClass]
    public class TaskAssignmentServiceTests
    {
        private TaskAssignmentService _service;
        private IRepository<TaskAssignment> _assignmentRepository;
        private IRepository<ProjectTask> _taskRepository;
        private IRepository<TeamMember> _memberRepository;

        [TestInitialize]
        public void Setup()
        {
            _assignmentRepository = new JsonRepository<TaskAssignment>("test_assignments.json");
            _taskRepository = new JsonRepository<ProjectTask>("test_tasks.json");
            _memberRepository = new JsonRepository<TeamMember>("test_members.json");
            _service = new TaskAssignmentService(_assignmentRepository, _taskRepository, _memberRepository);

            foreach (var assignment in _assignmentRepository.GetAll().ToList())
            {
                _assignmentRepository.Delete(assignment);
            }
            _assignmentRepository.SaveChanges();

            foreach (var task in _taskRepository.GetAll().ToList())
            {
                _taskRepository.Delete(task);
            }
            _taskRepository.SaveChanges();

            foreach (var member in _memberRepository.GetAll().ToList())
            {
                _memberRepository.Delete(member);
            }
            _memberRepository.SaveChanges();
        }

        [TestMethod]
        public void AssignTask_ValidData_ShouldAssignSuccessfully()
        {
            var task = new ProjectTask("Завдання", "Опис", DateTime.Now.AddDays(7), zdfzfhd.Medium, 8);
            _taskRepository.Add(task);
            _taskRepository.SaveChanges();

            var member = new TeamMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            _memberRepository.Add(member);
            _memberRepository.SaveChanges();

            var assignment = _service.AssignTask(task.Id, member.Id);
            Assert.IsNotNull(assignment);
            Assert.AreEqual(task.Id, assignment.TaskId);
            Assert.AreEqual(member.Id, assignment.MemberId);
            Assert.AreEqual(aer.NotStarted, assignment.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void AssignTask_InvalidTask_ShouldThrowException()
        {
            var member = new TeamMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            _memberRepository.Add(member);
            _memberRepository.SaveChanges();

            _service.AssignTask(Guid.NewGuid(), member.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateAssignmentException))]
        public void AssignTask_DuplicateAssignment_ShouldThrowException()
        {
            var task = new ProjectTask("Завдання", "Опис", DateTime.Now.AddDays(7), zdfzfhd.Medium, 8);
            _taskRepository.Add(task);
            _taskRepository.SaveChanges();

            var member = new TeamMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            _memberRepository.Add(member);
            _memberRepository.SaveChanges();

            _service.AssignTask(task.Id, member.Id);

            _service.AssignTask(task.Id, member.Id);
        }

        [TestMethod]
        public void UpdateProgress_ValidPercentage_ShouldUpdateStatus()
        {
            var task = new ProjectTask("Завдання", "Опис", DateTime.Now.AddDays(7), zdfzfhd.Medium, 8);
            _taskRepository.Add(task);
            _taskRepository.SaveChanges();

            var member = new TeamMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            _memberRepository.Add(member);
            _memberRepository.SaveChanges();

            var assignment = _service.AssignTask(task.Id, member.Id);

            _service.UpdateProgress(assignment.Id, 50);

            var updated = _service.GetAssignmentById(assignment.Id);
            Assert.AreEqual(50, updated.CompletionPercentage);
            Assert.AreEqual(aer.InProgress, updated.Status);
        }

        [TestMethod]
        public void GetProjectStatistics_NoData_ShouldReturnZero()
        {
            var stats = _service.GetProjectStatistics();

            Assert.AreEqual(0, stats.TotalTasks);
            Assert.AreEqual(0, stats.TotalMembers);
            Assert.AreEqual(0, stats.CompletedTasks);
            Assert.AreEqual(0.0, stats.CompletionRate);
        }

        [TestMethod]
        public void GetProjectStatistics_WithData_ShouldCalculateCorrectly()
        {
            var task1 = new ProjectTask("Завдання 1", "Опис", DateTime.Now.AddDays(7), zdfzfhd.Medium, 8);
            var task2 = new ProjectTask("Завдання 2", "Опис", DateTime.Now.AddDays(5), zdfzfhd.High, 12);
            _taskRepository.Add(task1);
            _taskRepository.Add(task2);
            _taskRepository.SaveChanges();

            var member = new TeamMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            _memberRepository.Add(member);
            _memberRepository.SaveChanges();

            var assignment = _service.AssignTask(task1.Id, member.Id);
            _service.UpdateProgress(assignment.Id, 100); 

            var stats = _service.GetProjectStatistics();

            Assert.AreEqual(2, stats.TotalTasks);
            Assert.AreEqual(1, stats.TotalMembers);
            Assert.AreEqual(1, stats.CompletedTasks);
            Assert.AreEqual(50.0, stats.CompletionRate);
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var assignment in _assignmentRepository.GetAll().ToList())
            {
                _assignmentRepository.Delete(assignment);
            }
            _assignmentRepository.SaveChanges();

            foreach (var task in _taskRepository.GetAll().ToList())
            {
                _taskRepository.Delete(task);
            }
            _taskRepository.SaveChanges();

            foreach (var member in _memberRepository.GetAll().ToList())
            {
                _memberRepository.Delete(member);
            }
            _memberRepository.SaveChanges();
        }
    }
}