using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiCRUDmyyrsepp.Models;
using SQLite;

namespace MauiCRUDmyyrsepp.ViewModels
{
    public class EmployeesViewModel : INotifyPropertyChanged
    {
        private Employee _operatingEmployee;
        private readonly SQLiteAsyncConnection _database;
        private bool _isBusy;
        public ObservableCollection<Employee> Employees { get; set; }
        public Employee OperatingEmployee
        {
            get => _operatingEmployee;
            set => SetProperty(ref _operatingEmployee, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value, nameof(IsBusy));
        }

        public string BusyText { get; set; }
        public ICommand SaveEmployeeCommand { get; }
        public ICommand SetOperatingEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EmployeesViewModel()
        {
            _database = InitializeDatabase();

            Employees = new ObservableCollection<Employee>();
            OperatingEmployee = new Employee();
            BusyText = "Loading...";

            SetOperatingEmployeeCommand = new Command<Employee>(SetOperatingEmployee);
            SaveEmployeeCommand = new Command(async () => await SaveEmployee());
            DeleteEmployeeCommand = new Command<int>(async (id) => await DeleteEmployee(id));

            LoadEmployees();
        }

        private SQLiteAsyncConnection InitializeDatabase()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Employees.db3");
            var database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<Employee>().Wait();
            return database;
        }

        private void SetOperatingEmployee(Employee employee)
        {
            OperatingEmployee = employee ?? new Employee();
        }

        private async Task SaveEmployee()
        {
            IsBusy = true;

            if (OperatingEmployee.Id != 0)
            {
                await _database.UpdateAsync(OperatingEmployee);
            }
            else
            {
                await _database.InsertAsync(OperatingEmployee);
            }

            OperatingEmployee = new Employee();
            await LoadEmployees();

            IsBusy = false;
        }

        private async Task DeleteEmployee(int employeeId)
        {
            var employee = await _database.Table<Employee>().Where(p => p.Id == employeeId).FirstOrDefaultAsync();
            if (employee != null)
            {
                await _database.DeleteAsync(employee);
                await LoadEmployees();
            }
        }

        private async Task LoadEmployees()
        {
            IsBusy = true;

            var employees = await _database.Table<Employee>().ToListAsync();
            Employees.Clear();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }

            IsBusy = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
