using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace Ass1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        DataGrid dg1;
        DataGrid dg2;
        Window w2;
        Label tot;
        public MainWindow()
        {
            InitializeComponent();
            //db connection
            SQLiteConnection sqlite_conn;
            sqlite_conn = DBquery.CreateConnection();
            // create table if doesn't exist
            DBquery.CreateTable(sqlite_conn);
            //update working list from db
            DBquery.ReadData(sqlite_conn);
            //create view
            WrapPanel wp = new WrapPanel();
            dg1 = new DataGrid() { ItemsSource = DBquery.WorkingList, Margin = new Thickness(10) };
            wp.Children.Add(dg1);
            Content = wp;
            Width = 500;
            Height = 300;
            Title = "Admin";
            dg1.ItemsSource = DBquery.WorkingList;
            dg1.Loaded += Dg1_Loaded;
            //start sales window
            Sales();
            Closed += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, EventArgs e)
        {
            w2.Close();
        }

        public static void List_ListChanged(object sender, ListChangedEventArgs e)
        {
            List<Product> temp = DBquery.WorkingList.ToList();
            List<Product> temp2 = new List<Product>();
            //create db connection
            SQLiteConnection sqlite_conn;
            sqlite_conn = DBquery.CreateConnection();
            //delete removed lines
            try
            {
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = "delete from product where ";
                foreach (Product p in DBquery.WorkingList)
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
                try { DBquery.InsertData(sqlite_conn, p); }
                catch (Exception) { temp2.Add(p); }
            }
            //modify existing products
            foreach (Product p in temp2)
            {
                DBquery.UpdateData(sqlite_conn, p);
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
            dg2 = new DataGrid() { ItemsSource = DBquery.WorkingList, Margin = new Thickness(10) };
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
            foreach (Product p in DBquery.WorkingList)
            {
                if (p.Added > 0)
                {
                    p.Kg -= p.Added;
                    p.Added = 0;
                }
            }
            tot.Content = "0";
        }

        private void Dg2_SelectedCellsChanged(object sender, EventArgs e)
        {
            double total = 0;
            foreach (var item in DBquery.WorkingList)
            {
                total += item.Subtotal;
            }
            tot.Content = total.ToString();
        }
    }
}
