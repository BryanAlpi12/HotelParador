using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace HotelParador;

public partial class LocationPage : ContentPage
{
    public LocationPage()
    {
        InitializeComponent();

        var parador = new Location(9.4011, -84.1608);

        var pin = new Pin
        {
            Label = "Hotel Parador",
            Location = parador,
            Address = "Manuel Antonio",
            Type = PinType.Place
        };

        Mapa.Pins.Add(pin);

        Mapa.MoveToRegion(
            MapSpan.FromCenterAndRadius(parador, Distance.FromKilometers(1))
        );
    }
}
