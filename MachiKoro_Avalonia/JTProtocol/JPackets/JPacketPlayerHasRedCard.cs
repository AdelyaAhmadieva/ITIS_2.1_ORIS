using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketPlayerHasRedCard
{
    [JField(1)] public int DiceValue;
    [JField(2)] public int ThrowerId;


}