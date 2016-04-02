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




namespace WebSnapShot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        public Bitmap GenerateScreenshot(string url, int width, int height)
        {

            WebBrowser wb = new WebBrowser();
            wb.NewWindow += wb_NewWindow;
            wb.ScrollBarsEnabled = false;
            wb.ScriptErrorsSuppressed = true;
            wb.Navigate(url);
            while (wb.ReadyState != WebBrowserReadyState.Complete) { Application.DoEvents(); }


            Thread.Sleep(int.Parse(ConfigurationSettings.AppSettings["WaitForLoadWebSite"].ToString().Trim()));


            wb.Width = width;
            wb.Height = height;

            if (width == -1)
            {

                wb.Width = wb.Document.Body.ScrollRectangle.Width;
            }

            if (height == -1)
            {

                wb.Height = wb.Document.Body.ScrollRectangle.Height;
            }

            Bitmap bitmap = new Bitmap(wb.Width, wb.Height);
            wb.DrawToBitmap(bitmap, new Rectangle(0, 0, wb.Width, wb.Height));
            wb.Dispose();
            return bitmap;
        }

        void wb_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            throw new NotImplementedException();
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            int StartMinute = int.Parse(ConfigurationSettings.AppSettings["RenderIntervalMin"].ToString().Trim());
            if (DateTime.Now.Minute == StartMinute)
            {
                button1_Click_1(new object(), new EventArgs());
            }
            //<add key="RenderHourEvenOdd"  value="odd"/>
            //int Even = DateTime.Now.Hour % 2;
            //if (ConfigurationSettings.AppSettings["RenderHourEvenOdd"].ToString().Trim().ToLower() == "even" && Even == 0)
            //{
            //    if (DateTime.Now.Minute >= StartMinute && DateTime.Now.Minute <= StartMinute + 1)
            //    {
            //        button1_Click_1(new object(), new EventArgs());
            //    }
            //}
            //if (ConfigurationSettings.AppSettings["RenderHourEvenOdd"].ToString().Trim().ToLower() == "odd" && Even != 0)
            //{
            //    if (DateTime.Now.Minute >= StartMinute && DateTime.Now.Minute <= StartMinute + 1)
            //    {
            //        button1_Click_1(new object(), new EventArgs());
            //    }
            //}
         
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


            Bitmap thumbnail = GenerateScreenshot(ConfigurationSettings.AppSettings["WebUrl"].ToString().Trim(),
               int.Parse(ConfigurationSettings.AppSettings["ImageWith"].ToString().Trim()),
                int.Parse(ConfigurationSettings.AppSettings["ImageHeight"].ToString().Trim()));

            Rectangle cropRect = new Rectangle(0,
                int.Parse(ConfigurationSettings.AppSettings["ImagePaddingTop"].ToString().Trim()),
                thumbnail.Width,
                thumbnail.Height - int.Parse(ConfigurationSettings.AppSettings["ImagePaddingTop"].ToString().Trim()));

            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(thumbnail, new Rectangle(0, 0, target.Width, target.Height),
                                cropRect,
                                GraphicsUnit.Pixel);
            }
            pictureBox1.Image = target;

            target.Save(ConfigurationSettings.AppSettings["ImagePath"].ToString().Trim(),System.Drawing.Imaging.ImageFormat.Jpeg);
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
            {

            }


            string FileName = ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "_" + string.Format("{0:00}", DateTime.Now.Hour) + "-" + string.Format("{0:00}", DateTime.Now.Minute) + "-" + string.Format("{0:00}", DateTime.Now.Second);
            string Fol = ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "\\";

            DirectoryInfo Dir = new DirectoryInfo(Fol);

            if (!Dir.Exists)
            {
                Dir.Create();
            }


            proc.StartInfo.Arguments = " -project " + "\"" + ConfigurationSettings.AppSettings["AeProjectFile"].ToString().Trim() + "\"" + "   -comp   \"" + ConfigurationSettings.AppSettings["Composition"].ToString().Trim() + "\" -output " + "\"" + Fol + FileName + ".mp4" + "\"";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.EnableRaisingEvents = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            if (!proc.Start())
            {
                return;
            }

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
            thumbnail.Dispose();

            
            button1.ForeColor = Color.White;
            button1.Text = "Start";
            button1.BackColor = Color.Navy;
            timer1.Enabled = true;
        }
    }
}
