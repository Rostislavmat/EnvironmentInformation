using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using java.util;
using System.Timers;
using System.Diagnostics;
using java.io;
using edu.stanford.nlp.pipeline;
using Console = System.Console;
using System.Collections.Generic;

namespace EnvironmentInformation
{
    public class Utility
    {
        public static bool isLetter(char x)
        {
            return (x <= 'z' && x >= 'a') || (x >= 'A' && x <= 'Z');
        }
    }

    

    public partial class Form1 : Form
    {
        Trie words = new Trie();

        private System.Object lockThis = new System.Object();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            OperatingSystem os = Environment.OSVersion;
            PlatformID OSid = os.Platform;
            string[] drives = Environment.GetLogicalDrives();
            string drivesString = "";
            foreach (string drive in drives)
            {
                drivesString += drive + ", ";
            }
            drivesString = drivesString.TrimEnd(' ', ',');
            lbx.Items.Add("Machine Name: \t" + Environment.MachineName);
            lbx.Items.Add("Operating System: \t" + Environment.OSVersion);
            lbx.Items.Add("Operating System ID:\t" + OSid);
            lbx.Items.Add("Current Folder: \t" + Environment.CurrentDirectory);
            lbx.Items.Add("CLR Version: \t" + Environment.Version);
            lbx.Items.Add("Present Drives: \t" + drivesString);
            lbx.Items.Add("Program Files: \t" + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

        }

        private void btnExit_Click(object sender, EventArgs e)
        {

            Environment.Exit(0);
        }



        private void btnClear_Click(object sender, EventArgs e)
        {
            lbx.Items.Clear();
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            DialogResult dialog = openFileDialog1.ShowDialog();

            if (dialog == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                lblFileName.Text = "File Name: " + fileName;
                string directoryName = Environment.CurrentDirectory;
                directoryName += "/arch/";
                try
                {
                    directoryName += Path.GetFileNameWithoutExtension(fileName);
                    ZipFile.ExtractToDirectory(fileName, directoryName);
                }
                catch
                {
                    Console.Error.WriteLine("Error during unzip");
                }
                //System.IO.Directory.Delete(directoryName, true);
                
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                byte[] byteText = new byte[fs.Length];
                fs.Read(byteText, 0, byteText.Length);
                txtFile.Text = System.Text.Encoding.ASCII.GetString(byteText);
                fs.Close();

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialog = saveFileDialog1.ShowDialog();

            if (dialog == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;
                FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                byte[] byteText = System.Text.Encoding.ASCII.GetBytes(txtFile.Text);
                fs.Write(byteText, 0, byteText.Length);
                fs.Close();
            }


        }



        private void button2_Click_1(object sender, EventArgs e)
        {
            System.IO.StreamReader file =
                new System.IO.StreamReader(Environment.CurrentDirectory + "\\words.txt");
            string line;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            long xx = 0;
            while ((line = file.ReadLine()) != null)
            {
                line = line.ToLower();
                xx++;
                string line2 = file.ReadLine();
                long num = Int64.Parse(line2);
                words.addWord(line, num);
            }
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            lbx.Items.Add("Read finished " + elapsedTime + " lines read: " + xx);
            file.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter file =
                new System.IO.StreamWriter(Environment.CurrentDirectory + "\\words_out.txt");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            long xx = 0;
            var toSave = words.getStatistics();

            foreach (var x in toSave)
            {
                file.WriteLine(x.Key);
                file.WriteLine(x.Value);
                xx++;
            }
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            lbx.Items.Add("Read finished " + elapsedTime + " lines wrote: " + xx*2);
            file.Close();
        }
    }
}
