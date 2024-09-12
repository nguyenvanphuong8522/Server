using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using MyLibrary;
using System.Numerics;

namespace Server
{
    public static class Server
    {
        private static Dictionary<int, Socket>? dictionarySocket;

        public static PlayerManager playerManager;

        public static Socket? listenSocket;

        private static IPEndPoint? ipEndPoint;

        private const int port = 8522;

        private const string ipAddress = "192.168.1.25";

        private static int socketCounter;

        static async Task Main()
        {
            SetUpData();
            StartServer();
            await WaitConnect();
        }

        // Initialize necessary data for the server.
        private static void SetUpData()
        {
            dictionarySocket = new Dictionary<int, Socket>();
            playerManager = new PlayerManager();
            ipEndPoint = new(IPAddress.Parse(ipAddress), port);
        }


        // Start the server and begin listening for client connections.
        static void StartServer()
        {
            if(ipEndPoint != null)
            {
                listenSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(ipEndPoint);
                listenSocket.Listen();
                Console.WriteLine("LISTENING...");
            }
        }



        // Wait for client connections and handle them when they occur.
        static async Task WaitConnect()
        {
            while (true)
            {
                var socket = await listenSocket.AcceptAsync();

                dictionarySocket.Add(socketCounter++, socket);
                Console.WriteLine($"Client[{socketCounter}] connected!");
                var newSection = CreateNewSection(socket);
            }
        }

        //Listen and handle message from the client.
        async static Task CreateNewSection(Socket clientSocket)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int messageCode = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                string messageReceived = Encoding.UTF8.GetString(buffer, 0, messageCode);
                if (messageCode == 0) return;
                string[] requests = MyUtility.StringSplitArray(messageReceived);
                var t = HandleManyRequest(requests, clientSocket);
            }
        }





        //Send a message to a client.
        public static async Task SendToSingleClient(Socket socket, string message)
        {
            string messageSpecify = message + '@';
            var sendBuffer = Encoding.UTF8.GetBytes(messageSpecify);
            await socket.SendAsync(sendBuffer, SocketFlags.None);
        }


        //Send all message to all client.
        public static async Task SendToAllClients(string message)
        {
            foreach (var item in dictionarySocket)
            {
                await SendToSingleClient(item.Value, message);
            }
        }



        //Send information about existing players to a newly connected client.
        public static async Task SendInfoAboutExistingPlayers(Socket socket)
        {
            string content = string.Empty;
            string inforNewPlayer = string.Empty;
            foreach (Player player in playerManager.listOfPlayer)
            {
                content = MyUtility.ConvertToMessagePosition(player.Id, player.position);
                inforNewPlayer = MyUtility.ConvertToDataRequestJson(content, MyMessageType.CREATE);
                await SendToSingleClient(socket, inforNewPlayer);
            }
        }





        //Handle a single request from a client.
        private static async Task HandleOneRequest(string request, Socket clientSocket)
         {
            if (string.IsNullOrEmpty(request)) return;

            MyDataRequest? data = JsonConvert.DeserializeObject<MyDataRequest>(request);
            string result = string.Empty;
            MyMessageType type = data.Type;

            switch (type)
            {
                case MyMessageType.CREATE:
                    int key = IndexOf(clientSocket);
                    await playerManager.SpawnNewPlayer(dictionarySocket[key]);
                    break;
                case MyMessageType.POSITION:
                    MessagePosition? playerPosition = JsonConvert.DeserializeObject<MessagePosition>(data.Content);
                    Player player = playerManager.listOfPlayer.Find(x => x.Id == playerPosition.id);
                    if (player == null) return;

                    player.UpdatePosition(playerPosition.Position);

                    result = ConvertToDataRequest(player.Id, player.position, MyMessageType.POSITION);
                    await SendToAllClients(result);

                    break;
                case MyMessageType.DESTROY:
                    MessageBase? messageBase = JsonConvert.DeserializeObject<MessageBase>(data.Content);
                    await DisconnectClient(clientSocket, messageBase.id);
                    break;
                case MyMessageType.TEXT:
                    MessageText messageText = JsonConvert.DeserializeObject<MessageText>(data.Content);
                    messageText.text = $"[{IndexOf(clientSocket)}]: " + messageText.text;

                    string content = JsonConvert.SerializeObject(messageText);
                    result = MyUtility.ConvertToDataRequestJson(content, MyMessageType.TEXT);
                    await SendToAllClients(result);
                    break;
                default:
                    break;
            }
        }


        //// Spawn a new player and send information to all clients.
        //private static async Task SpawnNewPlayer(Socket socket, int index = 0)
        //{
        //    Player newPlayer = playerManager.spawnManager.GetPrefab(index);
        //    playerManager.listOfPlayer.Add(newPlayer);

        //    string result = ConvertToDataRequest(newPlayer.Id, newPlayer.position, MyMessageType.CREATE);

        //    await SendToSingleClient(socket, result);
        //    await SendToAllClients(result);
        //    if (playerManager.listOfPlayer.Count > 1)
        //    {
        //        await SendInfoAboutExistingPlayers(socket);
        //    }
        //}


        //Handle multiple request from a client.

        private static async Task HandleManyRequest(string[] requests, Socket clientSocket)
        {
            foreach (string request in requests)
            {
                await HandleOneRequest(request, clientSocket);
            }
        }


        //Find the index of socket in the dictionary.
        private static int IndexOf(Socket socket)
        {
            return dictionarySocket.FirstOrDefault(kvp => kvp.Value == socket).Key;
        }

        private static async Task DisconnectClient(Socket socket, int idPlayer)
        {
            dictionarySocket[IndexOf(socket)].Close();
            dictionarySocket.Remove(IndexOf(socket));
            Player player = playerManager.listOfPlayer.Find(x => x.Id == idPlayer);
            playerManager.listOfPlayer.Remove(player);
        }

        public static string ConvertToDataRequest(int id, MyVector3 pos, MyMessageType type)
        {
            string content = MyUtility.ConvertToMessagePosition(id, pos);
            string result = MyUtility.ConvertToDataRequestJson(content, type);
            return result;
        }
    }
}