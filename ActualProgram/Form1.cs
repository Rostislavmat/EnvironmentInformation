using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;
using Google.Cloud.Translation.V2;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text;
using System.IO;
namespace NewLangGeneration
{

    public partial class Form1 : Form
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        SqlConnection connection;
        readonly Trie _words = new Trie();
        private int wordNumber = 100;
        const string BookPath = @"books\texts";
        private string _currentLanguage = "Polish";
        public Form1()
        {
            InitializeComponent();
            builder.DataSource = "new-lang-helper.database.windows.net";
            builder.UserID = "jabberwocky";
            builder.Password = "1q2w3e4R";
            builder.InitialCatalog = "newLangHelper";
            connection= new SqlConnection(builder.ConnectionString);
            WordsInit();
        }



        private void btnExit_Click(object sender, EventArgs e) => Environment.Exit(0);


        private void btnClear_Click(object sender, EventArgs e) => lbx.Items.Clear();

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
                        _words.AddWord(reader.GetString(0), reader.GetInt32(1));
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


        private void btnRead_Click(object sender, EventArgs e)
        {
            DialogResult dialog = openFileDialog1.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;

                try
                {
                    if (Directory.Exists("data_to_parse"))
                    {
                        Directory.Delete("data_to_parse", true);
                    }
                    ZipFile.ExtractToDirectory(fileName, "data_to_parse");
                    const string parsePath = "data_to_parse";
                    var di = new DirectoryInfo(parsePath);
                    FileInfo[] files = di.GetFiles("*.txt");
                    var names = new List<string>();
                    
                    foreach(var x in files)
                    {
                        names.Add(x.FullName);
                    }
                    Parallel.ForEach(names, thread_DoWork);
                    lbx.Items.Add("analyzing finished");
                    Directory.Delete("data_to_parse", true);
                }
                catch
                {
                    lbx.Items.Add("Wrong file");
                }


            }

        }


        private void button3_Click(object sender, EventArgs e)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            long xx = 0;
            var toSave = _words.GetStatistics();
            connection.Open();
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE words SET amount = @amount WHERE word = @word;");
            String sql = sb.ToString();
            foreach (var x in toSave)
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@word", x.Key);
                    command.Parameters.AddWithValue("@amount", x.Value.ToString());
                    int rowsAffected = command.ExecuteNonQuery();
                }
                xx++;
            }
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            lbx.Items.Add("Read finished " + elapsedTime + " lines wrote: " + xx * 2);
        }



        private void thread_DoWork(string fileName)
        {
            var reader = new SmartReader(fileName);
            string nxt;
            while ((nxt = reader.NextChar()) != null)
            {
                _words.AnalyzeString(nxt);
            }
            reader.Close();
        }
        class Translation
        {
            public string Eng { get; }
            public string Pol { get; }

            public Translation(string en, string pl)
            {
                Eng = en;
                Pol = pl;
            }

        }
        private void button4_Click(object sender, EventArgs e)
        {
            string fileName = "top" + wordNumber.ToString() + ".txt";

            var file = new StreamWriter(fileName);
            string resultName = _currentLanguage.ToLower() + ".txt";
            var dict = new StreamReader(resultName);
            var translation = new Dictionary<string, string>();
            connection.Open();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * from ");
            sb.Append(_currentLanguage.ToLower());
            sb.Append("; ");
            String sql = sb.ToString();
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        translation[reader.GetString(0)] = reader.GetString(1);
                    }
                }
            }
            sb = new StringBuilder();
            var result = new List<Tuple<string, long>>();
            sb.Append("SELECT * from words");
            sql = sb.ToString();
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Tuple<string, long>(reader.GetString(0), reader.GetInt32(1)));
                    }
                }
            }
            connection.Close();
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            var resultToJson = new List<Translation>();
            for (int i = 0; i < wordNumber; i++)
            {
                resultToJson.Add(new Translation(result[i].Item1, translation[result[i].Item1]));
            }
            file.WriteLine(JsonConvert.SerializeObject(resultToJson));
            file.Close();
            lbx.Items.Add("top"  + wordNumber.ToString() + " finished ");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentLanguage = (string)listBox1.SelectedItem;
        }
        private void createTable()
        {
            connection.Open();
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(_currentLanguage.ToLower());
            sb.Append(" ( word varchar(25), translation(255) );");
            String sql = sb.ToString();
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        private bool checkTable()
        {
            connection.Open();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT COUNT(*) FROM information_schema.TABLES WHERE (TABLE_NAME = '");
            sb.Append(_currentLanguage.ToLower());
            sb.Append("' ;");
            String sql = sb.ToString();
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetInt32(0)==0)
                        {
                            connection.Close();
                            createTable();
                            return true;
                        }
                        else
                        {
                            connection.Close();
                            return false;
                        }
                    }
                }
            }
            connection.Close();
            return false;
        }
        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            if (!checkTable())
            {
                return;
            }
            string lang;
            string fileName = _currentLanguage.ToLower() + ".txt";
            switch (_currentLanguage)
            {
                case "Russian":
                    lang = "ru";
                    break;
                case "Polish":
                    lang = "pl";
                    break;
                default:
                    lang = "pl";
                    break;
            }
            connection.Open();
            String sql = "Select word from words";
            List<String> words = new List<string>();
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        words.Add(reader.GetString(0));
                    }
                }
            }
            var client = TranslationClient.Create();
            var result = new StreamWriter(fileName);
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(_currentLanguage.ToLower());
            sb.Append(" ( [word], [transtalion]) ");
            sb.Append("VALUES (@word, @translation);");
            sql = sb.ToString();
            

            foreach (var word in words)
            {
                var response = client.TranslateText(word, lang, "en");
                result.WriteLine(response.TranslatedText);
                var translation = response.TranslatedText;
                if (translation != string.Empty)
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@word", word);
                        command.Parameters.AddWithValue("@translation", translation);
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            connection.Close();

        }

        private void button5_Click(object sender, EventArgs e)
        {

            var files = Directory.GetFiles("books", "*.zip");
            Directory.Delete(BookPath, true);
            Directory.CreateDirectory(BookPath);
            Parallel.ForEach(files, Unzip);
            if (File.Exists("archive.zip"))
            {
                File.Delete("archive.zip");
            }
            ZipFile.CreateFromDirectory(BookPath, "archive.zip");
            lbx.Items.Add("preparing archive finished");
        }

        private static void Unzip(string filename)
        {
            string x = BookPath + '\\' + Path.GetFileNameWithoutExtension(filename);
            if (Directory.Exists(x))
            {
                Directory.Delete(BookPath + "\\" + Path.GetFileNameWithoutExtension(filename),true);
            }
            try
            {
                ZipFile.ExtractToDirectory(filename, BookPath);
            }
            catch
            {
            }

            var pureName = Path.GetFileNameWithoutExtension(filename);
            var dirName = Path.Combine(BookPath, pureName);

            if (Directory.Exists(dirName))
            {
                File.Copy(Path.Combine(dirName, $"{pureName}.txt"),
                    Path.Combine(BookPath, $"{pureName}.txt"));

                Directory.Delete(dirName, true);
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            wordNumber = Int32.Parse((string)listBox2.SelectedItem);
        }
    }

}
