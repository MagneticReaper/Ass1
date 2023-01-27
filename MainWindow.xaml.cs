
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;


namespace Ass1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        BindingList<Product> list;
        DataGrid dg1;
        DataGrid dg2;
        Window w2;
        Label tot;
        public MainWindow()
        {
            InitializeComponent();
            //db connection
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            // create table if doesn't exist
            CreateTable(sqlite_conn);
            //update working list from db
            ReadData(sqlite_conn);
            //create view
            WrapPanel wp = new WrapPanel();
            dg1 = new DataGrid() { ItemsSource = list, Margin = new Thickness(10) };
            wp.Children.Add(dg1);
            Content = wp;
            Width = 500;
            Height = 300;
            Title = "Admin";
            dg1.ItemsSource = list;
            dg1.Loaded += Dg1_Loaded;
            //start sales window
            Sales();
            Closed += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, EventArgs e)
        {
            w2.Close();
        }

        private void List_ListChanged(object sender, ListChangedEventArgs e)
        {
            List<Product> temp = list.ToList();
            List<Product> temp2 = new List<Product>();
            //create db connection
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            //delete removed lines
            try
            {
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = "delete from product where ";
                foreach (Product p in list)
                {
                    sqlite_cmd.CommandText += "ID != " + p.Id + " AND ";
                }
                sqlite_cmd.CommandText = sqlite_cmd.CommandText.Remove(sqlite_cmd.CommandText.Length - 5, 5);
                sqlite_cmd.CommandText += ";";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }
            //try to add new products
            foreach (Product p in temp)
            {
                try { InsertData(sqlite_conn, p); }
                catch (Exception) { temp2.Add(p); }
            }
            //modify existing products
            foreach (Product p in temp2)
            {
                UpdateData(sqlite_conn, p);
            }
        }

        private void Dg1_Loaded(object sender, RoutedEventArgs e)
        {
            dg1.Columns[2].Header = "KG available";
            dg1.Columns[3].Header = "Price per KG";
            dg1.Columns[4].Visibility = Visibility.Hidden;
            dg1.Columns[5].Visibility = Visibility.Hidden;
            SizeToContent = SizeToContent.WidthAndHeight;
        }


        public void Sales()
        {
            //create client window
            w2 = new Window { Width = 500, Height = 300, Title = "Sales" };
            w2.SizeToContent = SizeToContent.WidthAndHeight;
            StackPanel wp = new StackPanel();
            WrapPanel sp = new WrapPanel() { Orientation = Orientation.Horizontal };
            dg2 = new DataGrid() { ItemsSource = list, Margin = new Thickness(10) };
            Button sale = new Button() { Content = "Complete Transaction", Padding = new Thickness(10), Margin = new Thickness(10) };
            sale.Click += Sale_Click;
            tot = new Label() { Content = "0", VerticalAlignment = VerticalAlignment.Center };
            sp.Children.Add(new Label() { Content = "Total: ", VerticalAlignment = VerticalAlignment.Center });
            sp.Children.Add(tot);
            sp.Children.Add(new Label() { Content = "$", VerticalAlignment = VerticalAlignment.Center });
            sp.Children.Add(sale);
            wp.Children.Add(dg2);
            wp.Children.Add(sp);
            w2.Content = wp;
            w2.Show();
            dg2.CanUserAddRows = false;
            dg2.Columns[1].Visibility = Visibility.Hidden;
            dg2.Columns[2].Header = "KG available";
            dg2.Columns[3].Header = "Price per KG";
            dg2.Columns[4].Header = "KG being sold";
            dg2.Columns[0].IsReadOnly = true;
            dg2.Columns[2].IsReadOnly = true;
            dg2.Columns[3].IsReadOnly = true;
            dg2.Columns[5].IsReadOnly = true;
            dg2.SelectionChanged += Dg2_SelectedCellsChanged;
            w2.Loaded += W2_Loaded;
            w2.Closed += W2_Closing;
        }

        private void W2_Closing(object sender, EventArgs e)
        {
            Close();
        }

        private void W2_Loaded(object sender, RoutedEventArgs e)
        {
            w2.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void Sale_Click(object sender, RoutedEventArgs e)
        {
            foreach (Product p in list)
            {
                if (p.Added > 0)
                {
                    p.Kg = p.Kg - p.Added;
                    p.Added = 0;
                }
            }
            tot.Content = "0";
        }

        private void Dg2_SelectedCellsChanged(object sender, EventArgs e)
        {
            double total = 0;
            foreach (var item in list)
            {
                total += item.Subtotal;
            }
            tot.Content = total.ToString();
        }

        class Product : INotifyPropertyChanged
        {
            string name;
            int id;
            int kg;
            double price;
            double subtotal;
            int added;
            public event PropertyChangedEventHandler PropertyChanged;
            public Product(string name, int id, int kg, double price)
            {
                this.name = name;
                this.id = id;
                this.kg = kg;
                this.price = price;
            }
            public Product() { }
            public string Name
            {
                get { return name; }
                set
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
            public int Id
            {
                get { return id; }
                set
                {
                    id = value;
                    OnPropertyChanged();
                }
            }
            public int Kg
            {
                get { return kg; }
                set
                {
                    kg = value;
                    OnPropertyChanged();
                }
            }
            public double Price
            {
                get { return price; }
                set
                {
                    price = value;
                    OnPropertyChanged();
                }
            }
            public int Added
            {
                get { return added; }
                set
                {
                    added = value;
                    OnPropertyChanged();
                    Subtotal = added * price;
                }
            }
            public double Subtotal
            {
                get { return subtotal; }
                set
                {
                    subtotal = value;
                    OnPropertyChanged();
                }
            }
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

        }


        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try { sqlite_conn.Open(); }
            catch (Exception) { }
            return sqlite_conn;
        }

        void CreateTable(SQLiteConnection conn)
        {
            try
            {
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

        void InsertData(SQLiteConnection conn, Product newProduct)
        {
            try
            {
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "INSERT INTO product (name, id, kg, price) VALUES('" + newProduct.Name + "', " + newProduct.Id + ", " + newProduct.Kg + "," + newProduct.Price + "); ";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }

        }
        void UpdateData(SQLiteConnection conn, Product oldProduct)
        {
            try
            {
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "UPDATE product SET name = '" + oldProduct.Name + "', kg = " + oldProduct.Kg + ", price = " + oldProduct.Price + " WHERE id = " + oldProduct.Id + ";";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }

        }

        void ReadData(SQLiteConnection conn)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM product";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            list = new BindingList<Product>();

            while (sqlite_datareader.Read())
            {
                string name = sqlite_datareader.GetString(0);
                int id = sqlite_datareader.GetInt32(1);
                int kg = sqlite_datareader.GetInt32(2);
                double price = sqlite_datareader.GetDouble(3);
                list.Add(new Product(name, id, kg, price));
            }
            list.ListChanged += List_ListChanged;
            conn.Close();
        }

    }
}

