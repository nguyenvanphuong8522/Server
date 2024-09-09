using System;
using System.Numerics;

namespace Server
{
    internal class SpawnManager
    {

        public Player GetPrefab(int index)
        {
            Random random = new Random();
            Player newPlayer = new Player();
            float x = random.Next(-20, 20);
            float z = random.Next(-20, 20);
            newPlayer.position = new Vector3(x, 3, z);
            int idRandom = random.Next(-1000, 1000);
            newPlayer.Id = idRandom;
            return newPlayer;
        }
    }
}