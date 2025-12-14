using System;

namespace HotelParador.Models
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        // Valor original recibido (ej. "1-4" o "2")
        public string GuestsRaw { get; set; } = "1";

        // Valor numérico normalizado que usarán la UI y la lógica
        public int MaxGuests { get; set; } = 1;
    }
}