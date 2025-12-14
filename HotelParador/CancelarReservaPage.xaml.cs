using Newtonsoft.Json;
using System.Net.Http;
using CommunityToolkit.Mvvm.Messaging;
using HotelParador.Services;

namespace HotelParador;

public partial class CancelarReservaPage : ContentPage
{
    private string supabaseUrl = "https://mduluguemexwsgkpnavb.supabase.co/rest/v1";
    private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1kdWx1Z3VlbWV4d3Nna3BuYXZiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMxNDk1MTQsImV4cCI6MjA3ODcyNTUxNH0.ndTochHhui6-4eQSNqhkNMgzDQf-ZbP8eb_gNRsoxUg";
    private HttpClient cliente = new HttpClient();

    private dynamic reservaEncontrada = null;

    public CancelarReservaPage()
    {
        InitializeComponent();
    }

    private async void OnBuscarReservaClicked(object sender, EventArgs e)
    {
        // Validar campos
        if (string.IsNullOrWhiteSpace(EntryNumeroReserva.Text))
        {
            await DisplayAlert("Error", "Por favor ingresa el número de reserva", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(EntryEmail.Text))
        {
            await DisplayAlert("Error", "Por favor ingresa tu correo electrónico", "OK");
            return;
        }

        try
        {
            BtnBuscar.IsEnabled = false;
            BtnBuscar.Text = "Buscando...";

            // Configurar headers
            cliente.DefaultRequestHeaders.Clear();
            cliente.DefaultRequestHeaders.Add("apikey", supabaseKey);
            cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            // Primero buscar el usuario por email
            var userUrl = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(EntryEmail.Text)}&select=id";
            var userResp = await cliente.GetAsync(userUrl);
            var userJson = await userResp.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<dynamic>>(userJson);

            if (users == null || users.Count == 0)
            {
                await DisplayAlert("No Encontrado", "No se encontró ningún usuario con ese correo", "OK");
                return;
            }

            var userId = users[0].id.ToString();

            // Buscar la reserva
            var reservaId = EntryNumeroReserva.Text.Replace("RES-", "").Replace("res-", "");
            var resUrl = $"{supabaseUrl}/reservations?id=eq.{reservaId}&user_id=eq.{userId}&select=*,rooms(*)";

            System.Diagnostics.Debug.WriteLine($"Buscando reserva: {resUrl}");

            var resResp = await cliente.GetAsync(resUrl);
            var resJson = await resResp.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Respuesta: {resJson}");

            var reservas = JsonConvert.DeserializeObject<List<dynamic>>(resJson);

            if (reservas == null || reservas.Count == 0)
            {
                await DisplayAlert("No Encontrado",
                    "No se encontró ninguna reservación con esos datos.\n\n" +
                    "Verifica que el número de reserva y el correo sean correctos.",
                    "OK");
                return;
            }

            // Mostrar la reserva encontrada
            reservaEncontrada = reservas[0];

            string roomName = "Habitación";
            try
            {
                if (reservaEncontrada.rooms != null)
                {
                    roomName = reservaEncontrada.rooms.name?.ToString()
                            ?? reservaEncontrada.rooms.Name?.ToString()
                            ?? $"Habitación #{reservaEncontrada.room_id}";
                }
            }
            catch { }

            LblHabitacion.Text = roomName;
            LblCheckin.Text = reservaEncontrada.checkin?.ToString() ?? "-";
            LblCheckout.Text = reservaEncontrada.checkout?.ToString() ?? "-";
            LblTotal.Text = $"${reservaEncontrada.total?.ToString() ?? "0"}";

            FrameResultado.IsVisible = true;

            await DisplayAlert("✅ Reserva Encontrada",
                "Se encontró tu reservación. Revisa los detalles y confirma la cancelación si es correcta.",
                "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            await DisplayAlert("Error", $"Ocurrió un error al buscar la reserva:\n{ex.Message}", "OK");
        }
        finally
        {
            BtnBuscar.IsEnabled = true;
            BtnBuscar.Text = "Buscar Reservación";
        }
    }

    private async void OnConfirmarCancelacionClicked(object sender, EventArgs e)
    {
        if (reservaEncontrada == null)
            return;

        bool confirmar = await DisplayAlert("Confirmar Cancelación",
            "¿Estás seguro de que deseas cancelar esta reservación?\n\n" +
            "Esta acción no se puede deshacer.",
            "Sí, Cancelar",
            "No");

        if (!confirmar)
            return;

        try
        {
            BtnConfirmarCancelacion.IsEnabled = false;
            BtnConfirmarCancelacion.Text = "Cancelando...";

            // Configurar headers
            cliente.DefaultRequestHeaders.Clear();
            cliente.DefaultRequestHeaders.Add("apikey", supabaseKey);
            cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");
            cliente.DefaultRequestHeaders.Add("Prefer", "return=minimal");

            // Eliminar la reserva
            var deleteUrl = $"{supabaseUrl}/reservations?id=eq.{reservaEncontrada.id}";
            var deleteResp = await cliente.DeleteAsync(deleteUrl);

            if (deleteResp.IsSuccessStatusCode)
            {

                WeakReferenceMessenger.Default.Send(new ReservaCanceladaMessage(reservaEncontrada.id.ToString()));

                await DisplayAlert("Cancelación Exitosa",
                    "Tu reservación ha sido cancelada correctamente.\n\n" +
                    "Recibirás un correo de confirmación en los próximos minutos.",
                    "OK");

                // Volver al menú
                await Navigation.PopAsync();
            }
            else
            {
                var errorMsg = await deleteResp.Content.ReadAsStringAsync();
                await DisplayAlert("Error",
                    $"No se pudo cancelar la reserva:\n{errorMsg}",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error:\n{ex.Message}", "OK");
        }
        finally
        {
            BtnConfirmarCancelacion.IsEnabled = true;
            BtnConfirmarCancelacion.Text = "Confirmar Cancelación";
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}