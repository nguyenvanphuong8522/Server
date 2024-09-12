using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using MyLibrary;
using System.Numerics;

namespace Server
{
    public class Server
    {
        public static Socket? listenSocket;
        private const int port = 8522;
        private const string ipAddress = "192.168.1.25";
        private static IPEndPoint? ipEndPoint;

        static async Task Main()
        {
            SetUpData();
            StartServer();
            await ConnectionManager.WaitConnect();
        }

        private static void SetUpData()
        {
            ipEndPoint = new(IPAddress.Parse(ipAddress), port);
            ConnectionManager.Initialize();
            PlayerManager.Initialize();
        }

        static void StartServer()
        {
            if (ipEndPoint != null)
            {
                listenSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(ipEndPoint);
                listenSocket.Listen();
                Console.WriteLine("LISTENING...");
            }
        }
    }
}