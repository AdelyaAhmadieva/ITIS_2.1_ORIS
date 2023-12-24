namespace MachiKoro_Client.Models;

public interface ICard
{
    public int Cost { get; set; }
    public CardTypes Type { get; set; }
    public int[] TriggerNubmers { get; set; }
    public int EarnCoinsTriggerNumber { get; set; }
    public string Name { get; set; }

   
}