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
    public static class ConnectionManager
    {
        public static Dictionary<int, Socket> dictionarySocket;
        public static int socketIndex = 0;

        public static void Initialize()
        {
            dictionarySocket = new Dictionary<int, Socket>();
        }

        public static async Task WaitConnect()
        {
            while (true)
            {
                var socket = await Server.listenSocket.AcceptAsync();
                dictionarySocket.Add(socketIndex++, socket);
                Console.WriteLine($"Client[{IndexOf(socket)}] connected!");
                var t = RequestHandler.CreateNewSession(socket);
            }
        }

        public static int IndexOf(Socket socket)
        {
            return dictionarySocket.FirstOrDefault(kvp => kvp.Value == socket).Key;
        }

        public static async Task RemoveSocket(int key)
        {
            dictionarySocket.Remove(key);
        }

        public static async Task DisconnectClient(Socket socket, int playerId)
        {
            int key = IndexOf(socket);
            dictionarySocket[key].Close();
            dictionarySocket.Remove(socketIndex);

            byte[] content = MessagePackSerializer.Serialize(new MessageBase(playerId));
            byte[] result = MyUtility.ConvertFinalMessageToBytes(MyMessageType.DESTROY, content);
            Console.WriteLine($"Client[{key}] disconnnected!");
            await PlayerManager.RemovePlayer(playerId);
            await MessageSender.SendToAllClients(result);
        }
    }
}
