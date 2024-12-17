using System.Net;
using System.Net.Sockets;
using System.Text;

namespace np_hw1_server
{
    public class ChatServer
    {
        public List<Client> Clients = new List<Client>();
        private TcpListener Listener { get; set; }
        private Dictionary<string, string> UserInfo = new Dictionary<string, string>() { { "User1", "password1" }, { "User2", "password2" } };
        private int Port { get; set; }
        public ChatServer(int port)
        {
            Port = port;
        }
        public async Task Listen()
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();
            Console.WriteLine("Server started and listening...");
            while (true)
            {
                TcpClient tcpclient = await Listener.AcceptTcpClientAsync();
                Client client = new Client { _Client = tcpclient, _Server = this  };
                client.stream = tcpclient.GetStream();
                Clients.Add(client);
                client.HandleClientAsync();
            }
        }
        public void BroadCastMessage(string message, Client client)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            foreach (var c in Clients)
            {
                if (c != client)
                {
                    client.stream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
        }
        public void DeleteClient(Client client)
        {
            Clients.Remove(client);
        }
        public bool AuthUser(string username, string password)
        {
            if(UserInfo.TryGetValue(username, out var _password)) return _password == password;
            return false;
        }
    }
    public class Client
    {
        public TcpClient _Client { get; set; }
        public string Username { get; set; }
        public NetworkStream stream { get; set; }
        public ChatServer _Server { get; set; }
        public async void HandleClientAsync()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string[] userInfo = Encoding.UTF8.GetString(buffer, 0, bytesRead).Split(' ');
            if(userInfo.Length != 2 || !_Server.AuthUser(userInfo[0], userInfo[1]))
            {
                Console.WriteLine("Authentication failed");
                _Client.Close();
                return;
            }
            Username = userInfo[0];
            Console.WriteLine($"{Username} has joined");

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if(message == "/quit")
                    {
                        Console.WriteLine($"{Username} has left the chat");
                        _Server.DeleteClient(this);
                        _Client.Close();
                        break;
                    }
                    else if (message.StartsWith("/pm"))
                    {
                        string[] newMessage = message.Split(' ', 3);
                        if(newMessage.Length >= 3)
                        {
                            string targetUser = newMessage[1];
                            string pm = $"Private message from {Username}: " + newMessage[2];
                            SendPM(targetUser, pm);
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"{Username}: {message}");
                        _Server.BroadCastMessage($"{Username}: {message}", this);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SendPM(string targetUsername, string message)
        {
            Client targetClient = _Server.Clients.FirstOrDefault(u => u.Username == targetUsername);
            if (targetClient != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                targetClient.stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
    public class Program
    {
        static async Task Main(string[] args)
        {
            ChatServer chat = new ChatServer(5000);
            await chat.Listen();
        }
    }
}
