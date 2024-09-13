using MyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class MessageSender
    {
        public static async Task SendToSingleClient(Socket socket, string message)
        {
            if (socket == null) return;
            string messageToSend = message + '@';
            var sendBuffer = Encoding.UTF8.GetBytes(messageToSend);
            await socket.SendAsync(sendBuffer, SocketFlags.None);
        }

        public static async Task SendToAllClients(string message)
        {
            foreach (var socket in ConnectionManager.dictionarySocket.Values)
            {
                if (socket.Connected)
                {
                    await SendToSingleClient(socket, message);
                }

            }
        }

        public static string ConvertToDataRequest(int id, MyVector3 pos, MyMessageType type)
        {
            string content = MyUtility.ConvertToMessagePosition(id, pos);
            return MyUtility.ConvertToDataRequestJson(content, type);
        }

        public static async Task SendInfoAboutExistingPlayers(Socket socket)
        {
            string content = string.Empty;
            string inforNewPlayer = string.Empty;
            foreach (Player player in PlayerManager.listOfPlayer)
            {
                content = MyUtility.ConvertToMessagePosition(player.Id, player.position);
                inforNewPlayer = MyUtility.ConvertToDataRequestJson(content, MyMessageType.CREATE);
                await SendToSingleClient(socket, inforNewPlayer);
            }
        }
    }
}
