﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public MainWindow()
        {
            InitializeComponent();
            DataGrid gv1 = new DataGrid();
        }

        public class product
        {
            string name;
            int id;
            int kg;
            double price;

            public string Name { get => name; set => name = value; }
            public int Id { get => id; set => id = value; }
            public int Kg { get => kg; set => kg = value; }
            public double Price { get => price; set => price = value; }
        }



    