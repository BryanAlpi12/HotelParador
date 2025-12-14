using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.ObjectModel;
using HotelParador.Services;
using CommunityToolkit.Mvvm.Messaging;


namespace HotelParador;

[QueryProperty(nameof(Email), "email")]
public partial class ProfilePage : ContentPage
{
    private string supabaseUrl = "https://mduluguemexwsgkpnavb.supabase.co/rest/v1";
    private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1kdWx1Z3VlbWV4d3Nna3BuYXZiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMxNDk1MTQsImV4cCI6MjA3ODcyNTUxNH0.ndTochHhui6-4eQSNqhkNMgzDQf-ZbP8eb_gNRsoxUg";

    private HttpClient cliente = new HttpClient();
    public string Email { get; set; }

    // Colección dinámica para las reservas
    public ObservableCollection<dynamic> Reservas { get; set; } = new ObservableCollection<dynamic>();

    public ProfilePage()
    {
        InitializeComponent();
        BindingContext = this; // IMPORTANTE
        CvReservas.ItemsSource = Reservas;
        BtnEditar.Clicked += BtnEditar_Clicked;
        BtnLogout.Clicked += BtnLogout_Clicked;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Registrar la recepción del mensaje de cancelación
        WeakReferenceMessenger.Default.Register<ReservaCanceladaMessage>(this, (r, message) =>
        {
            System.Diagnostics.Debug.WriteLine($"[ProfilePage] Reserva cancelada: {message.ReservaId}");
            // Recargar las reservas para reflejar la cancelación
            _ = LoadReservationsAsync(Email);
        });

        // Cargar reservas al aparecer
        if (!string.IsNullOrEmpty(Email))
        {
            _ = LoadUserAsync(Email);
            _ = LoadReservationsAsync(Email);
        }
    
        else
        {
            DisplayAlert("Debug", "El Email está vacío o nulo", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Desregistrar el mensaje para evitar que se duplique la suscripción
        WeakReferenceMessenger.Default.Unregister<ReservaCanceladaMessage>(this);
    }

    async Task LoadUserAsync(string email)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== LoadUserAsync iniciado para: {email} ===");

            cliente.DefaultRequestHeaders.Clear();
            cliente.DefaultRequestHeaders.Add("apikey", supabaseKey);
            cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            var url = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(email)}&select=*";
            System.Diagnostics.Debug.WriteLine($"URL: {url}");

            var resp = await cliente.GetAsync(url);
            var js = await resp.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Status: {resp.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Response: {js}");

            if (!resp.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", $"Error al cargar usuario:\n{resp.StatusCode}\n{js}", "OK");
                return;
            }

            var users = JsonConvert.DeserializeObject<List<dynamic>>(js);

            if (users != null && users.Count > 0)
            {
                var user = users[0];
                string username = user.username?.ToString() ?? "Sin nombre";
                string userEmail = user.email?.ToString() ?? "Sin email";

                System.Diagnostics.Debug.WriteLine($"Usuario encontrado: {username} - {userEmail}");

                // Actualizar UI en el thread principal
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LblNombre.Text = username;
                    LblEmail.Text = userEmail;
                });
            }
            else
            {
                await DisplayAlert("Info", "No se encontró el usuario en la base de datos", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR en LoadUserAsync: {ex.Message}\n{ex.StackTrace}");
            await DisplayAlert("Error", $"LoadUserAsync:\n{ex.Message}", "OK");
        }
    }

    async Task LoadReservationsAsync(string email)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== LoadReservationsAsync iniciado para: {email} ===");

            cliente.DefaultRequestHeaders.Clear();
            cliente.DefaultRequestHeaders.Add("apikey", supabaseKey);
            cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            // 1. Obtener user_id
            var userUrl = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(email)}&select=id";
            System.Diagnostics.Debug.WriteLine($"URL User ID: {userUrl}");

            var rUser = await cliente.GetAsync(userUrl);
            var usersJson = await rUser.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"User ID Response: {usersJson}");

            var users = JsonConvert.DeserializeObject<List<dynamic>>(usersJson);

            if (users == null || users.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No se encontró el usuario para las reservas");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                   

                });
                return;
            }

            var userId = users[0].id.ToString();
            System.Diagnostics.Debug.WriteLine($"User ID obtenido: {userId}");

            // 2. Obtener reservas con la información de rooms
            var resUrl = $"{supabaseUrl}/reservations?user_id=eq.{userId}&select=*,rooms(*)";
            System.Diagnostics.Debug.WriteLine($"URL Reservas: {resUrl}");

            var rRes = await cliente.GetAsync(resUrl);
            var resJson = await rRes.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Reservas Status: {rRes.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Reservas Response: {resJson}");

            if (!rRes.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", $"Error al cargar reservas:\n{rRes.StatusCode}\n{resJson}", "OK");
                return;
            }

            var resList = JsonConvert.DeserializeObject<List<dynamic>>(resJson);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Reservas.Clear();
            });

            List<string> roomNames = new List<string>();

            if (resList != null && resList.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Se encontraron {resList.Count} reservas");

                foreach (var reserva in resList)
                {
                    // Obtener el nombre de la habitación
                    string roomName = "Habitación";
                    try
                    {
                        if (reserva.rooms != null && reserva.rooms.Name != null)
                        {
                            roomName = reserva.rooms.Name.ToString();
                        }
                    }
                    catch
                    {
                        // Si falla, usa el valor por defecto
                    }

                    roomNames.Add(roomName);

                    System.Diagnostics.Debug.WriteLine($"Reserva: {roomName} - {reserva.checkin} - {reserva.checkout} - {reserva.total}");

                    // Crear objeto anónimo para el binding
                    var reservaObj = new
                    {
                        id = reserva.id?.ToString() ?? "0",
                        room_name = roomName,
                        checkin = reserva.checkin?.ToString() ?? "-",
                        checkout = reserva.checkout?.ToString() ?? "-",
                        total = reserva.total?.ToString() ?? "0"
                    };

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Reservas.Add(reservaObj);
                    });
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                   
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No se encontraron reservas");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                  
                });
                await DisplayAlert("Info", "No tienes reservas registradas", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR en LoadReservationsAsync: {ex.Message}\n{ex.StackTrace}");
            await DisplayAlert("Error", $"LoadReservationsAsync:\n{ex.Message}", "OK");
        }
    }

    private async void BtnEditar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(Email))
        {
            await DisplayAlert("Error", "No hay email disponible para editar", "OK");
            return;
        }
        await Shell.Current.GoToAsync($"{nameof(ProfilePageEdit)}?email={Uri.EscapeDataString(Email)}");
    }

    private async void BtnLogout_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///MainPage");
    }
}