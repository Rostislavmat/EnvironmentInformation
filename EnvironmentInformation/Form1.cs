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
using java.io;
using edu.stanford.nlp.pipeline;
using Console = System.Console;
using System.Collections.Generic;

namespace EnvironmentInformation
{


    /*public class WeirdString
    {
        int begin;
        int cur_to_write;
        char[] storage;




        public WeirdString(string x,int len)
        {
            if (x.Length > len)
                throw new ArgumentException("len parametr should be less then x length");
            while (storage.Length + x.Length != len)
            {
                storage += " ";
            }
            this.storage += x;
        }



        public static WeirdString operator +(WeirdString c1, char c)
        {
            c1.storage[c1.cur_to_write] = c;
            c1.cur_to_write++;
            c1.cur_to_write %= c1.storage.Length;

        }
    }*/
    

    public partial class Form1 : Form
    {
        Trie words = new Trie();

        /*Dictionary<string , long> words =
            new Dictionary<string , long>();*/
        private System.Object lockThis = new System.Object();

        public void UpdateOnBase(string path)
        {
            System.IO.StreamReader file =
            new System.IO.StreamReader(path);
            string line;

            while ((line = file.ReadLine()) != null)
            {
                line = line.ToLower();

                string line2 = file.ReadLine();
                long num = Int64.Parse(line2);
                try
                {
                }
                catch (KeyNotFoundException)
                {
                }
            }
            file.Close();
        }

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

            while ((line = file.ReadLine()) != null)
            {
                line = line.ToLower();

                string line2 = file.ReadLine();
                long num = Int64.Parse(line2);
                try
                {
                    //words[line] += num;
                }
                catch (KeyNotFoundException)
                {
                    //words[line] = num;
                }
            }
            file.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter file =
                new System.IO.StreamWriter(Environment.CurrentDirectory + "\\words.txt");
            /*foreach (var x in words)
            {
                file.WriteLine(x.Key);
                file.WriteLine(x.Value);
            }*/
            file.Close();
        }
    }
}
