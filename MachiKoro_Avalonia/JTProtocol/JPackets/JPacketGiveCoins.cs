using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketGiveCoins
{
    [JField(1)] public int CoinsToGive;
}