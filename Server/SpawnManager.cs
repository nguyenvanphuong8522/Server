using MyLibrary;

namespace Server
{
    public class SpawnManager
    {

        public Player GetPrefab(int index)
        {
            Random random = new Random();
            Player newPlayer = new Player();
            float x = random.Next(-20, 20);
            float z = random.Next(-20, 20);
            newPlayer.position = new MyVector3(x, 3, z);
            int idRandom = random.Next(-1000, 1000);
            newPlayer.Id = idRandom;
            return newPlayer;
        }
    }
}