using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelParador.Services;
using HotelParador.Models;

namespace HotelParador
{
    public partial class NewBookingPage : ContentPage
    {
        private readonly ReservationService _reservationService;
        private List<RoomDto> _availableRooms;
        private RoomDto _selectedRoom;
        private int _numberOfGuests = 2;

        public NewBookingPage()
        {
            InitializeComponent();
            _reservationService = new ReservationService();

            // Evitar interacción hasta que las habitaciones se carguen
            RoomPicker.IsEnabled = false;
            RoomsLoadingIndicator.IsVisible = false;
            RoomsLoadingIndicator.IsRunning = false;
            RoomsLoadingLabel.IsVisible = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializeFormAsync();
        }

        private async Task InitializeFormAsync()
        {
            try
            {
                // Configurar fechas mínimas
                var tomorrow = DateTime.Now.AddDays(1);
                CheckInPicker.MinimumDate = tomorrow;
                CheckInPicker.Date = tomorrow;

                CheckOutPicker.MinimumDate = tomorrow.AddDays(1);
                CheckOutPicker.Date = tomorrow.AddDays(1);

                // Cargar habitaciones
                await LoadRoomsFromDatabase();

                // Estado inicial del formulario
                CheckFormValidity();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al inicializar: {ex.Message}", "OK");
            }
        }

        private async Task LoadRoomsFromDatabase()
        {
            try
            {
                // Mostrar loader y deshabilitar picker
                RoomsLoadingIndicator.IsVisible = true;
                RoomsLoadingIndicator.IsRunning = true;
                RoomsLoadingLabel.IsVisible = true;
                RoomPicker.IsEnabled = false;
                RoomPicker.ItemsSource = new List<string>();

                // Obtener habitaciones desde el servicio (devuelve List<dynamic>)
                var roomsResult = await _reservationService.GetAvailableRoomsAsync();

                if (roomsResult == null || roomsResult.Count == 0)
                {
                    await DisplayAlert("Info", "No hay habitaciones disponibles", "OK");
                    RoomPicker.ItemsSource = new List<string>();
                    RoomPicker.IsEnabled = false;
                    return;
                }

                // Mapear dynamic -> RoomDto
                _availableRooms = new List<RoomDto>();
                foreach (var r in roomsResult)
                {
                    try
                    {
                        // Obtener max_guests (IMPORTANTE: usa max_guests, no guests)
                        int maxGuests = 2; // Default
                        string guestsRaw = null;

                        if (r.max_guests != null)
                        {
                            // Convertir a int (puede venir como decimal desde la BD)
                            if (r.max_guests is int intValue)
                            {
                                maxGuests = intValue;
                            }
                            else if (r.max_guests is long longValue)
                            {
                                maxGuests = (int)longValue;
                            }
                            else if (r.max_guests is decimal decValue)
                            {
                                maxGuests = (int)decValue;
                            }
                            else
                            {
                                // Intentar parsear como string
                                int.TryParse(r.max_guests.ToString(), out maxGuests);
                            }

                            guestsRaw = maxGuests.ToString();
                        }

                        // Obtener precio
                        decimal price = 0m;
                        if (r.price != null)
                        {
                            price = (decimal)r.price;
                        }

                        // Crear DTO
                        _availableRooms.Add(new RoomDto
                        {
                            Id = r.id != null ? (int)r.id : 0,
                            Name = r.name != null ? r.name.ToString() : "Habitación",
                            Description = r.description != null ? r.description.ToString() : string.Empty,
                            Price = price,
                            ImageUrl = r.image_url != null ? r.image_url.ToString() : string.Empty,
                            GuestsRaw = guestsRaw,
                            MaxGuests = Math.Max(1, maxGuests)
                        });

                        System.Diagnostics.Debug.WriteLine($"[LoadRooms] {r.name}: max_guests={maxGuests}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al mapear habitación: {ex.Message}");
                        // Continuar con la siguiente habitación
                    }
                }

                if (_availableRooms.Count == 0)
                {
                    await DisplayAlert("Info", "No hay habitaciones disponibles", "OK");
                    RoomPicker.ItemsSource = new List<string>();
                    RoomPicker.IsEnabled = false;
                    return;
                }

                // Llenar el Picker
                var roomNames = new List<string>();
                foreach (var room in _availableRooms)
                {
                    roomNames.Add($"{room.Name} - ${room.Price}/noche");
                }

                RoomPicker.ItemsSource = roomNames;
                RoomPicker.IsEnabled = true;

                System.Diagnostics.Debug.WriteLine($"[LoadRooms] {_availableRooms.Count} habitaciones cargadas exitosamente");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar habitaciones: {ex.Message}", "OK");
                RoomPicker.ItemsSource = new List<string>();
                RoomPicker.IsEnabled = false;
            }
            finally
            {
                RoomsLoadingIndicator.IsVisible = false;
                RoomsLoadingIndicator.IsRunning = false;
                RoomsLoadingLabel.IsVisible = false;
            }
        }

