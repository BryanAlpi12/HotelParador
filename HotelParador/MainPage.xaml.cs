namespace HotelParador
{
    public partial class MainPage : ContentPage
    {


        public MainPage()
        {
            InitializeComponent();
            BtnEmpezar.Clicked += BtnEmpezar_Clicked;
        }

        private async void BtnEmpezar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Sesion());
        }



    }
}
