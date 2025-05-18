using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace GaugeApp
{
    public partial class MainWindow : Window
    {
        // holds the path to the image the user picked
        private string _selectedImagePath;

        public MainWindow()
        {
            InitializeComponent();

            // wire up button clicks in code

        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // A simple POCO for serialization
        public class GaugeModel
        {

        }
    }
}
