using MyLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
namespace Server
{
    public static class RequestHandler
    {
        public static async Task CreateNewSession(Socket clientSocket)
        {
            while (true)
            {
                byte[] buffer = new byte[10];
                int messageCode = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                Console.WriteLine("receive");
                int length = BitConverter.ToInt32(buffer, 0);

                Console.WriteLine(length); 

                byte[] buffer2 = new byte[10];
                int messageCode2 = await clientSocket.ReceiveAsync(buffer2, SocketFlags.None);
                

                int length2 = BitConverter.ToInt32(buffer2, 0);


                Console.WriteLine(length2);

                //await HandleOneRequest(byteData, clientSocket);
            }
        }


        public static async Task HandleOneRequest(byte[] request, Socket clientSocket)
        {
            if (request.Length == 0) return;

            MyDataRequest? data = MessagePackSerializer.Deserialize<MyDataRequest>(request);
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
                    var t2 = ConnectionManager.DisconnectClient(clientSocket, messageBase.id);
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


    }
}
