using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlaneTK.Configuration;
using PlaneTK.PlaneControls.ControlHandlers;
using PlaneTK.StreamManager.DataPacket;
using UnityEngine;

namespace PlaneTK.Controls.ControlHandlers
{
    public class DefaultControlHandler : IControlHandler
    {

        private ConfigModel _model;

        public ControlPacket.MessageTypes MessageType { get; private set; }


        private Dictionary<KeyCode, float> _keyCodes = new Dictionary<KeyCode, float>()
        {
            {KeyCode.Alpha0, 0},
            {KeyCode.Alpha1, 0.1f},
            {KeyCode.Alpha2, 0.2f},
            {KeyCode.Alpha3, 0.3f},
            {KeyCode.Alpha4, 0.4f},
            {KeyCode.Alpha5, 0.5f},
            {KeyCode.Alpha6, 0.6f},
            {KeyCode.Alpha7, 0.7f},
            {KeyCode.Alpha8, 0.85f},
            {KeyCode.Alpha9, 1f},
        };

        public void Init(ConfigModel configModel)
        {
            _model = configModel;
            MessageType = ControlPacket.MessageTypes.ControlData;
            _lastMain = _model.MainEngine.equillibrium_position;
        }

        private byte _lastMain;

        public ControlPacket Handle()
        {
            ControlPacket packet = ControlPacket.Packet()
                .Right(_model.RightWing.equillibrium_position)
                .Left(_model.LeftWing.equillibrium_position)
                .Tail(_model.Tail.equillibrium_position)
                .Main(_lastMain);

            float vertical = Input.GetAxis("Vertical");
       //     Debug.Log("Tail");
            packet.Tail(_model.Tail.GetMappedValue(-1, 1, vertical));
            float horizontal = Input.GetAxis("Horizontal");
         //   Debug.Log("L and R");
            if (horizontal > 0)
                packet.Left(_model.LeftWing.GetMappedValue(0, 1, horizontal));
            if (horizontal < 0)
                packet.Right(_model.RightWing.GetMappedValue(0, -1, horizontal));

            if (Mathf.Abs(vertical) < 0.1f)
                packet.Tail(_model.Tail.equillibrium_position);

            foreach (var keyValuePair in _keyCodes)
            {
                if (Input.GetKey(keyValuePair.Key))
                {
                    _lastMain = _model.MainEngine.GetMappedValue(0, 1, keyValuePair.Value, false);
                    packet.Main(_lastMain);
                }
            }

            return packet;
        }
    }
}
