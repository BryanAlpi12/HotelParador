// mapping después de recibir _availableRoomsDynamic
var roomsDynamic = await _reservationService.GetAvailableRoomsAsync(); // returns List<dynamic>
_availableRooms = new List<RoomDto>();
foreach (var r in roomsDynamic)
{
    string guestsRaw = null;
    try { guestsRaw = r.guests?.ToString(); } catch { guestsRaw = null; }

    int maxGuests = ParseMaxGuestsToken(guestsRaw);

    _availableRooms.Add(new RoomDto
    {
        Id = r.id != null ? (int)r.id : 0,
        Name = r.name != null ? r.name.ToString() : "Habitación",
        Description = r.description != null ? r.description.ToString() : string.Empty,
        Price = r.price != null ? (decimal)r.price : 0m,
        ImageUrl = r.image_url != null ? r.image_url.ToString() : string.Empty,
        GuestsRaw = guestsRaw ?? maxGuests.ToString(),
        MaxGuests = Math.Max(1, maxGuests)
    });
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