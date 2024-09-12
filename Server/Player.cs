using System.Numerics;
using MyLibrary;
namespace Server
{
    internal class Player
    {
        private int _id;

        public int Id
        {

            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }

        }

        public MyVector3? position;

        public void UpdatePosition(MyVector3 newPos)
        {
            position = newPos;
        } 
    }
}