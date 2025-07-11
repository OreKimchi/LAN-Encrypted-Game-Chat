using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureChat4InARow
{
    public partial class GameInviteForm : Form
    {
        public event Action<bool> OnDecision;
        private bool declined = false;
        private bool accepted = false;

        public GameInviteForm(string requesterUsername)
        {
            InitializeComponent();
            labelMessage.Text = $"User [{requesterUsername}] invites you to a game.";
        }


        private void GameInviteForm_Load(object sender, EventArgs e)
        {

        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            accepted = true;
            OnDecision?.Invoke(true);
            this.Close();
        }

        private void DeclineButton_Click(object sender, EventArgs e)
        {
            declined = true;
            OnDecision?.Invoke(false);
            this.Close();
        }

        private void GameInviteForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (!declined && !accepted)
            {
                e.Cancel = true;
                OnDecision?.Invoke(false);
                this.FormClosing -= GameInviteForm_Closing;
                this.Close();
            }
        }
    }
}