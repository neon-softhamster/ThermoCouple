using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ThermoCouple.MVVM.Command {
    public class UpdateScreenModeC : ICommand {
        public event EventHandler CanExecuteChanged;
        ViewModel.ViewModel viewModel;

        public UpdateScreenModeC(ViewModel.ViewModel vM) {
            viewModel = vM;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (CanExecute(parameter)) {
                viewModel.UpdScrMode(parameter);
                Console.WriteLine("UpdateScreenModeC command goes");
            }
        }
    }
}
