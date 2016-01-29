using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;

namespace WebSnapShot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        { }
        private void timer1_Tick(object sender, EventArgs e)
        {
            int StartMinute = int.Parse(ConfigurationSettings.AppSettings["RenderIntervalMin"].ToString().Trim());
            if (DateTime.Now.Minute == StartMinute)
            {
                button1_Click_1(new object(), new EventArgs());
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            DoMain();
        }
        protected void DoMain()
        {
            timer1.Enabled = false;
            richTextBox1.Text = "";
            button1.ForeColor = Color.White;
            button1.Text = "Started";
            button1.BackColor = Color.Red;
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(ConfigurationSettings.AppSettings["WebUrl"].ToString().Trim() + "&dt=" + DateTime.Now.ToString("yyyyMMddhhmmss"), ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim());

            }
            catch { }
            pictureBox1.Image = Image.FromFile(ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim());
            Application.DoEvents();
            Process proc = new Process();
            proc.StartInfo.FileName = "\"" + ConfigurationSettings.AppSettings["AeRenderPath"].ToString().Trim() + "\"";
            try
            {
                string[] Dirsold = Directory.GetDirectories(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());
                foreach (var item in Dirsold)
                {
                    DirectoryInfo Dd = new DirectoryInfo(item);
                    if (Dd.CreationTime.AddDays(3) < DateTime.Now)
                        Dd.Delete(true);
                }
            }
            catch
            { }
            string FileName = ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "_" + string.Format("{0:00}", DateTime.Now.Hour) + "-" + string.Format("{0:00}", DateTime.Now.Minute) + "-" + string.Format("{0:00}", DateTime.Now.Second);
            string Fol = ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "\\";
            DirectoryInfo Dir = new DirectoryInfo(Fol);
            if (!Dir.Exists)
            { Dir.Create(); }
            proc.StartInfo.Arguments = " -project " + "\"" + ConfigurationSettings.AppSettings["AeProjectFile"].ToString().Trim() + "\"" + "   -comp   \"" + ConfigurationSettings.AppSettings["Composition"].ToString().Trim() + "\" -output " + "\"" + Fol + FileName + ".mp4" + "\"";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.EnableRaisingEvents = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            if (!proc.Start())
            { return; }
            proc.PriorityClass = ProcessPriorityClass.Normal;
            StreamReader reader = proc.StandardOutput;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                richTextBox1.Text += (line) + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }
            proc.Close();
            button1.ForeColor = Color.White;
            button1.Text = "Start";
            button1.BackColor = Color.Navy;
            timer1.Enabled = true;
        }
    }
}
