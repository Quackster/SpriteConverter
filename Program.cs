using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteConverter
{
    class Program
    {
        class FurniItem
        {
            public string Type;
            public string SpriteId;
            public string FileName;
            public string Revision;
            public string Unknown;
            public string Length;
            public string Width;
            public string Colour;
            public string Name;
            public string Description;

            public FurniItem(List<string> data)
            {
                this.Type = data[0];
                this.SpriteId = data[1];
                this.FileName = data[2];
                this.Revision = data[3];
                this.Unknown = data[4];
                this.Length = data[5];
                this.Width = data[6];
                this.Colour = data[7];
                this.Name = data[8];
                this.Description = data[9];
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Converter for furnidata.txt");

            string fileContents = File.ReadAllText("furnidata.txt");
            var furnidataList = JsonConvert.DeserializeObject<List<List<string>>>(fileContents);

            List<FurniItem> itemList = new List<FurniItem>();

            foreach (var stringArray in furnidataList)
                itemList.Add(new FurniItem(stringArray));

            MySqlConnectionStringBuilder pCSB = new MySqlConnectionStringBuilder();

            // Server
            pCSB.Server = "localhost";
            pCSB.Port = 3306;
            pCSB.UserID = "root";
            pCSB.Password = "verysecret";

            // Database
            pCSB.Database = "kepler";
            pCSB.MinimumPoolSize = 5;
            pCSB.MaximumPoolSize = 10;
            pCSB.SslMode = MySqlSslMode.None;

            MySqlConnection conn = new MySqlConnection(pCSB.ToString());
            conn.Open();

            foreach (FurniItem item in itemList)
                UpdateRows(conn, item);

            conn.Close();

            Console.WriteLine("Finished");
            Console.Read();
        }

        private static void UpdateRows(MySqlConnection sqlConnection, FurniItem item)
        {
            if (item.Name.Length == 0)
            {
                return;
            }

            MySqlCommand command;
            
            command = new MySqlCommand("SELECT * FROM items_definitions WHERE sprite = @sprite;", sqlConnection);
            command.Parameters.AddWithValue("@sprite", item.FileName);
            command.CommandType = CommandType.Text;

            if (command.ExecuteScalar() == null)
            {
                Console.WriteLine("Command: " + item.Name );
            }
        }
    }
}