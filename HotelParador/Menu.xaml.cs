using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HotelParador
{
    public partial class Menu : ContentPage
    {
        public Menu(string nombre)
        {
            InitializeComponent();
            //LblNombre.Text = "Bienvenido " + nombre;
        }

        private async void BtnLocation_Clicked(object sender, EventArgs e)
        {
            // Navegación correcta (ruta normal)
            await Shell.Current.GoToAsync("LocationPage");
        }

        private async void GoToProfile(object sender, EventArgs e)
        {
            // Navegación correcta (ruta normal)
            await Shell.Current.GoToAsync("ProfilePage");
        }

        private async void OnBookingTapped(object sender, TappedEventArgs e)
        {
            // Si BookingPage NO usa Shell, esta está bien:
            await Navigation.PushAsync(new BookingPage());
        }

        private async void GoToProfileEdit(object sender, EventArgs e)
        {
            // Navegación correcta (ruta normal)
            await Shell.Current.GoToAsync("ProfilePageEdit");
        }

    }
}
