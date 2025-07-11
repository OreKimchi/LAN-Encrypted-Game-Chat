using System;
using System.Drawing;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureChat4InARow
{
    public partial class GameForm : Form
    {
        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly byte[] sessionAesKey;

        private readonly GameBoard gameBoard;
        private readonly Panel[,] cells = new Panel[GameBoard.Rows, GameBoard.Columns];

        private bool isMyTurn = true;
        private readonly bool isPlayer1;

        private readonly string playerUsername;
        private readonly string opponentUsername;

        private readonly Color colorP1;
        private readonly Color colorP2;

        public GameForm(TcpClient client, byte[] sessionAesKey, bool isPlayer1, string playerUsername, string opponentUsername, Color colorP1, Color colorP2)
        {
            InitializeComponent();
            this.client = client;
            this.stream = client.GetStream();
            this.sessionAesKey = sessionAesKey;
            gameBoard = new GameBoard();
            this.isPlayer1 = isPlayer1;
            this.isMyTurn = isPlayer1;
            this.playerUsername = playerUsername;
            this.opponentUsername = opponentUsername;
            this.colorP1 = colorP1;
            this.colorP2 = colorP2;
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            this.FormClosing += GameForm_FormClosing;

            tableLayoutPanel1.Controls.Clear(); // just in case

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    Panel panel = new Panel
                    {
                        BackColor = ColorTranslator.FromHtml("#53586E"),
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2),
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = col  // store column for use in click handler
                    };

                    panel.Click += Panel_Click;

                    cells[row, col] = panel;
                    tableLayoutPanel1.Controls.Add(panel, col, row);
                }
            }
        }

        private async void Panel_Click(object sender, EventArgs e)
        {
            if (!isMyTurn)
            {
                Foundation.LogPrint(Logs.ClientLog, "It's not your turn!", Foundation.failColor);
                return;
            }

            if (sender is Panel clickedPanel)
            {
                int column = (int)clickedPanel.Tag;

                int row = gameBoard.PlacePiece(column);
                if (row == -1)
                {
                    Foundation.LogPrint(Logs.ClientLog, "Column is full!", Foundation.failColor);
                    return;
                }

                // Set color based on current player
                Color pieceColor = isPlayer1 ? colorP1 : colorP2;

                cells[row, column].BackColor = pieceColor;

                #region SendGameMove
                async void SendGameMove(int col)
                {
                    string columnInBase64 = Convert.ToBase64String(new byte[] { (byte)col });
                    await Foundation.SendEncryptedMessage(client.GetStream(), ServerMessageType.GameMove, columnInBase64, sessionAesKey);

                }
                #endregion
                SendGameMove(column);

                #region SendGameOver
                async void SendGameOver(string result)
                {
                    await Foundation.SendEncryptedMessage(stream, ServerMessageType.GameOver, result, sessionAesKey);
                }
                #endregion

                // Check for win
                if (gameBoard.CheckWin(row, column))
                {
                    SendGameOver("win");
                    Foundation.LogPrint(Logs.ClientLog, "You win!", Color.Gold);
                    Foundation.LogPrint(Logs.ClientLog, "Game will close in 5 seconds...", Foundation.closingColor);
                    DisableBoard();
                    await Task.Delay(5000);
                    Close();
                    return;
                }

                // Check for draw
                if (gameBoard.IsDraw())
                {
                    SendGameOver("draw");
                    Foundation.LogPrint(Logs.ClientLog, "Game ended in a draw.");
                    Foundation.LogPrint(Logs.ClientLog, "Game will close in 5 seconds...", Foundation.closingColor);
                    DisableBoard();
                    await Task.Delay(5000);
                    Close();
                    return;
                }

                gameBoard.SwitchPlayer();
                isMyTurn = false;

            }
        }

        public async void ShowOpponentWin()
        {
            Foundation.LogPrint(Logs.ClientLog, "Opponent wins!", Foundation.failColor);
            Foundation.LogPrint(Logs.ClientLog, "Game will close in 5 seconds...", Foundation.closingColor);
            DisableBoard();
            await Task.Delay(5000);
            Close();
        }

        public async void ShowDraw()
        {
            Foundation.LogPrint(Logs.ClientLog, "It's a draw.");
            Foundation.LogPrint(Logs.ClientLog, "Game will close in 5 seconds...", Foundation.closingColor);
            DisableBoard();
            await Task.Delay(5000);
            Close();
        }

        public void DisableBoard()
        {
            foreach (Panel panel in cells)
            {
                panel.Enabled = false;
            }
        }

        public void ApplyOpponentMove(int column)
        {
            if (isMyTurn) return; // Prevent applying a move when it's your turn

            int row = gameBoard.PlacePiece(column);
            if (row == -1) return;

            Color color = isPlayer1 ? colorP2 : colorP1;

            cells[row, column].BackColor = color;

            if (gameBoard.CheckWin(row, column))
            {
                DisableBoard();
                return;
            }

            if (gameBoard.IsDraw())
            {
                DisableBoard();
                return;
            }

            gameBoard.SwitchPlayer();
            isMyTurn = true; // It's now your turn
        }

        private async void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Notify opponent only if we're still in a game
                if (!string.IsNullOrEmpty(playerUsername))
                {
                    await Foundation.SendEncryptedMessage(stream, ServerMessageType.OpponentLeft, playerUsername, sessionAesKey);

                    Foundation.LogPrint(Logs.ClientLog, $"Exited Game", Foundation.closingColor);
                }
            }
            catch (Exception ex)
            {
                Foundation.LogPrint(Logs.ClientLog, $"Error notifying opponent: {ex.Message}", Foundation.errorColor);
            }
        }


    }
}
