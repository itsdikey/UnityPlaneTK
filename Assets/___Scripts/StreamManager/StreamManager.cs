using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using PlaneTK.StreamManager.DataPacket;
using UnityEngine;


namespace PlaneTK.StreamManager
{
    public class StreamManager : MonoBehaviour
    {
        public static StreamManager Instance { get; private set; }

        private bool _isInitialized;

        SerialPort _stream;

        public event Action<byte[]> DataCallback;

        private byte _message = 0;

        public byte SyncByte { get; set; }

        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            Instance = this;
        }

        public void Init(string portNumber, Action successCallback, Action<Exception> errorCallback)
        {
            try
            {
                _stream = new SerialPort(portNumber, 115200);
                _stream.ReadTimeout = 50;
                _stream.Open();
                _isInitialized = true;
                successCallback?.Invoke();
            }
            catch (Exception e)
            {
                _isInitialized = false;
                errorCallback?.Invoke(e);
            }
        }

        public void Write(ControlPacket packet)
        {
            _message = (byte)((++_message) % 256);
            packet.Index(_message);
            packet.CalculateControlByte();
            packet.SyncByte(SyncByte);
            Write(packet.Data, -1,0,packet.IsReadable);
        }

        public void Write(byte[] message, int length = -1, int offset = 0, bool isReadable = true)
        {
            if (length == -1)
                length = message.Length;

            _stream.Write(message, offset, length);
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < message.Length; i++)
            {
                sb.Append($"#{i} " + message[i] + " ");
            }
            //Debug.Log("Sent " + sb.ToString());
            _stream.BaseStream.Flush();

            if (isReadable)
            {
                StartCoroutine
                (
                    AsynchronousReadFromArduinoBytes
                    (OnDataCallback,
                        () => Debug.LogError("Error!"), // Error callback
                        1000f                          // Timeout (milliseconds)
                    )
                );
            }
            else
            {
                OnDataCallback(new byte[]{250,250});
            }
                
          
            
        }

        public IEnumerator AsynchronousReadFromArduinoBytes(Action<byte[]> callback, Action fail = null, float timeout = float.PositiveInfinity)
        {
            DateTime initialTime = DateTime.Now;
            DateTime nowTime;
            TimeSpan diff = default(TimeSpan);

            byte[] dataPayload = new byte[32];
            bool read = false;
            do
            {
                try
                {
                    dataPayload = new byte[64];
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < 64; i++)
                    {
                        dataPayload[i] = (byte)_stream.ReadByte();
                        sb.Append($"#{i} "+dataPayload[i] + " ");
                        if (i == 31)
                            sb.AppendLine();
                    }

                   // Debug.Log("Received "+sb.ToString());
                    
                    

                    //var bytesRead = _stream.Read(dataString,0,32);
                    //Debug.Log("Count "+bytesRead);
                    read = true;
                }
                catch (TimeoutException)
                {
                    dataPayload = null;
                }

                if (read)
                {
                    var data = new byte[32];
                    Array.Copy(dataPayload,32,data,0,32);
                    
                    callback(data);
                    yield break; // Terminates the Coroutine
                }
                else
                    yield return null; // Wait for next frame

                nowTime = DateTime.Now;
                diff = nowTime - initialTime;

            } while (diff.Milliseconds < timeout);

            if (fail != null)
                fail();
            yield return null;
        }

        protected virtual void OnDataCallback(byte[] obj)
        {
            DataCallback?.Invoke(obj);
        }
    }
}

