using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ThermoCouple.MVVM.Model;
using ThermoCouple.MVVM.Command;
using System.Data.Common;
using System.ComponentModel;
using ScottPlot;
using System.Windows.Threading;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ThermoCouple.MVVM.ViewModel {
    public class ViewModel : BaseVM {
        // Поля
        private Driver tcDriver;
        private bool isConnected;
        private bool connectButtonAvaliability;
        private bool mainPanelAvaliability;
        private bool isAutoscale;
        private string selectedPort;
        private string errorMessage;
        private string connectButtonText;
        private string dataLoggerButtonText;
        private string currentTemperature;
        private string dataPath;
        private string statusBar;
        private string selectedNoise;
        private string selectedFrequency;
        private string selectedBrightness;
        private string tempStabilityCrit;
        private IList<string> portsList;
        private IList<string> frequencyList;
        private IList<string> noiseList;
        private IList<string> brightnessList;
        private WpfPlot graph;
        readonly ScottPlot.Plottable.DataLogger logger;
        private INIManager iniManager;

        // Конструктор
        public ViewModel() {
            // подключение к драйверу термопары
            tcDriver = new Driver();
            tcDriver.PropertyChanged += ConnectionPropChanged;

            // создание графика
            graph = new WpfPlot();
            logger = graph.Plot.AddDataLogger();
            graph.Plot.YAxis.SetBoundary(0, 100);
            graph.Plot.XAxis.Label("Uptime, min", size: 16);
            graph.Plot.YAxis.Label("Temperature, °C", size: 16);
            graph.Plot.Title("Temperature on time", size: 20);
            graph.Plot.Layout(left: 50, right: 50, bottom: 50, top: 25);
            graph.Plot.AxisAuto();
            graph.Refresh();

            // чтение config.ini файла
            iniManager = new INIManager(System.AppDomain.CurrentDomain.BaseDirectory + "config.ini");
            SelectedNoise = iniManager.GetPrivateString("settings", "noise");
            SelectedFrequency = iniManager.GetPrivateString("settings", "frequency");
            SelectedBrightness = iniManager.GetPrivateString("settings", "brightness");
            TempStabilityCrit = iniManager.GetPrivateString("settings", "stability");
            IsAutoscale = Convert.ToBoolean(iniManager.GetPrivateString("monitor", "autoscale"));

            // Заполнение текстовых полей
            DataLoggerButtonText = "Start logging";
            ConnectButtonText = "Connect";
            CurrentTemperature = "No data yet";
            Status = "Status: Please connect the sensor to receive data";
            DataPath = System.AppDomain.CurrentDomain.BaseDirectory + "thermal_data";

            // заполение списков
            portsList = tcDriver.SerialPortsList;
            frequencyList = new List<string>() { "0.5", "1", "2", "5" };
            noiseList = new List<string>() { "0", "0.5", "1", "2" };
            brightnessList = new List<string>() { "0", "1", "2", "3", "4", "5", "6" };
            selectedPort = portsList[0];

            // логические значения
            isConnected = tcDriver.IsConnected;
            ConnectButtonAvaliability = true;
            MainPanelAvaliability = false;

            // создание экземпляров команд
            RefreshPortList = new RefreshPortListC(this);
            ConnectToPort = new ConnectToPortC(this);
            LogData = new StartDataWritingC(this);
            UpdateScreenMode = new UpdateScreenModeC(this);
        }

        void ConnectionPropChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "isConnected":
                    if (!tcDriver.IsConnected) {
                        ConnectButtonText = "Connect";
                        MainPanelAvaliability = false;
                        Status = "Status: Please connect the sensor to receive data";
                    } else {
                        ConnectButtonText = "Disconnect";
                        Error = "";
                        MainPanelAvaliability = true;
                        Status = "Status: No data is currently being recorded";
                    }
                    isConnected = tcDriver.IsConnected;
                    ConnectButtonAvaliability = true;
                    break;

                case "errorMessage":
                    Error = tcDriver.ErrorMessage;
                    ConnectButtonText = "Connect";
                    ConnectButtonAvaliability = true;
                    Console.WriteLine("Error msg had been delivered");
                    break;

                case "currentTemperature":
                    CurrentTemperature = tcDriver.CurrentTemperature;
                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        logger.Add(tcDriver.Time, Convert.ToDouble(tcDriver.CurrentTemperature, CultureInfo.InvariantCulture));
                        graph.Refresh();
                    }));
                    Console.WriteLine("currentTemperature is delivered");
                    break;
            }
        }

        // Команды

        public ICommand LogData { get; }
        public ICommand RefreshPortList { get; }
        public ICommand ConnectToPort { get; }
        public ICommand UpdateScreenMode { get; }

        public void RefreshPorts() {
            tcDriver.InitializePorts();
            Ports = tcDriver.SerialPortsList;
            SelectedPort = tcDriver.SerialPortsList[0];
        }

        public void Connect() {
            if (isConnected) {
                tcDriver.DisconnectFromArduino();

                isConnected = false;
                ConnectButtonText = "Connect";
                CurrentTemperature = "No data";
                MainPanelAvaliability = false;
                tcDriver.NeedToWrite = false;
                DataLoggerButtonText = "Start logging";
                Application.Current.Dispatcher.Invoke(new Action(() => { 
                    logger.Clear();
                    graph.Refresh();
                }));
                Status = "Status: Please connect the sensor to receive data";
            } else {
                tcDriver.PortName = selectedPort;
                Task.Run(() => tcDriver.ConnectToArduinoAsync());

                ConnectButtonText = "Connection...";
                ConnectButtonAvaliability = false;
            }
        }

        public void WriteData() {
            tcDriver.PathToWrite = dataPath;
            tcDriver.NeedToWrite = !tcDriver.NeedToWrite;
            if (tcDriver.NeedToWrite) {
                Status = "Status: Data recording in progress";
                DataLoggerButtonText = "Stop logging";
            } else {
                Status = "Status: No data is currently being recorded";
                DataLoggerButtonText = "Start logging";
            }
        }

        public void UpdScrMode(object parameter) {
            tcDriver.TransmitCommand("screen mode", Convert.ToDouble(parameter, CultureInfo.InvariantCulture));
            iniManager.WritePrivateString("settings", "screenmode", parameter.ToString());
        }

        // Свойства
        public bool MainPanelAvaliability {
            get { return mainPanelAvaliability; }
            set {
                mainPanelAvaliability = value;
                OnPropertyChanged(nameof(MainPanelAvaliability));
                Console.WriteLine("MainPanelAvaliability updated");
            }
        }
        public string CurrentTemperature {
            get {
                if (isConnected)
                    return currentTemperature.Remove(4) + "°C";
                else return currentTemperature;
            }
            set {
                currentTemperature = value;
                OnPropertyChanged(nameof(CurrentTemperature));
                Console.WriteLine("CurrentTemperature updated");
            }
        }
        public IList<string> Ports {
            get { return portsList; }
            set {
                portsList = value;
                OnPropertyChanged(nameof(Ports));
                Console.WriteLine("Ports are being updated");
            }
        }
        public string SelectedPort {
            get { return selectedPort; }
            set {
                selectedPort = value;
                OnPropertyChanged("CurrentPort");
            }
        }
        public string Error {
            get { return errorMessage; }
            set {
                errorMessage = value;
                OnPropertyChanged("Error");
            }
        }
        public string ConnectButtonText {
            get { return connectButtonText; }
            set {
                connectButtonText = value;
                OnPropertyChanged(nameof(ConnectButtonText));
            }
        }
        public bool ConnectButtonAvaliability {
            get { return connectButtonAvaliability; }
            set {
                connectButtonAvaliability = value;
                OnPropertyChanged(nameof(ConnectButtonAvaliability));
            }
        }
        public string DataPath {
            get { return dataPath; }
            set {
                dataPath = value;
                OnPropertyChanged(nameof(DataPath));
            }
        }
        public string Status {
            get => statusBar;
            set {
                statusBar = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        public string SelectedFrequency {
            get { return selectedFrequency; }
            set {
                selectedFrequency = value;
                tcDriver.Dt = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                tcDriver.TransmitCommand("frequency", Convert.ToDouble(value, CultureInfo.InvariantCulture));
                iniManager.WritePrivateString("settings", "frequency", value);
                OnPropertyChanged(nameof(SelectedFrequency));
            }
        }
        public IList<string> FrequencyList { 
            get { return frequencyList; }
            set {
                frequencyList = value;
                OnPropertyChanged(nameof(FrequencyList));
            }
        }
        public string SelectedNoise {
            get { return selectedNoise; }
            set {
                selectedNoise = value;
                tcDriver.TransmitCommand("noise", Convert.ToDouble(value, CultureInfo.InvariantCulture));
                iniManager.WritePrivateString("settings", "noise", value);
                OnPropertyChanged(nameof(SelectedNoise));
            }
        }
        public IList<string> NoiseList { 
            get { return noiseList; } 
            set {
                noiseList = value;
                OnPropertyChanged(nameof(NoiseList));
            } 
        }
        public string SelectedBrightness {
            get { return selectedBrightness; }
            set {
                selectedBrightness = value;
                tcDriver.TransmitCommand("brigthness", Convert.ToDouble(value, CultureInfo.InvariantCulture));
                iniManager.WritePrivateString("settings", "brightness", value);
                OnPropertyChanged(nameof(SelectedBrightness));
            }
        }
        public IList<string> BrightnessList {
            get { return brightnessList; }
            set {
                brightnessList = value;
                OnPropertyChanged(nameof(BrightnessList));
            }
        }
        public string DataLoggerButtonText { 
            get { return dataLoggerButtonText; }
            set { 
                dataLoggerButtonText = value; 
                OnPropertyChanged(nameof(DataLoggerButtonText));
            }
        }
        public WpfPlot TemperatureGraph {
            get => graph;
            set {
                graph = value;
                OnPropertyChanged(nameof(TemperatureGraph));
            }
        }
        public string TempStabilityCrit {
            get { return tempStabilityCrit; }
            set {
                try {
                    double isDouble = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    tempStabilityCrit = value;
                } catch {
                    tempStabilityCrit = "0.5";
                }
                iniManager.WritePrivateString("settings", "stability", value);
                OnPropertyChanged(nameof(TempStabilityCrit));
            }
        }
        public bool IsAutoscale {
            get => isAutoscale; 
            set {
                isAutoscale = value;
                logger.ManageAxisLimits = value;
                iniManager.WritePrivateString("monitor", "autoscale", Convert.ToString(value));
                OnPropertyChanged(nameof(IsAutoscale));
            }
        }
    }
}
