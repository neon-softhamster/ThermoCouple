using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ThermoCouple.MVVM.Command {
    public class StartDataWritingC : ICommand {
        public event EventHandler CanExecuteChanged;
        ViewModel.ViewModel viewModel;

        public StartDataWritingC(ViewModel.ViewModel vM) {
            viewModel = vM;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (CanExecute(parameter)) {
                viewModel.WriteData();
                Console.WriteLine("StartDataWritingC command goes");
            }
        }
    }
}
