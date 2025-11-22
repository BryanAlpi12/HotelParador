namespace HotelParador
{
    public partial class App : Application
    {
        public static string UserEmail { get; set; } = string.Empty;

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}