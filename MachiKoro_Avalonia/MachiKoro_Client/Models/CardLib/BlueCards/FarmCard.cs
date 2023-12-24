namespace MachiKoro_Client.Models;

public class FarmCard : ICard
{
    public CardTypes Type { get; set; }
    
    public int Cost { get; set; }
    public int[] TriggerNubmers { get; set; }
    public int EarnCoinsTriggerNumber { get; set; }
    public string Name { get; set; }

    public FarmCard()
    {
        Type = CardTypes.Blue;
        Cost = 1;
        TriggerNubmers = new [] { 2 };
        EarnCoinsTriggerNumber = 1;
        Name = "FarmCard";


    }  
}