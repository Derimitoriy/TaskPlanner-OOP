using System;
using System.Linq;
using TaskPlanner.BLL.Exceptions;
using TaskPlanner.BLL.Services;
using TaskPlanner.DAL.Entities;
using TaskPlanner.DAL.Repositories;
using TaskStatus = TaskPlanner.DAL.Entities.aer;


namespace TaskPlanner.PL
{
    class Program
    {
        private static TeamMemberService _memberService;
        private static ProjectTaskService _taskService;
        private static TaskAssignmentService _assignmentService;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            InitializeServices();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║      ПЛАНУВАЛЬНИК ЗАВДАНЬ - TASK PLANNER           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Console.WriteLine();

            bool running = true;
            while (running)
            {
                try
                {
                    DisplayMainMenu();
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            TeamMemberMenu();
                            break;
                        case "2":
                            ProjectTaskMenu();
                            break;
                        case "3":
                            AssignmentMenu();
                            break;
                        case "4":
                            SearchMenu();
                            break;
                        case "5":
                            StatisticsMenu();
                            break;
                        case "0":
                            running = false;
                            Console.WriteLine("\nДо побачення!");
                            break;
                        default:
                            Console.WriteLine("\n❌ Невірний вибір. Спробуйте ще раз.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Помилка: {ex.Message}");
                }

                if (running)
                {
                    Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
                    Console.ReadKey();
                }
            }
        }

        private static void InitializeServices()
        {
            var memberRepo = new JsonRepository<TeamMember>("members.json");
            var taskRepo = new JsonRepository<ProjectTask>("tasks.json");
            var assignmentRepo = new JsonRepository<TaskAssignment>("assignments.json");

            _memberService = new TeamMemberService(memberRepo, assignmentRepo);
            _taskService = new ProjectTaskService(taskRepo, assignmentRepo);
            _assignmentService = new TaskAssignmentService(assignmentRepo, taskRepo, memberRepo);
        }

        private static void DisplayMainMenu()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ГОЛОВНЕ МЕНЮ                          ║");
            Console.WriteLine("╠════════════════════════════════════════════════════╣");
            Console.WriteLine("║  1. Управління членами команди                     ║");
            Console.WriteLine("║  2. Управління завданнями                          ║");
            Console.WriteLine("║  3. Призначення та виконання завдань               ║");
            Console.WriteLine("║  4. Пошук                                          ║");
            Console.WriteLine("║  5. Статистика проекту                             ║");
            Console.WriteLine("║  0. Вихід                                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Console.Write("\nВаш вибір: ");
        }

        #region Team Member Menu

