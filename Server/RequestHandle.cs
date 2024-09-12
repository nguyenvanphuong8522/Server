using MyLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class RequestHandler
    {
        public static async Task CreateNewSession(Socket clientSocket)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int messageCode = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                if (messageCode == 0) return;

                string messageReceived = Encoding.UTF8.GetString(buffer, 0, messageCode);
                string[] requests = MyUtility.StringSplitArray(messageReceived);
                await HandleManyRequests(requests, clientSocket);
            }
        }

        public static async Task HandleOneRequest(string request, Socket clientSocket)
        {
            if (string.IsNullOrEmpty(request)) return;

            MyDataRequest? data = JsonConvert.DeserializeObject<MyDataRequest>(request);
            if (data == null) return;
            string result;
            switch (data.Type)
            {
                case MyMessageType.CREATE:
                    int key = ConnectionManager.IndexOf(clientSocket);
                    await PlayerManager.SpawnNewPlayer(ConnectionManager.dictionarySocket[key]);
                    break;
                case MyMessageType.POSITION:
                    MessagePosition? playerPosition = JsonConvert.DeserializeObject<MessagePosition>(data.Content);
                    Player player = PlayerManager.listOfPlayer.Find(x => x.Id == playerPosition.id);
                    if (player == null) return;
                    player.UpdatePosition(playerPosition.Position);
                    result = MessageSender.ConvertToDataRequest(player.Id, player.position, MyMessageType.POSITION);
                    await MessageSender.SendToAllClients(result);

                    break;
                case MyMessageType.DESTROY:
                    MessageBase? messageBase = JsonConvert.DeserializeObject<MessageBase>(data.Content);
                    await ConnectionManager.DisconnectClient(clientSocket, messageBase.id);
                    break;
                case MyMessageType.TEXT:
                    MessageText messageText = JsonConvert.DeserializeObject<MessageText>(data.Content);
                    messageText.text = $"[{ConnectionManager.IndexOf(clientSocket)}]: " + messageText.text;

                    string content = JsonConvert.SerializeObject(messageText);
                    result = MyUtility.ConvertToDataRequestJson(content, MyMessageType.TEXT);
                    await MessageSender.SendToAllClients(result);
                    break;
                default:
                    break;
            }
        }

        public static async Task HandleManyRequests(string[] requests, Socket clientSocket)
        {
            foreach (string request in requests)
            {
                await HandleOneRequest(request, clientSocket);
            }
        }
    }
}
