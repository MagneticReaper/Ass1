using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ass1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        List<Product> list;

        public MainWindow()
        {
            //db connection
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            CreateTable(sqlite_conn);
            InsertData(sqlite_conn, new Product("Apple", 124567, 23, 2.10));
            InsertData(sqlite_conn, new Product("Orange", 345678, 20, 2.49));
            InsertData(sqlite_conn, new Product("Raspberry", 125678, 25, 2.35));
            InsertData(sqlite_conn, new Product("Blueberry", 456732, 29, 1.45));
            InsertData(sqlite_conn, new Product("Cauliflower", 238901, 24, 2.22));
            //create list and start auto updater

            Thread update = new Thread(UpdateData);
            update.Start();

            //initialise window and components
            InitializeComponent();
            DataGrid gv1 = new DataGrid();
            gv1.ItemsSource = list;
            Content = gv1;
        }

        void UpdateData()
        {
            SQLiteConnection sqlite_conn2;
            sqlite_conn2 = CreateConnection();
            ReadData(sqlite_conn2);
            Thread.Sleep(1000);
            UpdateData();
        }

        class Product
        {
            string name;
            int id;
            int kg;
            double price;

            public Product(string name, int id, int kg, double price)
            {
                this.name = name;
                this.id = id;
                this.kg = kg;
                this.price = price;
            }

            public string Name { get => name; set => name = value; }
            public int Id { get => id; set => id = value; }
            public int Kg { get => kg; set => kg = value; }
            public double Price { get => price; set => price = value; }
        }


        SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
             // Open the connection:
         try
            {
                sqlite_conn.Open();
            }
            catch (Exception)
            {
                
            }
            return sqlite_conn;
        }

        void CreateTable(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            string Createsql = "DROP TABLE product; CREATE TABLE product (name VARCHAR(20), id INT, kg INT, price REAL)";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();

        }

        void InsertData(SQLiteConnection conn, Product newProduct)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO product (name, id, kg, price) VALUES('"+ newProduct.Name+"', "+newProduct.Id+", "+ newProduct.Kg+","+newProduct.Price+"); ";
            sqlite_cmd.ExecuteNonQuery();
        }

        void ReadData(SQLiteConnection conn)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM product";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            list = new List<Product>();
            while (sqlite_datareader.Read())
            {
                string name = sqlite_datareader.GetString(0);
                int id = sqlite_datareader.GetInt32(1);
                int kg = sqlite_datareader.GetInt32(2);
                double price = sqlite_datareader.GetDouble(3);
                list.Add(new Product(name,id,kg,price));
            }
            conn.Close();
        }
    }
}

