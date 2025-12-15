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

    private async void GoToProfile(object sender, EventArgs e)
    {
        // Verificar que hay un email guardado
        if (!string.IsNullOrEmpty(App.UserEmail))
        {
            System.Diagnostics.Debug.WriteLine($"Email guardado en App: {App.UserEmail}");

            // Navegar pasando el email como parámetro
            await Shell.Current.GoToAsync($"ProfilePage?email={Uri.EscapeDataString(App.UserEmail)}");
        }
        else
        {
            await DisplayAlert("Error", "No hay sesión activa. Por favor inicia sesión nuevamente.", "OK");
        }
    }
    // Evento para ir a Location 
    private async void OnLocationTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LocationPage());
    }
    // Introducción
    private async void OnIntroduccionClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IntroduccionPage());
    }

    // Historia
    private async void OnHistoriaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HistoriaPage());
    }

    // Ayuda - Abre el navegador web
    private async void OnAyudaClicked(object sender, EventArgs e)
    {
        try
        {
            await Browser.OpenAsync("https://hotelparador.com/", BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir el navegador: {ex.Message}", "OK");
        }
    }

    // Cancelar Reservación
    private async void OnCancelarReservaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CancelarReservaPage());
    }

    // Para las habitaciones de "Perfect for you"
    private async void OnGardenRoomTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NewBookingPage());
    }

    private async void OnGardenPlusRoomTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NewBookingPage());
    }

    // Para las habitaciones de "For this summer"
    private async void OnTropicalRoomTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NewBookingPage());
    }

    private async void OnPremiumPlusRoomTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NewBookingPage());
    }
}


