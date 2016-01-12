/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluidSys {
    public partial class InputForm : Form {
        /// <summary>
        /// Gets the value from this InputForm.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Creates a new instance of the FluidSys.InputForm class.
        /// </summary>
        public InputForm() {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the form as a modal dialog box with the specified message
        /// and owner.
        /// </summary>
        /// <param name="owner">Any object that implements System.Windows.Forms.IWin32Window 
        /// that represents the top-level window that will own the modal dialog box.</param>
        /// <param name="message">The message to display to the user.</param>
        /// <returns></returns>
        public DialogResult ShowDialog(IWin32Window owner, string message) {
            label3.Text = message;
            this.ShowDialog(owner);
            return this.DialogResult;
        }

        /// <summary>
        /// Shows the form as a modal dialog box with the specified message.
        /// </summary>      
        /// <param name="message">The message to display to the user.</param>
        /// <returns></returns>
        public DialogResult ShowDialog(string message) {
            label3.Text = message;
            this.ShowDialog();
            return this.DialogResult;
        }

        private void label1_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            Value = textBox1.Text;
        }
    }
}
