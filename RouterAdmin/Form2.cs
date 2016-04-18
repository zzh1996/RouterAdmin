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

namespace RouterAdmin
{
    public partial class Form2 : Form
    {
        Router router;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            router = new Router();
            textBox1.Text = router.GetPhone();
            ActiveControl = textBox2;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Regex.Match(textBox1.Text, @"^([0-9]{11})$").Success || !Regex.Match(textBox2.Text, @"^([0-9]{6})$").Success)
            {
                MessageBox.Show("输入格式错误！");
                return;
            }
            router.ChangePassword(textBox1.Text, textBox2.Text);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = "宽带状态："+router.GetDialStatus();
        }
    }
}
