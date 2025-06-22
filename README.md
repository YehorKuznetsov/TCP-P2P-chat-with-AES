# TCP P2P Chat with AES Encryption

This is a simple peer-to-peer (P2P) chat application built in C# that uses TCP sockets for communication and AES encryption for secure message transfer. The application allows two users to exchange encrypted messages directly over the network.

## Features

- Secure message exchange using AES (Advanced Encryption Standard)
- Encrypted communication with shared key and IV
- Peer-to-peer TCP connection (no central server required)
- Chat logging to a local file
- Graceful shutdown with Ctrl+C or "exit" command
- Cross-platform compatible (Windows, Linux via .NET)

## Getting Started

### Prerequisites

- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- Git
- Optional: Rider / Visual Studio / VS Code

### Clone the Repository
git clone https://github.com/YOUR_USERNAME/TCP-P2P-chat-with-AES.git
cd TCP-P2P-chat-with-AES

Configuration
Create a .env file in the root directory with the following content:

AES_KEY=Your32CharacterLongKeyHere123456
AES_IV=Your16CharIV12345

AES_KEY must be 32 bytes
AES_IV must be 16 bytes

Build and Run
dotnet build
dotnet run

The app will:

Start listening for incoming messages

Prompt you for your name

Ask for the IP address of the peer

Start the chat loop

File Structure

/TCP-P2P-chat-with-AES
├── TCP.cs             // Main logic
├── TCP P2P.csproj     // Project file
├── TCP P2P.sln        // Solution file
├── .gitignore
└── README.md
Logging
All received messages are stored in chatlog.txt with timestamps.

Security Notes
The key and IV are static in .env. In production use, consider a secure key exchange mechanism (e.g., Diffie–Hellman).

AES is used in CBC mode via .NET's built-in Aes.Create().

License
This project is open-source and licensed under the MIT License.

Author
Developed by Yehor Kuznetsov
