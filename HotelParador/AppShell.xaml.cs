namespace HotelParador
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("LocationPage", typeof(LocationPage));
            Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
            Routing.RegisterRoute(nameof(ProfilePageEdit), typeof(ProfilePageEdit));

           
            Routing.RegisterRoute(nameof(IntroduccionPage), typeof(IntroduccionPage));
            Routing.RegisterRoute(nameof(HistoriaPage), typeof(HistoriaPage));
            Routing.RegisterRoute(nameof(CancelarReservaPage), typeof(CancelarReservaPage));
        }
    }
}