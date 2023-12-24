using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MachiKoro_Client.Models;

namespace MachiKoro_Client.ViewModels;

public class CardShopWindowViewModel : ViewModelBase
{
    public void Initialize()
    {
        var files = Directory.GetFiles("../../../Assets/CardImages");
        var images = files.Select(x => new CardImage(new FileInfo(x).FullName));
        CardList = new ObservableCollection<CardImage>(images);

    }

    public CardShopWindowViewModel()
    {
        Initialize();
    }

    private ObservableCollection<CardImage> _cardList;
    public ObservableCollection<CardImage> CardList
    {
        get => _cardList;
        set
        {
            if(value != null)_cardList = value;
        }
    }
}