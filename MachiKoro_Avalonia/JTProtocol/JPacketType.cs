namespace JTProtocol;

public enum JPacketType
{
    Unknown,
    Connection,
    DiceThrowAction,
    DiceThrowResult,
    PlayerHasRedCard,
    PlayerHasBlueCard,
    GiveCoins,
    TakeCoins,
    Payment,
    ChangeTurn,
    PlayersInformation,
    EndGame
}