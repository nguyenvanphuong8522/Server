using MessagePack;
using Shared.Math;

namespace Shared.Messages;

[MessagePackObject]
public class MessagePosition
{
    [Key(0)]
    public int Id;

    [Key(1)]
    public MyVector3 Position;

    public MessagePosition()
    {
    }

    public MessagePosition(int id, MyVector3 position)
    {
        Id = id;
        Position = position;
    }
}