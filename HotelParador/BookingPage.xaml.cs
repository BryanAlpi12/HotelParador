using Microsoft.Maui.Controls;
using System;

namespace HotelParador;

    public partial class BookingPage : ContentPage
{
    public BookingPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnNewBookingClicked(object sender, EventArgs e)
    {
       
        await Navigation.PushAsync(new NewBookingPage());
    }
    
    private async void OnViewDetailsClicked(object sender, EventArgs e)
    {
      
        await DisplayAlert(
            "Detalles de Reservación",
            "🛏️ Habitación: Garden Room\n" +
            "💰 Precio por noche: xxxx \n\n" +
            "📅 Check-in: 15 de Diciembre 2025\n" +
            "📅 Check-out: 20 de Diciembre 2025\n" +
            "🌙 Duración: 5 noches\n\n" +
            "👥 Huéspedes: 2\n\n" +
            "💰 Total: xxxx \n\n" +
            "✅ Estado: Confirmada\n\n" +
            "⏰ Faltan x días para tu llegada",
            "Cerrar"
        );
    } 
}