        private void OnRoomSelected(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var selectedIndex = picker.SelectedIndex;

            // Protección: comprobar rango antes de indexar
            if (selectedIndex == -1 || _availableRooms == null || selectedIndex >= _availableRooms.Count)
            {
                RoomPreview.IsVisible = false;
                _selectedRoom = null;
                CheckFormValidity();
                return;
            }

            _selectedRoom = _availableRooms[selectedIndex];

            // Mostrar preview de la habitación
            RoomPreview.IsVisible = true;
            RoomPreviewName.Text = _selectedRoom.Name;
            RoomPreviewImage.Source = string.IsNullOrEmpty(_selectedRoom.ImageUrl)
                ? "default_room.jpg"
                : _selectedRoom.ImageUrl;

            // Mostrar capacidad
            RoomCapacity.Text = !string.IsNullOrEmpty(_selectedRoom.GuestsRaw)
                ? $"{_selectedRoom.GuestsRaw} huéspedes"
                : $"{_selectedRoom.MaxGuests} huéspedes";

            // Ajustar número de huéspedes si excede el máximo
            if (_numberOfGuests > _selectedRoom.MaxGuests)
            {
                _numberOfGuests = _selectedRoom.MaxGuests;
                GuestsLabel.Text = _numberOfGuests.ToString();
            }

            CalculatePrices();
            CheckFormValidity();
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            if (CheckOutPicker.Date <= CheckInPicker.Date)
            {
                CheckOutPicker.Date = CheckInPicker.Date.AddDays(1);
            }

            CalculatePrices();
            CheckFormValidity();
        }

        private void OnIncreaseGuests(object sender, EventArgs e)
        {
            if (_selectedRoom == null) return;

            if (_numberOfGuests < _selectedRoom.MaxGuests)
            {
                _numberOfGuests++;
                GuestsLabel.Text = _numberOfGuests.ToString();
                MaxGuestsWarning.IsVisible = false;
            }
            else
            {
                string maxText = !string.IsNullOrEmpty(_selectedRoom.GuestsRaw)
                    ? _selectedRoom.GuestsRaw
                    : _selectedRoom.MaxGuests.ToString();

                MaxGuestsWarning.Text = $"Máximo {maxText} huéspedes para esta habitación";
                MaxGuestsWarning.IsVisible = true;
            }
        }

        private void OnDecreaseGuests(object sender, EventArgs e)
        {
            if (_numberOfGuests > 1)
            {
                _numberOfGuests--;
                GuestsLabel.Text = _numberOfGuests.ToString();
                MaxGuestsWarning.IsVisible = false;
            }
        }

        private void CalculatePrices()
        {
            if (_selectedRoom == null)
            {
                PriceSummary.IsVisible = false;
                return;
            }

            var nights = (CheckOutPicker.Date - CheckInPicker.Date).Days;
            if (nights < 1) nights = 1;

            NightsLabel.Text = $"{nights} {(nights == 1 ? "noche" : "noches")}";

            // Calcular precios
            decimal roomPrice = _selectedRoom.Price;
            decimal total = roomPrice * nights;
            decimal taxes = total * 0.13m;
            decimal serviceFee = 10.00m;
            decimal totalWithExtras = total + taxes + serviceFee;

            // Actualizar labels
            SubtotalDescLabel.Text = $"${roomPrice:N2} x {nights} {(nights == 1 ? "noche" : "noches")}";
            SubtotalLabel.Text = $"${total:F2}";
            TaxesLabel.Text = $"${taxes:F2}";
            ServiceFeeLabel.Text = $"${serviceFee:F2}";
            TotalLabel.Text = $"${totalWithExtras:F2}";

            PriceSummary.IsVisible = true;
        }

