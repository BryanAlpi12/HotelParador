using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HotelParador.Services
{
    public class ReservationService
    {
        // Cambia aquí por tus valores reales (no los subas al repo)
        private readonly string baseUrl = Environment.GetEnvironmentVariable("HOTELPARADOR_SUPABASE_URL")
            ?? "https://mduluguemexwsgkpnavb.supabase.co/rest/v1";
        private readonly string apiKey = Environment.GetEnvironmentVariable("HOTELPARADOR_SUPABASE_KEY")
            ?? "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1kdWx1Z3VlbWV4d3Nna3BuYXZiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjMxNDk1MTQsImV4cCI6MjA3ODcyNTUxNH0.ndTochHhui6-4eQSNqhkNMgzDQf-ZbP8eb_gNRsoxUg";

        // Reusar HttpClient
        private static readonly HttpClient cliente = new HttpClient();

        // Modelo inline para Reservaciones con Habitación
        public class ReservationWithRoom
        {
            public int id { get; set; }
            public int user_id { get; set; }
            public int room_id { get; set; }
            public int? room_inventory_id { get; set; }
            public string room_number { get; set; }
            public DateTime checkin { get; set; }
            public DateTime checkout { get; set; }
            public decimal total { get; set; }
            public DateTime created_at { get; set; }

            // Datos de la habitación
            public string room_name { get; set; }
            public string room_description { get; set; }
            public decimal room_price { get; set; }
            public string room_image_url { get; set; }
            public int room_guests { get; set; }

            public int NumberOfNights => (checkout - checkin).Days;
        }

        // ✅ MÉTODO CORREGIDO - Obtener reservaciones del usuario con datos de habitación
        public async Task<List<ReservationWithRoom>> GetUserReservationsAsync(string userEmail)
        {
            try
            {
                Debug.WriteLine($"[GetUserReservations] Iniciando para email: {userEmail}");

                // 1. Obtener user_id usando el email
                string userUrl = $"{baseUrl}/users?email=eq.{userEmail}";

                cliente.DefaultRequestHeaders.Clear();
                cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var userResponse = await cliente.GetAsync(userUrl);
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<dynamic>>(userJson);

                if (users == null || users.Count == 0)
                {
                    Debug.WriteLine("[GetUserReservations] Usuario no encontrado");
                    return new List<ReservationWithRoom>();
                }

                // ✅ Protección contra null en user_id
                int userId = users[0].id != null ? (int)users[0].id : 0;
                Debug.WriteLine($"[GetUserReservations] User ID encontrado: {userId}");

                // 2. Obtener reservaciones del usuario
                string reservationsUrl = $"{baseUrl}/reservations?user_id=eq.{userId}&order=created_at.desc";

                var reservationsResponse = await cliente.GetAsync(reservationsUrl);
                var reservationsJson = await reservationsResponse.Content.ReadAsStringAsync();
                var reservations = JsonConvert.DeserializeObject<List<dynamic>>(reservationsJson);

                if (reservations == null || reservations.Count == 0)
                {
                    Debug.WriteLine("[GetUserReservations] No hay reservaciones");
                    return new List<ReservationWithRoom>();
                }

                Debug.WriteLine($"[GetUserReservations] {reservations.Count} reservaciones encontradas");

                // 3. Obtener todas las habitaciones (tipos)
                string roomsUrl = $"{baseUrl}/rooms";
                var roomsResponse = await cliente.GetAsync(roomsUrl);
                var roomsJson = await roomsResponse.Content.ReadAsStringAsync();
                var rooms = JsonConvert.DeserializeObject<List<dynamic>>(roomsJson);

                // 4. Obtener inventario de habitaciones (números específicos)
                string inventoryUrl = $"{baseUrl}/room_inventory";
                var inventoryResponse = await cliente.GetAsync(inventoryUrl);
                var inventoryJson = await inventoryResponse.Content.ReadAsStringAsync();
                var inventory = JsonConvert.DeserializeObject<List<dynamic>>(inventoryJson);

                // 5. Hacer JOIN manual con protección contra nulls
                var reservationsWithRooms = new List<ReservationWithRoom>();

                foreach (var reservation in reservations)
                {
                    try
                    {
                        // ✅ Extraer y validar campos de la reservación
                        int reservationId = reservation.id != null ? (int)reservation.id : 0;
                        int resUserId = reservation.user_id != null ? (int)reservation.user_id : 0;
                        int roomId = reservation.room_id != null ? (int)reservation.room_id : 0;

                        int? roomInventoryId = null;
                        if (reservation.room_inventory_id != null)
                        {
                            roomInventoryId = (int)reservation.room_inventory_id;
                        }

                        // Buscar el tipo de habitación
                        dynamic room = null;
                        if (rooms != null)
                        {
                            foreach (var r in rooms)
                            {
                                int rId = r.id != null ? (int)r.id : 0;
                                if (rId == roomId)
                                {
                                    room = r;
                                    break;
                                }
                            }
                        }

                        // Buscar la habitación física específica
                        dynamic roomInventory = null;
                        if (roomInventoryId.HasValue && inventory != null)
                        {
                            foreach (var inv in inventory)
                            {
                                int invId = inv.id != null ? (int)inv.id : 0;
                                if (invId == roomInventoryId.Value)
                                {
                                    roomInventory = inv;
                                    break;
                                }
                            }
                        }

                        // ✅ Extraer room_guests con protección completa contra nulls
                        int roomGuests = 2; // Default
                        if (room != null)
                        {
                            // Intentar obtener de 'guests' primero
                            if (room.guests != null)
                            {
                                if (room.guests is int gInt)
                                    roomGuests = gInt;
                                else if (room.guests is long gLong)
                                    roomGuests = (int)gLong;
                                else if (room.guests is decimal gDec)
                                    roomGuests = (int)gDec;
                                else
                                    int.TryParse(room.guests.ToString(), out roomGuests);
                            }
                            // Si no existe 'guests', intentar con 'max_guests'
                            else if (room.max_guests != null)
                            {
                                if (room.max_guests is int mgInt)
                                    roomGuests = mgInt;
                                else if (room.max_guests is long mgLong)
                                    roomGuests = (int)mgLong;
                                else if (room.max_guests is decimal mgDec)
                                    roomGuests = (int)mgDec;
                                else
                                    int.TryParse(room.max_guests.ToString(), out roomGuests);
                            }
                        }

                        // ✅ Extraer precio con protección contra nulls
                        decimal roomPrice = 0m;
                        if (room?.price != null)
                        {
                            if (room.price is decimal priceDecimal)
                                roomPrice = priceDecimal;
                            else if (room.price is double priceDouble)
                                roomPrice = (decimal)priceDouble;
                            else if (room.price is int priceInt)
                                roomPrice = priceInt;
                            else
                                decimal.TryParse(room.price.ToString(), out roomPrice);
                        }

                        // ✅ Extraer total con protección contra nulls
                        decimal total = 0m;
                        if (reservation.total != null)
                        {
                            if (reservation.total is decimal totalDecimal)
                                total = totalDecimal;
                            else if (reservation.total is double totalDouble)
                                total = (decimal)totalDouble;
                            else if (reservation.total is int totalInt)
                                total = totalInt;
                            else
                                decimal.TryParse(reservation.total.ToString(), out total);
                        }

                        // ✅ Crear objeto con todos los campos protegidos
                        reservationsWithRooms.Add(new ReservationWithRoom
                        {
                            id = reservationId,
                            user_id = resUserId,
                            room_id = roomId,
                            room_inventory_id = roomInventoryId,
                            room_number = roomInventory?.room_number ?? "No asignada",
                            checkin = DateTime.Parse(reservation.checkin.ToString()),
                            checkout = DateTime.Parse(reservation.checkout.ToString()),
                            total = total,
                            created_at = DateTime.Parse(reservation.created_at.ToString()),
                            room_name = room?.name ?? "Habitación no encontrada",
                            room_description = room?.description ?? "",
                            room_price = roomPrice,
                            room_image_url = room?.image_url ?? "",
                            room_guests = Math.Max(1, roomGuests) // Asegurar mínimo 1
                        });

                        Debug.WriteLine($"[GetUserReservations] ✓ Reservación #{reservationId} mapeada correctamente");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[GetUserReservations] Error al mapear reservación: {ex.Message}");
                        // Continuar con la siguiente reservación
                    }
                }

                Debug.WriteLine($"[GetUserReservations] Retornando {reservationsWithRooms.Count} reservaciones con datos completos");
                return reservationsWithRooms;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetUserReservations] Error: {ex.Message}");
                Debug.WriteLine($"[GetUserReservations] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        // Crear nueva reservación CON ASIGNACIÓN AUTOMÁTICA DE HABITACIÓN
        public async Task<dynamic> CreateReservationAsync(
            string userEmail,
            int roomId,
            DateTime checkin,
            DateTime checkout)
        {
            try
            {
                Debug.WriteLine($"[CreateReservation] Iniciando para roomId: {roomId}");

                // 1. Obtener user_id
                string userUrl = $"{baseUrl}/users?email=eq.{userEmail}";

                cliente.DefaultRequestHeaders.Clear();
                cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var userResponse = await cliente.GetAsync(userUrl);
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<dynamic>>(userJson);

                if (users == null || users.Count == 0)
                {
                    throw new Exception("Usuario no encontrado");
                }

                int userId = (int)users[0].id;
                Debug.WriteLine($"[CreateReservation] User ID: {userId}");

                // 2. Obtener información del tipo de habitación
                string roomUrl = $"{baseUrl}/rooms?id=eq.{roomId}";
                var roomResponse = await cliente.GetAsync(roomUrl);
                var roomJson = await roomResponse.Content.ReadAsStringAsync();
                var rooms = JsonConvert.DeserializeObject<List<dynamic>>(roomJson);

                if (rooms == null || rooms.Count == 0)
                {
                    throw new Exception("Tipo de habitación no encontrado");
                }

                var room = rooms[0];
                Debug.WriteLine($"[CreateReservation] Tipo de habitación: {room.name}");

                // 3. BUSCAR UNA HABITACIÓN DISPONIBLE de este tipo
                string availableRoomUrl = $"{baseUrl}/room_inventory?room_id=eq.{roomId}&is_available=eq.true&limit=50";
                cliente.DefaultRequestHeaders.Clear();
                cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var availableResponse = await cliente.GetAsync(availableRoomUrl);
                var availableJson = await availableResponse.Content.ReadAsStringAsync();
                Debug.WriteLine($"[CreateReservation] availableRoomUrl Status: {availableResponse.StatusCode} Payload: {availableJson}");

                if (!availableResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Error consultando inventario: {availableResponse.StatusCode} - {availableJson}");
                }

                var availableRooms = JsonConvert.DeserializeObject<List<dynamic>>(availableJson) ?? new List<dynamic>();

                // Filtrar status "available" sin depender de mayúsculas
                var validAvailable = new List<dynamic>();
                foreach (var ar in availableRooms)
                {
                    string status = ar?.status != null ? ar.status.ToString() : null;
                    if (string.IsNullOrEmpty(status) || status.Equals("available", StringComparison.OrdinalIgnoreCase))
                    {
                        validAvailable.Add(ar);
                    }
                }

                if (validAvailable.Count == 0)
                {
                    throw new Exception($"No hay habitaciones disponibles del tipo {room.name}. Por favor, intenta con otra categoría.");
                }

                var selectedRoom = validAvailable[0];
                int roomInventoryId = (int)selectedRoom.id;
                string roomNumber = selectedRoom.room_number;
                int floor = selectedRoom.floor != null ? (int)selectedRoom.floor : 0;

                Debug.WriteLine($"[CreateReservation] Habitación asignada: {roomNumber} (Piso {floor})");

                // 4. Calcular el total
                int numberOfNights = (checkout - checkin).Days;
                decimal total = (decimal)room.price * numberOfNights;

                Debug.WriteLine($"[CreateReservation] Total calculado: ${total} ({numberOfNights} noches)");

                // 5. Crear la reservación con la habitación específica
                var newReservation = new
                {
                    user_id = userId,
                    room_id = roomId,
                    room_inventory_id = roomInventoryId,
                    checkin = checkin.ToString("yyyy-MM-dd"),
                    checkout = checkout.ToString("yyyy-MM-dd"),
                    total = total,
                    created_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                // 6. Insertar reservación en BD
                string insertUrl = $"{baseUrl}/reservations";

                cliente.DefaultRequestHeaders.Clear();
                cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                cliente.DefaultRequestHeaders.Add("Prefer", "return=representation");

                var jsonContent = JsonConvert.SerializeObject(newReservation);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await cliente.PostAsync(insertUrl, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[CreateReservation] Error HTTP: {response.StatusCode}");
                    Debug.WriteLine($"[CreateReservation] Response: {responseJson}");
                    throw new Exception($"Error al crear reservación: {responseJson}");
                }

                Debug.WriteLine($"[CreateReservation] Reservación creada exitosamente");

                // 7. MARCAR LA HABITACIÓN COMO OCUPADA
                await MarkRoomAsOccupied(roomInventoryId);

                Debug.WriteLine($"[CreateReservation] ✓ Proceso completado");
                Debug.WriteLine($"[CreateReservation] ✓ Habitación {roomNumber} (Piso {floor}) asignada y marcada como ocupada");

                var createdReservations = JsonConvert.DeserializeObject<List<dynamic>>(responseJson);
                var createdReservation = createdReservations?[0];

                // Agregar el número de habitación al objeto retornado
                if (createdReservation != null)
                {
                    createdReservation.room_number = roomNumber;
                    createdReservation.floor = floor;
                }

                return createdReservation;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CreateReservation] Error: {ex.Message}");
                Debug.WriteLine($"[CreateReservation] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        // Marcar habitación como ocupada en tabla room_inventory
        private async Task MarkRoomAsOccupied(int roomInventoryId)
        {
            try
            {
                string updateUrl = $"{baseUrl}/room_inventory?id=eq.{roomInventoryId}";

                cliente.DefaultRequestHeaders.Clear();
                cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var updateData = new
                {
                    is_available = false,
                    status = "occupied"
                };

                var jsonContent = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await cliente.PatchAsync(updateUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[MarkRoomAsOccupied] ✓ Habitación {roomInventoryId} marcada como ocupada");
                }
                else
                {
                    Debug.WriteLine($"[MarkRoomAsOccupied] ⚠ Advertencia: No se pudo actualizar el estado de la habitación {roomInventoryId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MarkRoomAsOccupied] Error: {ex.Message}");
            }
        }

        // Obtener tipos de habitaciones con cantidad disponible
        public async Task<List<dynamic>> GetAvailableRoomsAsync()
        {
            try
            {
                Debug.WriteLine("[GetAvailableRooms] Iniciando consulta");

                // 1. Obtener todos los tipos de habitaciones
                string roomsUrl = $"{baseUrl}/rooms";

                cliente.DefaultRequestHeaders.Clear();
                cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var roomsResponse = await cliente.GetAsync(roomsUrl);
                var roomsJson = await roomsResponse.Content.ReadAsStringAsync();
                Debug.WriteLine($"[GetAvailableRooms] roomsUrl Status: {roomsResponse.StatusCode} Payload: {roomsJson}");

                if (!roomsResponse.IsSuccessStatusCode)
                    throw new Exception($"GetAvailableRooms - Error HTTP rooms: {roomsResponse.StatusCode} - {roomsJson}");

                var rooms = JsonConvert.DeserializeObject<List<dynamic>>(roomsJson);
                if (rooms == null || rooms.Count == 0)
                {
                    Debug.WriteLine("[GetAvailableRooms] No se encontraron tipos de habitaciones");
                    return new List<dynamic>();
                }

                Debug.WriteLine($"[GetAvailableRooms] {rooms.Count} tipos de habitaciones encontrados");

                // 2. Para cada tipo, contar cuántas habitaciones disponibles hay
                var roomsWithAvailability = new List<dynamic>();

                foreach (var room in rooms)
                {
                    int roomId = (int)room.id;

                    // Contar habitaciones disponibles de este tipo
                    string countUrl = $"{baseUrl}/room_inventory?room_id=eq.{roomId}&is_available=eq.true&limit=50";
                    cliente.DefaultRequestHeaders.Clear();
                    cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                    cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var countResponse = await cliente.GetAsync(countUrl);
                    var countJson = await countResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[GetAvailableRooms] countUrl for room {roomId} Status: {countResponse.StatusCode} Payload: {countJson}");

                    if (!countResponse.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"[GetAvailableRooms] Warning: HTTP {countResponse.StatusCode} al consultar inventario para room_id={roomId}");
                        continue;
                    }

                    var availableRooms = JsonConvert.DeserializeObject<List<dynamic>>(countJson) ?? new List<dynamic>();

                    // Filtrar en código por status (case-insensitive)
                    int availableCount = 0;
                    foreach (var ar in availableRooms)
                    {
                        string status = ar?.status != null ? ar.status.ToString() : null;
                        if (string.IsNullOrEmpty(status) || status.Equals("available", StringComparison.OrdinalIgnoreCase))
                        {
                            availableCount++;
                        }
                    }

                    // Solo mostrar tipos que tengan habitaciones disponibles
                    if (availableCount > 0)
                    {
                        Debug.WriteLine($"[GetAvailableRooms] ✓ {room.name}: {availableCount} habitaciones disponibles");
                        roomsWithAvailability.Add(room);
                    }
                    else
                    {
                        Debug.WriteLine($"[GetAvailableRooms] ✗ {room.name}: Sin habitaciones disponibles");
                    }
                }

                if (roomsWithAvailability.Count == 0)
                {
                    Debug.WriteLine("[GetAvailableRooms] ⚠ Advertencia: No hay habitaciones disponibles en ninguna categoría");
                }
                else
                {
                    Debug.WriteLine($"[GetAvailableRooms] Retornando {roomsWithAvailability.Count} tipos con disponibilidad");
                }

                return roomsWithAvailability;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetAvailableRooms] Error: {ex.Message}");
                Debug.WriteLine($"[GetAvailableRooms] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        // Cancelar reservación y liberar habitación
        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            try
            {
                Debug.WriteLine($"[CancelReservation] Cancelando reservación ID: {reservationId}");

                // 1. Obtener la reservación para saber qué habitación liberar
                string reservationUrl = $"{baseUrl}/reservations?id=eq.{reservationId}";

                cliente.DefaultRequestHeaders.Clear();
                cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await cliente.GetAsync(reservationUrl);
                var json = await response.Content.ReadAsStringAsync();
                var reservations = JsonConvert.DeserializeObject<List<dynamic>>(json);

                if (reservations == null || reservations.Count == 0)
                {
                    Debug.WriteLine("[CancelReservation] Reservación no encontrada");
                    return false;
                }

                var reservation = reservations[0];

                // 2. Si tiene habitación asignada, liberarla
                if (reservation.room_inventory_id != null)
                {
                    int roomInventoryId = (int)reservation.room_inventory_id;
                    await ReleaseRoom(roomInventoryId);
                }

                // 3. Eliminar la reservación
                string deleteUrl = $"{baseUrl}/reservations?id=eq.{reservationId}";
                await cliente.DeleteAsync(deleteUrl);

                Debug.WriteLine($"[CancelReservation] ✓ Reservación {reservationId} cancelada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CancelReservation] Error: {ex.Message}");
                return false;
            }
        }

        // Liberar habitación (marcar como disponible)
        private async Task ReleaseRoom(int roomInventoryId)
        {
            try
            {
                string updateUrl = $"{baseUrl}/room_inventory?id=eq.{roomInventoryId}";

                cliente.DefaultRequestHeaders.Clear();
                cliente.DefaultRequestHeaders.Add("apikey", apiKey);
                cliente.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var updateData = new
                {
                    is_available = true,
                    status = "available"
                };

                var jsonContent = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                await cliente.PatchAsync(updateUrl, content);
                Debug.WriteLine($"[ReleaseRoom] ✓ Habitación {roomInventoryId} liberada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ReleaseRoom] Error: {ex.Message}");
            }
        }
    }
}