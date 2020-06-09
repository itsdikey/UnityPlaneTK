#include <SPI.h>
//#include "RF24.h"
#include "NRFLite.h"
//RF24 radio(8,9);
/**********************************************************/
byte addresses[][6] = {"Receiver","Sender"};

NRFLite _radio;
void setup() {
  
  Serial.begin(115200);
  // put your setup code here, to run once:
  //configureRadio();

  configureRadioLite();
}


void configureRadioLite()
{
  _radio.init(0, 8, 9);
}

//void configureRadio()
//{
//  radio.begin();
//  radio.setAutoAck(false);
//  //radio.enableDynamicAck();
//    // Set the PA Level low to prevent power supply related issues since this is a
//  // getting_started sketch, and the likelihood of close proximity of the devices. RF24_PA_MAX is default.
//    radio.setPALevel(RF24_PA_LOW);
//    radio.openWritingPipe(addresses[0]);
//    radio.openReadingPipe(1,addresses[1]);
//
//    // Start the radio listening for data
//    radio.startListening();
//    //radio.printDetails();
//}
//uint32_t configTimer =  millis();
void loop() {
  // put your main code here, to run repeatedly:
//   if(radio.failureDetected){
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


    byte data[32];
    byte readBytes = 0;
    if(Serial.available()>0)
    {
      Serial.readBytes(data,32);
      Serial.flush();
      readBytes = 32;
      _radio.send(1,&data, 32);
       Serial.write(data,32);
    }

    int hasData = _radio.hasData();

    if(hasData>0)
    {
      byte receivedData[32];
       _radio.readData(&receivedData);
       Serial.write(receivedData,32);

    }
    

    

//    if(readBytes>0){
//          radio.stopListening();
//        if(!radio.write(&data, 32))
//        {
//          //Serial.print("Failed sending data");
//        }
//        else
//        {
//          //Serial.print("Sent data");
//        }
//         
//         uint32_t failTimer = millis();                                                         // Variable for the received timestamp
//         
//        
//        radio.startListening();
//        unsigned long started_waiting_at = micros();               // Set up a timeout period, get the current microseconds
//        boolean timeout = false;                                   // Set up a variable to indicate if a response was received or not
//    
//        while ( ! radio.available() ){                             // While nothing is received
//          if (micros() - started_waiting_at > 500000 ){            // If waited longer than 200ms, indicate timeout and exit while loop
//            timeout = true;
//            break;
//        }      
//      }
//        
//    if ( timeout ){                                             // Describe the results
//       // Serial.println(F("Failed, response timed out."));
//    }else{
//        byte received[32];
//        int index = 0;
//      if(radio.available()){
//          //byte target[1];
//          
//          radio.read( &received, 32);
//          received[0] = radio.getDynamicPayloadSize();
//          //received[index] = target[0];
//          //index++;
//          
//          
//        }
//        byte filtered[32];
//        for(int i = 0; i < 32; i++)
//        {
//          filtered[i] = received[i];
//        }
//   
//        Serial.write(filtered,32);
//        Serial.write(data,32);
//      }
//    }


    

        
     
  }
