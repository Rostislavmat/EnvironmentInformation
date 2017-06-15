using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SQL
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "new-lang-helper.database.windows.net";
            builder.UserID = "jabberwocky";
            builder.Password = "1q2w3e4R";
            builder.InitialCatalog = "newLangHelper";
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {

                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("delete from polish; ");
                String sql = sb.ToString();
                var file = new StreamReader("words.txt");
                string line;
                 sb = new StringBuilder();
                sb.Append("INSERT INTO words ([word], [amount]) ");
                sb.Append("VALUES (@word, @amount);");
                sql = sb.ToString();
                while ((line = file.ReadLine()) != null)
                {
                    line = line.ToLower();
                    string line2 = file.ReadLine();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@word", line);
                        command.Parameters.AddWithValue("@amount", line2);
                        command.ExecuteNonQuery();
                    }
                }
                file.Close();
                file = new StreamReader("polish.txt");
                sb = new StringBuilder();
                sb.Append("INSERT INTO polish ([word], [translation]) ");
                sb.Append("VALUES (@word, @translation);");
                sql = sb.ToString();
                while ((line = file.ReadLine()) != null)
                {
                    line = line.ToLower();
                    string line2 = file.ReadLine();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@word", line);
                        command.Parameters.AddWithValue("@translation", line2);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
