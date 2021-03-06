﻿using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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

            public FurniItem()
            {
            }

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

        private static MySqlConnectionStringBuilder holoDbString;
        private static MySqlConnectionStringBuilder keplerDbString;

        private static Dictionary<String, int> maxInteractions;
        private static Dictionary<int, List<CatalogueItem>> catalogueItems;
        private static List<FurniItem> ItemList = new List<FurniItem>();

        static void Main(string[] args)
        {
            maxInteractions = new Dictionary<string, int>();

            Console.WriteLine("Converter for furnidata.txt");



            string furnidataPath = "furnidata2.xml";
            var furnidataExtension = Path.GetExtension(furnidataPath);

            if (furnidataExtension == ".xml")
            {
                ParseFurnidataXML(furnidataPath);
            }
            else
            {
                var officialFileContents = File.ReadAllText(furnidataPath);
                officialFileContents = officialFileContents.Replace("]]\n[[", "],[");
                var officialFurnidataList = JsonConvert.DeserializeObject<List<string[]>>(officialFileContents);

                foreach (var stringArray in officialFurnidataList)
                {
                    ItemList.Add(new FurniItem(stringArray));
                }
            }

            holoDbString = new MySqlConnectionStringBuilder();
            holoDbString.Server = "localhost";
            holoDbString.Port = 3306;
            holoDbString.UserID = "kepler";
            holoDbString.Password = "verysecret";
            holoDbString.Database = "holodb";
            holoDbString.MinimumPoolSize = 5;
            holoDbString.MaximumPoolSize = 10;
            holoDbString.SslMode = MySqlSslMode.None;

            keplerDbString = new MySqlConnectionStringBuilder();
            keplerDbString.Server = "localhost";
            keplerDbString.Port = 3306;
            keplerDbString.UserID = "root";
            keplerDbString.Password = "123";
            keplerDbString.Database = "kurkku";
            keplerDbString.MinimumPoolSize = 5;
            keplerDbString.MaximumPoolSize = 10;
            keplerDbString.SslMode = MySqlSslMode.None;

            /*using (MySqlConnection connection = new MySqlConnection(holoDbString.ToString()))
            {
                connection.Open();

                var cmd = new MySqlCommand("SELECT * FROM catalogue_items", connection);
                cmd.CommandType = CommandType.Text;

                var row = cmd.ExecuteReader();

                while (row.Read())
                {
                    string nameCct = row["name_cct"].ToString();
                    int maxStatus = Convert.ToInt32(row["status_max"].ToString());

                    if (maxInteractions.ContainsKey(nameCct))
                    {
                        Console.WriteLine(nameCct + " / " + maxStatus);
                        continue;
                    }

                    maxInteractions.Add(nameCct, maxStatus);
                }
            }

            foreach (var kvp in maxInteractions)
            {
                using (MySqlConnection conn = new MySqlConnection(keplerDbString.ToString()))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE items_definitions SET max_status = @status_max WHERE sprite = @sprite", conn);
                    cmd.Parameters.AddWithValue("@status_max", kvp.Value);
                    cmd.Parameters.AddWithValue("@sprite", kvp.Key);
                    cmd.ExecuteNonQuery();
                }
            }*/

            MySqlConnection sqlConnection = new MySqlConnection(keplerDbString.ToString());
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

            foreach (FurniItem item in ItemList)
            {
                using (MySqlConnection conn = new MySqlConnection(keplerDbString.ToString()))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE item_definitions SET sprite_id = @sprite_id, name = @name, description = @description WHERE sprite = @sprite", conn);
                    cmd.Parameters.AddWithValue("@sprite", item.FileName);
                    cmd.Parameters.AddWithValue("@sprite_id", item.SpriteId);
                    cmd.Parameters.AddWithValue("@name", item.Name);
                    cmd.Parameters.AddWithValue("@description", item.Description);
                    cmd.ExecuteNonQuery();
                }
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
//            Console.Read();
        }

        private static void ParseFurnidataXML(string furnidataPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(furnidataPath);

            var floorTypes = xmlDoc.SelectNodes("//furnidata/roomitemtypes/furnitype");
            var floorItems = floorTypes.Count;

            for (int i = 0; i < floorItems; i++)
            {
                var itemNode = floorTypes.Item(i);

                if (itemNode.Attributes.GetNamedItem("classname") == null)
                    continue;

                var className = itemNode.Attributes.GetNamedItem("classname").InnerText;

                var length = itemNode.ChildNodes.Item(2).InnerText;
                var width = itemNode.ChildNodes.Item(3).InnerText;
                var name = itemNode.ChildNodes.Item(5).InnerText;
                var description = itemNode.ChildNodes.Item(6).InnerText;

                FurniItem furniItem = new FurniItem();
                furniItem.Length = int.Parse(length);
                furniItem.Width = int.Parse(width);
                furniItem.Width = int.Parse(width);
                furniItem.Type = "S";
                furniItem.SpriteId = itemNode.Attributes.GetNamedItem("id").InnerText;
                furniItem.FileName = className;
                furniItem.Name = name;
                furniItem.Description = description;
                ItemList.Add(furniItem);
            }

            var wallTypes = xmlDoc.SelectNodes("//furnidata/wallitemtypes/furnitype");
            var wallItems = wallTypes.Count;

            for (int i = 0; i < wallItems; i++)
            {
                var itemNode = wallTypes.Item(i);

                if (itemNode.Attributes.GetNamedItem("classname") == null)
                    continue;

                var className = itemNode.Attributes.GetNamedItem("classname").InnerText;
                var name = itemNode.ChildNodes.Item(5).InnerText;
                var description = itemNode.ChildNodes.Item(6).InnerText;

                FurniItem furniItem = new FurniItem();
                furniItem.Type = "I";
                furniItem.FileName = className;
                furniItem.SpriteId = itemNode.Attributes.GetNamedItem("id").InnerText;
                furniItem.Name = name;
                furniItem.Description = description;
                ItemList.Add(furniItem);
            }
        }

        /*private static void UpdateRows(FurniItem item)
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
        }*/
    }
}

