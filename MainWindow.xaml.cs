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
        bool isConnectedToArduino = false;
        public void ConnectToArduino () {
            try {
                string currentPortName = ComPortsBox.SelectedItem.ToString();
                serialPort = new SerialPort(currentPortName, 9600);
                serialPort.Open();
                serialPort.ReadTimeout = 1000;
                serialPort.WriteTimeout = 20;
                if (IsArduinoPort()) {
                    ConnectButton.Content = "Disconnect";
                    isConnectedToArduino = true;
                }
            } catch (Exception exc) { 
                MessageBox.Show(exc.Message); 
            }
        }

        private void DisconnectFromArduino() {
            ConnectButton.Content = "Connect";
            isConnectedToArduino = false;
            serialPort.Close();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e) {
            if (!isConnectedToArduino) {
                ConnectToArduino();
            } else {
                DisconnectFromArduino();
            }
        }

        // Проверка открытия порта и его верификация
        public bool IsArduinoPort() {
            serialPort.Write("r");
            string answer = serialPort.ReadLine();
            if (answer == "Salve\r") {
                return true;
            } else {
                return false;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            InitializePorts();
        }
        #endregion
    }
}
