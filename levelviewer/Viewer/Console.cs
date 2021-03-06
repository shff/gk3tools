using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Viewer
{
    public partial class ConsoleForm : Form
    {
        public ConsoleForm()
        {
            InitializeComponent();
        }

        public void Write(string text, params object[] args)
        {
            txtConsole.AppendText(string.Format(text, args));
        }

        private void txtCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Gk3Main.Console.CurrentConsole.RunCommand(txtCommand.Text);
                txtCommand.Text = "";
                e.Handled = true;
            }
        }

        private void ConsoleForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void txtCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                txtCommand.Text = Gk3Main.Console.CurrentConsole.PreviousCommand;
            }
        }
    }

    class FormConsole : Gk3Main.Console
    {
        public FormConsole(ConsoleForm console)
        {
            _console = console;
        }

        public override void Write(Gk3Main.ConsoleSeverity severity, string text, params object[] arg)
        {
            if (severity >= MinSeverity)
                _console.Write(text, arg);
        }

        public override void ReportError(string error)
        {
            base.ReportError(error);

            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public Gk3Main.ConsoleSeverity MinSeverity
        {
            get; set;
        }

        private ConsoleForm _console;
    }
}