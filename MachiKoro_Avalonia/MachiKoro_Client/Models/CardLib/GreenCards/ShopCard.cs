namespace MachiKoro_Client.Models;

public class ShopCard : ICard
{
    public CardTypes Type { get; set; }
    
    public int Cost { get; set; }
    public int[] TriggerNubmers { get; set; }
    public int EarnCoinsTriggerNumber { get; set; }
    public string Name { get; set; }

    public ShopCard()
    {
        Type = CardTypes.Green;
        Cost = 2;
        TriggerNubmers = new [] { 4 };
        EarnCoinsTriggerNumber = 3;

    }  
}