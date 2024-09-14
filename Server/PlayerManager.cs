using MessagePack;
using MyLibrary;
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
            MessagePosition newMessagePosition = new MessagePosition(newPlayer.Id, newPlayer.position);
            byte[] dataSend = RequestHandler.SendMessageConverted(MyMessageType.CREATE, MessagePackSerializer.Serialize(newMessagePosition));

            await MessageSender.SendToSingleClient(socket, dataSend);
            await MessageSender.SendInfoAboutExistingPlayers(socket);
            await MessageSender.SendToAllClients(dataSend);
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
