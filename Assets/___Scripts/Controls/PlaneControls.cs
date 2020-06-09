using System;
using System.Collections;
using System.Collections.Generic;
using PlaneTK.Configuration;
using PlaneTK.Controls.ControlHandlers;
using PlaneTK.HUD;
using PlaneTK.PlaneControls.ControlHandlers;
using PlaneTK.StreamManager.DataPacket;
using UnityEngine;

namespace PlaneTK.PlaneControls
{
    public class PlaneControls : MonoBehaviour
    {
        [SerializeField] private HUDControlls _controlls;

        private IControlHandler _controlHandler;
        private ConfigModel _configuration;
        private int i = 0;

        private void Awake()
        {
            StreamManager.StreamManager.Instance.DataCallback += Instance_DataCallback;
            _configuration = ConfigurationManager.Instance.GetConfig();

            _controlHandler = new DefaultControlHandler();
            _controlHandler.Init(_configuration);
        }

        private void Instance_DataCallback(byte[] obj)
        {
            _canSend = true;
            if (obj[0] == 250)
            {
                return;
            }
            ControlPacket packet = ControlPacket.FromBytes(obj);
            if (packet.Type == ControlPacket.MessageTypes.ControlDataRetrieve ||
                packet.Type == ControlPacket.MessageTypes.RetrieveSensorData)
            {
                var sensorData = packet.GetSensorData();
                if (sensorData.Rotation ==  - new Vector3(180,180,180))
                    return;
                ProcessSensorData(sensorData);
            }
        }

        private void ProcessSensorData(PlaneSensorData sensorData)
        {
            /*Debug.Log(sensorData.Rotation);*/
           // Debug.Log(sensorData.Rotation);
            _controlls.UpdateSensorData(sensorData);
        }

        private bool _canSend = true;

        private void Update()
        {
            if (_canSend)
            {
                i++;
                i %= 2;
                StreamManager.StreamManager.Instance.Write(
                    _controlHandler.Handle()
                        .MessageType(i==0?ControlPacket.MessageTypes.ControlDataRetrieve: ControlPacket.MessageTypes.ControlData));
                _canSend = false;
                StopAllCoroutines();
                StartCoroutine(SetToTrue());
            }
               
        }

        private IEnumerator SetToTrue()
        {
            yield return new WaitForSeconds(0.4f);
            _canSend = true; 
        }
    }


}

