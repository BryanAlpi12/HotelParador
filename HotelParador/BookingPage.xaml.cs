using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using HotelParador.Services;
using static HotelParador.Services.ReservationService;
using CommunityToolkit.Mvvm.Messaging;
using HotelParador.Services;

namespace HotelParador
{
    public partial class BookingPage : ContentPage
    {
        private readonly ReservationService _reservationService;

        public BookingPage()
        {
            InitializeComponent();
            _reservationService = new ReservationService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Registrar la recepción del mensaje de cancelación
            WeakReferenceMessenger.Default.Register<ReservaCanceladaMessage>(this, (r, message) =>
            {
                System.Diagnostics.Debug.WriteLine($"[BookingPage] Reserva cancelada: {message.ReservaId}");
                // Recargar la lista de reservas para reflejar la cancelación
                LoadReservations();
            });

            // Cargar reservas al aparecer
            LoadReservations();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Desregistrar el mensaje para evitar duplicados
            WeakReferenceMessenger.Default.Unregister<ReservaCanceladaMessage>(this);
        }


        private async void LoadReservations()
        {
            try
            {
                // Mostrar loading
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                ReservationsScrollView.IsVisible = false;
                EmptyState.IsVisible = false;

                // Obtener email del usuario logueado (guardado en App.UserEmail)
                string userEmail = App.UserEmail;

                if (string.IsNullOrEmpty(userEmail))
                {
                    await DisplayAlert("Error", "No hay usuario logueado", "OK");
                    return;
                }

                // Obtener reservaciones del usuario desde Supabase
                var reservations = await _reservationService.GetUserReservationsAsync(userEmail);

                // Ocultar loading
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;

                // Si no hay reservaciones, mostrar estado vacío
                if (reservations == null || !reservations.Any())
                {
                    EmptyState.IsVisible = true;
                    return;
                }

                // Mostrar lista de reservaciones
                ReservationsScrollView.IsVisible = true;
                ReservationsContainer.Children.Clear();

                foreach (var reservation in reservations)
                {
                    var card = CreateReservationCard(reservation);
                    ReservationsContainer.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;

                await DisplayAlert("Error",
                    $"No se pudieron cargar las reservaciones: {ex.Message}",
                    "OK");
            }
        }

        private Frame CreateReservationCard(ReservationWithRoom reservation)
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 15,
                Padding = 0,
                HasShadow = true,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = 140 },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            // Imagen de la habitación
            var imageContainer = new Grid();

            var roomImage = new Image
            {
                Source = string.IsNullOrEmpty(reservation.room_image_url)
                    ? "default_room.jpg"
                    : reservation.room_image_url,
                Aspect = Aspect.AspectFill,
                HeightRequest = 140
            };

            // Badge de estado
            var statusBadge = new Frame
            {
                BackgroundColor = GetStatusColor(reservation.checkin, reservation.checkout),
                CornerRadius = 15,
                Padding = new Thickness(10, 5),
                HasShadow = false,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(10)
            };
            statusBadge.Content = new Label
            {
                Text = GetStatusText(reservation.checkin, reservation.checkout),
                TextColor = Colors.White,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold
            };

            imageContainer.Children.Add(roomImage);
            imageContainer.Children.Add(statusBadge);

            // Información de la reservación
            var infoStack = new VerticalStackLayout
            {
                Padding = 15,
                Spacing = 10
            };

            // Nombre del hotel
            var hotelLabel = new Label
            {
                Text = "Hotel Parador",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#333333")
            };

            // Habitación
            var roomInfo = new HorizontalStackLayout { Spacing = 5 };
            roomInfo.Children.Add(new Label { Text = "🛏️", FontSize = 14 });
            roomInfo.Children.Add(new Label
            {
                Text = reservation.room_name,
                FontSize = 14,
                TextColor = Color.FromArgb("#666666")
            });

            // Fechas
            var dateInfo = new HorizontalStackLayout { Spacing = 5 };
            dateInfo.Children.Add(new Label { Text = "📅", FontSize = 14 });
            dateInfo.Children.Add(new Label
            {
                Text = $"{reservation.checkin:dd MMM} - {reservation.checkout:dd MMM yyyy} ({reservation.NumberOfNights} {(reservation.NumberOfNights == 1 ? "noche" : "noches")})",
                FontSize = 14,
                TextColor = Color.FromArgb("#666666")
            });

