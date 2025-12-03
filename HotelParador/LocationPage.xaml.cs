using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace HotelParador;

public partial class LocationPage : ContentPage
{
    // Coordenadas del Hotel Parador - Plus Code: 9RWJ+WM8
    private const double HotelLatitude = 9.3970;
    private const double HotelLongitude = -84.1183;
    private const string HotelName = "Hotel Parador";
    private const string HotelAddress = "Manuel Antonio Norte, Quepos, Costa Rica";

    public LocationPage()
    {
        InitializeComponent();
        InitializeMap();
    }

    private async void InitializeMap()
    {
        try
        {
            // Pequeño delay para asegurar que el mapa se cargue
            await Task.Delay(500);

            // Crear ubicación del hotel
            var hotelLocation = new Location(HotelLatitude, HotelLongitude);

            // Crear pin para el hotel
            var pin = new Pin
            {
                Label = HotelName,
                Location = hotelLocation,
                Address = HotelAddress,
                Type = PinType.Place
            };

            // Agregar pin al mapa
            Mapa.Pins.Add(pin);

            // Centrar el mapa en el hotel con zoom apropiado
            Mapa.MoveToRegion(
                MapSpan.FromCenterAndRadius(hotelLocation, Distance.FromKilometers(1))
            );

            System.Diagnostics.Debug.WriteLine("Mapa inicializado correctamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al inicializar mapa: {ex.Message}");
            await DisplayAlert("Error", $"No se pudo cargar el mapa: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnGoogleMapsClicked(object sender, EventArgs e)
    {
        try
        {
            // URL para Google Maps
            var googleMapsUrl = $"https://www.google.com/maps/search/?api=1&query={HotelLatitude},{HotelLongitude}";

            // Intentar abrir con la app de Google Maps primero
            var supportsUri = await Launcher.CanOpenAsync($"comgooglemaps://?q={HotelLatitude},{HotelLongitude}");

            if (supportsUri)
            {
                // Si tiene la app instalada
                await Launcher.OpenAsync($"comgooglemaps://?q={HotelLatitude},{HotelLongitude}");
            }
            else
            {
                // Si no tiene la app, abrir en navegador
                await Launcher.OpenAsync(googleMapsUrl);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir Google Maps: {ex.Message}", "OK");
        }
    }

    private async void OnWazeClicked(object sender, EventArgs e)
    {
        try
        {
            // URL para Waze
            var wazeUrl = $"https://waze.com/ul?ll={HotelLatitude},{HotelLongitude}&navigate=yes";

            // Intentar abrir con la app de Waze primero
            var supportsUri = await Launcher.CanOpenAsync($"waze://?ll={HotelLatitude},{HotelLongitude}&navigate=yes");

            if (supportsUri)
            {
                // Si tiene la app instalada
                await Launcher.OpenAsync($"waze://?ll={HotelLatitude},{HotelLongitude}&navigate=yes");
            }
            else
            {
                // Si no tiene la app, abrir en navegador
                await Launcher.OpenAsync(wazeUrl);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir Waze: {ex.Message}", "OK");
        }
    }
}