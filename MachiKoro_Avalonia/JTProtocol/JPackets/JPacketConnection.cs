using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketConnection
{
    [JField(1)] public bool IsSuccessful;
    [JField(2)] public int id;
}