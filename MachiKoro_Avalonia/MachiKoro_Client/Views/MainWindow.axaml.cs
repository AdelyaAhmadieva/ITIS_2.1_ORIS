using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia.Helpers;
using DynamicData;
using MachiKoro_Client.Helpers;
using MachiKoro_Client.ViewModels;
using Metsys.Bson;
using Bitmap = System.Drawing.Bitmap;

namespace MachiKoro_Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Width = 1600;
        Height = 900;
    }



    private void ConnectButton_OnClick(object? sender, RoutedEventArgs e)
    {

        ConnectButton.IsVisible = false;
        ThrowDiceButton.IsVisible = true;
        ByCardButton.IsVisible = true;
        StartGamePanel.IsVisible = false;
    }


    private void ByCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        UpPanel.IsVisible = false;
        MidPanel.IsVisible = false;
        DownPanel.IsVisible = false;

        BackToGameButton.IsVisible = true;
        CardShop.IsVisible = true;
    }

    private void BackToGame_OnClick(object? sender, RoutedEventArgs e)
    {
        UpPanel.IsVisible = true;
        MidPanel.IsVisible = true;
        DownPanel.IsVisible = true;

        BackToGameButton.IsVisible = false;
        CardShop.IsVisible = false;   
    }

    private void ChooseCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        //ByCardButton.IsEnabled = false;
        ChooseCardButton.IsEnabled = false;
    }

    private void ThrowDiceButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ChangeTurnButton.IsEnabled = true;
        ByCardButton.IsEnabled = true;
        ThrowDiceButton.IsEnabled = false;
    }
    
    
}

/*

void FillShop()
{

var CardShop = new Models.CardShop().Cards;
foreach (var VARIABLE in CardShop)
{
    this.CardShop.Items.Add(new Image()
    {
        MaxWidth = 200,
        Source = ImageHelper.LoadFromResource("/Assets/CardImages/Wheat_Field.jpg")
    });
    this.CardShop.Items.Add(ByCardButton);

}
}

//Source = ImageHelper.LoadFromResource("/Assets/CardImages/Wheat_Field.jpg")

*/