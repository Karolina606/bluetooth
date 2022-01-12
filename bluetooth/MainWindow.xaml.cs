using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Linq;
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
using System.Windows.Threading;
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
            lblCorrect.Content = "Szukanie urządzeń rozpoczęte";
            lblWrong.Content = "";

            //localEndPoint = new BluetoothEndPoint(choosenRadio.LocalAddress, BluetoothService.SerialPort);
            client = new BluetoothClient();
            //_client = new BluetoothClient(localEndPoint);


            bluetoothDevice = (IReadOnlyCollection< BluetoothDeviceInfo>)client.DiscoverDevices();
           // _bluetoothDevice = (BluetoothDeviceInfo[])_client.DiscoverDevices();
            if (btAdapters.Length == 0) { 
                Console.WriteLine("Nie znaleziono urządzeń");
                lblCorrect.Content = "";
                lblWrong.Content = "Nie znaleziono urządzeń";
            }
            else
            {

                Console.WriteLine("Znalezionych urządzeń:" + bluetoothDevice.Count);
                foreach (var device in bluetoothDevice)
                {
                    Console.WriteLine(device.DeviceName.ToString());
                    cbDevices.Items.Add(device.DeviceName + ", MAC: " + device.DeviceAddress.ToString() + ", RSSI: ");
                }
                cbDevices.SelectedIndex = 0;
            }
            Console.WriteLine("Szukanie zakończone");
            lblCorrect.Content = "Szukanie zakończone";
            lblWrong.Content = "";
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
                lblCorrect.Content = "Sparowano z urządzeniem " + choosenDevice.DeviceName.ToString();
                lblWrong.Content = "";
            }
            catch
            {
                Console.WriteLine("Nie można sparować z urządzeniem " + choosenDevice.DeviceName.ToString());
                lblCorrect.Content = "";
                lblWrong.Content = "Nie można sparować z urządzeniem";
            }
        }

        public void chooseFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = @"PNG File (*.png)|*.png| JPG File (*.jpg)|*.jpg| JPEG File (*.jpeg)|*.jpeg;";
            fileDialog.ShowDialog();

            pathToFile = fileDialog.InitialDirectory + fileDialog.FileName;

            lblFile.Content = System.IO.Path.GetFileName(pathToFile);
            lbFilesNames.Items.Add(System.IO.Path.GetFileName(pathToFile));
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

                //progressBar.Value++;
                //Thread.Sleep(100);
            }
            catch
            {
                Console.WriteLine("Nie można wysłać");
            }
        }

        public void sendFiles()
        {
            Console.WriteLine("Rozpoczęto wysyłanie");
            lblCorrect.Content = "Rozpoczęto wysyłanie";
            lblWrong.Content = "";


            //progressBar.Minimum = 0;
            progressBar.Maximum = lbFilesNames.Items.Count;
            Console.WriteLine("Ilosc elementow do wyslania = " + lbFilesNames.Items.Count);
            progressBar.Value = 0;

            foreach (string fileName in lbFilesNames.Items)
            {
               
                bool isCorrect = true;
                try
                {
                    string filePath = (string)fileName;
                    var uri = new Uri("obex://" + choosenDevice.DeviceAddress + "/" + filePath);
                    ObexWebRequest request = new ObexWebRequest(uri);
                    request.ReadFile(filePath);
                    ObexWebResponse response = (ObexWebResponse)request.GetResponse();
                    // response.Close();

                }
                catch (Exception e)
                {
                    isCorrect = false;
                }

                if (isCorrect)
                {
                    Console.WriteLine("Zakończono wysyłanie");
                    lblCorrect.Content = "Poprawnie wysłano plik";
                    lblWrong.Content = "";
                    //progressBar.Value++;
                    progressBar.Dispatcher.Invoke(() => progressBar.Value += 1, DispatcherPriority.Background);
                    Thread.Sleep(100);
                }
                else
                {
                    Console.WriteLine("Nie można wysłać");
                    lblWrong.Content = "Nie wysłano pliku";
                    lblCorrect.Content = "";
                }

            }

            lbFilesNames.Items.Clear();
            //progressBar.Value = 0;
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
            lblCorrect.Content = "Próbuje połączyć się z urządzeniem";
            lblWrong.Content = "";
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