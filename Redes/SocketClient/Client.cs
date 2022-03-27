using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client
{
    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }

    public static void StartClient()
    {
        try
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}", sender?.RemoteEndPoint?.ToString());

                PersistentSocket persistent = new PersistentSocket(sender);

                persistent.CreateIncomingOutgoingThread();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public class PersistentSocket
    {
        Socket SocketConnection { get; set; }

        public PersistentSocket(Socket socketConnection)
        {
            this.SocketConnection = socketConnection;
        }

        public void CreateIncomingOutgoingThread()
        {
            Thread incomingSocketThread = new Thread(HandleIncoming);
            incomingSocketThread.Start();

            Thread outgoingSocketThread = new Thread(HanldeOutgoing);
            outgoingSocketThread.Start();
        }

        public void HandleIncoming()
        {
            byte[] bytes = new byte[1024];

            while (true)
            {
                int bytesRec = SocketConnection.Receive(bytes);
                Console.WriteLine("> {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
            }
        }

        public void HanldeOutgoing()
        {
            while (true)
            {
                string? message = Console.ReadLine();
                byte[] msg = Encoding.ASCII.GetBytes(message);

                // Send the data through the socket.
                int bytesSent = SocketConnection.Send(msg);
            }
        }
    }
}