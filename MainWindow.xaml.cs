using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace ThermoCouple {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SerialPort serialPort;
        string[] serialPorts;

        public MainWindow() {
            InitializeComponent();

            // scan ports and add to list
            InitializePorts();
        }

        // method to fill box with ports
        public void InitializePorts() {
            serialPorts = SerialPort.GetPortNames();
            if (serialPorts.Count() != 0) {
                foreach (string serial in serialPorts) {
                    var serialItems = ComPortsBox.Items;
                    // if the serial is not yet inside the combobox -> add a serial port name to combo box
                    if (!serialItems.Contains(serial)) {
                        ComPortsBox.Items.Add(serial);
                    }
                }
                // combobox as default selected item 
                ComPortsBox.SelectedItem = serialPorts[0];
            }
        }

        #region WPF to Arduino connection
        private void ConnectButton_Click(object sender, RoutedEventArgs e) {
            if (!isConnectedToArduino) {
                ConnectButton.IsEnabled = false;
                ConnectButton.Content = "Connection...";
                ConnectToArduino();
            } else {
                DisconnectFromArduino();
            }
        }

        bool isConnectedToArduino = false;
        bool isArduino = false;
        public async void ConnectToArduino () {
            isConnectedToArduino = false;
            isArduino = false;

            try {
                string currentPortName = ComPortsBox.SelectedItem.ToString();
                serialPort = new SerialPort(currentPortName, 9600);
                await Task.Run(() => serialPort.Open());
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;

                await CheckIfIsArduinoPortAsync();
                if (isArduino) {
                    ConnectButton.Content = "Disconnect";
                    isConnectedToArduino = true;
                } else {
                    ConnectButton.Content = "Connect";
                    isConnectedToArduino = false;
                }
            } catch (Exception exc) { 
                MessageBox.Show(exc.Message);
            }

            ConnectButton.IsEnabled = true;
        }

        private void DisconnectFromArduino() {
            ConnectButton.Content = "Connect";
            isConnectedToArduino = false;
            serialPort.Close();
        }

        // Проверка открытия порта и его верификация
        public async Task CheckIfIsArduinoPortAsync() {
            serialPort.ReadTimeout = 2000;
            serialPort.WriteTimeout = 2000;

            await Task.Run(() => {
                try {
                    serialPort.Write("r");
                    string answer = serialPort.ReadLine();
                    if (answer == "Salve\r") {
                        isArduino = true;
                    } else {
                        isArduino = false;
                    }
                } catch (Exception exc) {
                    MessageBox.Show(exc.Message);
                }
            });
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            InitializePorts();
        }
        #endregion

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DragMove();
            }
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e) {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized) {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            } else {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void CloseAppButton_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
    }
}
