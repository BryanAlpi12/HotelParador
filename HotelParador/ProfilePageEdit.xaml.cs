using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using HotelParador.Models;

namespace HotelParador;

[QueryProperty(nameof(Email), "email")]
public partial class ProfilePageEdit : ContentPage
{
    private readonly string supabaseUrl = "https://mduluguemexwsgkpnavb.supabase.co/rest/v1";
    private readonly string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1kdWx1Z3VlbWV4d3Nna3BuYXZiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMxNDk1MTQsImV4cCI6MjA3ODcyNTUxNH0.ndTochHhui6-4eQSNqhkNMgzDQf-ZbP8eb_gNRsoxUg";
    private readonly HttpClient cliente = new HttpClient();

    private bool _hasLoaded = false; // Para evitar cargar varias veces

    public string Email { get; set; }

    public ProfilePageEdit()
    {
        InitializeComponent();
        BtnGuardar.IsEnabled = false; // deshabilitar hasta cargar usuario
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoaded || string.IsNullOrWhiteSpace(Email))
            return;

        _hasLoaded = true;
        await LoadUserAsync(Email);
    }

    private async Task LoadUserAsync(string email)
    {
        try
        {
            var url = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(email)}&select=*";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.TryAddWithoutValidation("apikey", supabaseKey);
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {supabaseKey}");

            var resp = await cliente.SendAsync(request);
            var js = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                await DisplayAlert("Error al cargar usuario", js, "OK");
                return;
            }

            var users = JsonConvert.DeserializeObject<List<User>>(js);
            var user = users?.FirstOrDefault();

            if (user != null)
            {
                EntryNombre.Text = user.username;
                EntryEmail.Text = user.email;
                BtnGuardar.IsEnabled = true;
            }
            else
            {
                await DisplayAlert("Aviso", "Usuario no encontrado", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void BtnGuardar_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await DisplayAlert("Error", "No se puede actualizar: email vacío", "OK");
                return;
            }

            var data = new { username = EntryNombre.Text ?? "" };
            var json = JsonConvert.SerializeObject(data);

            var url = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(Email)}";
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);

            request.Headers.TryAddWithoutValidation("apikey", supabaseKey);
            request.Headers.TryAddWithoutValidation("Prefer", "return=representation");

            request.Content = new StringContent(json, Encoding.UTF8);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resp = await cliente.SendAsync(request);
            var respContent = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Perfil actualizado", "OK");
                await Shell.Current.GoToAsync(".."); // vuelve a la página anterior
            }
            else
            {
                await DisplayAlert("Error al actualizar", respContent, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
