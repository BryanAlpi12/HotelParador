using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelParador.Models
{
    public class Reservation
    {
        public long id { get; set; }                 // ID de la reserva
        public int user_id { get; set; }             // ID del usuario
        public long? room_id { get; set; }           // ID de la habitación (puede ser null)
        public DateTime? checkin { get; set; }       // Fecha de entrada
        public DateTime? checkout { get; set; }      // Fecha de salida
        public decimal? total { get; set; }          // Total de la reserva
        public DateTime? created_at { get; set; }    // Fecha de creación
        public int? room_inventory_id { get; set; }  // ID de inventario de habitación (puede ser null)

        // Propiedades opcionales para la UI
        public string room_name { get; set; }        // Nombre de la habitación
    }

    // DTO opcional si quieres mapear la reserva junto con la info de la habitación
    public class ReservationDto
    {
        public long id { get; set; }
        public DateTime? checkin { get; set; }
        public DateTime? checkout { get; set; }
        public decimal? total { get; set; }
        public RoomDto rooms { get; set; }            // Información de la habitación
    }
}