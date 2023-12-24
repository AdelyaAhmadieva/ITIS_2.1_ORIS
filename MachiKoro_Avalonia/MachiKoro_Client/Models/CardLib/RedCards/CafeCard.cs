namespace MachiKoro_Client.Models;

public class CafeCard : ICard
{
    public CardTypes Type { get; set; }
    public int Cost { get; set; }
    public int[] TriggerNubmers { get; set; }
    public int EarnCoinsTriggerNumber { get; set; }
    public string Name { get; set; }

    public CafeCard()
    {
        Type = CardTypes.Red;
        Cost = 2;
        TriggerNubmers = new [] { 3 };
        EarnCoinsTriggerNumber = 1;
    }
}