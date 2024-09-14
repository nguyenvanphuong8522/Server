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

        public static byte[] SendMessageConverted(MyMessageType type, byte[] bytesData)
        {
            byte[] bytesType = MessagePackSerializer.Serialize(type);
            int lengthData = bytesData.Length;
            byte[] bytesLengthSolid = new byte[13];
            byte[] byteLength = BitConverter.GetBytes(lengthData);
            Buffer.BlockCopy(byteLength, 0, bytesLengthSolid, 0, byteLength.Length);

            byte[] mainData = new byte[lengthData + 1];

            Buffer.BlockCopy(bytesData, 0, mainData, 1, lengthData);
            Buffer.BlockCopy(bytesType, 0, mainData, 0, 1);
            byte[] result = new byte[mainData.Length + 13];
            Buffer.BlockCopy(bytesLengthSolid, 0, result, 0, bytesLengthSolid.Length);
            Buffer.BlockCopy(mainData, 0, result, 13, mainData.Length);
            return result;
        }

        public static MyMessageType ByteToType(byte value)
        {
            byte[] bytes = { value };
            MyMessageType type = ByteToType(bytes);
            return type;
        }
        public static MyMessageType ByteToType(byte[] bytes)
        {
            MyMessageType type = MessagePackSerializer.Deserialize<MyMessageType>(bytes);
            return type;
        }
        public static async Task CreateNewSession(Socket clientSocket)
        {
            while (true)
            {
                byte[] buffer = new byte[13];
                await clientSocket.ReceiveAsync(buffer, SocketFlags.None);

                int length = BitConverter.ToInt32(buffer, 0);

                byte[] byteType = new byte[1];
                await clientSocket.ReceiveAsync(byteType, SocketFlags.None);

                MyMessageType type = ByteToType(byteType);

                byte[] mainData = new byte[length];
                int messageCode2 = await clientSocket.ReceiveAsync(mainData, SocketFlags.None);

                await HandleOneRequest(mainData, clientSocket, type);
            }
        }


        public static async Task HandleOneRequest(byte[] request, Socket clientSocket, MyMessageType type)
        {
            if (request.Length == 0) return;

            string result;
            switch (type)
            {
                case MyMessageType.CREATE:
                    int key = ConnectionManager.IndexOf(clientSocket);
                    await PlayerManager.SpawnNewPlayer(ConnectionManager.dictionarySocket[key]);
                    break;
                case MyMessageType.POSITION:
                    MessagePosition? playerPosition = MessagePackSerializer.Deserialize<MessagePosition>(request);
                    Player player = PlayerManager.listOfPlayer.Find(x => x.Id == playerPosition.id);
                    if (player == null) return;
                    player.UpdatePosition(playerPosition.Position);
                    MessagePosition messagePosition = new MessagePosition(playerPosition.id, playerPosition.Position);
                    byte[] data = MessagePackSerializer.Serialize(messagePosition);
                    byte[] resultFinal = SendMessageConverted(MyMessageType.POSITION, data);
                    await MessageSender.SendToAllClients(resultFinal);

                    break;
                case MyMessageType.DESTROY:
                    MessageBase? messageBase = MessagePackSerializer.Deserialize<MessageBase>(request);
                    var t2 = ConnectionManager.DisconnectClient(clientSocket, messageBase.id);
                    break;
                case MyMessageType.TEXT:
                    MessageText messageText = MessagePackSerializer.Deserialize<MessageText>(request);
                    messageText.text = $"[{ConnectionManager.IndexOf(clientSocket)}]: " + messageText.text;

                    byte[] data2 = MessagePackSerializer.Serialize(messageText);

                    byte[] result2 = SendMessageConverted(MyMessageType.TEXT, data2);
                    await MessageSender.SendToAllClients(result2);
                    break;
                default:
                    break;
            }
        }


    }
}
