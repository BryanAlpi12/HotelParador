namespace HotelParador.Services
{
    public class ReservaCanceladaMessage
    {
        public string ReservaId { get; set; } = "";

        public ReservaCanceladaMessage(string reservaId)
        {
            ReservaId = reservaId;
        }
    }
}
