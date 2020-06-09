using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using Random = System.Random;


public class TestScript : MonoBehaviour
{
    SerialPort _stream;

    void Awake()
    {
        _stream = new SerialPort("COM3", 9600);
        _stream.ReadTimeout = 50;
        _stream.Open();
        _bytes.Clear();
        for (int i = 0; i < 32; i++)
        {
            _bytes.Add(0);
        }
    }

    public void WriteToArduino()
    {
        var message = "PING";
        Write(message);
       
    }

    public void On()
    {
        Write("ON");
    }

    public void Off()
    {
        Write("OFF");
    }

    private List<byte> _bytes = new List<byte>();
    private void Update()
    {
        if (Input.GetAxis("Horizontal") > 0)
        {

        }
        _bytes[0] = (byte) (Input.GetAxis("Vertical") * 20);
        Write(_bytes.ToArray(), 32);
        /*if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            
            Write(_bytes.ToArray(),32);
        }*/
            /*Write(new byte[]{0},1,0);
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Write(new byte[] { 1 }, 1, 0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Write(new byte[] { 2 }, 1, 0);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Write(new byte[] { 3 }, 1, 0);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            Write(new byte[] { 4 }, 1, 0);*/
    }

    private void Write(string message)
    {
        _stream.WriteLine(message);
        _stream.BaseStream.Flush();
        StartCoroutine
        (
            AsynchronousReadFromArduino
            ((string s) => Debug.Log(s),     // Callback
                () => Debug.LogError("Error!"), // Error callback
                10000f                          // Timeout (milliseconds)
            )
        );
    }

    private void Write(byte[] message, int length = -1, int offset = 0)
    {
        if (length == -1)
            length = message.Length;
        _stream.Write(message, offset,length);
        _stream.BaseStream.Flush();
        StartCoroutine
        (
            AsynchronousReadFromArduinoBytes
            ((byte[] s) =>
                {
                    for (var i = 0; i < message.Length; i++)
                    {
                        Debug.Log($"Sent {_bytes[i]}, Read {s[i]}");
                    }
                },
                () => Debug.LogError("Error!"), // Error callback
                10000f                          // Timeout (milliseconds)
            )
        );
    }


    public IEnumerator AsynchronousReadFromArduinoBytes(Action<byte[]> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        byte[] dataString = new byte[32];
        bool read = false;
        do
        {
            try
            {
                dataString = new byte[32];

                for (int i = 0; i < 32; i++)
                {
                    dataString[i] = (byte)_stream.ReadByte();
                }
                //var bytesRead = _stream.Read(dataString,0,32);
                //Debug.Log("Count "+bytesRead);
                read = true;
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (read)
            {
                callback(dataString);
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

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            try
            {
                dataString = _stream.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
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

    private void OnDestroy()
    {
        _stream.Close();
    }
}
