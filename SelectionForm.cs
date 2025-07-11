using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SecureChat4InARow
{
    public partial class SelectionForm : Form
    {
        public UserSelect UserSelect { get; private set; }

        public SelectionForm()
        {
            InitializeComponent();
        }

        private void SelectionForm_Load(object sender, EventArgs e)
        {

        }

        private void ServerButton_Click(object sender, EventArgs e)
        {
            #region CanBeServer
            bool CanBeServer()
            {
                try
                {
                    using (UdpClient broadcastSocket = new UdpClient()) //creates a udp client to broadcast the request to be the server
                    {
                        IPEndPoint allPoints = new IPEndPoint(IPAddress.Broadcast, Foundation.GUARD_PORT);
                        byte[] request = { (byte)ServerMessageType.CanBeServer }; //creates a message for the request

                        broadcastSocket.Send(request, request.Length, allPoints); //broadcasts the request
                        broadcastSocket.Client.ReceiveTimeout = 500; //gives time to replay

                        IPEndPoint receivingPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] reply = broadcastSocket.Receive(ref receivingPoint); //waits for replay

                        return (ServerMessageType)reply[0] != ServerMessageType.Deny; //if replay is deny then returns false and no server
                    }
                        
                }
                catch
                {
                    // Timeout or no reply = no server running
                    return true;
                }
            }
            #endregion

            if (CanBeServer())
            {
                UserSelect = UserSelect.Server;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
            else
            {
                MessageBox.Show("Another server is already running on this network.", "Server Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
                
        }

        private void ClientButton_Click(object sender, EventArgs e)
        {
            UserSelect = UserSelect.Client;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
