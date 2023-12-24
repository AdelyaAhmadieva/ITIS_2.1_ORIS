using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketChangeTurn
{
    [JField(1)] public int PlayerID;
}