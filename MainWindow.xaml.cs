using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

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
        private UserPreferenceChangedEventHandler UserPreferenceChanged;
        List<Style> gridStyles;
        //constructor
        public MainWindow()
        {
            MakeStyles();
            //windows accent colors event and brush
            UserPreferenceChanged = new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;

            Background = new SolidColorBrush(AccentColor.GetAccentColor());
            WindowStyle = WindowStyle.ToolWindow;
            ResizeMode = ResizeMode.NoResize;
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
            dg1 = new DataGrid() { ItemsSource = DBquery.WorkingList, Margin = new Thickness(10), GridLinesVisibility = DataGridGridLinesVisibility.None, RowHeaderWidth = 0};
            dg1.RowStyle = gridStyles[0];
            dg1.Style = gridStyles[2];
            dg1.ColumnHeaderStyle = gridStyles[3];
            dg1.CellStyle = gridStyles[1];
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

        //sales window method
        public void Sales()
        {
            //create client window
            w2 = new Window { Width = 500, Height = 300, Title = "Sales" };
            w2.Background = new SolidColorBrush(AccentColor.GetAccentColor());
            w2.SizeToContent = SizeToContent.WidthAndHeight;
            StackPanel wp = new StackPanel();
            DockPanel dp1 = new DockPanel() { FlowDirection = FlowDirection.RightToLeft};
            DockPanel dp2 = new DockPanel() { Background = new SolidColorBrush(Color.FromArgb(31,0,0,0)), Margin = new Thickness(0,0,11,10)};
            DockPanel dp3 = new DockPanel() { HorizontalAlignment = HorizontalAlignment.Center};
            dg2 = new DataGrid() { ItemsSource = DBquery.WorkingList, Margin = new Thickness(10), GridLinesVisibility = DataGridGridLinesVisibility.None, RowHeaderWidth = 0, HorizontalAlignment = HorizontalAlignment.Stretch, 
                VerticalAlignment = VerticalAlignment.Stretch, };
            Button sale = new Button() { Content = "Complete Transaction", Padding = new Thickness(10), Margin = new Thickness(11,0,10,10), HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromArgb(31, 255, 255, 255)), BorderThickness = new Thickness(0), Foreground = Brushes.White };
            sale.Click += Sale_Click;
            tot = new Label() { Content = "0", VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(2) };
            dp3.Children.Add(new Label() { Content = "$", VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(2) });
            dp3.Children.Add(tot);
            dp3.Children.Add(new Label() { Content = ":Total", VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(2) });
            dp2.Children.Add(dp3);
            dp1.Children.Add(sale);
            dp1.Children.Add(dp2);
            wp.Children.Add(dg2);
            wp.Children.Add(dp1);
            w2.Content = wp;
            w2.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w2.WindowStyle = WindowStyle.ToolWindow;
            w2.ResizeMode = ResizeMode.NoResize;
            w2.Show();
            dg2.RowStyle = gridStyles[0];
            dg2.Style = gridStyles[2];
            dg2.ColumnHeaderStyle = gridStyles[3];
            dg2.CellStyle = gridStyles[4];
            dg2.CanUserAddRows = false;
            dg2.Columns[4].CellStyle = gridStyles[1];
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

        //Event methods
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
        //accentColor event
        void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.VisualStyle)
            {

                Background = new SolidColorBrush(AccentColor.GetAccentColor());
                w2.Background = new SolidColorBrush(AccentColor.GetAccentColor());

            }
        }

        //accent color supporting class
        public class AccentColor
        {
            [DllImport("uxtheme.dll", EntryPoint = "#95")]
            public static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);
            [DllImport("uxtheme.dll", EntryPoint = "#96")]
            public static extern uint GetImmersiveColorTypeFromName(IntPtr pName);
            [DllImport("uxtheme.dll", EntryPoint = "#98")]
            public static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

            public static Color GetAccentColor()
            {
                var userColorSet = GetImmersiveUserColorSetPreference(false, false);
                var colorType = GetImmersiveColorTypeFromName(Marshal.StringToHGlobalUni("ImmersiveStartSelectionBackground"));
                var colorSetEx = GetImmersiveColorFromColorSetEx((uint)userColorSet, colorType, false, 0);
                return ConvertDWordColorToRGB(colorSetEx);
            }

            private static Color ConvertDWordColorToRGB(uint colorSetEx)
            {
                byte redColor = (byte)((0x000000FF & colorSetEx) >> 0);
                byte greenColor = (byte)((0x0000FF00 & colorSetEx) >> 8);
                byte blueColor = (byte)((0x00FF0000 & colorSetEx) >> 16);
                //byte alphaColor = (byte)((0xFF000000 & colorSetEx) >> 24);
                return Color.FromRgb(redColor, greenColor, blueColor);
            }

        }

        public void MakeStyles()
        {
            // Style list (dom added)
            gridStyles = new List<Style>();
            Style st1 = new Style() { TargetType = typeof(DataGridRow) };
            st1.Setters.Add(new Setter(DataGridRow.BackgroundProperty, Brushes.Transparent));
            st1.Setters.Add(new Setter(DataGridRow.BorderThicknessProperty, new Thickness(0)));
            Style st2 = new Style() { TargetType = typeof(DataGridCell) };
            st2.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Color.FromArgb(63, 255, 255, 255))));
            st2.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(5)));
            st2.Setters.Add(new Setter(DataGridCell.MarginProperty, new Thickness(1)));
            st2.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0)));
            st2.Setters.Add(new Setter(DataGridCell.FontWeightProperty, FontWeights.DemiBold));
            Style st3 = new Style() { TargetType = typeof(DataGrid) };
            st3.Setters.Add(new Setter(DataGrid.BackgroundProperty, Brushes.Transparent));
            st3.Setters.Add(new Setter(DataGrid.BorderThicknessProperty, new Thickness(0)));
            Style st4 = new Style() { TargetType = typeof(DataGridColumnHeader) };
            st4.Setters.Add(new Setter(DataGridColumnHeader.BackgroundProperty, new SolidColorBrush(Color.FromArgb(31, 0, 0, 0))));
            st4.Setters.Add(new Setter(DataGridColumnHeader.PaddingProperty, new Thickness(5)));
            st4.Setters.Add(new Setter(DataGridColumnHeader.MarginProperty, new Thickness(1)));
            st4.Setters.Add(new Setter(DataGridColumnHeader.ForegroundProperty, Brushes.White));
            Style st5 = new Style() { TargetType = typeof(DataGridCell) };
            st5.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Color.FromArgb(31, 255, 255, 255))));
            st5.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(5)));
            st5.Setters.Add(new Setter(DataGridCell.MarginProperty, new Thickness(1)));
            st5.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0)));
            gridStyles.Add(st1);
            gridStyles.Add(st2);
            gridStyles.Add(st3);
            gridStyles.Add(st4);
            gridStyles.Add(st5);
        }
    }
}
