using Newtonsoft.Json;
using System.Net.Http;
using CommunityToolkit.Mvvm.Messaging;
using HotelParador.Services;
using static HotelParador.Services.ReservationService;
using System;
using System.Threading.Tasks;

namespace HotelParador;

public partial class CancelarReservaPage : ContentPage
{
    private readonly ReservationService _reservationService;
    private readonly ReservationWithRoom _reservation;
    private readonly string _userEmail;

    public CancelarReservaPage(ReservationWithRoom reservation, string userEmail)
    {
        InitializeComponent();
        _reservationService = new ReservationService();
        _reservation = reservation;
        _userEmail = userEmail;

        // Cargar información de la reservación
        CargarDetallesReservacion();
    }

    private void CargarDetallesReservacion()
    {
        try
        {
            // Mostrar detalles de la reservación
            ReservacionIdLabel.Text = $"#{_reservation.id}";
            RoomNameLabel.Text = _reservation.room_name;
            DatesLabel.Text = $"{_reservation.checkin:dd/MM/yyyy} - {_reservation.checkout:dd/MM/yyyy} ({_reservation.NumberOfNights} {(_reservation.NumberOfNights == 1 ? "noche" : "noches")})";
            TotalLabel.Text = $"${_reservation.total:N2} USD";

            // Pre-llenar el email del usuario (opcional, puedes quitarlo si quieres que lo escriban)
            // EmailEntry.Text = _userEmail;
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error al cargar detalles: {ex.Message}", "OK");
        }
    }

    private async void OnConfirmarCancelacionClicked(object sender, EventArgs e)
    {
        try
        {
            // Validar que el email no esté vacío
            string inputEmail = EntryEmail.Text?.Trim();

            if (string.IsNullOrWhiteSpace(inputEmail))
            {
                await DisplayAlert(
                    "Campo Requerido",
                    "Por favor, ingresa tu correo electrónico para confirmar la cancelación.",
                    "OK"
                );
                return;
            }

            // Validar que el email coincida con el usuario logueado
            if (!inputEmail.Equals(_userEmail, StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert(
                    "Error de Verificación",
                    "El correo electrónico ingresado no coincide con el usuario de esta reservación.\n\n" +
                    "Por favor, verifica e intenta nuevamente.",
                    "OK"
                );
                return;
            }

            // Confirmar una última vez
            var confirm = await DisplayAlert(
                "Confirmación Final",
                "¿Estás completamente seguro de cancelar esta reservación?\n\n" +
                "Esta acción es irreversible.",
                "Sí, cancelar",
                "No"
            );

            if (!confirm) return;

            // Mostrar loading
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            BtnConfirmarCancelacion.IsEnabled = false;
            BtnConfirmarCancelacion.Text = "Procesando...";

            // Llamar al servicio de cancelación (tu método existente)
            bool success = await _reservationService.CancelReservationAsync(_reservation.id);

            // Ocultar loading
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            BtnConfirmarCancelacion.IsEnabled = true;
            BtnConfirmarCancelacion.Text = "Confirmar Cancelación";

            if (success)
            {
                await DisplayAlert(
                    "✓ Cancelación Exitosa",
                    $"Tu reservación #{_reservation.id} ha sido cancelada correctamente.\n\n" +
                    $"La habitación {_reservation.room_name} ha sido liberada.",
                    "OK"
                );

                // Volver a la página de reservaciones (y recargar la lista)
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert(
                    "Error",
                    "No se pudo cancelar la reservación.\n\n" +
                    "Por favor, intenta nuevamente o contacta con soporte.",
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            BtnConfirmarCancelacion.IsEnabled = true;
            BtnConfirmarCancelacion.Text = "Confirmar Cancelación";

            await DisplayAlert(
                "Error",
                $"Ocurrió un error al cancelar la reservación:\n\n{ex.Message}",
                "OK"
            );
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}