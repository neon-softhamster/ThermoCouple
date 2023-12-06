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

        ConnectionVM viewModel;

        public ConnectToPortC(ConnectionVM vM) {
            viewModel = vM;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (CanExecute(parameter)) {
                viewModel.Connect();
                Console.WriteLine("ConnectToPortC command goes");
            }
        }
    }
}
