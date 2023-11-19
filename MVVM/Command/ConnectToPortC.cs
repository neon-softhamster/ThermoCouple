using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ThermoCouple.MVVM.ViewModel;

namespace ThermoCouple.MVVM.Command {
    internal class ConnectToPortC : ICommand {
        public event EventHandler CanExecuteChanged;
        ConnectionVM _viewModel;

        public ConnectToPortC(ConnectionVM vM) {
            _viewModel = vM;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (CanExecute(parameter))
                _viewModel._connection.ConnectToArduinoAsync();
            Console.WriteLine("BBBBBBBBBBBBB");
        }
    }
}
