using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using HotelParador.Models;

namespace HotelParador;


public partial class Registro : ContentPage
{
    private string url = "https://mduluguemexwsgkpnavb.supabase.co/rest/v1/users";
    private string AApikey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1kdWx1Z3VlbWV4d3Nna3BuYXZiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMxNDk1MTQsImV4cCI6MjA3ODcyNTUxNH0.ndTochHhui6-4eQSNqhkNMgzDQf-ZbP8eb_gNRsoxUg";

    HttpClient cliente = new HttpClient();

    public Registro()
    {
        InitializeComponent();
        BtnRegistrarse.Clicked += BtnRegistrarse_Clicked;
    }

    private async void BtnRegistrarse_Clicked(object sender, EventArgs e)
    {
        var nuevoUsuario = new User
        {
            username = nombre.Text,
            email = usr.Text,
            password = pwd.Text
        };

        var jsonData = JsonConvert.SerializeObject(nuevoUsuario);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        cliente.DefaultRequestHeaders.Clear();
        cliente.DefaultRequestHeaders.Add("apikey", AApikey);
        cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {AApikey}");

        var response = await cliente.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("Éxito", "El usuario fue registrado", "OK");
            await Navigation.PushAsync(new Sesion());
        }
        else
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Error", errorMsg, "OK");
        }

    }
}
