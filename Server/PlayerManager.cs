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

        public static async Task SpawnNewPlayer(Socket socket, int index = 0)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));

            Player newPlayer = spawnManager.GetPrefab(index);
            if (newPlayer == null) throw new Exception("Failed to spawn new player.");

            listOfPlayer.Add(newPlayer);

            string playerInfo = MessageSender.ConvertToDataRequest(newPlayer.Id, newPlayer.position, MyMessageType.CREATE);

            Task sendToSingleClientTask = MessageSender.SendToSingleClient(socket, playerInfo);
            Task sendToAllClientsTask = MessageSender.SendToAllClients(playerInfo);

            await Task.WhenAll(sendToSingleClientTask, sendToAllClientsTask);

            if (listOfPlayer.Count > 1)
            {
                await MessageSender.SendInfoAboutExistingPlayers(socket);
            }
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
