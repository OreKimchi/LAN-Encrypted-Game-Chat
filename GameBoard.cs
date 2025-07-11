namespace SecureChat4InARow
{
    internal class GameBoard
    {
        public const int Rows = 6;
        public const int Columns = 7;

        private readonly Player[,] board;
        public Player CurrentPlayer { get; private set; } = Player.Player1;

        public GameBoard()
        {
            board = new Player[Rows, Columns];
        }

        public int PlacePiece(int column)
        {
            if (column < 0 || column >= Columns)
                return -1;

            for (int row = Rows - 1; row >= 0; row--)
            {
                if (board[row, column] == Player.None)
                {
                    board[row, column] = CurrentPlayer;
                    return row;
                }
            }
            return -1; // column full
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
        }

        public bool CheckWin(int lastRow, int lastCol)
        {
            Player player = board[lastRow, lastCol];
            if (player == Player.None) return false;

            return CountDirection(lastRow, lastCol, 1, 0) + CountDirection(lastRow, lastCol, -1, 0) > 2 || // vertical
                   CountDirection(lastRow, lastCol, 0, 1) + CountDirection(lastRow, lastCol, 0, -1) > 2 || // horizontal
                   CountDirection(lastRow, lastCol, 1, 1) + CountDirection(lastRow, lastCol, -1, -1) > 2 || // diagonal \
                   CountDirection(lastRow, lastCol, 1, -1) + CountDirection(lastRow, lastCol, -1, 1) > 2;   // diagonal /
        }

        private int CountDirection(int row, int col, int dRow, int dCol)
        {
            int count = 0;
            Player player = board[row, col];

            for (int i = 1; i < 4; i++)
            {
                int r = row + dRow * i;
                int c = col + dCol * i;

                if (r < 0 || r >= Rows || c < 0 || c >= Columns || board[r, c] != player)
                    break;
                count++;
            }

            return count;
        }

        public bool IsDraw()
        {
            for (int c = 0; c < Columns; c++)
                if (board[0, c] == Player.None)
                    return false;
            return true;
        }
    }
}

