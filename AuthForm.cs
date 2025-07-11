using System;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureChat4InARow
{
    public partial class AuthForm : Form
    {
        public TcpClient Client { get; private set; }
        public NetworkStream Stream { get; private set; }
        public string Username { get; private set; }

        private byte[] sessionAesKey;
        public byte[] SessionKey => sessionAesKey;

        bool tryauthRunning = false;

        public AuthForm()
        {
            InitializeComponent();
            statusLabel.ForeColor = Color.Red;
        }

        private void AuthForm_Load(object sender, EventArgs e)
        {
            Foundation.RegisterLogControl(Logs.AuthLog, AuthLog);
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            if (!tryauthRunning)
            {
                await TryAuth(isRegister: false);
            }
            else
            {
                statusLabel.Text = "Authentication in progress...";
            }
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            if (!tryauthRunning)
            {
                await TryAuth(isRegister: true);
            }
            else
            {
                statusLabel.Text = "Authentication in progress...";
            }
        }

        private async Task TryAuth(bool isRegister)
        {
            tryauthRunning = true;

            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text.Trim();

            if (username == "" || password == "")
            {
                statusLabel.Text = "Enter username and password.";
                tryauthRunning = false;
                return;
            }

            try
            {
                Client = new TcpClient();
                await Client.ConnectAsync(Foundation.SERVERIP, Foundation.SERVERPORT); //connects client
                Stream = Client.GetStream();

                #region Keys Exchange
                //(privateKeyXml, publicKeyXml) = Enc.GenerateRsaKeyPair();

                //sends public key to server
                //byte[] pubKeyBytes = Encoding.UTF8.GetBytes(publicKeyXml);
                //await Stream.WriteAsync(pubKeyBytes, 0, pubKeyBytes.Length);

                //receive server public key
                byte[] buffer = new byte[4096];
                int bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);

                string serverPublicKeyXml = "";
                if (buffer[0] == (byte)ServerMessageType.PublicKey)
                {
                    serverPublicKeyXml = Encoding.UTF8.GetString(buffer, 1, bytesRead - 1);
                }
                Foundation.LogPrint(Logs.AuthLog, "Received server public key", Foundation.encryptionColor);

                byte[] aesKey;
                using (var aes = Aes.Create()) //generates AES key
                {
                    aes.KeySize = 256;
                    aes.GenerateKey();
                    aesKey = aes.Key;
                }
                Foundation.LogPrint(Logs.AuthLog, "Generated AES key", Foundation.encryptionColor);

                // Encrypt AES key with server's public RSA key
                RSA serverRsa = Enc.CreateFromPublicKey(serverPublicKeyXml);
                byte[] encryptedAesKey = serverRsa.Encrypt(aesKey, RSAEncryptionPadding.Pkcs1); //encrypts AES key using servers public key

                // Prepare message: [Type][Encrypted AES Key]
                byte[] msg = new byte[1 + encryptedAesKey.Length];
                msg[0] = (byte)ServerMessageType.SessionAESKey;
                Array.Copy(encryptedAesKey, 0, msg, 1, encryptedAesKey.Length);

                // Send encrypted session key to server
                await Stream.WriteAsync(msg, 0, msg.Length);

                Foundation.LogPrint(Logs.AuthLog, "Sent encrypted AES key to server", Foundation.encryptionColor);

                // Store the AES key locally for encryption/decryption
                sessionAesKey = aesKey;
                #endregion

                #region Login/Register request
                //encrypt the login/register payload with AES and send
                string usernamePassword = $"{username}:{password}";
                ServerMessageType type = isRegister ? ServerMessageType.Register : ServerMessageType.Login;

                await Foundation.SendEncryptedMessage(Stream, type, usernamePassword, sessionAesKey); //sends to server login/register request and the name and password for it
                
                if (isRegister) { Foundation.LogPrint(Logs.AuthLog, "Sent register request to server", Foundation.connectingColor); }
                else { Foundation.LogPrint(Logs.AuthLog, "Sent login request to server", Foundation.connectingColor); }
                #endregion

                buffer = new byte[1024];
                bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);

                await Task.Delay(7000);

                if (buffer[0] == (byte)ServerMessageType.LoginResult)
                {
                    byte[] encryptedMessage = buffer.Skip(1).Take(bytesRead - 1).ToArray();
                    string response = Enc.DecryptWithAes(encryptedMessage, sessionAesKey);

                    if (response == "OK")
                    {
                        Username = username;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        statusLabel.Text = response;
                        Stream.Close();
                        Client.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
                tryauthRunning = false;
            }

            tryauthRunning = false;
        }
    }
}
