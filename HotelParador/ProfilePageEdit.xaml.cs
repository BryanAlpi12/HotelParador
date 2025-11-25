using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using HotelParador.Models;
using System.Collections.ObjectModel;

namespace HotelParador;

[QueryProperty(nameof(Email), "email")]
public partial class ProfilePageEdit : ContentPage
{
    private string supabaseUrl = "https://mduluguemexwsgkpnavb.supabase.co/rest/v1";
    private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."; 
    HttpClient cliente = new HttpClient();

    public string Email { get; set; }

    public ProfilePageEdit()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(Email))
        {
            await LoadUserAsync(Email);
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

            var url = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(email)}&select=*";
            var resp = await cliente.GetAsync(url);
            var js = await resp.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(js);
            var list = JsonConvert.DeserializeObject<List<User>>(js);
            if (list != null && list.Count > 0)
            {
                var user = list[0];
                EntryNombre.Text = user.username;
                EntryEmail.Text = user.email;
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
            var nombre = EntryNombre.Text ?? "";
            var data = new { username = nombre };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var url = $"{supabaseUrl}/users?email=eq.{Uri.EscapeDataString(Email)}";
            cliente.DefaultRequestHeaders.Clear();
            cliente.DefaultRequestHeaders.Add("apikey", supabaseKey);
            cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            var resp = await cliente.PatchAsync(url, content);
            if (resp.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Perfil actualizado", "OK");
                await Shell.Current.GoToAsync(".."); 
            }
            else
            {
                await DisplayAlert("Error", "No se pudo actualizar", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
