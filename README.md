# DotNetChat
A client-server chat application developed as a school project using C# and .NET 8.

<img width="828" height="443" alt="image" src="https://github.com/user-attachments/assets/be567b6c-fbb0-43a3-97a2-0bc53bc86a46" />

## Features
- Real-time group chat using TCP sockets
- Private messaging between users using /pm username
- Session-based "authentication" using reconnect tokens
- Automatic reconnect support
- Chat history synchronization on login/reconnect
- Heartbeat system for connection stability
- Lightweight custom binary protocol (no JSON / no HTTP)

## Protocol

The protocol uses simple byte-based packets over TCP.

<details>
<summary>Packet IDs (click to expand)</summary>

### Client → Server

| ID | Description |
|----|-------------|
| 1 | Login |
| 2 | Disconnect |
| 3 | Send public message |
| 4 | Send private message |
| 5 | Sync request |
| 6 | Reconnect request |
| 7 | Heartbeat request |

### Server → Client

| ID | Description |
|----|-------------|
| 1 | Error response |
| 2 | Public chat message |
| 3 | Private message |
| 4 | Private message (sync only) |
| 5 | Sync response (chat history) |
| 6 | Reconnect success |
| 7 | Heartbeat response |

> IDs are shared between client and server depending on direction.

</details>

- All strings use UTF-16 encoding (Encoding.Unicode)


## Getting Started
### Prerequisites
This project targets .NET 8. Install the .NET 8 SDK or newer.

You can install the SDK using winget:

```winget install Microsoft.DotNet.SDK.8```

Verify the installation:

```dotnet --version```

### Building

Clone the Repository:

```git clone https://github.com/D3nn11s/DotNetChat```

Navigate to either the Client or Server project directory:
```
cd DotNetChat

cd Client
# or
cd Server
```
Build the project:

```dotnet build```

The compiled executable will be located in:

```bin/Debug/net8.0-windows/```