/*using MySql.Data.MySqlClient;
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

        private static MySqlConnectionStringBuilder holoDbString;
        private static MySqlConnectionStringBuilder keplerDbString;

        private static Dictionary<String, string> vendingItems;
        private static Dictionary<int, List<CatalogueItem>> catalogueItems;

        static void Main(string[] args)
        {
            vendingItems = new Dictionary<string, string>();

            Console.WriteLine("Converter for furnidata.txt");

            string fileContents = File.ReadAllText("furnidata.txt");
            var furnidataList = JsonConvert.DeserializeObject<List<string[]>>(fileContents);

            List<FurniItem> itemList = new List<FurniItem>();
            catalogueItems = new Dictionary<int, List<CatalogueItem>>();

            foreach (var stringArray in furnidataList)
                itemList.Add(new FurniItem(stringArray));

            holoDbString = new MySqlConnectionStringBuilder();
            holoDbString.Server = "localhost";
            holoDbString.Port = 3306;
            holoDbString.UserID = "kepler";
            holoDbString.Password = "verysecret";
            holoDbString.Database = "v33";
            holoDbString.MinimumPoolSize = 5;
            holoDbString.MaximumPoolSize = 10;
            holoDbString.SslMode = MySqlSslMode.None;

            keplerDbString = new MySqlConnectionStringBuilder();
            keplerDbString.Server = "localhost";
            keplerDbString.Port = 3306;
            keplerDbString.UserID = "kepler";
            keplerDbString.Password = "verysecret";
            keplerDbString.Database = "dev";
            keplerDbString.MinimumPoolSize = 5;
            keplerDbString.MaximumPoolSize = 10;
            keplerDbString.SslMode = MySqlSslMode.None;

            using (MySqlConnection connection = new MySqlConnection(holoDbString.ToString()))
            {
                connection.Open();

                var cmd = new MySqlCommand("SELECT * FROM catalogue_items", connection);
                cmd.CommandType = CommandType.Text;

                var row = cmd.ExecuteReader();

                while (row.Read())
                {
                    string nameCct = row["name_cct"].ToString();
                    string drinkIds = row["drink_ids"].ToString();

                    if (vendingItems.ContainsKey(nameCct) || drinkIds == null || drinkIds.Length == 0)
                    {
                        continue;
                    }

                    vendingItems.Add(nameCct, drinkIds);
                }
            }

            foreach (var kvp in vendingItems)
            {
                using (MySqlConnection conn = new MySqlConnection(keplerDbString.ToString()))
                {
                    Console.WriteLine(kvp.Value.Replace("\r\n", ","));

                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE items_definitions SET drink_ids = @drink_ids, interactor = 'vending_machine' WHERE sprite = @sprite", conn);
                    cmd.Parameters.AddWithValue("@drink_ids", kvp.Value.Replace("\r\n", ","));
                    cmd.Parameters.AddWithValue("@sprite", kvp.Key);
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Finished");
            Console.Read();
        }
    }
}
*/













