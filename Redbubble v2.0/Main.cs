using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Redbubble;
using RedbubbleInput;
using CsvHelper;
using System.IO;

namespace Redbubble_v2._0
{
    public partial class Main : Form
    {
        #region Properties
        String FileName;
        Thread Worker = null;
        Thread Timer = null;
        bool isRunning = false;
        #endregion
        public Main()
        {
            InitializeComponent();
            
        }
        private void Main_Load(object sender, EventArgs e)
        {
            Timer = new Thread(new ThreadStart(() => {
                int count = 15;
                while (true)
                {
                    if (isRunning)
                        count = 15;
                    else
                        count--;
                    if (count == 0)
                        Environment.Exit(Environment.ExitCode);
                    Thread.Sleep(60000);
                }
            }));
            Timer.Start();
        }
        #region Open Chrome Port 9222
        private void button1_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "Chrome\\chrome.exe";
            p.StartInfo.Arguments = "--remote-debugging-port=9222 \"https://redbubble.com\"";
            p.Start();
        } 
        #endregion
        #region File Input
        private void DragDropHandler(object sender, DragEventArgs e)
        {
            var dropped = ((string[])e.Data.GetData(DataFormats.FileDrop));
            var files = dropped.ToList();
            if (!files.Any())
                return;
            FileName = files[0];
            lbInput.Text = FileName;
        }

        private void DragEnterHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        private void label1_DoubleClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileName = openFileDialog1.FileName;
                lbInput.Text = FileName;
            }
        }

        #endregion

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lbInfo.Text = trackBar.Value.ToString();
            Processor.GetInstance().TimeOffset = trackBar.Value;
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            Worker?.Abort();
            if (!File.Exists(FileName))
                return;
            List<RedbubbleInputInfo> inputs = null;
            using (StreamReader reader = new StreamReader(FileName))
            using (var csv = new CsvReader(reader))
            {
                try
                {
                    inputs = csv.GetRecords<RedbubbleInputInfo>().ToList();
                }
                catch
                {
                    MessageBox.Show("Check your input");
                    return;
                }
            }
            if (inputs == null || inputs.Count == 0)
            {
                MessageBox.Show("Check your input");
                return;
            }
            int currentNumberOfImagesUploaded = 0; 
            MethodInvoker updateProgress = delegate
            {
                progressBar.Value = currentNumberOfImagesUploaded * 100 / inputs.Count;
                lbProgress.Text = $"Uploaded {currentNumberOfImagesUploaded} in {inputs.Count} files";
            };
            string logFile = "Log.txt";
            List<string> uploadedImages = File.Exists(logFile) ? File.ReadAllLines(logFile).ToList() : new List<string>();
            Worker = new Thread(new ThreadStart(
            () =>
            { 
                isRunning = true;
                Processor.GetInstance().CreateDriver(); 
                foreach (RedbubbleInputInfo input in inputs)
                {
                    currentNumberOfImagesUploaded++;
                    if (uploadedImages.Contains(input.Image.TrimEnd().TrimStart()) || !File.Exists(input.Image))
                        continue;
                    Processor.GetInstance().Begin(input); 
                    Invoke(updateProgress);
                    File.WriteAllLines(logFile, new List<string> { input.Image.TrimStart().TrimEnd() }); 
                } 
                Processor.GetInstance().Dispose();
                Processor.GetInstance().RandomSleep(3045, 4980);
                isRunning = false;
            }
            ));
            Worker.Start();
        }
        #region Dispose  
        private void btnStop_Click(object sender, EventArgs e)
        {
            Worker?.Abort();
            Processor.GetInstance().Dispose();
            isRunning = false;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Worker?.Abort();
            Timer?.Abort();
            Processor.GetInstance().Dispose();
        }
        #endregion

     
    }
}