        private static void TeamMemberMenu()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════╗");
                Console.WriteLine("║         УПРАВЛІННЯ ЧЛЕНАМИ КОМАНДИ                 ║");
                Console.WriteLine("╠════════════════════════════════════════════════════╣");
                Console.WriteLine("║  1. Додати виконавця                               ║");
                Console.WriteLine("║  2. Переглянути всіх виконавців                    ║");
                Console.WriteLine("║  3. Видалити виконавця                             ║");
                Console.WriteLine("║  0. Назад                                          ║");
                Console.WriteLine("╚════════════════════════════════════════════════════╝");
                Console.Write("\nВаш вибір: ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": AddTeamMember(); break;
                        case "2": ViewAllMembers(); break;
                        case "3": DeleteTeamMember(); break;
                        case "0": back = true; break;
                        default: Console.WriteLine("\n❌ Невірний вибір."); break;
                    }
                }
                catch (BusinessLogicException ex)
                {
                    Console.WriteLine($"\n❌ {ex.Message}");
                }

                if (!back)
                {
                    Console.WriteLine("\nНатисніть будь-яку клавішу...");
                    Console.ReadKey();
                }
            }
        }

        private static void AddTeamMember()
        {
            Console.Clear();
            Console.WriteLine("═══ ДОДАВАННЯ НОВОГО ВИКОНАВЦЯ ═══\n");

            Console.Write("Ім'я: ");
            string firstName = Console.ReadLine();

            Console.Write("Прізвище: ");
            string lastName = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Посада: ");
            string position = Console.ReadLine();

            var member = _memberService.AddMember(firstName, lastName, email, position);
            Console.WriteLine($"\n✓ Виконавця {member.GetFullName()} успішно додано!");
        }

        private static void ViewAllMembers()
        {
            Console.Clear();
            Console.WriteLine("═══ СПИСОК ВСІХ ВИКОНАВЦІВ ═══\n");

            var members = _memberService.GetAllMembers();
            int count = 0;

            foreach (var member in members)
            {
                count++;
                Console.WriteLine($"{count}. {member}");
                Console.WriteLine($"   ID: {member.Id}");
                int workload = _memberService.GetMemberWorkload(member.Id);
                Console.WriteLine($"   Активних завдань: {workload}");
                Console.WriteLine();
            }

            if (count == 0)
                Console.WriteLine("Список порожній.");
        }

        private static void DeleteTeamMember()
        {
            Console.Clear();
            Console.WriteLine("═══ ВИДАЛЕННЯ ВИКОНАВЦЯ ═══\n");

            Console.Write("Введіть ID виконавця: ");
            if (!Guid.TryParse(Console.ReadLine(), out Guid memberId))
            {
                Console.WriteLine("❌ Некоректний ID");
                return;
            }

            var member = _memberService.GetMemberById(memberId);
            if (member == null)
            {
                Console.WriteLine("❌ Виконавця не знайдено");
                return;
            }

            Console.WriteLine($"\nВи дійсно хочете видалити {member}? (так/ні): ");
            string confirm = Console.ReadLine()?.ToLower();

            if (confirm == "так")
            {
                _memberService.DeleteMember(memberId);
                Console.WriteLine("\n✓ Виконавця успішно видалено!");
            }
        }

        #endregion

        #region Project Task Menu

        private static void ProjectTaskMenu()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════╗");
                Console.WriteLine("║           УПРАВЛІННЯ ЗАВДАННЯМИ                    ║");
                Console.WriteLine("╠════════════════════════════════════════════════════╣");
                Console.WriteLine("║  1. Додати завдання                                ║");
                Console.WriteLine("║  2. Переглянути всі завдання                       ║");
                Console.WriteLine("║  0. Назад                                          ║");
                Console.WriteLine("╚════════════════════════════════════════════════════╝");
                Console.Write("\nВаш вибір: ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": AddTask(); break;
                        case "2": ViewAllTasks(); break;
                        case "0": back = true; break;
                        default: Console.WriteLine("\n❌ Невірний вибір."); break;
                    }
                }
                catch (BusinessLogicException ex)
                {
                    Console.WriteLine($"\n❌ {ex.Message}");
                }

                if (!back)
                {
                    Console.WriteLine("\nНатисніть будь-яку клавішу...");
                    Console.ReadKey();
                }
            }
        }

        private static void AddTask()
        {
            Console.Clear();
            Console.WriteLine("═══ ДОДАВАННЯ НОВОГО ЗАВДАННЯ ═══\n");

            Console.Write("Назва завдання: ");
            string title = Console.ReadLine();

            Console.Write("Опис завдання: ");
            string description = Console.ReadLine();

            DateTime deadline;
            while (true)
            {
                Console.Write("Термін виконання (ДД.ММ.РРРР): ");
                if (DateTime.TryParse(Console.ReadLine(), out deadline))
                    break;
                Console.WriteLine("❌ Некоректний формат дати!");
            }

            Console.Write("Пріоритет (1-Low, 2-Medium, 3-High, 4-Critical): ");
            if (!int.TryParse(Console.ReadLine(), out int priorityInt) || priorityInt < 1 || priorityInt > 4)
                priorityInt = 2;

            zdfzfhd priority = (zdfzfhd)priorityInt;

            Console.Write("Оцінка часу (годин): ");
            if (!int.TryParse(Console.ReadLine(), out int hours) || hours <= 0)
                hours = 8;

            var task = _taskService.AddTask(title, description, deadline, priority, hours);
            Console.WriteLine($"\n✓ Завдання '{task.Title}' успішно додано!");
        }

        private static void ViewAllTasks()
        {
            Console.Clear();
            Console.WriteLine("═══ СПИСОК ВСІХ ЗАВДАНЬ ═══\n");

            var tasks = _taskService.GetAllTasks();
            int count = 0;

            foreach (var task in tasks)
            {
                count++;
                Console.WriteLine($"{count}. {task.Title}");
                Console.WriteLine($"   ID: {task.Id}");
                Console.WriteLine($"   Термін: {task.Deadline:dd.MM.yyyy}");
                Console.WriteLine($"   Пріоритет: {task.Priority}");
                Console.WriteLine();
            }

            if (count == 0)
                Console.WriteLine("Немає завдань.");
        }

        #endregion

        #region Assignment Menu

        private static void AssignmentMenu()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════════════════╗");
                Console.WriteLine("║      ПРИЗНАЧЕННЯ ТА ВИКОНАННЯ ЗАВДАНЬ              ║");
                Console.WriteLine("╠════════════════════════════════════════════════════╣");
                Console.WriteLine("║  1. Призначити завдання виконавцю                  ║");
                Console.WriteLine("║  2. Переглянути всі призначення                    ║");
                Console.WriteLine("║  3. Оновити прогрес виконання                      ║");
                Console.WriteLine("║  0. Назад                                          ║");
                Console.WriteLine("╚════════════════════════════════════════════════════╝");
                Console.Write("\nВаш вибір: ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": AssignTaskToMember(); break;
                        case "2": ViewAllAssignments(); break;
                        case "3": UpdateAssignmentProgress(); break;
                        case "0": back = true; break;
                        default: Console.WriteLine("\n❌ Невірний вибір."); break;
                    }
                }
                catch (BusinessLogicException ex)
                {
                    Console.WriteLine($"\n❌ {ex.Message}");
                }

                if (!back)
                {
                    Console.WriteLine("\nНатисніть будь-яку клавішу...");
                    Console.ReadKey();
                }
            }
        }

        private static void AssignTaskToMember()
        {
            Console.Clear();
            Console.WriteLine("═══ ПРИЗНАЧЕННЯ ЗАВДАННЯ ═══\n");

            Console.Write("ID завдання: ");
            if (!Guid.TryParse(Console.ReadLine(), out Guid taskId))
            {
                Console.WriteLine("❌ Некоректний ID завдання");
                return;
            }

            Console.Write("ID виконавця: ");
            if (!Guid.TryParse(Console.ReadLine(), out Guid memberId))
            {
                Console.WriteLine("❌ Некоректний ID виконавця");
                return;
            }

            var assignment = _assignmentService.AssignTask(taskId, memberId);
            Console.WriteLine($"\n✓ Завдання успішно призначено!");
        }

        private static void ViewAllAssignments()
        {
            Console.Clear();
            Console.WriteLine("═══ ВСІ ПРИЗНАЧЕННЯ ═══\n");

            var assignments = _assignmentService.GetAllAssignments();
            int count = 0;

            foreach (var assignment in assignments)
            {
                count++;
                Console.WriteLine($"{count}. {assignment}");
                Console.WriteLine($"   ID: {assignment.Id}");
                Console.WriteLine();
            }

            if (count == 0)
                Console.WriteLine("Немає призначень.");
        }

        private static void UpdateAssignmentProgress()
        {
            Console.Clear();
            Console.WriteLine("═══ ОНОВЛЕННЯ ПРОГРЕСУ ═══\n");

            Console.Write("ID призначення: ");
            if (!Guid.TryParse(Console.ReadLine(), out Guid assignmentId))
            {
                Console.WriteLine("❌ Некоректний ID");
                return;
            }

            Console.Write("Відсоток виконання (0-100): ");
            if (!int.TryParse(Console.ReadLine(), out int percentage) || percentage < 0 || percentage > 100)
            {
                Console.WriteLine("❌ Некоректний відсоток");
                return;
            }

            _assignmentService.UpdateProgress(assignmentId, percentage);
            Console.WriteLine("\n✓ Прогрес успішно оновлено!");
        }

        #endregion

        #region Search Menu

        private static void SearchMenu()
        {
            Console.Clear();
            Console.WriteLine("═══ ПОШУК ═══\n");
            Console.WriteLine("Функція пошуку буде додана пізніше.");
            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        #endregion

        #region Statistics Menu

        private static void StatisticsMenu()
        {
            Console.Clear();
            Console.WriteLine("═══ СТАТИСТИКА ПРОЕКТУ ═══\n");

            var stats = _assignmentService.GetProjectStatistics();

            Console.WriteLine($"Загальна інформація:");
            Console.WriteLine($"  • Всього завдань: {stats.TotalTasks}");
            Console.WriteLine($"  • Членів команди: {stats.TotalMembers}");
            Console.WriteLine();

            Console.WriteLine($"Розподіл завдань:");
            Console.WriteLine($"  ✓ Виконано: {stats.CompletedTasks}");
            Console.WriteLine($"  ⏳ В процесі: {stats.InProgressTasks}");
            Console.WriteLine($"  ⚠️  Прострочено: {stats.OverdueTasks}");
            Console.WriteLine();

            Console.WriteLine($"Рівень завершеності: {stats.CompletionRate:F2}%");
        }

        #endregion
    }
}
