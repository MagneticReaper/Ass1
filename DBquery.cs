using System;
using System.ComponentModel;
using System.Data.SQLite;

namespace Ass1
{

    public static class DBquery
    {
        static BindingList<Product> workingList;

        internal static BindingList<Product> WorkingList { get => workingList; set => workingList = value; }

        public static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try { sqlite_conn.Open(); }
            catch (Exception) { }
            return sqlite_conn;
        }

        public static void CreateTable(SQLiteConnection conn)
        {
            try
            {
                // Create table named "product" and setting up the variables (Dom added)
                SQLiteCommand sqlite_cmd;
                string Createsql = //"DROP TABLE product; " + //for resetting db
                    "CREATE TABLE product (name VARCHAR(20), id INT UNIQUE, kg INT, price REAL)";
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = Createsql;
                sqlite_cmd.ExecuteNonQuery();
                InsertData(conn, new Product("Apple", 124567, 23, 2.10));
                InsertData(conn, new Product("Orange", 345678, 20, 2.49));
                InsertData(conn, new Product("Raspberry", 125678, 25, 2.35));
                InsertData(conn, new Product("Blueberry", 456732, 29, 1.45));
                InsertData(conn, new Product("Cauliflower", 238901, 24, 2.22));
            }
            catch (Exception) { }
        }

        internal static void InsertData(SQLiteConnection conn, Product newProduct)
        {
            try
            {
                // Commands established to insert data into the table (Dom added)
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "INSERT INTO product (name, id, kg, price) VALUES('" + newProduct.Name + "', " + newProduct.Id + ", " + newProduct.Kg + "," + newProduct.Price + "); ";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }

        }
        internal static void UpdateData(SQLiteConnection conn, Product oldProduct)
        {
            try
            {
                // Parameters to update the data within the table (Dom added)
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "UPDATE product SET name = '" + oldProduct.Name + "', kg = " + oldProduct.Kg + ", price = " + oldProduct.Price + " WHERE id = " + oldProduct.Id + ";";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }

        }

        public static void ReadData(SQLiteConnection conn)
        {
            // Commands to allow data to be read and scripted onto the console screen (Dom added)
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM product";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            WorkingList = new BindingList<Product>();

            while (sqlite_datareader.Read())
            {
                string name = sqlite_datareader.GetString(0);
                int id = sqlite_datareader.GetInt32(1);
                int kg = sqlite_datareader.GetInt32(2);
                double price = sqlite_datareader.GetDouble(3);
                WorkingList.Add(new Product(name, id, kg, price));
            }

            WorkingList.ListChanged += MainWindow.List_ListChanged;
            conn.Close();
            // Closing the connection with the server (Dom added)
        }
    }
}
