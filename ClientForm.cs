using System;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureChat4InARow
{
    public partial class ClientForm : Form
    {
        private TcpClient client;
        private readonly NetworkStream stream;
        private readonly string username;
        private readonly byte[] sessionAesKey;

        private GameForm gameForm;

        public ClientForm(TcpClient client, NetworkStream stream, string username, byte[] sessionAesKey)
        {
            InitializeComponent();

            this.client = client;
            this.stream = stream;
            this.username = username;
            this.sessionAesKey = sessionAesKey;
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            try
            {
                Foundation.RegisterLogControl(Logs.ClientLog, ClientLog);

                Foundation.LogPrint(Logs.ClientLog, "Connected to server!", Color.Green);
                usernameLabel.Text = "Logged to: " + username;

                async Task ListenToServerAsync(NetworkStream actStream)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    try
                    {
                        while (true)
                        {
                            #region Receive message from server and decrypts
                            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); //counts the bytes passed from the client and inserts the message into buffer

                            if (bytesRead == 0) { break; }

                            ServerMessageType type = (ServerMessageType)buffer[0];

                            byte[] encryptedPayload = buffer.Skip(1).Take(bytesRead - 1).ToArray(); //extract the encrypted msg
                            string decryptedPayload = "";
                            if (encryptedPayload.Length != 0)
                            {
                                decryptedPayload = Enc.DecryptWithAes(encryptedPayload, sessionAesKey); //decrypts the msg using AES key
                            }
                            #endregion

                            if (type == ServerMessageType.Chat)
                            {
                                string msg = decryptedPayload;
                                Foundation.LogPrint(Logs.ClientLog, msg, Foundation.chatColor);
                            }

                            #region Game Handeling
                            else if (type == ServerMessageType.GameRequest)
                            {
                                string fromUser = decryptedPayload;

                                Foundation.LogPrint(Logs.ClientLog, $"User [{fromUser}] has invited you to a game", Color.DodgerBlue);

                                var inviteForm = new GameInviteForm(fromUser);

                                inviteForm.OnDecision += async accepted =>
                                {
                                    ServerMessageType responseType = accepted ? ServerMessageType.GameAccept : ServerMessageType.GameDecline;

                                    await Foundation.SendEncryptedMessage(stream, responseType, fromUser, sessionAesKey);

                                    string logMessage = accepted
                                        ? $"Accepted game invitation from user [{fromUser}]"
                                        : $"Declined game invitation from user [{fromUser}]";
                                    Color logColor = accepted ? Color.Green : Foundation.failColor;

                                    Foundation.LogPrint(Logs.ClientLog, logMessage, logColor);
                                };

                                inviteForm.Show(); // Non-blocking!
                            }

                            else if (type == ServerMessageType.GameDecline)
                            {
                                string declinedBy = decryptedPayload;
                                Foundation.LogPrint(Logs.ClientLog, $"Game request was declined by user [{declinedBy}]", Foundation.failColor);
                            }

                            else if (type == ServerMessageType.UnconnectedUser)
                            {
                                Foundation.LogPrint(Logs.ClientLog, $"Game request was sent to unconnected user", Foundation.failColor);
                            }

                            else if (type == ServerMessageType.UserInGame)
                            {
                                Foundation.LogPrint(Logs.ClientLog, $"User is currently in another game", Foundation.failColor);
                            }

                            else if (type == ServerMessageType.GameStart)
                            {
                                string playersNames = decryptedPayload;
                                string[] names = playersNames.Split(':');

                                if (names.Length != 2)
                                {
                                    Foundation.LogPrint(Logs.ClientLog, "Invalid GameStart payload.", Foundation.failColor);
                                    return;
                                }

                                string player1 = names[0];
                                string player2 = names[1];

                                bool amPlayer1 = player1 == username;
                                string opponent = amPlayer1 ? player2 : player1;

                                Color colorP1 = Color.White;
                                Color colorP2 = Color.Black;

                                gameForm = new GameForm(client, sessionAesKey, amPlayer1, username, opponent, colorP1, colorP2)
                                {
                                    Owner = this
                                };
                                gameForm.Show();

                                string startGameMsg = "Game started. You are " + (amPlayer1 ? $"playing first ({colorP1})" : $"playing second ({colorP2})");

                                Color color = amPlayer1 ? colorP1 : colorP2;

                                Foundation.LogPrint(Logs.ClientLog, startGameMsg, color);
                            }

                            else if (type == ServerMessageType.GameMove)
                            {
                                string moveInBase64 = decryptedPayload;

                                byte[] dcryptedmoveFromBase64 = Convert.FromBase64String(moveInBase64);
                                int column = dcryptedmoveFromBase64[0];

                                // Apply on UI thread
                                if (gameForm != null && !gameForm.IsDisposed)
                                {
                                    gameForm.Invoke(new Action(() =>
                                    {
                                        gameForm.ApplyOpponentMove(column);
                                    }));
                                }
                            }

                            else if (type == ServerMessageType.GameOver)
                            {
                                string resultOfGame = decryptedPayload;

                                if (gameForm != null && !gameForm.IsDisposed)
                                {
                                    gameForm.Invoke(new Action(() =>
                                    {
                                        if (resultOfGame == "win")
                                            gameForm.ShowOpponentWin();
                                        else if (resultOfGame == "draw")
                                            gameForm.ShowDraw();
                                    }));
                                }
                            }

                            else if (type == ServerMessageType.OpponentLeft)
                            {
                                string leaver = decryptedPayload;
                                Foundation.LogPrint(Logs.ClientLog, $"Opponent [{leaver}] has left the game.", Foundation.failColor);

                                if (gameForm != null && !gameForm.IsDisposed)
                                {
                                    gameForm.Invoke(new Action(() =>
                                    {
                                        gameForm.DisableBoard();
                                        Foundation.LogPrint(Logs.ClientLog, "Game will close in 5 seconds...", Foundation.closingColor);

                                        Task.Delay(5000).ContinueWith(_ =>
                                        {
                                            if (!gameForm.IsDisposed)
                                            {
                                                gameForm.Invoke(new Action(() =>
                                                {
                                                    gameForm.Close();
                                                }));
                                            }
                                        });
                                    }));
                                }
                            }

                            #endregion

                            else if (type == ServerMessageType.ServerShutdown)
                            {
                                Foundation.LogPrint(Logs.ClientLog, "Server is shutting down. Disconnecting...", Foundation.closingColor);
                                await Task.Delay(4000);
                                this.Invoke(new MethodInvoker(() => this.Close())); //closes the window
                                return;
                            }

                            //handle other message types
                        }
                    }
                    catch (Exception ex)
                    {
                        Foundation.LogPrint(Logs.ClientLog, $"Connection lost: {ex.Message}", Foundation.errorColor);
                    }
                }

                Task listenTask;
                listenTask = ListenToServerAsync(stream);
            }
            catch (Exception ex)
            {
                Foundation.LogPrint(Logs.ClientLog, $"Connection failed: {ex.Message}", Foundation.errorColor);
            }
        }

        #region Message Sending
        private async void SendButton_Click(object sender, EventArgs e)
        {
            string message = chatInput.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                await Foundation.SendEncryptedMessage(stream, ServerMessageType.Chat, message, sessionAesKey);
                Foundation.LogPrint(Logs.ClientLog, "Me: " + message, Foundation.chatColor);
                chatInput.Clear();
            }
        }

        private async void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true; //prevent default newline

                string message = chatInput.Text;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    await Foundation.SendEncryptedMessage(stream, ServerMessageType.Chat, message, sessionAesKey);
                    Foundation.LogPrint(Logs.ClientLog, "Me: " + message, Foundation.chatColor);
                    chatInput.Clear();
                }
            }
        }
        #endregion

        private async void ChallengeButton_Click(object sender, EventArgs e)
        {
            string targetUser = opponentTextBox.Text.Trim();
            if (string.IsNullOrEmpty(targetUser))
            {
                Foundation.LogPrint(Logs.ClientLog, $"Please enter a username to challenge", Foundation.failColor);
                return;
            }
            else if (targetUser == username)
            {
                Foundation.LogPrint(Logs.ClientLog, $"You can't challenge yourself", Foundation.failColor);
                return;
            }

            await Foundation.SendEncryptedMessage(stream, ServerMessageType.GameRequest, targetUser, sessionAesKey);

            Foundation.LogPrint(Logs.ClientLog, $"Sent Game request to user [{targetUser}]", Color.DodgerBlue);
        }

        private void ClientForm_Closing(object sender, FormClosingEventArgs e)
        {
            #region CloseClient
            void CloseClient()
            {
                try
                {
                    if (client != null)
                    {
                        if (client.Connected)
                        {

                            client.Close();

                        }
                        client = null;
                    }
                }
                catch (Exception ex)
                {
                    Foundation.LogPrint(Logs.ClientLog, $"Error during disconnect: {ex.Message}", Foundation.errorColor);
                }
            }
            #endregion
            CloseClient();
        }
    }
}
