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
            public int Length;
            public int Width;
            public string Colour;
            public string Name;
            public string Description;

            public FurniItem(string[] data)
            {
                this.Type = data[0];
                this.SpriteId = data[1];
                this.FileName = data[2];
                this.Revision = data[3];
                this.Unknown = data[4];
                try
                {
                    this.Length = Convert.ToInt32(data[5]);
                    this.Width = Convert.ToInt32(data[6]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR parsing length and width: " + this.FileName);
                }

                this.Colour = data[7];
                this.Name = data[8];
                this.Description = data[9];
            }
        }

        class CatalogueItem
        {
            public string FileName;
            public int ItemId;

            public CatalogueItem(string fileName, int itemId)
            {
                FileName = fileName;
                ItemId = itemId;
            }
        }

        private static MySqlConnectionStringBuilder pCSB;
        private static Dictionary<int, List<CatalogueItem>> catalogueItems;

        static void Main(string[] args)
        {
            Console.WriteLine("Converter for furnidata.txt");

            string fileContents = File.ReadAllText("furnidata.txt");
            var furnidataList = JsonConvert.DeserializeObject<List<string[]>>(fileContents);

            List<FurniItem> itemList = new List<FurniItem>();
            catalogueItems = new Dictionary<int, List<CatalogueItem>>();
              
            foreach (var stringArray in furnidataList)
                itemList.Add(new FurniItem(stringArray));

            pCSB = new MySqlConnectionStringBuilder();

            // Server
            pCSB.Server = "localhost";
            pCSB.Port = 3306;
            pCSB.UserID = "kepler";
            pCSB.Password = "verysecret";

            // Database
            pCSB.Database = "dev";
            pCSB.MinimumPoolSize = 5;
            pCSB.MaximumPoolSize = 10;
            pCSB.SslMode = MySqlSslMode.None;

            MySqlConnection sqlConnection = new MySqlConnection(pCSB.ToString());
            sqlConnection.Open();

           /*MySqlCommand command;
            
            command = new MySqlCommand("DELETE FROM items_definitions", sqlConnection);
            command.ExecuteNonQuery();

            command = new MySqlCommand("SELECT * FROM catalogue_items", sqlConnection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                int id = (int)reader["id"];
                int pageId = (int)reader["page_id"];
                int definitionId = (int)reader["definition_id"];

                using (MySqlConnection connection = new MySqlConnection(pCSB.ToString()))
                {
                    connection.Open();

                    var cmd = new MySqlCommand("SELECT * FROM items_definitions2 WHERE id = @definition_id;", connection);
                    cmd.Parameters.AddWithValue("@definition_id", definitionId);
                    cmd.CommandType = CommandType.Text;

                    var row = cmd.ExecuteReader();

                    if (row.Read())
                    {
                        if (!catalogueItems.ContainsKey(pageId))
                        {
                            catalogueItems.Add(pageId, new List<CatalogueItem>());
                        }

                        catalogueItems[pageId].Add(new CatalogueItem(row["sprite"].ToString(), definitionId));
                    }
                }
            }*/

            foreach (FurniItem item in itemList)
            {
                if (item.FileName.StartsWith("carpet_soft*9"))
                {
                    using (MySqlConnection conn = new MySqlConnection(pCSB.ToString()))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("UPDATE items_definitions SET sprite_id = @sprite_id WHERE sprite = @sprite", conn);
                        cmd.Parameters.AddWithValue("@sprite", item.FileName);
                        cmd.Parameters.AddWithValue("@sprite_id", item.SpriteId);
                        cmd.ExecuteNonQuery();
                    }
                }
                //UpdateRows(item);
            }

            /*foreach (var kvp in catalogueItems)
            {
                int pageId = kvp.Key;
                List<CatalogueItem> catalogueItem = kvp.Value;

                int newId = -1;

                foreach (CatalogueItem item in kvp.Value)
                {
                    using (MySqlConnection conn = new MySqlConnection(pCSB.ToString()))
                    {
                        conn.Open();

                        MySqlCommand cmd = new MySqlCommand("SELECT * FROM items_definitions WHERE sprite = @sprite;", conn);
                        cmd.Parameters.AddWithValue("@sprite", item.FileName);

                        reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            newId = (int)reader["id"];
                        }
                    }

                    using (MySqlConnection conn = new MySqlConnection(pCSB.ToString()))
                    {
                        conn.Open();

                        MySqlCommand cmd = new MySqlCommand("UPDATE catalogue_items SET definition_id = @new_id WHERE definition_id = @old_id", conn);
                        cmd.Parameters.AddWithValue("@old_id", item.ItemId);
                        cmd.Parameters.AddWithValue("@new_id", newId);
                        cmd.ExecuteNonQuery();

                    }
                }
            }*/

            sqlConnection.Close();

            Console.WriteLine("Finished");
            Console.Read();
        }

        private static void UpdateRows(FurniItem item)
        {
            if (item.FileName.Length == 0)
            {
                return;
            }

            if (item.FileName == "wallpaper" || item.FileName == "floor" || item.FileName == "landscape" || item.FileName == "poster")
            {
                return;
            }

            using (MySqlConnection sqlConnection = new MySqlConnection(pCSB.ToString()))
            {
                sqlConnection.Open();

                MySqlCommand command;

                command = new MySqlCommand("SELECT * FROM items_definitions2 WHERE sprite = @sprite;", sqlConnection);
                command.Parameters.AddWithValue("@sprite", item.FileName);
                command.CommandType = CommandType.Text;

                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    using (MySqlConnection conn = new MySqlConnection(pCSB.ToString()))
                    {
                        conn.Open();

                        var cmd = new MySqlCommand("INSERT INTO items_definitions (sprite, sprite_id, colour, length, width, top_height, behaviour) " +
                        " VALUES (@sprite, @sprite_id, @colour, @length, @width, @top_height, @behaviour);", conn);
                        cmd.Parameters.AddWithValue("@sprite", item.FileName);
                        cmd.Parameters.AddWithValue("@sprite_id", item.SpriteId);
                        cmd.Parameters.AddWithValue("@colour", item.Colour);
                        cmd.Parameters.AddWithValue("@length", item.Length);
                        cmd.Parameters.AddWithValue("@width", item.Width);
                        cmd.Parameters.AddWithValue("@top_height", (double)reader["top_height"]);
                        cmd.Parameters.AddWithValue("@behaviour", (string)reader["behaviour"]);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}