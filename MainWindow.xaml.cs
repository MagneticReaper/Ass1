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
        // declare datagrids
        DataGrid dg1;
        DataGrid dg2;
        // delcare Sales window
        Window w2;
        // declare the total label for sale window
        Label tot;
        // declare the event handler for Windows changing accent color
        private UserPreferenceChangedEventHandler UserPreferenceChanged;
        // declare a list of styles
        List<Style> gridStyles;

        //admin window constructor
        public MainWindow()
        {
            // initialises and fills the style list
            MakeStyles();
            //windows accent colors new event handler
            UserPreferenceChanged = new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
            // subscribe sys events pref changed to the new event handler
            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
            //set window background to a new brush using the getAccent color method.
            Background = new SolidColorBrush(AccentColor.GetAccentColor());
            // change window type to tool window (removes icon, minimize and maximize buttons)
            WindowStyle = WindowStyle.ToolWindow;
            // disallow resize
            ResizeMode = ResizeMode.NoResize;
            // initialize Window components
            InitializeComponent();
            // db connection
            SQLiteConnection sqlite_conn = DBquery.CreateConnection();
            // create table if doesn't exist
            DBquery.CreateTable(sqlite_conn);
            // update working list from db
            DBquery.ReadData(sqlite_conn);
            // initialise dg1 and set styles
            dg1 = new DataGrid() { ItemsSource = DBquery.WorkingList, Margin = new Thickness(10), GridLinesVisibility = DataGridGridLinesVisibility.None, RowHeaderWidth = 0};
            dg1.RowStyle = gridStyles[0];
            dg1.Style = gridStyles[2];
            dg1.ColumnHeaderStyle = gridStyles[3];
            dg1.CellStyle = gridStyles[1];
            //set admin window content to be the datagrid
            Content = dg1;
            // set window title
            Title = "Admin";
            // window loaded event
            dg1.Loaded += Dg1_Loaded;
            //start sales window
            Sales();
            // window closing event
            Closed += MainWindow_Closing;
        }

        //sales window method
        public void Sales()
        {
            //create sales window
            w2 = new Window { Width = 500, Height = 300, Title = "Sales" };
            w2.Background = new SolidColorBrush(AccentColor.GetAccentColor());
            w2.SizeToContent = SizeToContent.WidthAndHeight;
            StackPanel wp = new StackPanel();
            DockPanel dp1 = new DockPanel() { FlowDirection = FlowDirection.RightToLeft};
            DockPanel dp2 = new DockPanel() { Background = new SolidColorBrush(Color.FromArgb(31,0,0,0)), Margin = new Thickness(0,0,11,10)};
            DockPanel dp3 = new DockPanel() { HorizontalAlignment = HorizontalAlignment.Center};
            // create datagrid dg2
            dg2 = new DataGrid() { ItemsSource = DBquery.WorkingList, Margin = new Thickness(10), GridLinesVisibility = DataGridGridLinesVisibility.None, RowHeaderWidth = 0, HorizontalAlignment = HorizontalAlignment.Stretch, 
                VerticalAlignment = VerticalAlignment.Stretch, };
            // create sale button
            Button sale = new Button() { Content = "Complete Transaction", Padding = new Thickness(10), Margin = new Thickness(11,0,10,10), HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromArgb(31, 255, 255, 255)), BorderThickness = new Thickness(0), Foreground = Brushes.White };
            //button click event
            sale.Click += Sale_Click;
            // create label for total amount of sale
            tot = new Label() { Content = "0", VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(2) };
            // add every child to it's parent
            dp3.Children.Add(new Label() { Content = "$", VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(2) });
            dp3.Children.Add(tot);
            dp3.Children.Add(new Label() { Content = ":Total", VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(2) });
            dp2.Children.Add(dp3);
            dp1.Children.Add(sale);
            dp1.Children.Add(dp2);
            wp.Children.Add(dg2);
            wp.Children.Add(dp1);
            //set the content of sales window to be wp
            w2.Content = wp;
            
            w2.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w2.WindowStyle = WindowStyle.ToolWindow;
            w2.ResizeMode = ResizeMode.NoResize;

            // window is now visible
            w2.Show();

            //set datagrid styles and parameters
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
            // event for calculating total
            dg2.SelectionChanged += Dg2_SelectedCellsChanged;
            // close admin if this window is closed event
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
            SQLiteConnection sqlite_conn = DBquery.CreateConnection();
            //delete removed lines
            try
            {
                SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = "delete from product where ";
                foreach (Product p in DBquery.WorkingList)
                {
                    sqlite_cmd.CommandText += "ID != " + p.Id + " AND ";
                }
                //remove the last AND . it is unwanted
                sqlite_cmd.CommandText = sqlite_cmd.CommandText.Remove(sqlite_cmd.CommandText.Length - 5, 5);
                sqlite_cmd.CommandText += ";";
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }
            //try to add new products
            foreach (Product p in temp)
            {
                try { DBquery.InsertData(sqlite_conn, p); }
                //catch the ones that can't be inserted; they already exist. save them in temp2 list to update instead
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
            // change column header names
            dg1.Columns[2].Header = "KG available";
            dg1.Columns[3].Header = "Price per KG";
            //hide sales columns from admin window
            dg1.Columns[4].Visibility = Visibility.Hidden;
            dg1.Columns[5].Visibility = Visibility.Hidden;
            //auto size the window
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void W2_Closing(object sender, EventArgs e)
        {
            Close();
        }

        private void Sale_Click(object sender, RoutedEventArgs e)
        {
            foreach (Product p in DBquery.WorkingList)
            {
                if (p.Added > 0)
                {
                    //substract from inventory for each added product
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
                //calculate total
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

        //accent color supporting class. code found on the internet. it's magic. (it gets colors from windows theme dll)
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
            // initialise style list
            gridStyles = new List<Style>();
            //create a new stylem target datagrid rows
            Style st1 = new Style() { TargetType = typeof(DataGridRow) };
            //add a setter for background property to set it to transparent
            st1.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
            //add a setter for border to set it to 0px
            st1.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));

            Style st2 = new Style() { TargetType = typeof(DataGridCell) };
            st2.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromArgb(63, 255, 255, 255))));
            st2.Setters.Add(new Setter(PaddingProperty, new Thickness(5)));
            st2.Setters.Add(new Setter(MarginProperty, new Thickness(1)));
            st2.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));
            st2.Setters.Add(new Setter(FontWeightProperty, FontWeights.DemiBold));

            Style st3 = new Style() { TargetType = typeof(DataGrid) };
            st3.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
            st3.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));

            Style st4 = new Style() { TargetType = typeof(DataGridColumnHeader) };
            st4.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromArgb(31, 0, 0, 0))));
            st4.Setters.Add(new Setter(PaddingProperty, new Thickness(5)));
            st4.Setters.Add(new Setter(MarginProperty, new Thickness(1)));
            st4.Setters.Add(new Setter(ForegroundProperty, Brushes.White));

            Style st5 = new Style() { TargetType = typeof(DataGridCell) };
            st5.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromArgb(31, 255, 255, 255))));
            st5.Setters.Add(new Setter(PaddingProperty, new Thickness(5)));
            st5.Setters.Add(new Setter(MarginProperty, new Thickness(1)));
            st5.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));

            // add each style to the style list
            gridStyles.Add(st1);
            gridStyles.Add(st2);
            gridStyles.Add(st3);
            gridStyles.Add(st4);
            gridStyles.Add(st5);
        }
    }
}
