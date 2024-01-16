using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermoCouple.MVVM.Model {
    public class DataModel {
        const int bufferMaxSize = 200; // максимально допустимый размер буфера с данными
        double[] time = new double[bufferMaxSize]; // в секундах, в 0 элементе самая свежая точка
        double[] measurement = new double[bufferMaxSize]; // темпертаура, в 0 элементе самая свежая точка
        double stabilityСriterion; // критерий стабильности, величина производной dT/dt, при которой температура считается стабильной
        double stabilityСriterionInterval; // интервал в с по которому определяется стабильность
        int stability; // -1 = температура падает, 0 = температура стабильна, +1 = температура растет
        double temperatureTrend;

        public double StabilityСriterion {
            get { return stabilityСriterion; }
            set { stabilityСriterion = value; }
        }
        public double StabilityСriterionInterval {
            get { return stabilityСriterionInterval; }
            set { stabilityСriterionInterval = value; }
        }
        public int Stability {
            get { return stability; }
            set { stability = value; }
        }
        public double TemperatureTrend {
            get { return temperatureTrend; }
            set { temperatureTrend = value; }
        }

        // конструктор
        public DataModel() {
            this.stabilityСriterion = 0.3;
            this.stabilityСriterionInterval = 0.5;
        }

        public void StabilityCheck() {
            int i = 1;
            double derivative = 0;
            double interval = 0;
            while (interval < stabilityСriterionInterval && time[i - 1] != 0  && i < (bufferMaxSize - 1)) {
                interval += Math.Abs(time[i - 1] - time[i]);
                derivative += (measurement[i - 1] - measurement[i]) / (time[i - 1] - time[i]);
                i++;
            }

            temperatureTrend = derivative / i;

            if (Math.Abs(temperatureTrend) < stabilityСriterion)
                stability = 0;
            else if(temperatureTrend > stabilityСriterion)
                stability = 1;
            else if(temperatureTrend < stabilityСriterion)
                stability = -1;
        }

        public void AddNewMeasurement(double newMeasurementX, double newMeasurementY) {
            for (int i = bufferMaxSize - 2; i >= 0; i -= 1) {
                measurement[i + 1] = measurement[i];
            }
            measurement[0] = newMeasurementY;

            for (int i = bufferMaxSize - 2; i >= 0; i -= 1) {
                time[i + 1] = time[i];
            }
            time[0] = newMeasurementX;
        }
    }
}
