using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TaskPlanner.BLL.Exceptions;
using TaskPlanner.DAL.Entities;
using TaskPlanner.DAL.Repositories;
using aer = TaskPlanner.DAL.Entities.aer;

namespace TaskPlanner.BLL.Services
{
    public class TeamMemberService
    {
        private readonly IRepository<TeamMember> _memberRepository;
        private readonly IRepository<TaskAssignment> _assignmentRepository;

        public TeamMemberService(
            IRepository<TeamMember> memberRepository,
            IRepository<TaskAssignment> assignmentRepository)
        {
            _memberRepository = memberRepository;
            _assignmentRepository = assignmentRepository;
        }

        public TeamMember AddMember(string firstName, string lastName, string email, string position)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ValidationException("Ім'я не може бути порожнім");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ValidationException("Прізвище не може бути порожнім");

            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("Email не може бути порожнім");

            if (!IsValidEmail(email))
                throw new ValidationException("Некоректний формат email");

            if (string.IsNullOrWhiteSpace(position))
                throw new ValidationException("Посада не може бути порожньою");

            if (_memberRepository.Find(m => m.Email.ToLower() == email.ToLower()).Any())
                throw new DuplicateEmailException(email);

            var member = new TeamMember(firstName, lastName, email, position);
            _memberRepository.Add(member);
            _memberRepository.SaveChanges();

            return member;
        }

        public void UpdateMember(Guid memberId, string firstName, string lastName, string email, string position)
        {
            var member = _memberRepository.GetById(memberId);
            if (member == null)
                throw new ValidationException("Члена команди не знайдено");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new ValidationException("Ім'я не може бути порожнім");

            if (!IsValidEmail(email))
                throw new ValidationException("Некоректний формат email");

            if (_memberRepository.Find(m => m.Email.ToLower() == email.ToLower() && m.Id != memberId).Any())
                throw new DuplicateEmailException(email);

            member.FirstName = firstName;
            member.LastName = lastName;
            member.Email = email;
            member.Position = position;

            _memberRepository.Update(member);
            _memberRepository.SaveChanges();
        }

        public void DeleteMember(Guid memberId)
        {
            var member = _memberRepository.GetById(memberId);
            if (member == null)
                throw new ValidationException("Члена команди не знайдено");

            var activeAssignments = _assignmentRepository
                .Find(a => a.MemberId == memberId &&
                          a.Status != aer.Completed &&
                          a.Status != aer.Cancelled)
                .Count();

            if (activeAssignments > 0)
                throw new MemberHasActiveTasksException(member.GetFullName(), activeAssignments);

            _memberRepository.DeleteById(memberId);
            _memberRepository.SaveChanges();
        }

        public IEnumerable<TeamMember> GetAllMembers()
        {
            return _memberRepository.GetAll();
        }

        public TeamMember GetMemberById(Guid memberId)
        {
            return _memberRepository.GetById(memberId);
        }

        public IEnumerable<TeamMember> SearchMembers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllMembers();

            searchTerm = searchTerm.ToLower();
            return _memberRepository.Find(m =>
                m.FirstName.ToLower().Contains(searchTerm) ||
                m.LastName.ToLower().Contains(searchTerm) ||
                m.Email.ToLower().Contains(searchTerm));
        }

        public int GetMemberWorkload(Guid memberId)
        {
            return _assignmentRepository
                .Find(a => a.MemberId == memberId &&
                          a.Status != aer.Completed &&
                          a.Status != aer.Cancelled)
                .Count();
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
    }
}