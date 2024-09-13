using MyLibrary;

namespace Server
{
    public class SpawnManager
    {
        private int idPlayer = 100;
        public Player GetPrefab()
        {
            Random random = new Random();
            Player newPlayer = new Player();
            float x = random.Next(-20, 20);
            float z = random.Next(-20, 20);
            newPlayer.position = new MyVector3(x, 3, z);
            newPlayer.Id = idPlayer++;
            return newPlayer;
        }
    }
}