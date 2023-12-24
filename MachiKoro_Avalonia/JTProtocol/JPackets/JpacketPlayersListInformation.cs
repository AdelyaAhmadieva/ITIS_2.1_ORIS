using JTProtocol.Serializer;

namespace JTProtocol.JPackets;

[Serializable]
public class JPacketPlayersListInformation
{
    [JField(1)] public List<Tuple<int, string>> PlayerInformationList;
}