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
        Cards.Add("Пекарня", new BackeryCard());
        Cards.Add("Магазин", new ShopCard());
        
        Cards.Add("Кафе", new CafeCard());
        
        Cards.Add("WheatFieldCard", new WheatFieldCard());
        
    }
}