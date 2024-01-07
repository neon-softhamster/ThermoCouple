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

namespace ThermoCouple.MVVM.ViewModel {
    public class ViewModel : BaseVM {
        // Поля
        private Driver thermoCoupleDriver;
        private bool isConnected;
        private bool connectButtonAvaliability;
        private bool mainPanelAvaliability;
        private string selectedPort;
        private string _errorMessage;
        private string connectButtonText;
        private string currentTemperature;
        private string dataPath;
        private string statusBar;
        private IList<string> portsList;
        private WpfPlot plot;

        // Конструктор
        public ViewModel() {
            thermoCoupleDriver = new Driver();
            thermoCoupleDriver.PropertyChanged += ConnectionPropChanged;

            portsList = thermoCoupleDriver.SerialPortsList;
            selectedPort = portsList[0];
            ConnectButtonText = "Connect";
            ConnectButtonAvaliability = true;
            isConnected = thermoCoupleDriver.IsConnected;
            CurrentTemperature = "No data yet";
            MainPanelAvaliability = false;
            DataPath = System.AppDomain.CurrentDomain.BaseDirectory + "thermal_data.txt";
            Status = "Status: Please connect the sensor to receive data";

            RefreshPortList = new RefreshPortListC(this);
            ConnectToPort = new ConnectToPortC(this);
            LogData = new StartDataWritingC(this);

            plot = new WpfPlot();
            plot.MinHeight = 200;
            var scottPlot = new ScottPlot.Plot();
            Random rand = new Random(0);
            TemperatureGraph.Plot.AddScatter(DataGen.NoisySin(rand, 100), DataGen.NoisyLinear(rand, 100));
            plot.Refresh();
        }

        void ConnectionPropChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "isConnected":
                    if (!thermoCoupleDriver.IsConnected) {
                        ConnectButtonText = "Connect";
                        MainPanelAvaliability = false;
                        Status = "Status: Please connect the sensor to receive data";
                    } else {
                        ConnectButtonText = "Disconnect";
                        Error = "";
                        MainPanelAvaliability = true;
                        Status = "Status: No data is currently being recorded";
                    }
                    isConnected = thermoCoupleDriver.IsConnected;
                    ConnectButtonAvaliability = true;
                    break;

                case "errorMessage":
                    Error = thermoCoupleDriver.ErrorMessage;
                    ConnectButtonText = "Connect";
                    ConnectButtonAvaliability = true;
                    Console.WriteLine("Error msg had been delivered");
                    break;

                case "currentTemperature":
                    CurrentTemperature = thermoCoupleDriver.CurrentTemperature;
                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        TemperatureGraph.Plot.Clear();
                        TemperatureGraph.Plot.AddScatter(thermoCoupleDriver.DataX, thermoCoupleDriver.DataY);
                        plot.Refresh();
                    }));
                    Console.WriteLine("currentTemperature is delivered");
                    break;
            }
        }

        // Команды

        public ICommand LogData { get; }
        public ICommand RefreshPortList { get; }
        public ICommand ConnectToPort { get; }

        public void RefreshPorts() {
            thermoCoupleDriver.InitializePorts();
            Ports = thermoCoupleDriver.SerialPortsList;
            SelectedPort = thermoCoupleDriver.SerialPortsList[0];
        }

        public void Connect() {
            if (isConnected) {
                thermoCoupleDriver.DisconnectFromArduino();

                isConnected = false;
                ConnectButtonText = "Connect";
                CurrentTemperature = "No data";
                MainPanelAvaliability = false;
                thermoCoupleDriver.NeedToWrite = !thermoCoupleDriver.NeedToWrite;
                Status = "Status: Please connect the sensor to receive data";
            } else {
                thermoCoupleDriver.PortName = selectedPort;
                Task.Run(() => thermoCoupleDriver.ConnectToArduinoAsync());

                ConnectButtonText = "Connection...";
                ConnectButtonAvaliability = false;
            }
        }

        public void WriteData() {
            thermoCoupleDriver.NeedToWrite = !thermoCoupleDriver.NeedToWrite;
            if (thermoCoupleDriver.NeedToWrite) {
                Status = "Status: Data recording in progress";
            } else {
                Status = "Status: No data is currently being recorded";
            }
            thermoCoupleDriver.PathToWrite = DataPath;
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
            get { return _errorMessage; }
            set {
                _errorMessage = value;
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

        public WpfPlot TemperatureGraph {
            get { return plot; }
            set {
                plot = value;
                OnPropertyChanged(nameof(Plot));
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
    }
}
