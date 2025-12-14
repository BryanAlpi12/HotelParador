using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using HotelParador.Models;

namespace HotelParador;

public partial class Sesion : ContentPage
{
    private string url = "https://mduluguemexwsgkpnavb.supabase.co/rest/v1/users";
    private string AApikey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1kdWx1Z3VlbWV4d3Nna3BuYXZiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMxNDk1MTQsImV4cCI6MjA3ODcyNTUxNH0.ndTochHhui6-4eQSNqhkNMgzDQf-ZbP8eb_gNRsoxUg";
    HttpClient cliente = new HttpClient();

    // Control de intentos
    private int intentosFallidos = 0;
    private const int MaxIntentos = 3;
    private bool bloqueado = false;

    public Sesion()
    {
        InitializeComponent();
        BtnIr.Clicked += BtnIr_Clicked;
        if (BackgroundImage != null)
        {
            BackgroundImage.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsLoading")
                {
                    System.Diagnostics.Debug.WriteLine($"Imagen cargando: {BackgroundImage.IsLoading}");
                }
            };
        }
    }

    private async void OnRegistrarseClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Registro());
    }

    private async void BtnIr_Clicked(object sender, EventArgs e)
    {
        // Verificar si está bloqueado
        if (bloqueado)
        {
            await DisplayAlert("Bloqueado",
                "Demasiados intentos fallidos. Por favor espera un momento antes de intentar nuevamente.",
                "OK");
            return;
        }

        var email = usr_email.Text;
        var password = pwd.Text;

        // Validar que los campos no estén vacíos
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Por favor ingresa tu email y contraseña", "OK");
            return;
        }

        try
        {
            // FILTRO EXACTO DE SUPABASE REST
            string fullUrl = $"{url}?email=eq.{Uri.EscapeDataString(email)}&password=eq.{Uri.EscapeDataString(password)}";

            cliente.DefaultRequestHeaders.Clear();
            cliente.DefaultRequestHeaders.Add("apikey", AApikey);
            cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {AApikey}");

            var response = await cliente.GetAsync(fullUrl);
            var json = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Response: {json}");

            var usuarios = JsonConvert.DeserializeObject<List<User>>(json);

            if (usuarios != null && usuarios.Count > 0)
            {
                // ? Login exitoso - reiniciar contador
                intentosFallidos = 0;

                var usuario = usuarios[0];
                App.UserEmail = usuario.email;

                await DisplayAlert("Bienvenido", $"¡Hola {usuario.username}! Sesión iniciada correctamente", "OK");
                await Navigation.PushAsync(new Menu(usuario.username));
            }
            else
            {
                // ? Credenciales incorrectas
                intentosFallidos++;
                int intentosRestantes = MaxIntentos - intentosFallidos;

                if (intentosFallidos >= MaxIntentos)
                {
                    // Bloquear la cuenta
                    bloqueado = true;
                    BtnIr.IsEnabled = false;
                    BtnIr.Text = "Bloqueado...";
                    BtnIr.BackgroundColor = Colors.Gray;

                    // Deshabilitar campos de entrada
                    usr_email.IsEnabled = false;
                    pwd.IsEnabled = false;

                    await DisplayAlert("Cuenta Bloqueada",
                        "Has excedido el número máximo de intentos (3).\n\n" +
                        "Tu cuenta estará bloqueada por 30 segundos.",
                        "Entendido");

                    // Iniciar contador de desbloqueo
                    _ = IniciarDesbloqueo();
                }
                else
                {
                    // Aún hay intentos disponibles
                    await DisplayAlert("? Error de Inicio de Sesión",
                        $"Credenciales incorrectas.\n\n" +
                        $"Intentos restantes: {intentosRestantes} de {MaxIntentos}",
                        "Reintentar");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en login: {ex.Message}");
            await DisplayAlert("Error",
                "Ocurrió un error al intentar iniciar sesión. Por favor intenta nuevamente.",
                "OK");
        }
    }

    private async Task IniciarDesbloqueo()
    {
        int segundosRestantes = 30;

        // Actualizar el botón cada segundo con cuenta regresiva
        while (segundosRestantes > 0)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BtnIr.Text = $"Bloqueado ({segundosRestantes}s)";
            });

            await Task.Delay(1000); // Esperar 1 segundo
            segundosRestantes--;
        }

        // Desbloquear después de 30 segundos
        MainThread.BeginInvokeOnMainThread(() =>
        {
            bloqueado = false;
            intentosFallidos = 0;
            BtnIr.IsEnabled = true;
            BtnIr.Text = "Iniciar Sesión";
            BtnIr.BackgroundColor = Color.FromArgb("#D1A011"); // Tu color original

            // Rehabilitar campos
            usr_email.IsEnabled = true;
            pwd.IsEnabled = true;
        });

        await DisplayAlert("Cuenta Desbloqueada",
            "Puedes intentar iniciar sesión nuevamente.",
            "OK");
    }

    private async void BtnRegistro_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Registro());
    }
}