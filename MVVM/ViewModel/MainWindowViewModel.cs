using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermoCouple.MVVM.ViewModel {
    public class MainWindowViewModel : BaseViewModel {
        // свойство, на которое биндится текст температуры
        public string CurrentTemperature { get; set; }
    }
}
