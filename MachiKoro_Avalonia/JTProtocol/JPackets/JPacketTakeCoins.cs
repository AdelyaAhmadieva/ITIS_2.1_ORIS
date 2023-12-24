using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketTakeCoins
{
    [JField(1)] public int CoinsToTake;
}