namespace HotelParador
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("LocationPage", typeof(LocationPage));
            Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
        }
    }
}
