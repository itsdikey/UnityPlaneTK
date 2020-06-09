#include <SPI.h>
//#include "RF24.h"
#include "NRFLite.h"
#include "printf.h"
#include <Servo.h>
#include <Arduino.h>
#include <TinyMPU6050.h>
#include <Adafruit_BMP085.h>
#include <SoftwareSerial.h> 
#include <TinyGPS.h> 
#include <EEPROM.h>

/******************************************************/
/******************** UNION ***************************/
/******************************************************/
union Data 
{
  int i;
  float f;
  byte data[4];
};

//Altimeter, acceleremtoer
MPU6050 mpu (Wire);
Adafruit_BMP085 bmp;

//radio
//RF24 radio(8,9);
NRFLite _radio;
/**********************************************************/
byte addresses[][6] = {"Receiver","Sender"};

int servoPinLeft = 4;
int servoPinRight = 5;
int servoPinTail = 3;
int servoPinMain = 2;
Servo servoTail;
Servo servoLeft;
Servo servoRight;
Servo servoMain;

///GPS
float lat = 28.5458,lon = 77.1703; // create variable for latitude and longitude object  
SoftwareSerial gpsSerial(0,1);//rx,tx 
TinyGPS gps;

byte syncByte;
void loadSyncByte()
{
  syncByte = EEPROM.read(0);
}

//loads and sets the equillibrium
void loadEq()
{
  if(EEPROM.read(13) == 11)
  {
    Serial.println("Loading EQ");
    // 2 main, 5 left, 8 right, 11 - tail
    servoMain.write(EEPROM.read(2));
    servoLeft.write(EEPROM.read(5));
    servoRight.write(EEPROM.read(8));
    servoTail.write(EEPROM.read(11));
  }
}

void configureRadioLite()
{
  _radio.init(1, 8, 9);
}


//void configureRadio()
//{
//  radio.begin();
// radio.setAutoAck(false);
//  //radio.enableDynamicAck();
//    // Set the PA Level low to prevent power supply related issues since this is a
//  // getting_started sketch, and the likelihood of close proximity of the devices. RF24_PA_MAX is default.
//    radio.setPALevel(RF24_PA_LOW);
//    radio.openWritingPipe(addresses[1]);
//    radio.openReadingPipe(1,addresses[0]);
//
//    // Start the radio listening for data
//    radio.startListening();
//    radio.printDetails();
//}

void configureServos()
{
  servoLeft.attach(servoPinLeft);
  servoRight.attach(servoPinRight);
  servoTail.attach(servoPinTail);
  servoMain.attach(servoPinMain);
}

void configureSensors()
{
    mpu.Initialize();
    Serial.println("=====================================");
    Serial.println("Starting calibration...");
    mpu.Calibrate();
    Serial.println("Calibration complete!");
    Serial.println("Offsets:");
    Serial.print("GyroX Offset = ");
    Serial.println(mpu.GetGyroXOffset());
    Serial.print("GyroY Offset = ");
    Serial.println(mpu.GetGyroYOffset());
    Serial.print("GyroZ Offset = ");
    Serial.println(mpu.GetGyroZOffset());

    if (!bmp.begin()) {
      Serial.println("Could not find a valid BMP085 sensor, check wiring!");
    while (1) {}
    }

    gpsSerial.begin(9600);
}

//uint32_t configTimer =  millis();
void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  // put your setup code here, to run once:
  //configureRadio();
  configureRadioLite();
  configureServos();
  configureSensors();
  loadEq();
}

void loop() {
  // put your main code here, to run repeatedly:
// if(radio.failureDetected){
//    radio.failureDetected = false;
//    delay(250);
//    Serial.println("Radio failure detected, restarting radio");
//    configureRadio();        
//  }
//  //Every 5 seconds, verify the configuration of the radio. This can be done using any 
//  //setting that is different from the radio defaults.
//  if(millis() - configTimer > 5000){
//    configTimer = millis();
//    if(radio.getDataRate() != RF24_1MBPS){
//      radio.failureDetected = true;
//      Serial.print("Radio configuration error detected");
//    }
//  }
//  byte data[32];
//  if( radio.available()){
//      
//      uint32_t failTimer = millis();                                                         // Variable for the received timestamp
//      while (radio.available()) {                                 // While there is data ready
//        if(millis()-failTimer > 500){
//          Serial.println("Radio available failure detected");
//          radio.failureDetected = true;
//          break;
//        }
//        radio.read( &data, 32);  // Get the payload     
//        processData(data);
//      }  
//}
byte data[32];
if(_radio.hasData()>0){
    _radio.readData(&data);
    processData(data);
  }
}

void processData(byte* data){

 //used for debugging to see what is the output
  

  if(!validData(data)) // data is not valid
      return;

  if(data[0]==0) //control data
  {
    processControlData(data);
  }

  if(data[0]==1) //configure equillibrium
  {
    processEquillibriumConfig(data);
    sendData(data);
  }

  if(data[0]==2) //retrieve equillibrium
  {
    processEquillibriumRetrieve(data);
    sendData(data);
  }


  if(data[0]==3) // retrieve sensor data
  {
    processRetrieveSensorData(data);
    sendData(data);
  }

  if(data[0]==4) //control data, retrieve data
  {
     processControlData(data);
     processRetrieveSensorData(data);
     sendData(data);
  }

  if(data[0]==5) //synchorinzation
  {
     synchronize(data);
     sendData(data);
  }
}


