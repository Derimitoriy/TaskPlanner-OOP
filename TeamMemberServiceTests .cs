using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TaskPlanner.BLL.Exceptions;
using TaskPlanner.BLL.Services;
using TaskPlanner.DAL.Entities;
using TaskPlanner.DAL.Repositories;
using TaskStatus = TaskPlanner.DAL.Entities.aer;
using TaskPlanner.BLL.Exceptions;

namespace TaskPlanner.Tests
{
    [TestClass]
    public class TeamMemberServiceTests
    {
        private TeamMemberService _service;
        private IRepository<TeamMember> _memberRepository;
        private IRepository<TaskAssignment> _assignmentRepository;

        [TestInitialize]
        public void Setup()
        {
            _memberRepository = new JsonRepository<TeamMember>("test_members.json");
            _assignmentRepository = new JsonRepository<TaskAssignment>("test_assignments.json");
            _service = new TeamMemberService(_memberRepository, _assignmentRepository);

            foreach (var member in _memberRepository.GetAll().ToList())
            {
                _memberRepository.Delete(member);
            }
            _memberRepository.SaveChanges();
        }

        [TestMethod]
        public void AddMember_ValidData_ShouldAddSuccessfully()
        {
            string firstName = "Іван";
            string lastName = "Петренко";
            string email = "ivan.petrenko@example.com";
            string position = "Розробник";

            var member = _service.AddMember(firstName, lastName, email, position);

            Assert.IsNotNull(member);
            Assert.AreEqual(firstName, member.FirstName);
            Assert.AreEqual(lastName, member.LastName);
            Assert.AreEqual(email, member.Email);
            Assert.AreEqual(position, member.Position);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void AddMember_EmptyFirstName_ShouldThrowException()
        {
            string firstName = "";
            string lastName = "Петренко";
            string email = "test@example.com";
            string position = "Розробник";

            
            _service.AddMember(firstName, lastName, email, position);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void AddMember_InvalidEmail_ShouldThrowException()
        {
            string firstName = "Іван";
            string lastName = "Петренко";
            string email = "invalid-email";
            string position = "Розробник";

            _service.AddMember(firstName, lastName, email, position);
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateEmailException))]
        public void AddMember_DuplicateEmail_ShouldThrowException()
        {
            string email = "test@example.com";
            _service.AddMember("Іван", "Петренко", email, "Розробник");

            _service.AddMember("Марія", "Коваленко", email, "Тестувальник");
        }

        [TestMethod]
        public void GetAllMembers_AfterAddingThree_ShouldReturnThree()
        {
            _service.AddMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            _service.AddMember("Марія", "Коваленко", "maria@example.com", "Тестувальник");
            _service.AddMember("Олег", "Сидоренко", "oleg@example.com", "Аналітик");

            var members = _service.GetAllMembers().ToList();

            Assert.AreEqual(3, members.Count);
        }

        [TestMethod]
        public void UpdateMember_ValidData_ShouldUpdate()
        {
            var member = _service.AddMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            string newEmail = "ivan.new@example.com";
            string newPosition = "Senior Розробник";

            _service.UpdateMember(member.Id, "Іван", "Петренко", newEmail, newPosition);
            var updated = _service.GetMemberById(member.Id);

            Assert.IsNotNull(updated);
            Assert.AreEqual(newEmail, updated.Email);
            Assert.AreEqual(newPosition, updated.Position);
        }

        [TestMethod]
        public void SearchMembers_ByName_ShouldReturnMatching()
        {
            _service.AddMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            _service.AddMember("Марія", "Коваленко", "maria@example.com", "Тестувальник");
            _service.AddMember("Іванна", "Сидоренко", "ivanna@example.com", "Аналітик");

            var results = _service.SearchMembers("Іван").ToList();

            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void GetMemberWorkload_NoTasks_ShouldReturnZero()
        {
            var member = _service.AddMember("Іван", "Петренко", "ivan@example.com", "Розробник");

            int workload = _service.GetMemberWorkload(member.Id);

            Assert.AreEqual(0, workload);
        }

        [TestMethod]
        public void DeleteMember_NoTasks_ShouldDelete()
        {
            var member = _service.AddMember("Іван", "Петренко", "ivan@example.com", "Розробник");
            var id = member.Id;

            _service.DeleteMember(id);
            var deleted = _service.GetMemberById(id);

            Assert.IsNull(deleted);
        }

        [TestMethod]
        public void DeleteMember_WithActiveTasks_ShouldThrowException()
        {
            var member = _service.AddMember("Іван", "Петренко", "ivan@example.com", "Розробник");

            var taskRepo = new JsonRepository<ProjectTask>("test_tasks.json");
            var task = new ProjectTask("Тестове завдання", "Опис", DateTime.Now.AddDays(7), zdfzfhd.Medium, 8);
            taskRepo.Add(task);
            taskRepo.SaveChanges();

            var assignment = new TaskAssignment(task.Id, member.Id);
            _assignmentRepository.Add(assignment);
            _assignmentRepository.SaveChanges();

            Assert.ThrowsException<MemberHasActiveTasksException>(() =>
                _service.DeleteMember(member.Id));
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var member in _memberRepository.GetAll().ToList())
            {
                _memberRepository.Delete(member);
            }
            _memberRepository.SaveChanges();

            foreach (var assignment in _assignmentRepository.GetAll().ToList())
            {
                _assignmentRepository.Delete(assignment);
            }
            _assignmentRepository.SaveChanges();
        }
    }
}