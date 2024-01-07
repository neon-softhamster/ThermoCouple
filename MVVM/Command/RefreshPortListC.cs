using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ThermoCouple.MVVM.ViewModel;

namespace ThermoCouple.MVVM.Command {
    public class RefreshPortListC : ICommand {
        public event EventHandler CanExecuteChanged;
        ViewModel.ViewModel viewModel;

        public RefreshPortListC(ViewModel.ViewModel vM) {
            viewModel = vM;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (CanExecute(parameter)) {
                viewModel.RefreshPorts();
                Console.WriteLine("RefreshPortListC command goes");
            }
        }
    }
}
