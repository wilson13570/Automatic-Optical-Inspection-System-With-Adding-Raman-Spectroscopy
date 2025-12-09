using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Raman1._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 預設讓密碼用＊顯示
            txtPassword.PasswordChar = '＊';
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblpassword_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (username == "admin" && password == "123456")
            {
                MainForm mainForm = new MainForm();
                mainForm.Show();
                this.Hide();  // 隱藏登入畫面
            }
            else
            {
                MessageBox.Show("帳號或密碼錯誤！", "登入失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '＊';
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
