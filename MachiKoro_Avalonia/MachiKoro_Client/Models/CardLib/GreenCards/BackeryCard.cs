namespace MachiKoro_Client.Models;

public class BackeryCard : ICard
{
    public CardTypes Type { get; set; }
    
    public int Cost { get; set; }
    public int[] TriggerNubmers { get; set; }
    public int EarnCoinsTriggerNumber { get; set; }
    public string Name { get; set; }

    public BackeryCard()
    {
        Type = CardTypes.Green;
        Cost = 1;
        TriggerNubmers = new [] { 2, 3 };
        EarnCoinsTriggerNumber = 1;
        Name = "BackeryCard";

    }
}