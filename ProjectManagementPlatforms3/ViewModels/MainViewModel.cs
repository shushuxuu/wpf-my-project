using ProjectManagementPlatform.Models;
using ProjectManagementPlatform.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ProjectManagementPlatform.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private ObservableCollection<Project> _projects;
        private Project _selectedProject;
        private Task _selectedTask;
        private string _newTaskTitle;
        private string _newTaskDescription;
        private string _selectedAssignee;
        private string _selectedPriority;
        private decimal _estimatedHours;

        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged();
            }
        }

        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedProjectName));

                if (_selectedProject != null)
                {
                    LoadTasksForSelectedProject();
                }
                else
                {
                    ProjectTasks.Clear();
                }
            }
        }

        public Task SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedTaskTitle));
                OnPropertyChanged(nameof(SelectedTaskStatus));
                OnPropertyChanged(nameof(SelectedTaskPriority));
                OnPropertyChanged(nameof(SelectedTaskHours));

                if (_selectedTask != null)
                {
                    NewTaskTitle = _selectedTask.Title;
                    NewTaskDescription = _selectedTask.Description;
                    SelectedAssignee = _selectedTask.AssignedTo;
                    SelectedPriority = _selectedTask.Priority;
                    EstimatedHours = _selectedTask.EstimatedHours; // Загружаем часы
                }
                else
                {
                    NewTaskTitle = "";
                    NewTaskDescription = "";
                    SelectedAssignee = "Не назначено";
                    SelectedPriority = "Medium";
                    EstimatedHours = 8; // Значение по умолчанию
                }
            }
        }

        public ObservableCollection<Task> ProjectTasks { get; set; }

        public List<string> AvailableAssignees { get; set; }
        public List<string> AvailablePriorities { get; set; }

        // Безопасные свойства для отображения
        public string SelectedProjectName => SelectedProject?.Name ?? "Проект не выбран";
        public string SelectedTaskTitle => SelectedTask?.Title ?? "Задача не выбрана";
        public string SelectedTaskStatus => SelectedTask != null ? GetStatusDisplayName(SelectedTask.Status) : "";
        public string SelectedTaskPriority => SelectedTask != null ? SelectedTask.DisplayPriority : "";
        public string SelectedTaskHours => SelectedTask != null ? $"{SelectedTask.EstimatedHours} ч." : "";

        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set
            {
                _newTaskTitle = value;
                OnPropertyChanged();
            }
        }

        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set
            {
                _newTaskDescription = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAssignee
        {
            get => _selectedAssignee;
            set
            {
                _selectedAssignee = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                _selectedPriority = value;
                OnPropertyChanged();
            }
        }

        public decimal EstimatedHours
        {
            get => _estimatedHours;
            set
            {
                if (value < 0) value = 0; // Минимум 0 часов
                if (value > 1000) value = 1000; // Максимум 1000 часов
                _estimatedHours = value;
                OnPropertyChanged();
            }
        }

        // Команды
        public ICommand AddProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand MarkTaskCompleteCommand { get; }
        public ICommand ClearTaskFormCommand { get; }
        public ICommand ChangeTaskStatusCommand { get; }

        public MainViewModel()
        {
            _projectRepository = new ProjectRepository();
            _taskRepository = new TaskRepository();

            Projects = new ObservableCollection<Project>();
            ProjectTasks = new ObservableCollection<Task>();

            AvailableAssignees = _taskRepository.GetAvailableAssignees();
            AvailablePriorities = new List<string> { "Low", "Medium", "High" };

            // Инициализация команд
            AddProjectCommand = new RelayCommand(AddProject);
            SaveProjectCommand = new RelayCommand(SaveProject);
            DeleteProjectCommand = new RelayCommand(DeleteProject);
            AddTaskCommand = new RelayCommand(AddTask, CanAddTask);
            SaveTaskCommand = new RelayCommand(SaveTask, CanSaveTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask, CanDeleteTask);
            MarkTaskCompleteCommand = new RelayCommand(MarkTaskComplete, CanMarkTaskComplete);
            ClearTaskFormCommand = new RelayCommand(ClearTaskForm);
            ChangeTaskStatusCommand = new RelayCommand(ChangeTaskStatus, CanChangeTaskStatus);

            LoadProjects();
            ClearTaskForm(null);
        }

        private void LoadProjects()
        {
            try
            {
                var projects = _projectRepository.GetAllProjects();
                Projects.Clear();
                foreach (var project in projects)
                {
                    Projects.Add(project);
                }

                if (Projects.Any())
                    SelectedProject = Projects[0];
                else
                    SelectedProject = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке проектов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTasksForSelectedProject()
        {
            ProjectTasks.Clear();
            if (SelectedProject != null)
            {
                try
                {
                    var tasks = _taskRepository.GetTasksByProject(SelectedProject.Id);
                    foreach (var task in tasks)
                    {
                        ProjectTasks.Add(task);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке задач: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region Команды для проектов
        private void AddProject(object parameter)
        {
            try
            {
                var newProject = new Project
                {
                    Name = "Новый проект",
                    Description = "Описание проекта",
                    StartDate = DateTime.Now,
                    Deadline = DateTime.Now.AddMonths(1),
                    Budget = 100000,
                    Status = "Planning",
                    ManagerId = 1
                };

                bool success = _projectRepository.AddProject(newProject);
                if (success)
                {
                    LoadProjects();
                    MessageBox.Show("Проект добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось добавить проект", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveProject(object parameter)
        {
            if (SelectedProject != null)
            {
                try
                {
                    bool success = _projectRepository.UpdateProject(SelectedProject);
                    if (success)
                    {
                        LoadProjects();
                        MessageBox.Show("Проект сохранен!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить проект", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении проекта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteProject(object parameter)
        {
            if (SelectedProject != null)
            {
                var result = MessageBox.Show($"Удалить проект '{SelectedProject.Name}' и все его задачи?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _projectRepository.DeleteProject(SelectedProject.Id);
                        if (success)
                        {
                            LoadProjects();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить проект", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion

        #region Команды для задач
        private void AddTask(object parameter)
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Выберите проект для добавления задачи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                MessageBox.Show("Введите название задачи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EstimatedHours <= 0)
            {
                MessageBox.Show("Укажите оценку часов (больше 0)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newTask = new Task
                {
                    Title = NewTaskTitle,
                    Description = NewTaskDescription,
                    ProjectId = SelectedProject.Id,
                    AssignedTo = SelectedAssignee ?? "Не назначено",
                    DueDate = DateTime.Now.AddDays(7),
                    Priority = SelectedPriority ?? "Medium",
                    Status = "ToDo",
                    EstimatedHours = EstimatedHours, // Используем введенное значение
                    ActualHours = 0,
                    CreatedAt = DateTime.Now
                };

                bool success = _taskRepository.AddTask(newTask);

                if (success)
                {
                    LoadTasksForSelectedProject();
                    ClearTaskForm(null);
                    MessageBox.Show("Задача добавлена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось добавить задачу", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении задачи: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveTask(object parameter)
        {
            if (SelectedTask != null && SelectedProject != null)
            {
                try
                {
                    var taskToUpdate = new Task
                    {
                        Id = SelectedTask.Id,
                        Title = NewTaskTitle,
                        Description = NewTaskDescription,
                        ProjectId = SelectedProject.Id,
                        AssignedTo = SelectedAssignee,
                        DueDate = SelectedTask.DueDate,
                        Priority = SelectedPriority,
                        Status = SelectedTask.Status,
                        EstimatedHours = EstimatedHours, // Сохраняем оценку часов
                        ActualHours = SelectedTask.ActualHours,
                        CreatedAt = SelectedTask.CreatedAt
                    };

                    bool success = _taskRepository.UpdateTask(taskToUpdate);

                    if (success)
                    {
                        SelectedTask.Title = NewTaskTitle;
                        SelectedTask.Description = NewTaskDescription;
                        SelectedTask.AssignedTo = SelectedAssignee;
                        SelectedTask.Priority = SelectedPriority;
                        SelectedTask.EstimatedHours = EstimatedHours;

                        LoadTasksForSelectedProject();
                        ClearTaskForm(null);
                        MessageBox.Show("Задача обновлена!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить задачу", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении задачи: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите проект и задачу для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteTask(object parameter)
        {
            if (SelectedTask != null)
            {
                var result = MessageBox.Show($"Удалить задачу '{SelectedTask.Title}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _taskRepository.DeleteTask(SelectedTask.Id);
                        if (success)
                        {
                            LoadTasksForSelectedProject();
                            ClearTaskForm(null);
                            MessageBox.Show("Задача удалена!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить задачу", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MarkTaskComplete(object parameter)
        {
            if (SelectedTask != null && SelectedProject != null)
            {
                // Сохраняем данные задачи ЛОКАЛЬНО
                string taskTitle = SelectedTask.Title;

                try
                {
                    var taskToUpdate = new Task
                    {
                        Id = SelectedTask.Id,
                        Title = SelectedTask.Title,
                        Description = SelectedTask.Description,
                        ProjectId = SelectedProject.Id,
                        AssignedTo = SelectedTask.AssignedTo,
                        DueDate = SelectedTask.DueDate,
                        Priority = SelectedTask.Priority,
                        Status = "Completed",
                        EstimatedHours = SelectedTask.EstimatedHours,
                        ActualHours = SelectedTask.EstimatedHours, // Фактические = оценке
                        CreatedAt = SelectedTask.CreatedAt
                    };

                    bool success = _taskRepository.UpdateTask(taskToUpdate);

                    if (success)
                    {
                        SelectedTask.Status = "Completed";
                        SelectedTask.ActualHours = SelectedTask.EstimatedHours;

                        LoadTasksForSelectedProject();

                        // Используем локальную переменную
                        MessageBox.Show($"Задача '{taskTitle}' отмечена как выполненная!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить статус задачи", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при завершении задачи: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для завершения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ChangeTaskStatus(object parameter)
        {
            if (SelectedTask != null && SelectedProject != null && parameter is string status)
            {
                try
                {
                    // Сохраняем данные задачи ЛОКАЛЬНО перед обновлением
                    string taskTitle = SelectedTask.Title;
                    int taskId = SelectedTask.Id;

                    var taskToUpdate = new Task
                    {
                        Id = taskId,
                        Title = SelectedTask.Title,
                        Description = SelectedTask.Description,
                        ProjectId = SelectedProject.Id,
                        AssignedTo = SelectedTask.AssignedTo,
                        DueDate = SelectedTask.DueDate,
                        Priority = SelectedTask.Priority,
                        Status = status,
                        EstimatedHours = SelectedTask.EstimatedHours,
                        ActualHours = SelectedTask.ActualHours,
                        CreatedAt = SelectedTask.CreatedAt
                    };

                    bool success = _taskRepository.UpdateTask(taskToUpdate);

                    if (success)
                    {
                        // Обновляем статус в выбранной задаче
                        SelectedTask.Status = status;

                        // Перезагружаем список задач
                        LoadTasksForSelectedProject();

                        string statusDisplay = GetStatusDisplayName(status);

                        // Используем локальную переменную taskTitle
                        MessageBox.Show($"Статус задачи '{taskTitle}' изменен на '{statusDisplay}'",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось изменить статус задачи", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для изменения статуса", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearTaskForm(object parameter)
        {
            NewTaskTitle = "";
            NewTaskDescription = "";
            SelectedAssignee = "Не назначено";
            SelectedPriority = "Medium";
            EstimatedHours = 8; // Сбрасываем к значению по умолчанию
            SelectedTask = null;
        }

        private string GetStatusDisplayName(string status)
        {
            switch (status)
            {
                case "ToDo":
                    return "К выполнению";
                case "InProgress":
                    return "В работе";
                case "Review":
                    return "На проверке";
                case "Completed":
                    return "Выполнена";
                default:
                    return status;
            }
        }

        // Проверки доступности команд
        private bool CanAddTask(object parameter) => SelectedProject != null;
        private bool CanSaveTask(object parameter) => SelectedTask != null && !string.IsNullOrWhiteSpace(NewTaskTitle);
        private bool CanDeleteTask(object parameter) => SelectedTask != null;
        private bool CanMarkTaskComplete(object parameter) => SelectedTask != null && SelectedTask.Status != "Completed";
        private bool CanChangeTaskStatus(object parameter) => SelectedTask != null && SelectedProject != null;

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}