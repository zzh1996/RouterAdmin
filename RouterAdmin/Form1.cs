using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RouterAdmin
{
    public partial class Form1 : Form
    {
        Router router;
        List<string> client;
        List<string> traffic;
        int totaldownspeed;
        int totalupspeed;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            router = new Router();
            client = new List<string>();
            traffic = new List<string>();
            /*List<string> r=router.GetTraffic();
            foreach(string s in r) {
                MessageBox.Show("^"+s+"$"+s.Length.ToString());
            }*/
        }

        private void GenIcon()
        {
            int downpixel = 32 * totaldownspeed / (512 * 1024);
            int uppixel = 32 * totalupspeed / (64 * 1024);

            Bitmap bmp = this.Icon.ToBitmap();
            Graphics g = Graphics.FromImage(bmp);
            SolidBrush drawBrush = new SolidBrush(Color.Blue);

            //g.FillRectangle(Brushes.White, new Rectangle(0, 0, 32, 32));
            g.FillRectangle(Brushes.Green, new Rectangle(0, 32 - downpixel, 16, downpixel));
            g.FillRectangle(Brushes.Red, new Rectangle(16, 32 - uppixel, 16, uppixel));

            g.Dispose();
            notifyIcon1.Icon = Icon.FromHandle(bmp.GetHicon());
            notifyIcon1.Text = "总下载:" + unitconv(totaldownspeed) + "\n总上传:" + unitconv(totalupspeed);
        }

        private double GetData()
        {
            DateTime starttime = DateTime.Now;
            int LastTrafficCount = traffic.Count;
            traffic = router.GetTraffic();//id,ip,mac,down,up,downspeed,upspeed
            if (traffic.Count != LastTrafficCount)
                client = router.GetClientList();//name,mac,ip,time
            return (DateTime.Now - starttime).TotalMilliseconds;
        }

        private void Redraw()
        {
            double delay = GetData();
            toolStripStatusLabel1.Text = delay.ToString("F") + "ms";
            dataGridView1.Rows.Clear();
            totaldownspeed = totalupspeed = 0;
            for (int i = 0, row = 0; i < traffic.Count; i += 7)
            {
                string clientname = null;
                for (int j = 0; j < client.Count; j += 4)
                {
                    if (client[j + 2] == traffic[i + 1])
                    {
                        clientname = client[j];
                        break;
                    }
                }
                if (clientname == null) continue;
                string downspeed = unitconv(double.Parse(traffic[i + 5]));
                string upspeed = unitconv(double.Parse(traffic[i + 6]));
                dataGridView1.Rows.Add(clientname, traffic[i + 1], downspeed, upspeed);
                dataGridView1.Rows[row].Cells[2].Style.BackColor = ratiocolor(int.Parse(traffic[i + 5]), 512 * 1024);
                dataGridView1.Rows[row].Cells[3].Style.BackColor = ratiocolor(int.Parse(traffic[i + 6]), 64 * 1024);
                row++;
                totaldownspeed += int.Parse(traffic[i + 5]);
                totalupspeed += int.Parse(traffic[i + 6]);
            }
            checkBox1.Checked = router.GetLimitStatus();
            GenIcon();
        }

        private Color ratiocolor(int n, int m)
        {
            double ratio = (double)n / m;
            if (ratio < 0) ratio = 0;
            if (ratio > 1) ratio = 1;
            int d = (int)(255 * (1 - ratio));
            return Color.FromArgb(255, d, d);
        }

        private string unitconv(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##}{1}/s", len, sizes[order]);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Redraw();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                router.LimitOn();
            }
            else {
                router.LimitOff();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //this.Hide();
            //e.Cancel = true;
            //notifyIcon1.ShowBalloonTip(2000, "(๑乛◡乛๑)", "程序已隐藏到托盘区，点击图标打开", ToolTipIcon.Info);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox2.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
