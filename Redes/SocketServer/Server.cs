using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server
{
    public static Dictionary<string, PersistentSocket>  clientsHashTable = new Dictionary<string, PersistentSocket> ();

    public static int Main(String[] args)
    {
        StartServer();
        return 0;
    }

    public static void StartServer()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        try
        {
            // Create a Socket that will use Tcp protocol
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method
            listener.Bind(localEndPoint);
            listener.Listen(10); // We will listen 10 requests at a time

            Console.WriteLine("Started listening for requests");
            Socket socket;

            while (true)
            {
                socket = listener.Accept();
                PersistentSocket persistentSocket = new PersistentSocket(socket);

                persistentSocket.CreateMonitoringThread();
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\n Press any key to continue...");
        Console.ReadKey();
    }

    public class PersistentSocket
    {
        Socket SocketConnection { get; set; }
        IPAddress Address { get; set; }
        public string Name { get; set; }

        public PersistentSocket(Socket socketConnection)
        {
            this.SocketConnection = socketConnection;
            this.Address = IPAddress.Parse(((IPEndPoint)SocketConnection.RemoteEndPoint).Address.ToString());
            
            string incomingDataFromClient = FetchReceivedMessage();
            this.Name = incomingDataFromClient;

            clientsHashTable.Add(incomingDataFromClient, this);

            byte[] msg = Encoding.ASCII.GetBytes(incomingDataFromClient + " Joined");
            Console.WriteLine("User {0} joined the chat from {1}", incomingDataFromClient, Address.ToString());

            PropagateMessage(msg);
        }

        public void CreateMonitoringThread()
        {
            Thread maintainedSocketThread = new Thread(MonitorSocket);
            maintainedSocketThread.Start();
        }

        public void MonitorSocket()
        {
            while (true)
            {
                string incomingDataFromClient = "";
                byte[] incomingDataBytes = new byte[1024];

                int bytesReceived = SocketConnection.Receive(incomingDataBytes);
                incomingDataFromClient += Encoding.ASCII.GetString(incomingDataBytes, 0, bytesReceived);

                byte[] msg;

                Console.WriteLine("User {0}: {1}", Address.ToString(), incomingDataFromClient);
                msg = Encoding.ASCII.GetBytes(Name + " sent " + incomingDataFromClient);

                PropagateMessage(msg);
            }
        }

        private string FetchReceivedMessage()
        {
            byte[] incomingDataBytes = new byte[1024];

            int bytesReceived = SocketConnection.Receive(incomingDataBytes);
            return Encoding.ASCII.GetString(incomingDataBytes, 0, bytesReceived);
        }

        public void PropagateMessage(byte[] msg) {
            foreach (var clientEntry in clientsHashTable)
            {
                PersistentSocket targetSocket = clientEntry.Value;
                targetSocket.SocketConnection.Send(msg);
            }
        }
    }
}