using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NewLangGeneration;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace IOClient
{
    public partial class Client : Form
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        SqlConnection connection;
        readonly Trie _words = new Trie();
        Dictionary<string, long> regularDistribution = new Dictionary<string, long>();
        private int wordNumber = 100;
        private string _currentLanguage = "Polish";
        private int amout;

        public Client()
        {
            InitializeComponent();
            builder.DataSource = "new-lang-helper.database.windows.net";
            builder.UserID = "jabberwocky";
            builder.Password = "1q2w3e4R";
            builder.InitialCatalog = "newLangHelper";
            connection = new SqlConnection(builder.ConnectionString);
            WordsInit();
        }

        private void WordsInit() => WordsInit(this, null);

        private void WordsInit(object sender, EventArgs e)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            connection.Open();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * from words; ");
            String sql = sb.ToString();
            int xx = 0;
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        xx++;
                        _words.AddWord(reader.GetString(0), 0);
                        regularDistribution[reader.GetString(0)] = reader.GetInt32(1);
                    }
                }
            }
            connection.Close();
            TimeSpan ts = stopWatch.Elapsed;
            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            lbx.Items.Add("Read finished " + elapsedTime + " lines read: " + xx);
        }

        private double calculatePopularity(long inText,long atAll,double ratio,int position)
        {
            return Math.Pow(inText/ (atAll*ratio),1.5)/ Math.Pow(position, 0.40); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialog = openFileDialog1.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                var reader = new SmartReader(fileName);
                string nxt;
                while ((nxt = reader.NextChar()) != null)
                {
                    _words.AnalyzeString(nxt);
                }
                reader.Close();
                var stats = _words.GetStatistics();
                var toSort = new List<Tuple<string, long>>();

                foreach (var word in stats)
                {
                    toSort.Add(new Tuple<string, long>(word.Key, word.Value));
                }
                toSort.Sort((x, y) => y.Item2.CompareTo(x.Item2));
                string resultName = Path.GetFileNameWithoutExtension(fileName) + "_top" + wordNumber.ToString() + ".txt";
                long mySum = toSort.Aggregate(0l,(currentSum, next) =>
                                      next.Item2 + currentSum);
                long allSum = regularDistribution.Aggregate(0l, (currentSum, next) =>
                                       next.Value + currentSum);
                double ratio = (mySum +.0) / allSum;

                var translation = new Dictionary<string, string>();
                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT * from ");
                sb.Append(_currentLanguage.ToLower());
                sb.Append("; ");
                String sql = sb.ToString();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader sqlReader = command.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            translation[sqlReader.GetString(0)] = sqlReader.GetString(1);
                        }
                    }
                }
                connection.Close();
                int amount = 0;
                var result = new StreamWriter(resultName);
                var popularity= new List<Tuple<string, double>>();

                foreach (var x in toSort)
                {
                    if (amount < wordNumber && regularDistribution[x.Item1]!=0 && x.Item2/(regularDistribution[x.Item1]*ratio) > 1.1)
                    {
                        amount++;
                        popularity.Add(new Tuple<string, double>
                            (x.Item1, calculatePopularity(x.Item2, regularDistribution[x.Item1], ratio, amount)));
                        
                    }
                }
                popularity.Sort((x, y) => y.Item2.CompareTo(x.Item2));
                for (int i=0;i<amount;i++)
                {
                    var x = popularity[i].Item1;
                    result.WriteLine(x);
                    result.WriteLine(translation[x]);
                }
                lbx.Items.Add("top" + amount.ToString() + " finished ");
                result.Close();
            }
        }

        private void languages_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentLanguage = (string)languages.SelectedItem;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            wordNumber = Int32.Parse((string)listBox1.SelectedItem);
        }
    }
}
