using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace MachiKoro_Client.Views;

public partial class CardShopWindow : Window
{
    private int a;

    public int A
    {
        get => a;
        set
        {
            a = value;
        }
    }
    public CardShopWindow(object dataContext)
    {
        InitializeComponent();
    }
    
}