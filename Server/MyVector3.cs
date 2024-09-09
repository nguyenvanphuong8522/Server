
using MessagePack;

[MessagePackObject]
public class MyVector3
{
    [Key(0)]
    public RequestType type;
    [Key(1)]
    public int id;
    [Key(2)]
    public float x;
    [Key(3)]
    public float y;
    [Key(4)]
    public float z;

    public MyVector3(float x, float y, float z, RequestType type)
    {
        this.type = type;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public MyVector3()
    {
        x = 0; y = 0; z = 0;
        type = RequestType.CREATE;
    }



}
