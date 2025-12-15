using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace HotelParador;

public partial class LocationPage : ContentPage
{
    private const double Latitud = 9.39757;
    private const double Longitud = -84.16920;
    private const string NombreHotel = "Hotel Parador";

    public LocationPage()
    {
        InitializeComponent();
        ConfigurarMapa();
    }

    private void ConfigurarMapa()
    {
        var ubicacionHotel = new Location(Latitud, Longitud);
        var pin = new Pin
        {
            Label = NombreHotel,
            Address = "Manuel Antonio Norte, Quepos, Costa Rica",
            Type = PinType.Place,
            Location = ubicacionHotel
        };

        Mapa.Pins.Add(pin);
        Mapa.MoveToRegion(MapSpan.FromCenterAndRadius(ubicacionHotel, Distance.FromKilometers(1)));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Método único para "Cómo llegar"
    private async void OnComoLlegarClicked(object sender, EventArgs e)
    {
        try
        {
            // URI geo: que Android reconoce y muestra selector de apps
            var uri = $"geo:{Latitud},{Longitud}?q={Latitud},{Longitud}({Uri.EscapeDataString(NombreHotel)})";
            await Launcher.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir la navegación: {ex.Message}", "OK");
        }
    }
}