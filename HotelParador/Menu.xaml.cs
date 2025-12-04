namespace HotelParador;

public partial class Menu : ContentPage
{
    public Menu(String nombre)
    {
        InitializeComponent();
    }

    // Evento para ir a Booking
    private async void OnBookingTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BookingPage());
    }

    // Evento para ir a Profile
    private async void GoToProfile(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage());
    }

    // Evento para ir a Location 
    private async void OnLocationTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LocationPage());
    }
}
