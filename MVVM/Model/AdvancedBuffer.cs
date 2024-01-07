using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermoCouple.MVVM.Model {
    public class AdvancedBuffer {
        private double[] buffer;
        private int startIndex;
        private int endIndex;

        public AdvancedBuffer(int bufferSize) {
            double[] buffer = new double[bufferSize];
            startIndex = 0;
            endIndex = 0;
        }

        public AdvancedBuffer() : this(1000) { }
    }
}
