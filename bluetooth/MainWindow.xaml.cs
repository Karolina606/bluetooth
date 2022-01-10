using System;
using System.Collections.Generic;
//using System.Linq;
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

using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.Win32;

namespace bluetooth
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            updateAdapters();
        }

        public BluetoothRadio[] btAdapters; //BT Devices (hardware in pc)
        public BluetoothClient client = new BluetoothClient();
        public IReadOnlyCollection<BluetoothDeviceInfo> bluetoothDevice;
        public BluetoothDeviceInfo choosenDevice;
        private BluetoothEndPoint localEndPoint;
        public BluetoothRadio choosenRadio;
        string pathToFile = @"C:\Users\nogac\source\repos\bluetooth";

        public void updateAdapters()
        {
            Console.WriteLine("Szukanie adaptera BT:");
            btAdapters = new[] { BluetoothRadio.Default};

            if (btAdapters.Length > 0)
                foreach (var adapter in btAdapters)
                {
                    Console.WriteLine(adapter.Name + " " + adapter.Mode + " " + adapter.LocalAddress.ToString());
                    cbAdapters.Items.Add(adapter.Name + ", MAC: " + adapter.LocalAddress.ToString());
                }
            cbAdapters.SelectedIndex = 0;
        }

        public void findDevices()
        {
            Console.WriteLine("Szukanie rozpoczęte");
            localEndPoint = new BluetoothEndPoint(choosenRadio.LocalAddress, BluetoothService.SerialPort);
            client = new BluetoothClient();
            //_client = new BluetoothClient(localEndPoint);


            bluetoothDevice = bluetoothDevice = (IReadOnlyCollection< BluetoothDeviceInfo>)client.DiscoverDevices();
           // _bluetoothDevice = (BluetoothDeviceInfo[])_client.DiscoverDevices();
            if (btAdapters.Length == 0) { Console.WriteLine("Nie znaleziono urządzeń"); }
            {

                Console.WriteLine("Znalezionych urządzeń:" + bluetoothDevice.Count);
                foreach (var device in bluetoothDevice)
                {
                    Console.WriteLine(device.DeviceName.ToString());
                    cbDevices.Items.Add(device.DeviceName + ", MAC: " + device.DeviceAddress.ToString());
                }
                cbDevices.SelectedIndex = 0;
            }
            Console.WriteLine("Szukanie zakończone");
        }

        public void connectToDevice()
        {
            try
            {
                choosenDevice.Refresh();
                choosenDevice.SetServiceState(BluetoothService.ObexObjectPush, true);
                BluetoothSecurity.PairRequest(
                    choosenDevice.DeviceAddress, "000000");
                Console.WriteLine("Sparowano z urządzeniem " + choosenDevice.DeviceName.ToString());
            }
            catch
            {
                Console.WriteLine("Nie można sparować z urządzeniem " + choosenDevice.DeviceName.ToString());
            }
        }

        public void chooseFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = @"PNG File (*.png)|*.png| JPG File (*.jpg)|*.jpg| JPEG File (*.jpeg)|*.jpeg;";
            fileDialog.ShowDialog();

            pathToFile = fileDialog.InitialDirectory + fileDialog.FileName;

            lbFilesNames.Items.Add(System.IO.Path.GetFileName(pathToFile));
            //if ((bool)chBManyFiles.IsChecked)
            //{
                
            //}
            //else
            //{
            //    lblFile.Content = System.IO.Path.GetFileName(pathToFile);
            //}
        }

        public void sendFile()
        {
            Console.WriteLine("Rozpoczęto wysyłanie");

            try
            {
                string filePath = (string)lblFile.Content;
                var uri = new Uri("obex://" + choosenDevice.DeviceAddress + "/" + filePath);
                ObexWebRequest request = new ObexWebRequest(uri);
                request.ReadFile(filePath);
                ObexWebResponse response = (ObexWebResponse)request.GetResponse();
                //response.Close();
                Console.WriteLine("Zakończono wysyłanie");
            }
            catch
            {
                Console.WriteLine("Nie można wysłać");
            }
        }

        public void sendFiles()
        {
            Console.WriteLine("Rozpoczęto wysyłanie");

            progressBar.Minimum = 0;
            progressBar.Maximum = lbFilesNames.Items.Count;
            progressBar.Value = 0;

            foreach (var fileName in lbFilesNames.Items)
            {
                try
                {
                    string filePath = (string)fileName;
                    var uri = new Uri("obex://" + choosenDevice.DeviceAddress + "/" + filePath);
                    ObexWebRequest request = new ObexWebRequest(uri);
                    request.ReadFile(filePath);
                    ObexWebResponse response = (ObexWebResponse)request.GetResponse();
                    //response.Close();
                    Console.WriteLine("Zakończono wysyłanie");
                }
                catch
                {
                    Console.WriteLine("Nie można wysłać");
                }

                progressBar.Value++;
            }

            progressBar.Value = 0;
        }

        private void btnChooseAdapterClick(object sender, RoutedEventArgs e)
        {
            choosenRadio = btAdapters[cbAdapters.SelectedIndex];
            findDevices();
        }

        private void btnChooseDeviceClick(object sender, RoutedEventArgs e)
        {
            int iterator = 0;
            foreach (var device in bluetoothDevice)
            {
                if (iterator == cbDevices.SelectedIndex)
                {
                    choosenDevice = device;
                }
                iterator++;
            }
            Console.WriteLine("Próbuje połączyć się z urządzeniem");
            connectToDevice();
        }

        private void btnSendImageClick(object sender, RoutedEventArgs e)
        {
            sendFiles();
        }

        private void btnChooseFileClick(object sender, RoutedEventArgs e)
        {
            chooseFile();
        }
    }
}