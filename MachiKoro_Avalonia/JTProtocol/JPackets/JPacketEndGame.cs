using JTProtocol.Serializer;
namespace JTProtocol.JPackets;

[Serializable]
    public class JPacketEndGame
    {
        [JField(1)] public int WinnerID;
    }