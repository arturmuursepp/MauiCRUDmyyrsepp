using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiCRUD.Data;
using MauiCRUD.Models;
using System.Collections.ObjectModel;

namespace MauiCRUD.ViewModels
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
                /*
                 * {System.TypeInitializationException: The type initializer for 'SQLite.SQLiteConnection' threw an exception.
                 ---> System.IO.FileNotFoundException: Could not load file or assembly 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b' or one of its dependencies.
                File name: 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b'
                   at SQLitePCL.Batteries_V2.Init()
                   at SQLite.SQLiteConnection..cctor()
                   --- End of inner exception stack trace ---
                   at SQLite.SQLiteConnectionWithLock..ctor(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteConnectionPool.Entry..ctor(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteConnectionPool.GetConnectionAndTransactionLock(SQLiteConnectionString connectionString, Object& transactionLock)
                   at SQLite.SQLiteConnectionPool.GetConnection(SQLiteConnectionString connectionString)
                   at SQLite.SQLiteAsyncConnection.GetConnection()
                   at SQLite.SQLiteAsyncConnection.<>c__DisplayClass33_0`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].<WriteAsync>b__0()
                   at System.Threading.Tasks.Task`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].InnerInvoke()
                   at System.Threading.Tasks.Task.<>c.<.cctor>b__273_0(Object obj)
                   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
                --- End of stack trace from previous location ---
                   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
                   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
                --- End of stack trace from previous location ---
                   at MAUISql.Data.DatabaseContext.<CreateTableIfNotExists>d__6`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 18
                   at MAUISql.Data.DatabaseContext.<GetTableAsync>d__7`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 23
                   at MAUISql.Data.DatabaseContext.<GetAllAsync>d__8`1[[MAUISql.Models.Product, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 29
                   at MAUISql.ViewModels.ProductsViewModel.<LoadProductsAsync>b__6_0() in D:\MAUI\MAUISql\MAUISql\ViewModels\ProductsViewModel.cs:line 34
                   at MAUISql.ViewModels.ProductsViewModel.ExecuteAsync(Func`1 operation, String busyText) in D:\MAUI\MAUISql\MAUISql\ViewModels\ProductsViewModel.cs:line 103}
                 */
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
