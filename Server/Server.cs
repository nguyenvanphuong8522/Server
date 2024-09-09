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

        #region Call API
        //private static HttpClient _httpClient;

        //public static void CallApi()
        //{
        //    _httpClient = new HttpClient();
        //    Task callTask = CallRestApiAsync();
        //}

        //public static async Task CallRestApiAsync()
        //{
        //    string url = "https://localhost:7087/api/category";

        //    HttpResponseMessage response = await _httpClient.GetAsync(url);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        string responseBody = await response.Content.ReadAsStringAsync();
        //        //Debug.LogError(responseBody);
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
        //    }
        //}
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

            string inforNewPlayer = ConvertToJson(newPlayer, RequestType.CREATE);
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
                await SendToSingleClient(socket, ConvertToJson(item, RequestType.CREATE));
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
                string[] requests = ConvertToArrayString(messageReceived);
                var t = HandleManyRequest(requests, clientSocket);
            }
        }

        private static async Task HandleOneRequest(string request, Socket clientSocket)
        {

            if (string.IsNullOrEmpty(request)) return;

            if (request[0] != '{'&& request[request.Length - 1] != '}')
            {
                //Console.WriteLine(request);
                var key = dictionaryClientSocket.FirstOrDefault(kvp => kvp.Value == clientSocket).Key;
                await SendToAllClient($"[{key}]: " + request);
            }

            MyVector3? dataNewPlayer = JsonConvert.DeserializeObject<MyVector3>(request);
            if (dataNewPlayer == null) {
                return;
            }
            if (dataNewPlayer.type == RequestType.CREATE)
            {
                var key = dictionaryClientSocket.FirstOrDefault(kvp => kvp.Value == clientSocket).Key;
                await SpawnNewPlayer(0, dictionaryClientSocket[key]);
            }
            else if (dataNewPlayer.type == RequestType.POSITION)
            {
                Player player = listOfPlayer.Find(x => x.Id == dataNewPlayer.id);
                Player sendPlayer = player;
                if (player != null)
                {
                    //Console.WriteLine(player.position);
                    player.position = new Vector3(dataNewPlayer.x, dataNewPlayer.y, dataNewPlayer.z);

                    await SendToAllClient(ConvertToJson(player, dataNewPlayer.type));
                }
            }
            else if(dataNewPlayer.type == RequestType.DESTROY)
            {
                var key = dictionaryClientSocket.FirstOrDefault(x => x.Value == clientSocket).Key;
                await RemovePlayer(dataNewPlayer.id);
                dictionaryClientSocket.Remove(key);
            }
            else
            {
                Console.WriteLine(request);
            }
        }

        private static async Task HandleManyRequest(string[] requests, Socket clientSocket)
        {
            foreach (string request in requests)
            {
                await HandleOneRequest(request, clientSocket);
            }
        }

        private static string[] ConvertToArrayString(string message)
        {
            string[] array = message.Split('@').Where(part => !string.IsNullOrWhiteSpace(part)).ToArray(); ;
            return array;
        }

        private static async Task RemovePlayer(int id)
        {
            Player player = listOfPlayer.Find(x => x.Id == id);
            listOfPlayer.Remove(player);
        }


        private static string ConvertToJson(Player player, RequestType type)
        {
            Vector3 curPos = player.position;
            MyVector3 newVector3 = new MyVector3(curPos.X, curPos.Y, curPos.Z, type);
            newVector3.id = player.Id;
            newVector3.type = type;

            string result = JsonConvert.SerializeObject(newVector3);
            return result;
        }


        //private void o()
        //{
        //    foreach (var item in dictionaryClientSocket)
        //    {
        //        item.Value.Shutdown(SocketShutdown.Both);
        //    }
        //}
    }
}