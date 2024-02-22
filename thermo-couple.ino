#include "math.h"
#include "max6675.h"  // max6675.h file is part of the library that you should download from Robojax.com
#define DISP7219_CLK_DELAY 10
#include <Arduino.h>
#include <GyverSegment.h>
#include <GyverTimers.h>

int soPin = 4;                          // SO = Serial Out
int csPin = 5;                          // CS = chip select CS pin
int sckPin = 6;                         // SCK = Serial Clock pin
MAX6675 robojax(sckPin, csPin, soPin);  // create instance object of thermocouple

int CLK = 3;
int DIO = 2;
Disp1637_4 digitScreen(DIO, CLK);  // create instance object of digital screen

String control_msg, control_msg_buffer;
const int sizeOfCommandRegister = 6;
String CommandRegister[sizeOfCommandRegister];
/* Register structure:
0. r - respond
1. d - send data
2. s - stop data
3. fXXXX - new freq, ms
4. nXXXX - new noise cutoff
5. bX - brightness of screen, X = 0..7
6. mX - display mapping (display mode) 0 - round value with °C, 1 - with decimal dot
*/
bool isNewValue = false;
int decimalSign = 6;
int main_frequency = 500;      // ms
int additional_frequency = 0;  // ms
float T_measurement;
float SmoothedT;
float averageNoise = 2;
int initialBrightness = 5;
int display_mode = 0;

void updateCommandRegister() {
  if (Serial.available() > 0) {
    control_msg_buffer = Serial.readString();
    if (control_msg_buffer.startsWith("r")) {
      CommandRegister[0] = control_msg_buffer;
    } else if (control_msg_buffer.startsWith("d")) {
      CommandRegister[1] = control_msg_buffer;
      CommandRegister[2] = "";
    } else if (control_msg_buffer.startsWith("s")) {
      CommandRegister[1] = "";
      CommandRegister[2] = control_msg_buffer;
    } else if (control_msg_buffer.startsWith("f")) {
      CommandRegister[3] = control_msg_buffer;
    } else if (control_msg_buffer.startsWith("n")) {
      CommandRegister[4] = control_msg_buffer;
    } else if (control_msg_buffer.startsWith("b")) {
      CommandRegister[5] = control_msg_buffer;
    } else if (control_msg_buffer.startsWith("m")) {
      CommandRegister[6] = control_msg_buffer;
    } else {
      Serial.println("Wrong key");
    }
  }
}

float expRunningAverageAdaptive(float newVal) {
  static float filVal = 0;
  float k;
  float delta = abs(newVal - filVal);

  // резкость фильтра зависит от модуля разности значений
  if (delta < averageNoise) {
    k = exp(delta - (averageNoise + 0.1));
  } else {
    k = 0.9;
  }

  filVal += (newVal - filVal) * k;
  return filVal;
}

ISR(TIMER1_A) {
  // send data
  if (CommandRegister[1].startsWith("d") && CommandRegister[1].startsWith("s") == false) {
    T_measurement = robojax.readCelsius();
    isNewValue = true;
  }
}

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(10);

  digitScreen.setCursor(0);
  digitScreen.brightness(initialBrightness);
  digitScreen.print("----");
  digitScreen.update();

  Timer1.setFrequencyFloat(1000 / main_frequency);
  Timer1.enableISR(CHANNEL_A);
}

void loop() {
  //digitScreen.tick();
  updateCommandRegister();

  // update noise cutoff
  if (CommandRegister[4].startsWith("n")) {
    averageNoise = CommandRegister[4].substring(1).toFloat();
    CommandRegister[4] = "";
  }

  // if stop
  if (CommandRegister[2].startsWith("s")) {
    digitScreen.setCursor(0);
    digitScreen.print("----");
    digitScreen.update();
    CommandRegister[2] = "";
  }

  // update frequency
  if (CommandRegister[3].startsWith("f")) {
    main_frequency = CommandRegister[3].substring(1).toInt();
    Timer1.setFrequencyFloat(1000 / main_frequency);
    CommandRegister[3] = "";
  }

  // update display mode
  if (CommandRegister[6].startsWith("m")) {
    display_mode = CommandRegister[6].substring(1).toInt();
    CommandRegister[6] = "";
  }

  // respond if asked
  if (CommandRegister[0].startsWith("r")) {
    Serial.println("Salve");
    delay(additional_frequency);
    CommandRegister[0] = "";
  }

  // update screen brightness
  if (CommandRegister[5].startsWith("b")) {
    digitScreen.brightness(CommandRegister[5].substring(1).toInt());
    CommandRegister[5] = "";
  }

  if ((CommandRegister[1].startsWith("d") && CommandRegister[1].startsWith("s") == false) && isNewValue == true) {
    isNewValue = false;
    SmoothedT = expRunningAverageAdaptive(T_measurement);
    Timer1.disableISR();
    Serial.println(SmoothedT);
    Timer1.enableISR();
    // write to screen
    digitScreen.setCursor(0);
    if (display_mode == 0) {
      digitScreen.print(String(round(SmoothedT)) + "*C");
    } else {
      digitScreen.print(SmoothedT);
    }
    digitScreen.update();
  }

  // delay area
  /*
  if (CommandRegister[1].startsWith("d") && CommandRegister[1].startsWith("s") == false) {
    delay(main_frequency);
  } else {
    delay(additional_frequency);
  }
  */
}