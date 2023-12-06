using ScottPlot.Renderable;
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
        private bool isArduino;
        private bool isConnected;
        private string errorMessage;
        private List<string> serialPortsList;
        private SerialPort serialPort;

        public bool IsConnected { get => isConnected; }
        public List<string> SerialPortsList { get => serialPortsList; }
        public SerialPort SerialPort { 
            get => serialPort; 
            set {
                if (isConnected == true) {
                    serialPort.Close();
                    DisconnectFromArduino();
                } else {
                    DisconnectFromArduino();
                }
            }
        }
        public string ErrorMessage { get => errorMessage; }

        public ConnectionM() {
            isConnected = false;
            serialPortsList = new List<string>();
            serialPort = new SerialPort();
            InitializePorts();
        }

        // method to fill list with ports
        public void InitializePorts() {
            serialPortsList.Clear();
            serialPortsList.AddRange(SerialPort.GetPortNames());
            serialPortsList.Sort();
            serialPort.PortName = serialPortsList[0];
        }

        // Подключение к порту и проверка на ардуину
        public async void ConnectToArduinoAsync() {
            isConnected = false;
            isArduino = false;

            try {
                serialPort = new SerialPort(serialPort.PortName, 9600);

                await Task.Run(() => serialPort.Open());

                await CheckIfIsArduinoPortAsync();

                if (isArduino) {
                    isConnected = true;
                    Console.WriteLine("Arduino found");
                } else {
                    isConnected = false;
                    Console.WriteLine("Arduino not found");
                }
            } catch (Exception exc) {
                errorMessage = exc.Message;
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
                        isArduino = true;
                    } else {
                        isArduino = false;
                    }
                } catch (Exception exc) {
                    errorMessage = exc.Message; /// Переслать во view
                }
            });
        }

        public void DisconnectFromArduino() {
            isConnected = false;
            if (serialPort.IsOpen) {
                serialPort.Close();
            }
        }
    }
}
