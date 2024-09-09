using System.Numerics;

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

        public Vector3 position;
    }
}