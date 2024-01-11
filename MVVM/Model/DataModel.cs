using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermoCouple.MVVM.Model {
    public class DataModel {
        const int bufferMaxSize = 100; // максимально допустимый размер буфера с данными
        double[] time = new double[bufferMaxSize];
        double[] measurements = new double[bufferMaxSize];
        double dt; // интервал времени
        double stabilityСriterion; // критерий стабильности, величина производной dT/dt
        double stabilityСriterionInterval; // интервал в с по которому определяется стабильность
        int stability; // -1 = температура падает, 0 = температура стабильна, +1 = температура растет

        public double[] TemperatureAxis { 
            get { return measurements; }
        }
        public double[] TimeAxis {
            get { return time; }
        }

        public DataModel() {
            this.stabilityСriterion = 0.05;
            this.stabilityСriterionInterval = 10;
            this.dt = 0.5;
        }

        public void AddNewMeasurement(double newMeasurementX, double newMeasurementY) {
            for (int i = bufferMaxSize - 2; i >= 0; i -= 1) {
                measurements[i + 1] = measurements[i];
            }
            measurements[0] = newMeasurementY;

            for (int i = bufferMaxSize - 2; i >= 0; i -= 1) {
                time[i + 1] = time[i];
            }
            time[0] = newMeasurementX;
        }
    }
}
