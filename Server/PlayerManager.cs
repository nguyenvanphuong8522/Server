using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PlayerManager
    {
        public List<Player> listOfPlayer = new List<Player>();
        public SpawnManager spawnManager = new();

        public async Task SpawnNewPlayer(Socket socket, int index = 0)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));

            Player newPlayer = spawnManager.GetPrefab(index);
            if (newPlayer == null) throw new Exception("Failed to spawn new player.");

            listOfPlayer.Add(newPlayer);

            string playerInfo = Server.ConvertToDataRequest(newPlayer.Id, newPlayer.position, MyMessageType.CREATE);

            Task sendToSingleClientTask = Server.SendToSingleClient(socket, playerInfo);
            Task sendToAllClientsTask = Server.SendToAllClients(playerInfo);

            await Task.WhenAll(sendToSingleClientTask, sendToAllClientsTask);

            if (listOfPlayer.Count > 1)
            {
                await Server.SendInfoAboutExistingPlayers(socket);
            }
        }
    }

}
