using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;


namespace SSHConnection
{
    public partial class Form1 : Form
    {
        private SshClient sshClient;
        private ShellStream shellStream;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click_Click(object sender, EventArgs e)
        {
            try
            {
                string hostname = txtHostname.Text;
                int port = 22;
                string username = txtUsername.Text;
                string password = txtPassword.Text;

                sshClient = new SshClient(hostname, port, username, password);
                sshClient.Connect();
                shellStream = sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

                rtbOutput.AppendText("Connected to SSH server.\n");
                txtCommand.ReadOnly = false;
                btnSendCommand.Enabled = true;
                btnConnect_Click.Enabled = false;
                btnDisconnect_Click.Enabled = true;
                txtHostname.Enabled = false;
                txtUsername.Enabled = false;
                txtPassword.Enabled = false;
                
            }
            catch (Exception ex)
            {
                rtbOutput.AppendText($"Error: {ex.Message}\n");
            }

        }

        private void btnDisconnect_Click_Click(object sender, EventArgs e)
        {

            if (sshClient != null && sshClient.IsConnected)
            {
                sshClient.Disconnect();
                rtbOutput.AppendText("Disconnected from SSH server.\n");
                txtCommand.ReadOnly = true;
                btnSendCommand.Enabled = false;
                btnConnect_Click.Enabled = true;
                btnDisconnect_Click.Enabled = false;
                txtHostname.Enabled = true;
                txtUsername.Enabled = true;
                txtPassword.Enabled = true;
            }

        }

        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                string command = txtCommand.Text;
                WriteToShellStream(command);

                txtCommand.Text = "";

            }
            else
            {
                rtbOutput.AppendText("Not connected to any SSH server.\n");
            }
        }

        private string StripAnsiSequences(string input)
        {
            // This regex pattern matches a wide range of ANSI escape sequences
            var ansiRegex = new Regex(@"\x1B[@-_][0-?]*[ -/]*[@-~]");
            return ansiRegex.Replace(input, string.Empty);
        }

        private void WriteToShellStream(string command)
        {
            shellStream.WriteLine(command);
            shellStream.Flush();

            var result = ReadShellStream(command);
            var cleanResult = StripAnsiSequences(result);  // Strip the ANSI sequences
            AppendTextToOutput(cleanResult);
        }

        private string ReadShellStream(string command)
        {
            var output = new StringBuilder();
            string line;
            while ((line = shellStream.ReadLine(TimeSpan.FromMilliseconds(100))) != null)
            {
                output.AppendLine(line);
            }

            var result = output.ToString();

            // Remove the command echo from the result
            var commandEchoIndex = result.IndexOf(command);
            if (commandEchoIndex >= 0)
            {
                result = result.Remove(commandEchoIndex, command.Length).Trim();
            }

            return result;
        }

        private void AppendTextToOutput(string text)
        {
            rtbOutput.AppendText(text);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            rtbOutput.SelectionStart = rtbOutput.Text.Length;
            rtbOutput.ScrollToCaret();
        }


        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) // Correct way to detect Enter key
            {
                btnConnect_Click_Click(sender, e);
                e.Handled = true; // Prevent the beep sound on Enter key press
            }
        }

        private void txtCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) // Correct way to detect Enter key
            {
                btnSendCommand_Click(sender, e);
                e.Handled = true; // Prevent the beep sound on Enter key press
            }
        }
    }
}