        private void CheckFormValidity()
        {
            var isValid = _selectedRoom != null && CheckOutPicker.Date > CheckInPicker.Date;

            ConfirmButton.IsEnabled = isValid;
            ConfirmButton.BackgroundColor = isValid
                ? Color.FromArgb("#6200EA")
                : Color.FromArgb("#CCCCCC");
        }

        private async void OnConfirmBookingClicked(object sender, EventArgs e)
        {
            try
            {
                if (_selectedRoom == null)
                {
                    await DisplayAlert("Error", "Selecciona una habitación", "OK");
                    return;
                }

                if (CheckOutPicker.Date <= CheckInPicker.Date)
                {
                    await DisplayAlert("Error", "La fecha de check-out debe ser después del check-in", "OK");
                    return;
                }

                var nights = (CheckOutPicker.Date - CheckInPicker.Date).Days;
                decimal total = _selectedRoom.Price * nights;

                var confirm = await DisplayAlert(
                    "Confirmar Reservación",
                    $"🏨 Hotel Parador\n" +
                    $"🛏️ {_selectedRoom.Name}\n" +
                    $"📅 {CheckInPicker.Date:dd/MM/yyyy} - {CheckOutPicker.Date:dd/MM/yyyy}\n" +
                    $"🌙 {nights} {(nights == 1 ? "noche" : "noches")}\n" +
                    $"💰 Total: ${total:N2} USD\n\n" +
                    $"¿Confirmar?",
                    "Sí",
                    "No"
                );

                if (!confirm) return;

                // Mostrar loading
                ConfirmButton.Text = "Procesando...";
                ConfirmButton.IsEnabled = false;

                string userEmail = App.UserEmail;

                if (string.IsNullOrEmpty(userEmail))
                {
                    await DisplayAlert("Error", "No hay usuario logueado", "OK");
                    ConfirmButton.Text = "Confirmar Reservación";
                    ConfirmButton.IsEnabled = true;
                    return;
                }

                // Crear reservación
                var reservation = await _reservationService.CreateReservationAsync(
                    userEmail,
                    _selectedRoom.Id,
                    CheckInPicker.Date,
                    CheckOutPicker.Date
                );

                ConfirmButton.Text = "Confirmar Reservación";
                ConfirmButton.IsEnabled = true;

                if (reservation != null)
                {
                    // Obtener el número de habitación si está disponible
                    string roomNumber = "";
                    string floorInfo = "";

                    try
                    {
                        if (reservation.room_number != null)
                        {
                            roomNumber = $"\n🚪 Habitación: {reservation.room_number}";

                            if (reservation.floor != null)
                            {
                                floorInfo = $" (Piso {reservation.floor})";
                            }
                        }
                    }
                    catch
                    {
                        // Si no hay room_number, continuar sin él
                    }

                    await DisplayAlert(
                        "¡Reservación Confirmada! ✓",
                        $"🎫 ID: #{reservation.id}\n" +
                        $"🏨 Habitación: {_selectedRoom.Name}" +
                        roomNumber + floorInfo + "\n\n" +
                        $"📅 Check-in: {CheckInPicker.Date:dd/MM/yyyy}\n" +
                        $"📅 Check-out: {CheckOutPicker.Date:dd/MM/yyyy}\n" +
                        $"💰 Total: ${total:N2} USD\n\n" +
                        $"¡Esperamos verte pronto!",
                        "OK"
                    );

                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo crear la reservación", "OK");
                }
            }
            catch (Exception ex)
            {
                ConfirmButton.Text = "Confirmar Reservación";
                ConfirmButton.IsEnabled = true;

                await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Cancelar",
                "¿Estás seguro? Se perderá la información.",
                "Sí",
                "No"
            );

            if (confirm)
            {
                await Navigation.PopAsync();
            }
        }

        // Helper para parsear el campo guests
        private int ParseMaxGuestsToken(string guestsRaw)
        {
            if (string.IsNullOrWhiteSpace(guestsRaw)) return 2;

            // Si es un número directo, usarlo
            if (int.TryParse(guestsRaw.Trim(), out var value))
            {
                return Math.Max(1, value);
            }

            // Si tiene formato como "2-4 guests", extraer el mayor número
            var matches = System.Text.RegularExpressions.Regex.Matches(guestsRaw, @"\d+");
            var max = 2;
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (int.TryParse(match.Value, out var num) && num > max)
                {
                    max = num;
                }
            }

            return Math.Max(1, max);
        }
    }
}