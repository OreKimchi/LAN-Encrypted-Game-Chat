using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SecureChat4InARow
{
    public enum Player
    {
        None = 0,
        Player1 = 1,
        Player2 = 2
    }

    public enum UserSelect
    {
        Server,
        Client
    }

    public enum ServerMessageType : byte
    {
        CanBeServer = 1,
        Deny = 2,
        ServerShutdown = 3,
        Chat = 4,
        Login = 5,
        Register = 6,
        LoginResult = 7,
        PublicKey = 8,
        SessionAESKey = 9,
        GameMove = 10,
        GameRequest = 11,
        GameAccept = 12,
        GameStart = 13,
        GameOver = 14,
        GameDecline = 15,
        UnconnectedUser = 16,
        UserInGame = 17,
        OpponentLeft = 18,
    }

    public enum Logs
    {
        ServerLog, 
        ClientLog, 
        AuthLog
    }


    internal static class Foundation
    {
        public const int SERVERPORT = 4590;
        public const int GUARD_PORT = 6000;
        public const string SERVERIP = "192.168.1.243"; //ip of computer with server

        #region Log prints
        private static readonly Dictionary<Logs, RichTextBox> logTargets = new Dictionary<Logs, RichTextBox>();

        public static void RegisterLogControl(Logs logType, RichTextBox richTextBox)
        {
            logTargets[logType] = richTextBox;
        }

        private static readonly SemaphoreSlim logLock = new SemaphoreSlim(1, 1);

        #region Text colors
        public static readonly Color encryptionColor = Color.SteelBlue;
        public static readonly Color connectingColor = Color.LightGreen;
        public static readonly Color chatColor = Color.Black;
        public static readonly Color closingColor = Color.Orange;
        public static readonly Color failColor = Color.Tomato;
        public static readonly Color errorColor = Color.Red;
        #endregion

        public static async void LogPrint(Logs target, string message, Color? color = null, int delay = 30)
        {
            if (!logTargets.TryGetValue(target, out RichTextBox logBox))
                return;

            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() => LogPrint(target, message, color, delay)));
                return;
            }

            await logLock.WaitAsync();
            try
            {
                string timestamp = $"{DateTime.Now:HH:mm:ss} - ";
                string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = (i == 0 ? timestamp : "\t        ") + lines[i];
                    logBox.SelectionStart = logBox.TextLength;
                    logBox.SelectionColor = color ?? Color.Black;

                    foreach (char c in line)
                    {
                        logBox.AppendText(c.ToString());
                        await Task.Delay(delay);
                    }

                    logBox.AppendText(Environment.NewLine);
                }

                logBox.ScrollToCaret();
                logBox.SelectionColor = Color.Black;
            }
            finally
            {
                logLock.Release();
            }
        }
        #endregion

        public static void TrdStart(Action act)
        {
            new Thread(() => act()) { IsBackground = true }.Start();
        }

        public async static Task SendEncryptedMessage(NetworkStream streamOfReceiver, ServerMessageType type, string payload, byte[] aesKeyOfReceiver)
        {
            try
            {
                byte[] encrypted = Enc.EncryptWithAes(payload, aesKeyOfReceiver); //encrypts message
                byte[] message = new byte[1 + encrypted.Length]; //array for encrypted message + type
                message[0] = (byte)type; //adds type
                Array.Copy(encrypted, 0, message, 1, encrypted.Length);
                await streamOfReceiver.WriteAsync(message, 0, message.Length);
            }
            catch (Exception ex)
            {
                LogPrint(Logs.ServerLog, $"Failed to send message of type [{type}]: {ex.Message}", Color.Red);
            }
        }
    }

    public class UserManager
    {
        private static readonly string filePath = "users.json";
        private static List<User> users = new List<User>(); //will hold all useres info

        static UserManager()
        {
            LoadUsers();
        }

        public static void LoadUsers()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                users = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
        }

        public static void SaveUsers()
        {
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static bool UserExists(string username) =>
        users.Exists(u => u.Username == username);

        public static bool ValidateUser(string username, string password)
        {
            User user = users.FirstOrDefault(u => u.Username == username); //finds user
            if (user == null) return false;

            string computedHash = HashPassword(password, user.Salt); //generate salted password
            return computedHash == user.PasswordHash;
        }

        public static bool RegisterUser(string username, string password)
        {
            if (UserExists(username)) return false;
            #region GenerateSalt
            string GenerateSalt(int length = 16)
            {
                byte[] saltBytes = new byte[length];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                    return Convert.ToBase64String(saltBytes);
                }
            }
            #endregion
            string salt = GenerateSalt();
            string hash = HashPassword(password, salt);

            users.Add(new User
            {
                Username = username,
                PasswordHash = hash,
                Salt = salt
            });

            SaveUsers();
            return true;
        }

        //private static string Hash(string password)
        //{
        //    using (var sha = System.Security.Cryptography.SHA256.Create())
        //    {
        //        byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //        return Convert.ToBase64String(bytes);
        //    }
        //}

        private static string HashPassword(string password, string salt)
        {
            const string pepper = "static-secret-if-needed"; // store in secure config

            string salted = salt + password + pepper;

            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(salted));
                return Convert.ToBase64String(bytes);
            }
        }

    }

    public static class Enc
    {
        public static (string PrivateKeyXml, string PublicKeyXml) GenerateRsaKeyPair()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false; //prevents saving the key to the Windows Crypto Key Store, keeping it memory only

                string privateKey = rsa.ToXmlString(true);  //includes private parameters (private and public)
                string publicKey = rsa.ToXmlString(false); //public only

                return (privateKey, publicKey); //returns a tuple
            }
        }

        public static RSA CreateFromPrivateKey(string privateXml)
        {
            RSA rsa = RSA.Create();
            rsa.FromXmlString(privateXml);
            return rsa;
        }

        public static RSA CreateFromPublicKey(string publicXml)
        {
            RSA rsa = RSA.Create();
            rsa.FromXmlString(publicXml);
            return rsa;
        }

        public static byte[] EncryptWithAes(string plainText, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); //creates a new IV for this message

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    // Combine IV + cipher data
                    byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
                    return result;
                }
            }
        }

        public static string DecryptWithAes(byte[] data, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] cipherText = new byte[data.Length - iv.Length];

                Array.Copy(data, 0, iv, 0, iv.Length);
                Array.Copy(data, iv.Length, cipherText, 0, cipherText.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }
    }
}
