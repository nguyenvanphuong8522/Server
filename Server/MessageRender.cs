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
    public static class MessageSender
    {
        public static async Task SendToSingleClient(Socket socket, string message)
        {
            if (socket == null) return;
            string messageToSend = message + '@';
            var sendBuffer = Encoding.UTF8.GetBytes(messageToSend);
            await socket.SendAsync(sendBuffer, SocketFlags.None);
        }
        public static async Task SendToSingleClient(Socket socket, byte[] data)
        {
            if (socket == null) return;
            await socket.SendAsync(data, SocketFlags.None);
        }

        public static async Task SendToAllClients(byte[] data)
        {
            foreach (var socket in ConnectionManager.dictionarySocket.Values)
            {
                if (socket.Connected)
                {
                    await SendToSingleClient(socket, data);
                }

            }
        }

        public static async Task SendInfoAboutExistingPlayers(Socket socket)
        {
            string content = string.Empty;
            string inforNewPlayer = string.Empty;
            foreach (Player player in PlayerManager.listOfPlayer)
            {
                MessagePosition newMessagePosition = new MessagePosition(player.Id, player.position);
                byte[] dataSend = MyUtility.ConvertFinalMessageToBytes(MyMessageType.CREATE, MessagePackSerializer.Serialize(newMessagePosition));
                
                await SendToSingleClient(socket, dataSend);
            }
        }
    }
}
