using MessagePack;

namespace Shared.Math
{
    [MessagePackObject]
    public class MyVector3
    {
        [Key(0)]
        public float X;

        [Key(1)]
        public float Y;

        [Key(2)]
        public float Z;

        public MyVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}