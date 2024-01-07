using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ThermoCouple.MVVM.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;

namespace ThermoCouple.MVVM.Model {
    public class Driver : BaseM {
        // Поля
        private bool isArduino;
        private bool isConnected;
        private bool needToWrite;
        private string pathToWrite;
        private string errorMessage;
        private string portName;
        private string currentTemperature;
        private double timer;
        private double dt;
        private List<string> serialPortsList;
        private SerialPort serialPort;
        DataModel dataModel = new DataModel();

        // Свойства
        public bool IsConnected { get => isConnected; }
        public List<string> SerialPortsList { get => serialPortsList; }
        public string PortName { 
            get => portName; 
            set => portName = value;
        }
        public string ErrorMessage { get => errorMessage; }
        public string CurrentTemperature { 
            get => currentTemperature;
            set => currentTemperature = value;
        }
        public double[] DataX {
            get => dataModel.TimeAxis;
        }
        public double[] DataY {
            get => dataModel.TemperatureAxis;
        }

        public bool NeedToWrite {
            get => needToWrite;
            set {
                needToWrite = value;
                timer = 0;
            }
        }

        public string PathToWrite {
            set => pathToWrite = value;
        }

        // Конструктор
        public Driver() {
            isConnected = false;
            serialPortsList = new List<string>();
            serialPort = new SerialPort();
            InitializePorts();
            timer = 0;
            dt = 0.5;
        }

        // Инициализация портов
        public void InitializePorts() {
            serialPortsList.Clear();
            serialPortsList.AddRange(SerialPort.GetPortNames());
            serialPortsList.Sort();
        }

        // Подключение к порту
        public async void ConnectToArduinoAsync() {
            DisconnectFromArduino();
            isConnected = false;
            isArduino = false;

            try {
                serialPort = new SerialPort(portName, 9600);
                serialPort.Open();
                await CheckIfIsArduinoPortAsync();

                if (isArduino) {
                    isConnected = true;
                    DebugMessage("Arduino found and connected");
                    serialPort.Write("d");
                    serialPort.DataReceived += SerialPort_DataReceived;
                } else {
                    isConnected = false;
                    DebugMessage("Arduino not found");
                }
                OnPropertyChanged(nameof(isConnected));
            } catch (Exception exc) {
                errorMessage = exc.Message;
                OnPropertyChanged(nameof(errorMessage));
            }
        }

        // Проверка на ардуину
        public async Task CheckIfIsArduinoPortAsync() {
            serialPort.ReadTimeout = 1000;
            serialPort.WriteTimeout = 1000;

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
                    errorMessage = exc.Message;
                    OnPropertyChanged(nameof(errorMessage));
                }
            });
        }

        // Отключение от ардуины
        public void DisconnectFromArduino() {
            isConnected = false;
            if (serialPort.IsOpen && isArduino) {
                serialPort.Write("s");
                serialPort.Close();
                serialPort.DataReceived -= SerialPort_DataReceived;
                DebugMessage("Disconnection");
            }
        }

        // Сообщение об ошибке
        public void DebugMessage(string message) {
            Console.WriteLine(message);
            try {
                StreamWriter sw = new StreamWriter("driver_log.txt", append: true);
                sw.Write(message + "\n");
                sw.Close();
            } catch (Exception exc) {
                MessageBox.Show(exc.Message);
            }
        }

        // Поток ком порта
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) {
            string incomeMessage = serialPort.ReadLine();
            Dispatcher.CurrentDispatcher.Invoke(new LineReceivedEvent(LineReceived), incomeMessage);
        }

        private delegate void LineReceivedEvent(string incomeMessage);

        private void LineReceived(string incomeMessage) {
            currentTemperature = incomeMessage.ToString().Remove(5);
            dataModel.AddNewMeasurement(currentTemperature);
            OnPropertyChanged(nameof(currentTemperature));

            // запись данных
            if (needToWrite) {
                try {
                    StreamWriter sw = new StreamWriter(pathToWrite, append: true);
                    sw.Write(timer.ToString() + "\t" + currentTemperature + "\n");
                    timer += dt;
                    sw.Close();
                } catch (Exception exc) {
                    MessageBox.Show(exc.Message);
                }
            }
        }
    }
}