/*using MySql.Data.MySqlClient;
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

            }

            this.Colour = data[7];
            this.Name = data[8];
            this.Description = data[9];
        }

        class ProductItem
        {
            public string FileName;
            public string Name;
            public string Description;

            public ProductItem(string[] data)
            {
                this.FileName = data[0];
                this.Name = data[1];
                this.Description = data[2];
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

        private static MySqlConnectionStringBuilder holoDbString;
        private static MySqlConnectionStringBuilder keplerDbString;

        private static Dictionary<String, int> maxInteractions;
        private static Dictionary<int, List<CatalogueItem>> catalogueItems;

        static void Main(string[] args)
        {
            maxInteractions = new Dictionary<string, int>();

            Console.WriteLine("Converter for furnidata.txt");

            string fileContents = File.ReadAllText("furnidata.txt");
            var furnidataList = JsonConvert.DeserializeObject<List<string[]>>(fileContents);

            List<FurniItem> itemList = new List<FurniItem>();
            List<ProductItem> productList = new List<ProductItem>();

            catalogueItems = new Dictionary<int, List<CatalogueItem>>();

            foreach (var stringArray in furnidataList)
                itemList.Add(new FurniItem(stringArray));

            Dictionary<int, string> posterDataName = new Dictionary<int, string>();
            Dictionary<int, string> posterDataDesc = new Dictionary<int, string>();

            keplerDbString = new MySqlConnectionStringBuilder();
            keplerDbString.Server = "localhost";
            keplerDbString.Port = 3306;
            keplerDbString.UserID = "root";
            keplerDbString.Password = "123";
            keplerDbString.Database = "keplerdev";
            keplerDbString.MinimumPoolSize = 5;
            keplerDbString.MaximumPoolSize = 10;
            keplerDbString.SslMode = MySqlSslMode.None;


            int counter = 0;
            string line;

            System.IO.StreamReader file = new System.IO.StreamReader("external_texts.txt");

            while ((line = file.ReadLine()) != null)
            {
                MySqlConnection conn = new MySqlConnection(keplerDbString.ToString());
                conn.Open();

                if (line.StartsWith("poster_") && line.Contains("_name"))
                {
                    int posterId = int.Parse(line.Split('_')[1]);
                    posterDataName.Add(posterId, line.Trim().Replace("poster_" + posterId + "_name=", ""));


                    MySqlCommand cmd = new MySqlCommand("UPDATE catalogue_items SET name = @name WHERE sale_code = @sale_code AND item_specialspriteid = @item_specialspriteid", conn);
                    cmd.Parameters.AddWithValue("@item_specialspriteid", posterId);
                    cmd.Parameters.AddWithValue("@name", posterDataName[posterId]);
                    cmd.Parameters.AddWithValue("@sale_code", "poster");
                    cmd.ExecuteNonQuery();
                }

                if (line.StartsWith("poster_") && line.Contains("_desc"))
                {
                    int posterId = int.Parse(line.Split('_')[1]);
                    posterDataDesc.Add(posterId, line.Trim().Replace("poster_" + posterId + "_desc=", ""));

                    MySqlCommand cmd = new MySqlCommand("UPDATE catalogue_items SET description = @description WHERE sale_code = @sale_code AND item_specialspriteid = @item_specialspriteid", conn);
                    cmd.Parameters.AddWithValue("@item_specialspriteid", posterId);
                    cmd.Parameters.AddWithValue("@description", posterDataDesc[posterId]);
                    cmd.Parameters.AddWithValue("@sale_code", "poster");
                    cmd.ExecuteNonQuery();
                }



                conn.Close();
                counter++;
            }

            file.Close();

            Console.WriteLine("Finished");
        }
    }
}
}
*/