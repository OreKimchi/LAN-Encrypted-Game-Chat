using System;
using System.Windows.Forms;

namespace SecureChat4InARow
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SelectionForm selectionForm = new SelectionForm();
            if (selectionForm.ShowDialog() == DialogResult.OK)
            {
                if (selectionForm.UserSelect == UserSelect.Server)
                {
                    Application.Run(new ServerForm());
                }
                else if (selectionForm.UserSelect == UserSelect.Client)
                {
                    var authForm = new AuthForm();
                    if (authForm.ShowDialog() == DialogResult.OK)
                    {
                        Application.Run(new ClientForm(authForm.Client, authForm.Stream, authForm.Username, authForm.SessionKey));
                    }

                    else
                    {
                        MessageBox.Show("Login or registration failed.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Selection canceled.");
            }
        }

    }
}
