namespace MachiKoro_Client.Models;

public class WheatFieldCard : ICard
{
    public CardTypes Type { get; set; }
    
    public int Cost { get; set; }
    public int[] TriggerNubmers { get; set; }
    public int EarnCoinsTriggerNumber { get; set; }
    public string Name { get; set; }

    public WheatFieldCard()
    {
        Type = CardTypes.Blue;
        Cost = 1;
        TriggerNubmers = new [] { 1 };
        EarnCoinsTriggerNumber = 1;
        Name = "WheatFieldCard";

    }  
}