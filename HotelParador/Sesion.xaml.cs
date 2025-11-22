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

    public Sesion()
    {
        InitializeComponent();
        BtnIr.Clicked += BtnIr_Clicked;
        BtnRegistro.Clicked += BtnRegistro_Clicked;
    }

    private async void BtnIr_Clicked(object sender, EventArgs e)
    {
        var email = usr.Text;
        var password = pwd.Text;

        // FILTRO EXACTO DE SUPABASE REST
        string fullUrl = $"{url}?email=eq.{email}&password=eq.{password}";

        cliente.DefaultRequestHeaders.Clear();
        cliente.DefaultRequestHeaders.Add("apikey", AApikey);
        cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {AApikey}");

        var response = await cliente.GetAsync(fullUrl);
        var json = await response.Content.ReadAsStringAsync();

        var usuarios = JsonConvert.DeserializeObject<List<User>>(json);

        if (usuarios != null && usuarios.Count > 0)
        {
            var usuario = usuarios[0];
            App.UserEmail = usuario.email;
            await DisplayAlert("Bienvenido", "Sesión iniciada", "OK");
            await Navigation.PushAsync(new Menu(usuario.username));
        }
        else
        {
            await DisplayAlert("Error", "Credenciales incorrectas", "OK");
        }
    }

    private async void BtnRegistro_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Registro());
    }
}
