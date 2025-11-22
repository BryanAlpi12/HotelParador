using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using HotelParador.Models; // define User, Reservation models
using System.Collections.ObjectModel;

namespace HotelParador;

[QueryProperty(nameof(Email), "email")]
public partial class ProfilePage : ContentPage
{
    private string supabaseUrl = "https://mduluguemexwsgkpnavb.supabase.co/rest/v1/users";
    private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1kdWx1Z3VlbWV4d3Nna3BuYXZiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMxNDk1MTQsImV4cCI6MjA3ODcyNTUxNH0.ndTochHhui6-4eQSNqhkNMgzDQf-ZbP8eb_gNRsoxUg";


    HttpClient cliente = new HttpClient();

    public string Email { get; set; }

    public ObservableCollection<Reservation> Reservas { get; set; } = new ObservableCollection<Reservation>();

    public ProfilePage()
    {
        InitializeComponent();
        BtnEditar.Clicked += BtnEditar_Clicked;
        BtnLogout.Clicked += BtnLogout_Clicked;
        CvReservas.ItemsSource = Reservas;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(Email))
        {
            _ = LoadUserAsync(Email);
            _ = LoadReservationsAsync(Email);
        }
    }

    async Task LoadUserAsync(string email)
    {
        try
        {
            cliente.DefaultRequestHeaders.Clear();
            cliente.DefaultRequestHeaders.Add("apikey", supabaseKey);
            cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");
            cliente.DefaultRequestHeaders.Add("Prefer", "return=representation");

            // Obtener usuario por email
            var url = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(email)}&select=*";
            var resp = await cliente.GetAsync(url);
            var js = await resp.Content.ReadAsStringAsync();

            var list = JsonConvert.DeserializeObject<List<User>>(js);
            if (list != null && list.Count > 0)
            {
                var user = list[0];
                LblNombre.Text = user.username ?? "Usuario";
                LblEmail.Text = user.email ?? "";

                
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    async Task LoadReservationsAsync(string email)
    {
        try
        {
            // Primero buscar user id (asumiendo tabla users tiene id)
            var urlUser = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(email)}&select=id";
            var rUser = await cliente.GetAsync(urlUser);
            var jUser = await rUser.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<dynamic>>(jUser);
            if (users != null && users.Count > 0)
            {
                var userId = users[0].id;

                var urlRes = $"{supabaseUrl}/reservations?user_id=eq.{userId}&select=*,rooms(*)";
                var rRes = await cliente.GetAsync(urlRes);
                var jRes = await rRes.Content.ReadAsStringAsync();

                // Mapea según tu esquema. Ejemplo sencillo:
                var entries = JsonConvert.DeserializeObject<List<ReservationDto>>(jRes);
                Reservas.Clear();
                foreach (var e in entries)
                {
                    Reservas.Add(new Reservation
                    {
                        id = e.id,
                        room_name = e.rooms != null ? e.rooms.name : "Habitación",
                        checkin = e.checkin,
                        checkout = e.checkout,
                        total = e.total
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // no bloquear la UI
        }
    }

    private async void BtnEditar_Clicked(object sender, EventArgs e)
    {
        // Navegar a página de edición, pasando email
        await Shell.Current.GoToAsync($"///EditProfilePage?email={Uri.EscapeDataString(Email)}");
    }

    private async void BtnLogout_Clicked(object sender, EventArgs e)
    {

        // Regresar al login o pantalla principal
        await Shell.Current.GoToAsync("///MainPage");
    }
}

// --- Models auxiliares (ajusta a tu BD) ---
public class Reservation
{
    public int id { get; set; }
    public string room_name { get; set; }
    public string checkin { get; set; }
    public string checkout { get; set; }
    public string total { get; set; }
}

// DTO que refleja join con rooms (ajusta si no haces join)
public class ReservationDto
{
    public int id { get; set; }
    public string checkin { get; set; }
    public string checkout { get; set; }
    public string total { get; set; }
    public dynamic rooms { get; set; }
}
