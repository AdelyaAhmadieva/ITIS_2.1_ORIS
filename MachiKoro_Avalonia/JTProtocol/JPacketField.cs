namespace JTProtocol;

public class JPacketField
{
    public byte FieldId { get; set; }
    public byte FieldSize { get; set; }
    public byte[] Contents { get; set; } = null!;
}