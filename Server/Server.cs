using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Diagnostics;
using System.Numerics;
using Newtonsoft.Json;
using MyLibrary;

namespace Server
{
    public class Server
    {
        #region Init
        private static Dictionary<int, Socket> dictionaryClientSocket = new Dictionary<int, Socket>();

        private static SpawnManager? spawnManager = new SpawnManager();
        private static Socket? listenSocket;
        private static List<Player> listOfPlayer = new List<Player>();

        static async Task Main()
        {
            InitSocket();
            await WaitConnect();
        }

        static void InitSocket()
        {
            IPEndPoint ipEndPoint = new(IPAddress.Parse("192.168.1.25"), 8522);

            listenSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listenSocket.Bind(ipEndPoint);
            listenSocket.Listen(100);
            Console.WriteLine("LISTENING...");
        }

        #endregion


        static async Task WaitConnect()
        {
            while (true)
            {
                var socket = await listenSocket.AcceptAsync();
                Random random = new Random();

                int randomKey = random.Next(1, 1001);
                dictionaryClientSocket.Add(randomKey, socket);
                Console.WriteLine("Has socket connected!");
                await SendToSingleClient(socket, randomKey.ToString());
                var taskListen = ListenClient(socket);
            }
        }

        private static async Task SpawnNewPlayer(int index, Socket socket)
        {
            Player newPlayer = spawnManager.GetPrefab(index);
            listOfPlayer.Add(newPlayer);

            string inforNewPlayer = ConvertToJson(newPlayer, MyMessageType.CREATE);
            await SendToSingleClient(socket, inforNewPlayer);
            await SendToAllClient(inforNewPlayer);
            if (listOfPlayer.Count > 1)
            {
                await SendInforOldPlayers(socket);
            }
        }

        private static async Task SendToSingleClient(Socket socket, string message)
        {
            string messageSpecify = message + '@';
            var sendBuffer = Encoding.UTF8.GetBytes(messageSpecify);
            await socket.SendAsync(sendBuffer, SocketFlags.None);
        }

        private static async Task SendToAllClient(string message)
        {
            foreach (var item in dictionaryClientSocket)
            {
                await SendToSingleClient(item.Value, message);
            }
        }

        private static async Task SendInforOldPlayers(Socket socket)
        {
            foreach (Player item in listOfPlayer)
            {
                await SendToSingleClient(socket, ConvertToJson(item, MyMessageType.CREATE));
            }
        }

        async static Task ListenClient(Socket clientSocket)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int messageCode = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                string messageReceived = Encoding.UTF8.GetString(buffer, 0, messageCode);
                if (messageCode == 0) return;
                string[] requests =  MyUtility.StringSplitArray(messageReceived);
                var t = HandleManyRequest(requests, clientSocket);
            }
        }

        private static async Task HandleOneRequest(string request, Socket clientSocket)
         {
            if (string.IsNullOrEmpty(request)) return;
            MyDataRequest? data = JsonConvert.DeserializeObject<MyDataRequest>(request);

            

            switch (data.MyRequestType)
            {
                case MyMessageType.CREATE:

                    var key = dictionaryClientSocket.FirstOrDefault(kvp => kvp.Value == clientSocket).Key;
                    await SpawnNewPlayer(0, dictionaryClientSocket[key]);
                    break;
                case MyMessageType.POSITION:
                    PlayerPosition? playerPosition = JsonConvert.DeserializeObject<PlayerPosition>(data.Content);
                    Player player = listOfPlayer.Find(x => x.Id == playerPosition.id);
                    Player sendPlayer = player;
                    if (player != null)
                    {
                        player.position = playerPosition.Vector3;
                        await SendToAllClient(ConvertToJson(player, MyMessageType.POSITION));
                    }
                    break;
                case MyMessageType.DESTROY:
                    PlayerPosition? playerPosition2 = JsonConvert.DeserializeObject<PlayerPosition>(data.Content);
                    var key2 = dictionaryClientSocket.FirstOrDefault(x => x.Value == clientSocket).Key;
                    await RemovePlayer(playerPosition2.id);
                    dictionaryClientSocket.Remove(key2);
                    break;
                case MyMessageType.TEXT:
                    Console.WriteLine(data.Content);
                    var key3 = dictionaryClientSocket.FirstOrDefault(kvp => kvp.Value == clientSocket).Key;
                    string contentWillSend = $"[{key3}]: " + data.Content;
                    MyDataRequest newDataRequest = new MyDataRequest();
                    newDataRequest.Content = contentWillSend;
                    newDataRequest.MyRequestType = MyMessageType.TEXT;
                    await SendToAllClient(JsonConvert.SerializeObject(newDataRequest));
                    break;
                default:
                    break;
            }
        }

        private static async Task HandleManyRequest(string[] requests, Socket clientSocket)
        {
            foreach (string request in requests)
            {
                await HandleOneRequest(request, clientSocket);
            }
        }

        private static async Task RemovePlayer(int id)
        {
            Player player = listOfPlayer.Find(x => x.Id == id);
            listOfPlayer.Remove(player);
        }


        private static string ConvertToJson(Player player, MyMessageType type)
        {
            PlayerPosition newplayerPosition = new PlayerPosition(player.Id, player.position);

            string content = JsonConvert.SerializeObject(newplayerPosition);
            MyDataRequest newDataRequest = new MyDataRequest();
            newDataRequest.MyRequestType = type;
            newDataRequest.Content = content;
            return JsonConvert.SerializeObject(newDataRequest);
        }
    }
}