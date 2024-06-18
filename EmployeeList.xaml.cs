using MauiCRUDmyyrsepp.ViewModels;

namespace MauiCRUDmyyrsepp
{
    public partial class EmployeeList : ContentPage
    {
        public EmployeeList()
        {
            InitializeComponent();
            BindingContext = new EmployeesViewModel();
        }
    }
}
