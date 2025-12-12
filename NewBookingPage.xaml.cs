private int ParseMaxGuestsToken(string guestsRaw)
{
    if (string.IsNullOrWhiteSpace(guestsRaw)) return 1;
    if (int.TryParse(guestsRaw.Trim(), out var v)) return Math.Max(1, v);

    var matches = System.Text.RegularExpressions.Regex.Matches(guestsRaw, @"\d+");
    var max = 1;
    foreach (System.Text.RegularExpressions.Match m in matches)
    {
        if (int.TryParse(m.Value, out var n) && n > max) max = n;
    }
    return Math.Max(1, max);
}

private int ParseMaxGuestsToken(string guestsRaw)
{
    if (string.IsNullOrWhiteSpace(guestsRaw)) return 1;
    if (int.TryParse(guestsRaw.Trim(), out var v)) return Math.Max(1, v);

    var matches = System.Text.RegularExpressions.Regex.Matches(guestsRaw, @"\d+");
    var max = 1;
    foreach (System.Text.RegularExpressions.Match m in matches)
    {
        if (int.TryParse(m.Value, out var n) && n > max) max = n;
    }
    return Math.Max(1, max);
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

        // Obtener habitaciones desde el servicio (puede devolver List<RoomDto> o List<dynamic>)
        var roomsResult = await _reservationService.GetAvailableRoomsAsync();

        if (roomsResult == null)
        {
            await DisplayAlert("Info", "No hay habitaciones disponibles", "OK");
            RoomPicker.ItemsSource = new List<string>();
            RoomPicker.IsEnabled = false;
            return;
        }

        // Si el servicio ya devuelve DTOs, asignar directamente
        if (roomsResult is List<RoomDto> dtoList)
        {
            _availableRooms = dtoList;
        }
        else
        {
            // Mapear dinámico -> RoomDto
            _availableRooms = new List<RoomDto>();
            try
            {
                foreach (var r in (IEnumerable<dynamic>)roomsResult)
                {
                    string guestsRaw = null;
                    try { guestsRaw = r.guests?.ToString(); } catch { guestsRaw = null; }

                    int maxGuests = ParseMaxGuestsToken(guestsRaw);

                    decimal price = 0m;
                    try
                    {
                        if (r.price != null) price = (decimal)r.price;
                    }
                    catch
                    {
                        decimal.TryParse(r?.price?.ToString() ?? "0", out price);
                    }

                    var roomDto = new RoomDto
                    {
                        Id = r.id != null ? (int)r.id : 0,
                        Name = r.name != null ? r.name.ToString() : "Habitación",
                        Description = r.description != null ? r.description.ToString() : string.Empty,
                        Price = price,
                        ImageUrl = r.image_url != null ? r.image_url.ToString() : string.Empty,
                        GuestsRaw = guestsRaw ?? maxGuests.ToString(),
                        MaxGuests = Math.Max(1, maxGuests)
                    };

                    _availableRooms.Add(roomDto);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadRoomsFromDatabase] Error mapping rooms: {ex.Message}");
            }
        }

        if (_availableRooms == null || _availableRooms.Count == 0)
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

    // Protección adicional: comprobar rango antes de indexar
    if (selectedIndex == -1 || _availableRooms == null || selectedIndex >= _availableRooms.Count)
    {
        RoomPreview.IsVisible = false;
        _selectedRoom = null;
        // Elimina cualquier referencia a _selectedRoomMaxGuests (no existe)
        CheckFormValidity();
        return;
    }

    _selectedRoom = _availableRooms[selectedIndex];

    RoomPreview.IsVisible = true;
    RoomPreviewName.Text = _selectedRoom.Name;
    RoomPreviewImage.Source = string.IsNullOrEmpty(_selectedRoom.ImageUrl)
        ? "default_room.jpg"
        : _selectedRoom.ImageUrl;

    // Mostrar texto original si existe, y usar MaxGuests para validaciones
    RoomCapacity.Text = $"{(_selectedRoom.GuestsRaw ?? _selectedRoom.MaxGuests.ToString())} huéspedes";

    if (_numberOfGuests > _selectedRoom.MaxGuests)
    {
        _numberOfGuests = _selectedRoom.MaxGuests;
        GuestsLabel.Text = _numberOfGuests.ToString();
    }

    CalculatePrices();
    CheckFormValidity();
}