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

namespace ThermoCouple.MVVM.ViewModel {
    public class ConnectionVM : BaseVM {
        // Свойства
        private IList<string> _portList;
        private string _currentPort;
        private string _errorMessage;
        public ConnectionM _connection;
        // Команды
        public ICommand RefreshPortList { get; }
        public ICommand ConnectToPort { get; }

        // Конструктор
        public ConnectionVM()
        {
            _connection = new ConnectionM();
            _portList = _connection.SerialPortsList;
            _currentPort = _connection.serialPort.PortName;
            _errorMessage = _connection.ErrorMessage;

            // Команды
            RefreshPortList = new RefreshPortListC(this);
            ConnectToPort = new ConnectToPortC(this);
        }

        public IList<string> Ports {
            get { return _portList; }
        }
        public string CurrentPort { 
            get { return _currentPort; }
            set { 
                _currentPort = value;
                _connection.serialPort.PortName = value;
            }
        }
        public string Error {
            get { return _errorMessage; }
        }
    }
}
