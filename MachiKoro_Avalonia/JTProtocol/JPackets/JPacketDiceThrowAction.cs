using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketDiceThrowAction
{
    [JField(1)] public int Value;
    [JField(2)] public int PlayerID;

}