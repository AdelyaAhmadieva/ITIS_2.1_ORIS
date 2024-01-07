namespace JTProtocol;

public static class JPacketTypeManager
{
    private static readonly Dictionary<JPacketType, Tuple<byte, byte>> TypeDictionary = new();

    static JPacketTypeManager()
    {
        RegisterType(JPacketType.Connection, 1, 0);
        RegisterType(JPacketType.DiceThrowAction, 2, 0);
        RegisterType(JPacketType.PlayerHasRedCard, 3, 0);
        RegisterType(JPacketType.PlayerHasBlueCard, 4, 0);
        RegisterType(JPacketType.GiveCoins, 5, 0);
        RegisterType(JPacketType.TakeCoins, 6, 0);
        RegisterType(JPacketType.Payment, 7, 0);
        RegisterType(JPacketType.DiceThrowResult, 8, 0);
        RegisterType(JPacketType.ChangeTurn, 9, 0);
        RegisterType(JPacketType.PlayersInformation, 10, 0);
        RegisterType(JPacketType.EndGame, 11, 0);
        
    }

    private static void RegisterType(JPacketType type, byte btype, byte bsubtype)
    {
        if (TypeDictionary.ContainsKey(type))
        {
            throw new Exception($"Packet type {type:G} is already registered.");
        }

        TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
    }

    public static Tuple<byte, byte> GetType(JPacketType type)
    {
        if (!TypeDictionary.TryGetValue(type, out var value))
        {
            throw new Exception($"Packet type {type:G} is not registered.");
        }

        return value;
    }

    public static JPacketType GetTypeFromPacket(JPacket packet)
    {
        var type = packet.PacketType;
        var subtype = packet.PacketSubtype;

        foreach (var (xPacketType, tuple) in TypeDictionary)
        {
            if (tuple.Item1 == type && tuple.Item2 == subtype)
            {
                return xPacketType;
            }
        }

        return JPacketType.Unknown;
    }
}