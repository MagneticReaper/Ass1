using System;
using System.ComponentModel;
using System.Data.SQLite;

namespace Ass1
{

    public static class DBquery
    {
        // new product list that later gets binded to the datagrid
        static BindingList<Product> workingList;
        // getters and setters
        internal static BindingList<Product> WorkingList { get => workingList; set => workingList = value; }

        public static SQLiteConnection CreateConnection()
        {
            // Create a new database connection:
            SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try { sqlite_conn.Open(); }
            catch (Exception) { }
            
            return sqlite_conn;
        }

        public static void CreateTable(SQLiteConnection conn)
        { // Create Table
            try
            {
                // get connection
                SQLiteCommand sqlite_cmd;
                // create command
                sqlite_cmd = conn.CreateCommand();
                // set the command
                sqlite_cmd.CommandText = "CREATE TABLE product (name VARCHAR(20), id INT UNIQUE, kg INT, price REAL)";
                // execute
                sqlite_cmd.ExecuteNonQuery();

                // insert rows into table using conn
                InsertData(conn, new Product("Apple", 124567, 23, 2.10));
                InsertData(conn, new Product("Orange", 345678, 20, 2.49));
                InsertData(conn, new Product("Raspberry", 125678, 25, 2.35));
                InsertData(conn, new Product("Blueberry", 456732, 29, 1.45));
                InsertData(conn, new Product("Cauliflower", 238901, 24, 2.22));
            }
            // fail silently when table already exists
            catch (Exception) { }
        }


        internal static void InsertData(SQLiteConnection conn, Product newProduct)
        {
            try
            {
                //create connection
                SQLiteCommand sqlite_cmd = conn.CreateCommand();
                // set sql command
                sqlite_cmd.CommandText = "INSERT INTO product (name, id, kg, price) VALUES('" + newProduct.Name + "', " + newProduct.Id + ", " + newProduct.Kg + "," + newProduct.Price + "); ";
                //execute command
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }

        }
        internal static void UpdateData(SQLiteConnection conn, Product oldProduct)
        {
            try
            {
                //create connection
                SQLiteCommand sqlite_cmd = conn.CreateCommand();
                // set sql command
                sqlite_cmd.CommandText = "UPDATE product SET name = '" + oldProduct.Name + "', kg = " + oldProduct.Kg + ", price = " + oldProduct.Price + " WHERE id = " + oldProduct.Id + ";";
                //execute command
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }
        }

        public static void ReadData(SQLiteConnection conn)
        {
            // create connection
            SQLiteCommand sqlite_cmd = conn.CreateCommand();
            // set sql command
            sqlite_cmd.CommandText = "SELECT * FROM product";
            // create reader / execute
            SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();

            // set/resets WorkingList
            workingList = new BindingList<Product>();

            while (sqlite_datareader.Read())
            {
                // for every row, get column 0,1,2,3 and place in a new Product that gets added to the workingList
                string name = sqlite_datareader.GetString(0);
                int id = sqlite_datareader.GetInt32(1);
                int kg = sqlite_datareader.GetInt32(2);
                double price = sqlite_datareader.GetDouble(3);
                WorkingList.Add(new Product(name, id, kg, price));
            }

            // subscribe event
            WorkingList.ListChanged += MainWindow.List_ListChanged;
            //close connection
            conn.Close();
        }
    }
}
