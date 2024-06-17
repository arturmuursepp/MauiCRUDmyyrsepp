namespace MauiCRUDmyyrsepp;

public partial class LandingPage : ContentPage
{
	public LandingPage()
	{
		InitializeComponent();
	}
    private async void NavButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EmployeeList());
    }
}