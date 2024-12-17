using System.Net.Sockets;
using System.Text;

namespace ls1_client
{
    public class Client
    {
        private static TcpClient client { get; set; } = new TcpClient();
        private static NetworkStream stream { get; set; } = null;
        private static string Username { get; set; }
        private static string Password { get; set; }
        static async Task Main(string[] args)
        {
            //{ "User1", "password1" } або { "User2", "password2" }
            Console.Write("Enter your username: ");
            Username = Console.ReadLine();
            Console.Write("Enter your password: ");
            Password = Console.ReadLine();
            client = new TcpClient("127.0.0.1", 5000);
            stream = client.GetStream();
            string userInfo = $"{Username} {Password}";
            byte[] buffer = Encoding.UTF8.GetBytes(userInfo);
            await stream.WriteAsync(buffer, 0, buffer.Length);

            Task receiveTask = ReceiveMessageAsync();
            Console.Write("Conected to server. To exit use /quit. For private message use /pm username and you message.\nEnter your message: ");
            while (true)
            {
                try
                {
                    string message = Console.ReadLine();
                    buffer = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    if (message == "/quit")
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            client.Close();

        }

        static async Task ReceiveMessageAsync()
        {
            try
            {
                byte[] buffer = new byte[1024];
                StringBuilder stringBuilder = new StringBuilder();
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    Console.WriteLine(stringBuilder.ToString());
                    stringBuilder.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
