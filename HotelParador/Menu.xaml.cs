using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HotelParador;

public partial class Menu : ContentPage
{
	public Menu(string nombre)
	{
		InitializeComponent();
		//LblNombre.Text = "Bienvenido " + nombre; 
		Page Sesion = new NavigationPage(new Sesion());
		


	}

    private async void BtnLocation_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///LocationPage");

    }
    private async void GoToProfile(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ProfilePage");
    }




}