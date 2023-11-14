using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ThermoCouple.MVVM.Model {
    public class ConnectionModel {
        public bool IsArduino { get; set; }
        public bool IsConnected { get; set; }
        public List<string> SerialPortsList { get; set; }

        public SerialPort serialPort;

        // method to fill box with ports
        public void InitializePorts() {
            string[] SerialPortsToCheck;
            SerialPortsToCheck = SerialPort.GetPortNames();
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
                string currentPortName = "COM5"; /////// Поменять на значение из view
                serialPort = new SerialPort(currentPortName, 9600);

                await Task.Run(() => serialPort.Open());

                await CheckIfIsArduinoPortAsync();

                if (IsArduino) {
                    IsConnected = true;
                } else {
                    IsConnected = false;
                }
            } catch (Exception exc) {
                MessageBox.Show(exc.Message); /// Переслать во view
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
                    MessageBox.Show(exc.Message); /// Переслать во view
                }
            });
        }
    }
}
