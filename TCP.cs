using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using DotNetEnv;

class Program
{
    const int Port = 12345;
    static byte[] Key;
    static byte[] IV;
    static volatile bool isRunning = true;
    static CancellationTokenSource cts = new CancellationTokenSource();
    static string? userName;
    
    static async Task ReadLoop(NetworkStream stream)
    {
        try
        {
            byte[] lengthBuffer = new byte[4];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4);
                if (bytesRead == 0) break;

                int length = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] buffer = new byte[length];

                int totalRead = 0;
                while (totalRead < length)
                {
                    int read = await stream.ReadAsync(buffer, totalRead, length - totalRead);
                    if (read == 0) break;
                    totalRead += read;
                }

                string message = DecryptMessage(buffer);
                Console.WriteLine($"\n{message}");
                Console.Write("> ");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[Read error] {ex.Message}");
        }
    }

    static async Task WriteLoop(NetworkStream stream)
    {
        try
        {
            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.ToLower() == "exit")
                {
                    stream.Close();
                    Environment.Exit(0);
                }

                string fullMessage = $"{userName}: {input}";
                byte[] encrypted = EncryptMessage(fullMessage);

                byte[] lengthPrefix = BitConverter.GetBytes(encrypted.Length);
                await stream.WriteAsync(lengthPrefix);
                await stream.WriteAsync(encrypted);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[Write error] {ex.Message}");
        }
    }

    static byte[] EncryptMessage(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using var writer = new StreamWriter(cs);
        writer.Write(plainText);
        writer.Flush();
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    static string DecryptMessage(byte[] cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using var ms = new MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        return reader.ReadToEnd();
    }

    static void LogToFile(string filePath, string message)
    {
        File.AppendAllText(filePath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    static async Task Main(string[] args)
    {
        Env.Load(@"C:\Users\egork\source\repos\Мои проекты\TCP P2P\.env");
        Console.Title = "[P2P Chat]";
        Key = Encoding.UTF8.GetBytes(Env.GetString("AES_KEY"));
        IV = Encoding.UTF8.GetBytes(Env.GetString("AES_IV"));

        Console.Write("Enter your name: ");
        userName = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(userName))
            userName = "User";

        Console.Write("Do you want to host the chat? (y/n): ");
        var isHost = Console.ReadLine()?.Trim().ToLower() == "y";

        TcpClient client;

        if (isHost)
        {
            string localIp = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToString();
            Console.WriteLine($"Your IP: {localIp}\n");
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine($"Waiting for incoming connection on port {Port}...");
            client = await listener.AcceptTcpClientAsync();
            listener.Stop();
            Console.WriteLine("Client connected.");
            
            var remoteIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            Console.WriteLine($"Connected with peer IP: {remoteIp}");
        }
        else
        {
            string localIp = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToString();
            Console.WriteLine($"Your IP: {localIp}\n");
            Console.Write("Enter host IP address(enter for 127.0.0.1): ");
            string? remoteIpInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(remoteIpInput)) remoteIpInput = "127.0.0.1";

            client = new TcpClient();
            await client.ConnectAsync(remoteIpInput, Port);
            Console.WriteLine("Connected to host.");
            
            var remoteIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            Console.WriteLine($"Connected to host. Peer IP: {remoteIp}");
        }

        var stream = client.GetStream();

        // Запуск чтения и записи одновременно
        var readTask = Task.Run(() => ReadLoop(stream));
        var writeTask = Task.Run(() => WriteLoop(stream));

        await Task.WhenAny(readTask, writeTask);
    }
}