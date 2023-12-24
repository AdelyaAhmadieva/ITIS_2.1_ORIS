using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketPayment
{
    [JField(1)] public int CoinsAmountToTake;
    [JField(2)] public int TakeFromClient;
    [JField(3)] public int GiveToClient;
}