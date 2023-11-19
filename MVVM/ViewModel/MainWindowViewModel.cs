using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ThermoCouple.MVVM.Model;

namespace ThermoCouple.MVVM.ViewModel {
    public class MainWindowViewModel : BaseVM {
        private string[] _SerialPortsList;

        public string[] SerialPortsList {
            get {
                return _SerialPortsList;
            }
            set {
                _SerialPortsList = value;
                OnPropertyChanged(nameof(SerialPortsList));
            }
        }
    }
}
