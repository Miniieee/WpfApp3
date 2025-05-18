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
            AddImageButton.Click += AddImageButton_Click;
            SaveButton.Click += SaveButton_Click;
        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select Gauge Photo"
            };

            if (dlg.ShowDialog() == true)
            {
                _selectedImagePath = dlg.FileName;
                // TODO: if you have an <Image x:Name="PreviewImage"> in your XAML, you can:
                // PreviewImage.Source = new BitmapImage(new Uri(_selectedImagePath));
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 1) Build a strongly-typed model
            var gauge = new GaugeModel
            {
                Status = StatusTextBox.Text,
                LastCalibration = LastCalDatePicker.SelectedDate,
                NextCalibrationDue = NextCalDatePicker.SelectedDate,
                ControlNo = ControlNoTextBox.Text,
                // TODO: pull in other fields the same way…
                ReferenceStandard = RefYesToggle.IsChecked == true
            };

            // 2) Serialize to JSON
            string json = JsonConvert.SerializeObject(gauge, Formatting.Indented);

            // 3) Write out a timestamped .json file next to your EXE
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFile = Path.Combine(
                folder,
                $"gage_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            File.WriteAllText(jsonFile, json);

            // 4) Read image bytes (if any)
            byte[] imgBytes = null;
            if (!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
                imgBytes = File.ReadAllBytes(_selectedImagePath);

            // 5) Insert into SQL Server
            //    Make sure you have a table like:
            //      CREATE TABLE Gages (
            //        Id INT IDENTITY PRIMARY KEY,
            //        JsonData NVARCHAR(MAX),
            //        ImageData VARBINARY(MAX)
            //      );
            var connStr = ConfigurationManager
                            .ConnectionStrings["MyGaugeDb"]
                            .ConnectionString;

            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
                    INSERT INTO Gages (JsonData, ImageData)
                    VALUES (@json, @img)";
                cmd.Parameters.Add("@json", SqlDbType.NVarChar)
                              .Value = json;
                if (imgBytes != null)
                    cmd.Parameters.Add("@img", SqlDbType.VarBinary, imgBytes.Length)
                                  .Value = imgBytes;
                else
                    cmd.Parameters.Add("@img", SqlDbType.VarBinary)
                                  .Value = DBNull.Value;

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show(
                "Gauge data and image saved successfully!",
                "Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    // A simple POCO for serialization
    public class GaugeModel
    {
        public string Status { get; set; }
        public DateTime? LastCalibration { get; set; }
        public DateTime? NextCalibrationDue { get; set; }
        public string ControlNo { get; set; }
        // .. add other properties here to match your form fields ..
        public bool ReferenceStandard { get; set; }
    }
}
