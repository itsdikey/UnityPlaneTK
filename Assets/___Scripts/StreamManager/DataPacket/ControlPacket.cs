using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlaneTK.Configuration;
using UnityEngine;

namespace PlaneTK.StreamManager.DataPacket
{
    public class ControlPacket
    {
        public byte[] Data = new byte[32];

        private ControlPacket()
        {

        }

        public bool IsReadable
        {
            get
            {
                if (Data[0] == (byte)MessageTypes.EquilibriumRetrieve ||
                    Data[0] == (byte)MessageTypes.RetrieveSensorData ||
                    Data[0] == (byte)MessageTypes.ControlDataRetrieve)
                    return true;

                return false;
            }
        }

        public MessageTypes Type => (MessageTypes)Data[0];

        public ControlPacket Tail(byte data)
        {
            Data[6] = data;
            return this;
        }

        public ControlPacket Left(byte data)
        {
            Data[4] = data;
            return this;
        }

        public ControlPacket Right(byte data)
        {
            Data[5] = data;
            return this;
        }

        public ControlPacket Main(byte data)
        {
            Data[7] = data;
            return this; 
        }

        public ControlPacket SyncByte(byte data)
        {
            Data[1] = data;
            return this;
        }

        public ControlPacket Index(byte data)
        {
            Data[2] = data;
            return this;
        }

        public ControlPacket ControlByte(byte controlByte)
        {
            Data[3] = controlByte;
            return this;
        }

        public ControlPacket CalculateControlByte()
        {
            ControlByte((byte)(Data[0] ^ Data[1] ^ Data[2]));
            return this;
        }


        public ControlPacket MessageType(MessageTypes type)
        {
            Data[0] = (byte)type;
            return this;
        }


        public PlaneSensorData GetSensorData()
        {
            return SensorDataFromPacket(this);
        }



        public static ControlPacket Packet()
        {
            return new ControlPacket();
        }

        public static ControlPacket FromConfig(ConfigModel model)
        {
            var packet = Packet();
            packet.MessageType(MessageTypes.EquilibriumConfig);
            packet.Data[4] = model.MainEngine.min_position;
            packet.Data[5] = model.MainEngine.equillibrium_position;
            packet.Data[6] = model.MainEngine.max_position;
            packet.Data[7] = model.LeftWing.min_position;
            packet.Data[8] = model.LeftWing.equillibrium_position;
            packet.Data[9] = model.LeftWing.max_position;
            packet.Data[10] = model.RightWing.min_position;
            packet.Data[11] = model.RightWing.equillibrium_position;
            packet.Data[12] = model.RightWing.max_position;
            packet.Data[13] = model.Tail.min_position;
            packet.Data[14] = model.Tail.equillibrium_position;
            packet.Data[15] = model.Tail.max_position;
            return packet;
        }

        public static ConfigModel ToConfig(ControlPacket packet)
        {
            ConfigModel model = new ConfigModel();
            if (packet.Data[20] == 11)
            {
                model.MainEngine.min_position = packet.Data[4];
                model.MainEngine.equillibrium_position = packet.Data[5];
                model.MainEngine.max_position = packet.Data[6];
                model.LeftWing.min_position = packet.Data[7];
                model.LeftWing.equillibrium_position = packet.Data[8];
                model.LeftWing.max_position = packet.Data[9];
                model.RightWing.min_position = packet.Data[10];
                model.RightWing.equillibrium_position = packet.Data[11];
                model.RightWing.max_position = packet.Data[12];
                model.Tail.min_position = packet.Data[13];
                model.Tail.equillibrium_position = packet.Data[14];
                model.Tail.max_position = packet.Data[15];
            }
            return model;
        }

        public static ControlPacket FromBytes(byte[] bytes)
        {
            var packet = Packet();
            packet.Data = bytes;
            return packet;
        }

        public static PlaneSensorData SensorDataFromPacket(ControlPacket packet)
        {
            var sensorData = new PlaneSensorData();
            var overflowByte = (int)packet.Data[8];
            float x, y, z;
            var hundreds = overflowByte / 100; //corresponds to z
            overflowByte -= hundreds * 100;
            var tens = overflowByte / 10; // corresponds to y
            overflowByte -= tens * 10;
            var units = overflowByte; // corresponds to x
            x = units * 255 + packet.Data[9];
            y = tens * 255 + packet.Data[10];
            z = hundreds * 255 + packet.Data[11];
            sensorData.Rotation = new Vector3(z-180, y-180, x-180);
            // altitude starts at 12
            sensorData.Altitude = BitConverter.ToSingle(packet.Data, 12);
            
            // temperature starts at 16
            sensorData.Temperature = BitConverter.ToSingle(packet.Data, 16);

            //check if GPS available
            sensorData.HasGPS = packet.Data[20] == 1;

            //if GPS then get it
            if (sensorData.HasGPS)
            {
                sensorData.Latitude = BitConverter.ToSingle(packet.Data, 21);
                sensorData.Longitude = BitConverter.ToSingle(packet.Data, 25);
            }

            return sensorData;
        }



        public enum MessageTypes
        {
            ControlData = 0,
            EquilibriumConfig = 1,
            EquilibriumRetrieve = 2,
            RetrieveSensorData = 3,
            ControlDataRetrieve = 4,
            Synchronize = 5
        }
    }

    public class PlaneSensorData
    {
        public float Temperature;
        public Vector3 Rotation;
        public bool HasGPS;
        public float Longitude;
        public float Latitude;
        public float Altitude;
    }

}
