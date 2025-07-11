using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureChat4InARow
{
    public partial class ServerForm : Form
    {
        private TcpListener listener;
        private static UdpClient socketStop;
        private static IPEndPoint requestPoint;

        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        private volatile bool isClosing = false;

        // Debugging: toggle AES encryption
        //private bool encryptionEnabled = false;

        string privateKeyXml, publicKeyXml;

        private readonly List<ClientSession> connectedClients = new List<ClientSession>();
        private readonly object clientListLock = new object();

        public ServerForm()
        {
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            Foundation.RegisterLogControl(Logs.ServerLog, ServerLog);
            StartServer();
        }

        private void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, Foundation.SERVERPORT); //creates a listener for clinets (ip and port) to connect
            listener.Server.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReuseAddress, true); //allows to reuse the socket
            listener.Start();

            #region StopOtherServers
            void StopOtherServers()
            {
                socketStop = new UdpClient(); //creates a udp client to send deny for others who ask to be the server
                socketStop.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); //allows to reuse the socket
                socketStop.Client.Bind(new IPEndPoint(IPAddress.Any, Foundation.GUARD_PORT)); //makes the client to lister on a specific port for all ip 
                requestPoint = new IPEndPoint(IPAddress.Any, 0); //will be the info about the one who asks to be the server

                var token = cancelTokenSource.Token;
                Foundation.TrdStart(() =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            byte[] data = socketStop.Receive(ref requestPoint); // waits for request
                            if (data.Length == 1 && (ServerMessageType)data[0] == ServerMessageType.CanBeServer)
                            {
                                byte[] deny = { (byte)ServerMessageType.Deny }; // create deny message
                                socketStop.Send(deny, deny.Length, requestPoint); // send deny
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            Foundation.LogPrint(Logs.ServerLog, $"UDP error: {ex.Message}", Foundation.errorColor);
                        }
                    }
                });
            }
            #endregion
            StopOtherServers();

            Foundation.LogPrint(Logs.ServerLog, $"Server started on port {Foundation.SERVERPORT}", Color.Green);

            (privateKeyXml, publicKeyXml) = Enc.GenerateRsaKeyPair();
            Foundation.LogPrint(Logs.ServerLog, $"Generated RSA keys", Foundation.encryptionColor);

            Foundation.TrdStart(ListenForClients);
            Foundation.LogPrint(Logs.ServerLog, $"Waiting for clients connection", Color.Green);
        }

        private void ListenForClients()
        {
            var token = cancelTokenSource.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    TcpClient client = listener.AcceptTcpClient(); //waits for client to connect

                    IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint; //takes the ip and the port of the client
                    string clientIpPort = $"{clientEndPoint.Address}:{clientEndPoint.Port}";

                    Task.Run(() => HandleClient(client, clientIpPort, token)); // Handle the client in its own task/thread
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    Foundation.LogPrint(Logs.ServerLog, $"Server error: {ex.Message}", Foundation.errorColor);
                }
            }
        }

        private async Task HandleClient(TcpClient client, string clientIpPort, CancellationToken token)

        {
            #region BroadcastChat
            void BroadcastChat(string message, ClientSession sender)

            {
                lock (clientListLock)
                {
                    foreach (ClientSession s in connectedClients)
                    {
                        if (s.Client != sender.Client && s.Client.Connected)
                        {
                            try
                            {
                                byte[] encrypted = Enc.EncryptWithAes(message, s.AesKey); //encryptes the message to broadcast with the clients AES key

                                //testing encryption
                                //byte[] encrypted;

                                //if (encryptionEnabled)
                                //{
                                //    encrypted = Enc.EncryptWithAes(message, s.AesKey);
                                //}
                                //else
                                //{
                                //    // Plaintext for debugging (not secure!)
                                //    encrypted = Encoding.UTF8.GetBytes(message);
                                //}

                                byte[] data = new byte[1 + encrypted.Length]; //creates data array for the message
                                data[0] = (byte)ServerMessageType.Chat; //adds message type
                                Array.Copy(encrypted, 0, data, 1, encrypted.Length); //inserts encrypted message to data array

                                s.Stream.Write(data, 0, data.Length); //sends data to client
                            }
                            catch (Exception ex)
                            {
                                Foundation.LogPrint(Logs.ServerLog, $"Broadcast error: {ex.Message}", Foundation.errorColor);
                            }
                        }
                    }

                }
            }
            #endregion
            try
            {
                using (NetworkStream stream = client.GetStream()) //to read and write bytes between client and server (using to close the stream at the end of use)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    #region Keys Exchange
                    // Read client's public key
                    //int clientPubKeyLen = await stream.ReadAsync(buffer, 0, buffer.Length); //reads client publickey length
                    //string clientPublicKeyXml = Encoding.UTF8.GetString(buffer, 0, clientPubKeyLen); //reads client publickey
                    //LogPrint($"Received client public key ({clientIpPort})", Color.DarkGray);

                    //send server public key
                    byte[] serverPubKeyBytes = Encoding.UTF8.GetBytes(publicKeyXml); //turns server public key to bytes
                    byte[] serverPubKeyMsg = new byte[1 + serverPubKeyBytes.Length]; //creates a message of bytes for type and public key
                    serverPubKeyMsg[0] = (byte)ServerMessageType.PublicKey; //inserts type of message
                    Array.Copy(serverPubKeyBytes, 0, serverPubKeyMsg, 1, serverPubKeyBytes.Length); //copies bytes public key to message
                    await stream.WriteAsync(serverPubKeyMsg, 0, serverPubKeyMsg.Length); //sends message to client
                    Foundation.LogPrint(Logs.ServerLog, $"Sent server public key to client at {clientIpPort}", Foundation.encryptionColor);

                    //wait for AES session key
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); //waits for AES key from client and inserts to buffer
                    if (buffer[0] != (byte)ServerMessageType.SessionAESKey)
                    {
                        Foundation.LogPrint(Logs.ServerLog, "Expected AES session key message", Foundation.failColor);
                        return;
                    }

                    byte[] encryptedAesKey = buffer.Skip(1).Take(bytesRead - 1).ToArray(); //skips the message type and extracts the encrypted AES key
                    byte[] sessionAesKey;
                    using (RSA rsa = Enc.CreateFromPrivateKey(privateKeyXml))
                    {
                        sessionAesKey = rsa.Decrypt(encryptedAesKey, RSAEncryptionPadding.Pkcs1); //decrypts the AES key using the servers private key
                    }
                    Foundation.LogPrint(Logs.ServerLog, $"Secure AES session key established from {clientIpPort}", Foundation.encryptionColor);

                    #endregion

                    #region Authentication and Login result
                    //authentication
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); //wait for message from client and inserts it into buffer
                    if (bytesRead == 0) return;

                    ServerMessageType type = (ServerMessageType)buffer[0];

                    if (type != ServerMessageType.Login && type != ServerMessageType.Register)
                    {
                        Foundation.LogPrint(Logs.ServerLog, "First message must be Login or Register", Foundation.failColor);
                        return;
                    }
                    byte[] encryptedPayload = buffer.Skip(1).Take(bytesRead - 1).ToArray();
                    string decryptedPayload = Enc.DecryptWithAes(encryptedPayload, sessionAesKey);
                    string[] parts = decryptedPayload.Split(':'); //takes rest of message (not type) and splits by ":"
                    if (parts.Length != 2)
                    {
                        Foundation.LogPrint(Logs.ServerLog, "Invalid credential format", Foundation.failColor);
                        return;
                    }

                    string username = parts[0];
                    string password = parts[1];
                    bool success = false;

                    if (type == ServerMessageType.Login)
                        success = UserManager.ValidateUser(username, password);
                    else
                        success = UserManager.RegisterUser(username, password);

                    //send login result to client
                    string response = success ? "OK" : (type == ServerMessageType.Register ? "Username already taken" : "Invalid credentials");
                    byte[] encryptedResponse = Enc.EncryptWithAes(response, sessionAesKey);
                    byte[] reply = new byte[1 + encryptedResponse.Length];
                    reply[0] = (byte)ServerMessageType.LoginResult;
                    Array.Copy(encryptedResponse, 0, reply, 1, encryptedResponse.Length);
                    await stream.WriteAsync(reply, 0, reply.Length);


                    if (!success)
                    {
                        Foundation.LogPrint(Logs.ServerLog, $"Auth failed for client [{username}] at {clientIpPort}", Foundation.failColor);
                        return;
                    }

                    if (type == ServerMessageType.Register)
                    {
                        Foundation.LogPrint(Logs.ServerLog, $"User [{username}] authenticated and registered from {clientIpPort}", Foundation.connectingColor);
                    }
                    else
                    {
                        Foundation.LogPrint(Logs.ServerLog, $"User [{username}] authenticated and logged in from {clientIpPort}", Foundation.connectingColor);
                    }

                    var session = new ClientSession
                    {
                        Client = client,
                        Username = username,
                        AesKey = sessionAesKey
                    };

                    lock (clientListLock)
                    {
                        connectedClients.Add(session);
                    }

                    #endregion

                    while (true)
                    {
                        #region Reads and decryptes message from client
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token); //counts the bytes passed from the client and inserts the message into buffer

                        if (bytesRead == 0) { break; }

                        type = (ServerMessageType)buffer[0];

                        encryptedPayload = buffer.Skip(1).Take(bytesRead - 1).ToArray(); //extract the encrypted msg
                        decryptedPayload = "";
                        if (encryptedPayload.Length != 0)
                        {
                            decryptedPayload = Enc.DecryptWithAes(encryptedPayload, sessionAesKey); //decrypts the msg using AES key
                        }
                        #endregion

                        if (type == ServerMessageType.Chat)
                        {
                            #region Testing Encryption
                            //LogPrint($"Encrypted data: {BitConverter.ToString(encryptedPayload)}", Color.Gray);
                            //string msg = Encoding.UTF8.GetString(encryptedPayload);
                            //sessionAesKey[0] ^= 0xFF;
                            //string msg;
                            //if (encryptionEnabled)
                            //{
                            //    msg = Enc.DecryptWithAes(encryptedPayload, session.AesKey);
                            //}
                            //else
                            //{
                            //    msg = Encoding.UTF8.GetString(encryptedPayload);
                            //}
                            #endregion

                            string msg = decryptedPayload;

                            Foundation.LogPrint(Logs.ServerLog, $"User [{username}] says: {msg}", Foundation.chatColor);
                            BroadcastChat($"User [{username}]: {msg}", session);

                        }

                        #region Game Handling
                        else if (type == ServerMessageType.GameRequest)
                        {
                            string opponentUsername = decryptedPayload;

                            ClientSession opponent;
                            lock (clientListLock)
                            {
                                opponent = connectedClients.FirstOrDefault(s => s.Username == opponentUsername);
                            }

                            if (opponent != null)
                            {
                                if (opponent.IsInGame || session.IsInGame)
                                {
                                    byte[] userInGameMsg = new byte[] { (byte)ServerMessageType.UserInGame };
                                    session.Stream.Write(userInGameMsg, 0, userInGameMsg.Length);
                                }
                                else
                                {
                                    Foundation.LogPrint(Logs.ServerLog, $"Received game request from user [{session.Username}] to user [{opponent.Username}]", Color.Green);
                                    await Foundation.SendEncryptedMessage(opponent.Stream, ServerMessageType.GameRequest, session.Username, opponent.AesKey);


                                    Foundation.LogPrint(Logs.ServerLog, $"Game request from user [{session.Username}] to user [{opponent.Username}] was sent", Color.Green);
                                }
                            }
                            else
                            {
                                Foundation.LogPrint(Logs.ServerLog, $"Game request from user [{session.Username}] was sent to unconnected user", Foundation.failColor);
                                byte[] unconnectedUserMsg = new byte[] { (byte)ServerMessageType.UnconnectedUser };
                                session.Stream.Write(unconnectedUserMsg, 0, unconnectedUserMsg.Length);
                            }
                        }

                        else if (type == ServerMessageType.GameDecline)
                        {
                            string initiatorUsername = decryptedPayload;

                            ClientSession initiator;
                            lock (clientListLock)
                            {
                                initiator = connectedClients.FirstOrDefault(c => c.Username == initiatorUsername);
                            }

                            if (initiator != null)
                            {
                                //send decline notification to initiator 
                                await Foundation.SendEncryptedMessage(initiator.Stream, ServerMessageType.GameDecline, session.Username, initiator.AesKey);

                                Foundation.LogPrint(Logs.ServerLog, $"Game request declined from user [{session.Username}]", Foundation.failColor);
                            }
                        }

                        else if (type == ServerMessageType.GameAccept)
                        {
                            string initiatorUsername = decryptedPayload;

                            ClientSession initiator, opponent;
                            lock (clientListLock)
                            {
                                initiator = connectedClients.FirstOrDefault(s => s.Username == initiatorUsername);
                                opponent = session;
                            }

                            if (initiator != null)
                            {
                                string player1Name = initiator.Username;
                                string player2Name = opponent.Username;

                                string payloadForInitiator = $"{player1Name}:{player2Name}";
                                string payloadForOpponent = $"{player1Name}:{player2Name}";

                                byte[] encForInitiator = Enc.EncryptWithAes(payloadForInitiator, initiator.AesKey);
                                byte[] encForOpponent = Enc.EncryptWithAes(payloadForOpponent, opponent.AesKey);

                                byte[] initiatorMsg = new byte[1 + encForInitiator.Length];
                                initiatorMsg[0] = (byte)ServerMessageType.GameStart;
                                Array.Copy(encForInitiator, 0, initiatorMsg, 1, encForInitiator.Length);

                                byte[] opponentMsg = new byte[1 + encForOpponent.Length];
                                opponentMsg[0] = (byte)ServerMessageType.GameStart;
                                Array.Copy(encForOpponent, 0, opponentMsg, 1, encForOpponent.Length);

                                initiator.Stream.Write(initiatorMsg, 0, initiatorMsg.Length);
                                opponent.Stream.Write(opponentMsg, 0, opponentMsg.Length);

                                initiator.GamePartner = opponent.Username;
                                opponent.GamePartner = initiator.Username;

                                Foundation.LogPrint(Logs.ServerLog, $"Game accepted from user [{initiator.Username}] to user [{opponent.Username}]", Color.Green);
                            }
                        }

                        else if (type == ServerMessageType.GameMove)
                        {
                            ClientSession opponent;
                            lock (clientListLock)
                            {
                                opponent = connectedClients.FirstOrDefault(c => c.Username == session.GamePartner);
                            }

                            if (opponent != null)
                            {
                                try
                                {
                                    //passes game move to opponent
                                    string gameMove = decryptedPayload;
                                    await Foundation.SendEncryptedMessage(opponent.Stream, ServerMessageType.GameMove, gameMove, opponent.AesKey);
                                }
                                catch (Exception ex)
                                {
                                    Foundation.LogPrint(Logs.ServerLog, $"GameMove forward failed: {ex.Message}", Foundation.errorColor);
                                }

                            }
                            else
                            {
                                Foundation.LogPrint(Logs.ServerLog, $"GameMove: opponent [{session.GamePartner}] not found.", Foundation.failColor);
                            }
                        }

                        else if (type == ServerMessageType.GameOver)
                        {
                            ClientSession opponent;
                            lock (clientListLock)
                            {
                                opponent = connectedClients.FirstOrDefault(c => c.Username == session.GamePartner);
                            }

                            if (opponent != null)
                            {
                                string gameResult = decryptedPayload;
                                //passes result of game from player to another
                                await Foundation.SendEncryptedMessage(opponent.Stream, ServerMessageType.GameOver, gameResult, opponent.AesKey);

                                opponent.GamePartner = null;
                                session.GamePartner = null;

                                Foundation.LogPrint(Logs.ServerLog, $"Game ended between user [{session.Username}] and user [{opponent.Username}]", Foundation.closingColor);
                            }
                        }

                        else if (type == ServerMessageType.OpponentLeft)
                        {
                            string leaverUsername = decryptedPayload;

                            ClientSession opponent;
                            lock (clientListLock)
                            {
                                opponent = connectedClients.FirstOrDefault(c => c.Username == session.GamePartner);
                            }

                            if (opponent != null)
                            {
                                //passes to opponent name of user that left the game
                                await Foundation.SendEncryptedMessage(opponent.Stream, ServerMessageType.OpponentLeft, leaverUsername, opponent.AesKey);

                                Foundation.LogPrint(Logs.ServerLog, $"User [{leaverUsername}] left the game. Notified opponent [{opponent.Username}]", Foundation.failColor);
                            }

                            //clear game partner relationships
                            session.GamePartner = null;
                            if (opponent != null) opponent.GamePartner = null;
                        }

                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    Foundation.LogPrint(Logs.ServerLog, $"Connection error ({clientIpPort}): {ex.Message}", Foundation.errorColor);
                }
            }
            finally
            {
                ClientSession sessionToRemove = null;

                lock (clientListLock)
                {
                    sessionToRemove = connectedClients.FirstOrDefault(s => s.Client == client);
                    if (sessionToRemove != null)
                        connectedClients.Remove(sessionToRemove);
                }
                if (sessionToRemove != null)
                {
                    Foundation.LogPrint(Logs.ServerLog, $"User [{sessionToRemove.Username}] disconnected from {clientIpPort}", Foundation.failColor);
                    if (sessionToRemove.IsInGame)
                    {
                        ClientSession opponent;
                        lock (clientListLock)
                        {
                            opponent = connectedClients.FirstOrDefault(s =>
                                s.Username == sessionToRemove.GamePartner);
                        }

                        if (opponent != null)
                        {
                            try
                            {
                                //sending handeled opponent that handled left the game
                                string quiterUsername = sessionToRemove.Username;

                                await Foundation.SendEncryptedMessage(opponent.Stream, ServerMessageType.OpponentLeft, quiterUsername, opponent.AesKey);

                                opponent.GamePartner = null;

                                Foundation.LogPrint(Logs.ServerLog, $"Notified opponent [{opponent.Username}] that user [{sessionToRemove.Username}] disconnected", Foundation.failColor);
                            }
                            catch (Exception ex)
                            {
                                Foundation.LogPrint(Logs.ServerLog, $"Failed to notify opponent [{opponent?.Username}] of disconnect: {ex.Message}", Foundation.errorColor);
                            }
                        }
                    }

                }
            }
        }

        private async void ServerForm_Closing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            if (!isClosing)
            {
                isClosing = true;

                #region SendShutdownMessage & CloseServer
                void SendShutdownMessage(ClientSession session)
                {
                    try
                    {
                        byte[] shutdownMsg = new byte[] { (byte)ServerMessageType.ServerShutdown };

                        session.Stream.Write(shutdownMsg, 0, shutdownMsg.Length);
                        session.Stream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Foundation.LogPrint(Logs.ServerLog, $"Error sending shutdown: {ex.Message}", Foundation.errorColor);
                    }
                }

                void CloseServer()
                {
                    cancelTokenSource.Cancel();

                    socketStop?.Close();

                    try
                    {
                        listener?.Stop();
                        Foundation.LogPrint(Logs.ServerLog, "Stopping listener...", Foundation.closingColor);
                    }
                    catch (Exception ex)
                    {
                        Foundation.LogPrint(Logs.ServerLog, $"Error stopping listener: {ex.Message}", Foundation.errorColor);
                    }

                    lock (clientListLock)
                    {
                        foreach (var session in connectedClients)
                        {
                            try
                            {
                                SendShutdownMessage(session);
                                session.Client?.Close();
                            }
                            catch { }
                        }


                        connectedClients.Clear();
                    }

                    Foundation.LogPrint(Logs.ServerLog, "Server shutdown complete. Will close in 5 seconds.", Foundation.closingColor);
                }
                #endregion

                CloseServer();
                await Task.Delay(7000);

                this.FormClosing -= ServerForm_Closing;
                this.Close();
            }
        }

        public class ClientSession
        {
            public TcpClient Client { get; set; }
            public string Username { get; set; }
            public byte[] AesKey { get; set; }
            public string GamePartner { get; set; }
            public bool IsInGame => GamePartner != null;
            public NetworkStream Stream => Client.GetStream();
        }
    }
}
