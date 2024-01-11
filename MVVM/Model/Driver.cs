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
using System.Diagnostics;
using System.Globalization;

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
        private double dt;
        private double timePoint;
        private TimeSpan upTime;
        private DateTime stopWatchUpTime;
        private DateTime stopWatchOnLogStart;
        private DateTime stopWatchDt;
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
        public double Time {
            get => timePoint;
            set => timePoint = value;
        }

        public bool NeedToWrite {
            get => needToWrite;
            set {
                if (value) {
                    StreamWriter sw = new StreamWriter(pathToWrite, append: true);
                    sw.Write("Log started at " + DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + "\n");
                    sw.Write("UTC time\t" + "Uptime\t\t" + "Δt, s\t" + "t, min\t" + "T, °C\n");
                    sw.Close();
                }
                stopWatchOnLogStart = DateTime.Now;
                stopWatchDt = DateTime.Now;
                needToWrite = value;
            }
        }

        public string PathToWrite {
            set {
                if (File.Exists(value + ".txt")) {
                    int file_number = 1;
                    pathToWrite = value;
                    while (File.Exists(pathToWrite + "_" + file_number.ToString() + ".txt") != false) {
                        file_number++;
                    }
                    pathToWrite = pathToWrite + "_" + file_number.ToString() + ".txt";
                } else {
                    pathToWrite = value + ".txt";
                }
            }
        }

        public double Dt {
            get => dt;
            set => dt = value;
        }

        // Конструктор
        public Driver() {
            isConnected = false;
            serialPortsList = new List<string>();
            serialPort = new SerialPort();
            InitializePorts();
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
                    stopWatchUpTime = DateTime.Now;
                    isConnected = true;
                    serialPort.DataReceived += SerialPort_DataReceived;
                    serialPort.Write("d");
                    DebugMessage("Arduino found and connected");
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
                Time = 0;
                serialPort.DataReceived -= SerialPort_DataReceived;
                DebugMessage("Disconnection");
            }
        }

        // Сообщение об ошибке в длог
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

        public void TransmitCommand(string type, double value) {
            if (isArduino)
                switch (type) {
                    case "frequency":
                        serialPort.WriteLine("f" + Math.Round(value * 1000 - 16).ToString());
                        break;
                    case "noise":
                        serialPort.WriteLine("n" + value.ToString());
                        break;
                    case "brigthness":
                        serialPort.WriteLine("b" + value.ToString());
                        break;
                    case "screen mode":
                        serialPort.WriteLine("m" + value.ToString());
                        break;
                }
        }

        // Поток ком порта
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) {
            string incomeMessage = serialPort.ReadLine();
            Dispatcher.CurrentDispatcher.Invoke(new LineReceivedEvent(LineReceived), incomeMessage);
        }

        private delegate void LineReceivedEvent(string incomeMessage);

        private void LineReceived(string incomeMessage) {
            upTime = (DateTime.Now - stopWatchUpTime).Duration();
            Time = upTime.TotalMinutes;
            try {
                currentTemperature = incomeMessage.Remove(5);
                dataModel.AddNewMeasurement(timePoint, double.Parse(currentTemperature, System.Globalization.CultureInfo.InvariantCulture));
                OnPropertyChanged(nameof(currentTemperature));
            } catch {
                errorMessage = "Wrong signal from sensor";
                OnPropertyChanged(nameof(errorMessage));
            }

            // запись данных
            if (needToWrite) {
                try {
                    StreamWriter sw = new StreamWriter(pathToWrite, append: true);
                    sw.Write(
                        DateTime.Now.ToString("hh:mm:ss:fff") + "\t" +
                        (DateTime.Now - stopWatchOnLogStart).Duration().ToString("hh\\:mm\\:ss\\:fff") + "\t" +
                        (DateTime.Now - stopWatchDt).Duration().ToString("ss\\:fff") + "\t" +
                        Convert.ToString(Math.Round(Time, 3), CultureInfo.InvariantCulture) + "\t" +
                        currentTemperature + "\n");
                    stopWatchDt = DateTime.Now;
                    sw.Close();
                } catch (Exception exc) {
                    needToWrite = false;
                    DebugMessage("Problem with writer:" + exc);
                }
            }
        }
    }
}
