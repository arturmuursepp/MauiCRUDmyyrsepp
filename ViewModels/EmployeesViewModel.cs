using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiCRUDmyyrsepp.Data;
using MauiCRUDmyyrsepp.Models;
using System.Collections.ObjectModel;

namespace MauiCRUDmyyrsepp.ViewModels
{
    public partial class EmployeesViewModel : ObservableObject
    {
        private readonly DatabaseContext _context;

        public EmployeesViewModel(DatabaseContext context)
        {
            _context = context;
        }

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private Employee _operatingEmployee = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText;

        public async Task LoadEmployeesAsync()
        {
            await ExecuteAsync(async () =>
            {
                var employees = await _context.GetAllAsync<Employee>();
                if (employees is not null && employees.Any())
                {
                    Employees ??= new ObservableCollection<Employee>();

                    foreach (var employee in employees)
                    {
                        Employees.Add(employee);
                    }
                }
            }, "Fetching employees...");
        }

        private async Task ExecuteAsync(Func<Task> operation, string? busyText = null)
        {
            IsBusy = true;
            BusyText = busyText ?? "Processing...";
            try
            {
                await operation?.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                IsBusy = false;
                BusyText = "Processing...";
            }
        }

        [ICommand]
        private void SetOperatingEmployee(Employee? employee) => OperatingEmployee = employee ?? new();

        [ICommand]
        private async Task SaveEmployeeAsync()
        {
            if (OperatingEmployee is null)
                return;

            var (isValid, errorMessage) = OperatingEmployee.Validate();
            if (!isValid)
            {
                await Shell.Current.DisplayAlert("Validation Error", errorMessage, "Ok");
                return;
            }

            var busyText = OperatingEmployee.Id == 0 ? "Creating employee..." : "Updating employee...";
            await ExecuteAsync(async () =>
            {
                if (OperatingEmployee.Id == 0)
                {
                    await _context.AddItemAsync<Employee>(OperatingEmployee);
                    Employees.Add(OperatingEmployee);
                }
                else
                {
                    if (await _context.UpdateItemAsync<Employee>(OperatingEmployee))
                    {
                        var employeeCopy = OperatingEmployee.Clone();

                        var index = Employees.IndexOf(OperatingEmployee);
                        Employees.RemoveAt(index);

                        Employees.Insert(index, employeeCopy);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Employee updating error", "Ok");
                        return;
                    }
                }
                SetOperatingEmployeeCommand.Execute(new());
            }, busyText);
        }

        [ICommand]
        private async Task DeleteEmployeeAsync(int id)
        {
            await ExecuteAsync(async () =>
            {
                if (await _context.DeleteItemByKeyAsync<Employee>(id))
                {
                    var employee = Employees.FirstOrDefault(e => e.Id == id);
                    Employees.Remove(employee);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete Error", "Employee was not deleted", "Ok");
                }
            }, "Deleting employee...");
        }


    }
}