bool validData(byte* data)
{
  return true;
  int dataCheck = (data[0]^data[1]^data[2]);
  Serial.println(dataCheck);
  if(dataCheck == data[3])
    return true;
  Serial.println("Data is not valid");
  return false;
}

//Sets values for servos for main plane parts
void processControlData(byte* data)
{
  servoLeft.write(data[4]);
  servoRight.write(data[5]);
  servoTail.write(data[6]);
  servoMain.write(data[7]);
}

void processEquillibriumConfig(byte* data)
{
  //write data to eeprom
  //order is main, left, right, tail, min, equilibirum, max
  // we are going to write to eeprom in the same succession as we receive starting from byte at index 1, as 0 is reserved for sync byte
  // 1 - mainMin, 2 - mainEq, 3 - mainMax
  // 4 - leftMin, 5 - leftEq, 6 - leftMax
  // 7 - rightMin, 8 - rightEq, 9 - rightMax
  // 10 - tailMin, 11 - tailEq, 12 - tailMax
  // 13 - we have 11 if equiliibrium profile available
  // in data array we start the info from 4th byte so 4-1 = 3 offset
  for(int addr = 1; addr<=12; addr++)
  {
      EEPROM.write(addr, data[addr+3]);
  }

  //equillibrium available
  EEPROM.write(13, 11);
  
}

void processEquillibriumRetrieve(byte* data)
{
  //retrieve data from eeprom and send it to the controller
  //check if data available
  if(EEPROM.read(13)!=11)
  {
    data[20] = 0;
    return;
  }
  data[20] = 11;
  //read bytes in the given succession
  for(int addr = 1; addr<=12; addr++)
  {
      data[addr+3] = EEPROM.read(addr);
  }
}

void processRetrieveSensorData(byte* data)
{
   //getting accelerometer position
   mpu.Execute();
   float x = mpu.GetAngX()+180;
   float y = mpu.GetAngY() + 180;
   float z = mpu.GetAngZ() + 180;
   byte overflowByte = 0;
   byte xByte;
   byte yByte;
   byte zByte;
   bool printAngles = true;
   if(printAngles)
   {
      Serial.println("angles");
      Serial.print("X = ");
      Serial.print(x);
      Serial.print("Y = ");
      Serial.print(y);
      Serial.print("Z = ");
      Serial.print(z);
   }
 
   //
   if(x>255)
   {
      overflowByte += 1;
      xByte = x-255;
   }
   else
   {
      xByte = x;
   }
   //
   if(y>255)
   {
      overflowByte += 10;
      yByte = y-255;
   }
   else
   {
      yByte = y;
   }
   //
   if(z>255)
   {
      overflowByte += 100;
      zByte = z-255;
   }
   else
   {
      zByte = z;
   }

   data[8] = overflowByte;
   data[9] = xByte;
   data[10] = yByte;
   data[11] = zByte;
   
   //getting altitude
   union Data altimeter;
   altimeter.f = bmp.readAltitude();
   data[12] = altimeter.data[0];
   data[13] = altimeter.data[1];
   data[14] = altimeter.data[2];
   data[15] = altimeter.data[3];

   //getting temperature
   union Data temperature;
   temperature.f = bmp.readTemperature();
   data[16] = temperature.data[0];
   data[17] = temperature.data[1];
   data[18] = temperature.data[2];
   data[19] = temperature.data[3];

   //get location
   bool gpsAvailable = true;
 
   
   while(gpsSerial.available()){ // check for gps data 
    if(gps.encode(gpsSerial.read()))// encode gps data 
    {  
      gps.f_get_position(&lat,&lon);
    }
    else
    {
      gpsAvailable = false;
      break;
    }
   }
   data[20] = gpsAvailable?1:0;
   if(gpsAvailable){
      union Data latitude;
      union Data longitude;
      latitude.f = lat;
      longitude.f = lon;
      data[21] = latitude.data[0];
      data[22] = latitude.data[1];
      data[23] = latitude.data[2];
      data[24] = latitude.data[3];

      data[25] = longitude.data[0];
      data[26] = longitude.data[1];
      data[27] = longitude.data[2];
      data[28] = longitude.data[3];
   }
   
}

void synchronize(byte* data)
{
  
  //write synchronization byte to the memory
  EEPROM.write(0,data[1]);
  loadSyncByte();
}

void printData(byte* data)
{
  for(int i=0; i<32; i++){
    Serial.print("#");
    Serial.print(i);
    Serial.print(" ");
    Serial.print(data[i]);
    Serial.print(" "); 
    }
  Serial.println(" ");
}

void sendData(byte* data)
{
  _radio.send(0,data,32);
  
//   radio.stopListening();                                        // First, stop listening so we can talk   
//   if(!radio.write( &data, 32))
//   {
//     Serial.print(F("Sent response failed"));
//   }
//   else
//   {
//     Serial.print(F("Sent response "));
//     Serial.println();
//     printData(data);
//   }
//   radio.startListening();
}
