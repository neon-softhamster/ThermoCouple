﻿using ScottPlot.Renderable;
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

namespace ThermoCouple.MVVM.ViewModel {
    public class ConnectionVM : BaseVM {
        public ConnectionVM() {
            connection = new ConnectionM();
            Ports = connection.SerialPortsList;
            CurrentPort = connection.SerialPort.PortName;
            isConnected = connection.IsConnected;

            RefreshPortList = new RefreshPortListC(this);
            ConnectToPort = new ConnectToPortC(this);
        }

        // Поля
        private IList<string> _portList;
        private string _currentPort;
        private string _errorMessage;
        private bool isConnected;
        private ConnectionM connection;

        // Команды
        public ICommand RefreshPortList { get; }
        public ICommand ConnectToPort { get; }

        // Свойства
        public ConnectionM Connection {
            get { return connection; }
            set { connection = value; }
        }

        public void RefreshPorts() {
            connection.InitializePorts();
            Ports = connection.SerialPortsList;
            CurrentPort = connection.SerialPortsList[0];
        }

        public void Connect() {
            if (isConnected) {
                connection.DisconnectFromArduino();
            } else {
                connection.ConnectToArduinoAsync();
            }
        }

        public IList<string> Ports {
            get { return _portList; }
            set { 
                _portList = value;
                OnPropertyChanged(nameof(Ports));
                Console.WriteLine("Ports are being updated");
            }
        }
        public string CurrentPort { 
            get { return _currentPort; }
            set { 
                _currentPort = value;
                connection.SerialPort.PortName = value;
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

        public string ConnectOrDisconnect {
            get {
                if (!connection.IsConnected) {
                    return "Connect";
                } else {
                    return "Disconnect";
                }
            }
            set { 
                isConnected = connection.IsConnected;
                OnPropertyChanged("ConnectOrDisconnect");
            }
        }
    }
}