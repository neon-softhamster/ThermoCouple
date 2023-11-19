using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ThermoCouple.MVVM.Model;

namespace ThermoCouple.MVVM.ViewModel {
    public class ConnectionVM : BaseVM {
        private IList<string> _portList;
        private string _currentPort;
        private ConnectionM _connection;

        public ConnectionVM()
        {
            _connection = new ConnectionM();
            _portList = _connection.SerialPortsList;
            _currentPort = _connection.serialPort.PortName;
        }

        public IList<string> Ports {
            get { return _portList; }
        }
        public string CurrentPort { 
            get { return _currentPort; }
            set { _currentPort = value; }
        }
    }
}
