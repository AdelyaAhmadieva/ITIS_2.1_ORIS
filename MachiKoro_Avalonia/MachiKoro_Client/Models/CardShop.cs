using System.Collections.Generic;

namespace MachiKoro_Client.Models;

public class CardShop
{
    public Dictionary<string, ICard> Cards;

    public CardShop()
    {
        Cards = new Dictionary<string, ICard>();
        ShopFilling();
        
    }

    private void ShopFilling()
    {
        Cards.Add("BackeryCard", new BackeryCard());
        Cards.Add("ShopCard", new ShopCard());
        
        Cards.Add("CaffeCard", new CaffeCard());
        
        Cards.Add("WheatFieldCard", new WheatFieldCard());
        Cards.Add("FarmCard", new FarmCard());
        
    }
}