            // Huéspedes
            var guestsInfo = new HorizontalStackLayout { Spacing = 5 };
            guestsInfo.Children.Add(new Label { Text = "👥", FontSize = 14 });
            guestsInfo.Children.Add(new Label
            {
                Text = $"Capacidad: {reservation.room_guests} {(reservation.room_guests == 1 ? "huésped" : "huéspedes")}",
                FontSize = 14,
                TextColor = Color.FromArgb("#666666")
            });

            // Código de reservación
            var codeInfo = new HorizontalStackLayout { Spacing = 5 };
            codeInfo.Children.Add(new Label { Text = "🎫", FontSize = 14 });
            codeInfo.Children.Add(new Label
            {
                Text = $"Código: #{reservation.id}",
                FontSize = 12,
                TextColor = Color.FromArgb("#999999")
            });

            // Separador
            var separator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Color.FromArgb("#EEEEEE"),
                Margin = new Thickness(0, 5)
            };

            // Footer con precio y botón
            var footer = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var priceStack = new VerticalStackLayout();
            priceStack.Children.Add(new Label
            {
                Text = "Total pagado",
                FontSize = 12,
                TextColor = Color.FromArgb("#999999")
            });
            priceStack.Children.Add(new Label
            {
                Text = $"${reservation.total:N2} USD",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#6200EA")
            });

            var detailsButton = new Button
            {
                Text = "Ver detalles",
                BackgroundColor = Color.FromArgb("#6200EA"),
                TextColor = Colors.White,
                CornerRadius = 10,
                Padding = new Thickness(20, 8),
                FontSize = 13
            };
            detailsButton.Clicked += (s, e) => OnViewDetailsClicked(reservation);

            footer.Children.Add(priceStack);
            Grid.SetColumn(detailsButton, 1);
            footer.Children.Add(detailsButton);

            // Agregar elementos al stack
            infoStack.Children.Add(hotelLabel);
            infoStack.Children.Add(roomInfo);
            infoStack.Children.Add(dateInfo);
            infoStack.Children.Add(guestsInfo);
            infoStack.Children.Add(codeInfo);
            infoStack.Children.Add(separator);
            infoStack.Children.Add(footer);

            // Agregar al grid
            grid.Children.Add(imageContainer);
            Grid.SetRow(infoStack, 1);
            grid.Children.Add(infoStack);

            frame.Content = grid;
            return frame;
        }

        private Color GetStatusColor(DateTime checkin, DateTime checkout)
        {
            var now = DateTime.Now.Date;

            if (now < checkin.Date)
                return Color.FromArgb("#4CAF50"); // Verde - Próxima
            else if (now >= checkin.Date && now <= checkout.Date)
                return Color.FromArgb("#FF9800"); // Naranja - En curso
            else
                return Color.FromArgb("#2196F3"); // Azul - Completada
        }

        private string GetStatusText(DateTime checkin, DateTime checkout)
        {
            var now = DateTime.Now.Date;

            if (now < checkin.Date)
                return "Confirmada";
            else if (now >= checkin.Date && now <= checkout.Date)
                return "En curso";
            else
                return "Completada";
        }

        private async void OnViewDetailsClicked(ReservationWithRoom reservation)
        {
            var daysUntil = (reservation.checkin.Date - DateTime.Now.Date).Days;
            var status = GetStatusText(reservation.checkin, reservation.checkout);

            var message = $"🏨 Hotel: Hotel Parador\n\n" +
                         $"🛏️ Habitación: {reservation.room_name}\n" +
                         $"💰 Precio por noche: ${reservation.room_price:N2}\n\n" +
                         $"📅 Check-in: {reservation.checkin:dddd, dd MMMM yyyy}\n" +
                         $"📅 Check-out: {reservation.checkout:dddd, dd MMMM yyyy}\n" +
                         $"🌙 Duración: {reservation.NumberOfNights} {(reservation.NumberOfNights == 1 ? "noche" : "noches")}\n\n" +
                         $"👥 Capacidad: {reservation.room_guests} {(reservation.room_guests == 1 ? "huésped" : "huéspedes")}\n\n" +
                         $"💰 Total: ${reservation.total:N2} USD\n\n" +
                         $"✅ Estado: {status}\n\n" +
                         $"🎫 ID Reservación: #{reservation.id}\n" +
                         $"📆 Reservado el: {reservation.created_at:dd/MM/yyyy HH:mm}";

            if (daysUntil > 0)
            {
                message += $"\n\n⏰ Faltan {daysUntil} {(daysUntil == 1 ? "día" : "días")} para tu llegada";
            }

            await DisplayAlert("Detalles de Reservación", message, "Cerrar");
        }

        private async void OnNewBookingClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewBookingPage());
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}