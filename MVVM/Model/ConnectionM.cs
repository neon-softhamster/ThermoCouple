using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ThermoCouple.MVVM.ViewModel;

namespace ThermoCouple.MVVM.Model {
    public class ConnectionM {
        private bool IsArduino { get; set; }
        private bool IsConnected { get; set; }
        public IList<string> SerialPortsList { get; set; }
        public SerialPort serialPort { get; set; }
        public string ErrorMessage { get; set; }

        public ConnectionM() {
            SerialPortsList = new List<string>();
            serialPort = new SerialPort();
            InitializePorts();
            serialPort.PortName = SerialPortsList[0];
        }

        // method to fill box with ports
        public void InitializePorts() {
            string[] SerialPortsToCheck = SerialPort.GetPortNames();
            if (SerialPortsToCheck.Count() != 0) {
                foreach (string serial in SerialPortsToCheck) {
                    // if the serial is not yet inside the combobox -> add a serial port name to combo box
                    if (!SerialPortsList.Contains(serial)) {
                        SerialPortsList.Add(serial);
                    }
                }
            }
        }

        // Подключение к порту и проверка на ардуину
        public async void ConnectToArduinoAsync() {
            IsConnected = false;
            IsArduino = false;

            try {
                serialPort = new SerialPort(serialPort.PortName, 9600);

                await Task.Run(() => serialPort.Open());

                await CheckIfIsArduinoPortAsync();

                if (IsArduino) {
                    IsConnected = true;
                } else {
                    IsConnected = false;
                }
            } catch (Exception exc) {
                this.ErrorMessage = exc.Message;
            }
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
                        IsArduino = true;
                    } else {
                        IsArduino = false;
                    }
                } catch (Exception exc) {
                    this.ErrorMessage = exc.Message; /// Переслать во view
                }
            });
        }
    }
}
