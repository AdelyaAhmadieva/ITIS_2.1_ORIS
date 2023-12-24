using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketDiceThrowResult
{
    [JField(1)] public int Result;
    [JField(2)] public int PlayerID;

}