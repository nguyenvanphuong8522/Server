using MessagePack;

namespace Shared.Messages;

[MessagePackObject]
public class MessageBase
{
    [Key(0)]
    public int Id;

    public MessageBase()
    {
    }

    public MessageBase(int id)
    {
        Id = id;
    }
}