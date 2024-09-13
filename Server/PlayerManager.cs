using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class PlayerManager
    {
        public static List<Player> listOfPlayer;
        public static SpawnManager spawnManager;

        public static void Initialize()
        {
            listOfPlayer = new List<Player>();
            spawnManager = new SpawnManager();
        }

        public static async Task SpawnNewPlayer(Socket socket)
        {
            Player newPlayer = spawnManager.GetPrefab();
            if (newPlayer == null) return;

            listOfPlayer.Add(newPlayer);

            string playerInfo = MessageSender.ConvertToDataRequest(newPlayer.Id, newPlayer.position, MyMessageType.CREATE);

            await MessageSender.SendToSingleClient(socket, playerInfo);
            await MessageSender.SendInfoAboutExistingPlayers(socket);
            await MessageSender.SendToAllClients(playerInfo);
        }

        public static async Task RemovePlayer(int id)
        {
            Player player = listOfPlayer.FirstOrDefault(x => x.Id == id);
            if (player != null)
            {
                listOfPlayer.Remove(player);
            }
        }
    }

}
