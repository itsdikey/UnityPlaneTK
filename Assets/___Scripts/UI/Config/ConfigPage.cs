using System;
using System.Collections;
using System.Collections.Generic;
using PlaneTK.Configuration;
using PlaneTK.StreamManager.DataPacket;
using PlaneTK.UI.Config.UIItems.SliderGroup;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PlaneTK.UI.Config
{
    public class ConfigPage : MonoBehaviour
    {
        [SerializeField] private SliderGroupController _equilibriumController;
        [SerializeField] private SliderGroupController _minValueController;
        [SerializeField] private SliderGroupController _maxValueController;

        [SerializeField] private Button _tailButton;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;
        [SerializeField] private Button _mainButton;


        private ConfigModel _configModel;
        private ConfigRange _targetRange;

        private Targets _target;

        public enum Targets
        {
            Tail,
            Right,
            Left,
            Main
        }

        public void Awake()
        {
            _equilibriumController.SendPressed += SendButton_Clicked;
            _minValueController.SendPressed += SendButton_Clicked;
            _maxValueController.SendPressed += SendButton_Clicked;
            _configModel = ConfigurationManager.Instance.GetConfig();
            StreamManager.StreamManager.Instance.DataCallback += Instance_DataCallback;
            StreamManager.StreamManager.Instance.SyncByte = _configModel.SyncNumber;

            UpdateTo(_configModel.Tail, Targets.Tail);
            
            _leftButton.onClick.AddListener(() =>
            {
                UpdateTo(_configModel.LeftWing,Targets.Left);
            });

            _rightButton.onClick.AddListener(() =>
            {
                UpdateTo(_configModel.RightWing, Targets.Right);
            });

            _mainButton.onClick.AddListener(() =>
            {
                UpdateTo(_configModel.MainEngine, Targets.Main);
            });

            _tailButton.onClick.AddListener(() =>
            {
                UpdateTo(_configModel.Tail, Targets.Tail);
            });
        }

        private void Instance_DataCallback(byte[] obj)
        {
            ControlPacket controlPacket = ControlPacket.FromBytes(obj);
            if (controlPacket.Type == ControlPacket.MessageTypes.EquilibriumRetrieve)
            {
                var model = ControlPacket.ToConfig(controlPacket);
                _configModel = model;
                ConfigurationManager.Instance.SaveConfig(_configModel);
            }
        }

        private void UpdateTo(ConfigRange configModelTail, Targets target)
        {
            _targetRange = configModelTail;
            _equilibriumController.SetValue(_targetRange.equillibrium_position);
            _minValueController.SetValue(_targetRange.min_position);
            _maxValueController.SetValue(_targetRange.max_position);
            _target = target;
        }

        private void SendButton_Clicked(byte data)
        {
            ControlPacket packet = ControlPacket.Packet();
            packet.Tail(_configModel.Tail.equillibrium_position);
            packet.Left(_configModel.LeftWing.equillibrium_position);
            packet.Right(_configModel.RightWing.equillibrium_position);
            packet.Main(_configModel.MainEngine.equillibrium_position);
            packet.MessageType(ControlPacket.MessageTypes.ControlData);
            switch (_target)
            {
                case Targets.Tail:
                    packet.Tail(data);
                    break;
                case Targets.Right:
                    packet.Right(data);
                    break;
                case Targets.Left:
                    packet.Left(data);
                    break;
                case Targets.Main:
                    packet.Main(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            StreamManager.StreamManager.Instance.Write(packet);
        }

        public void Save()
        {
            _targetRange.equillibrium_position = _equilibriumController.Value;
            _targetRange.max_position = _maxValueController.Value;
            _targetRange.min_position = _minValueController.Value;
            ConfigurationManager.Instance.SaveConfig(_configModel);
        }

        public void Synchronize()
        {
            var syncNumber = Random.Range(0, 256);

            _configModel.SyncNumber = (byte) syncNumber;
            ConfigurationManager.Instance.SaveConfig(_configModel);
            StreamManager.StreamManager.Instance.Write(
                ControlPacket.Packet()
                    .MessageType(ControlPacket.MessageTypes.Synchronize)
                    .SyncByte((byte)syncNumber));
            StreamManager.StreamManager.Instance.SyncByte = (byte) syncNumber;
        }

        public void RetrieveData()
        {
            StreamManager.StreamManager.Instance.Write(
                ControlPacket.Packet()
                    .MessageType(ControlPacket.MessageTypes.EquilibriumRetrieve));
        }

        public void SendData()
        {
            StreamManager.StreamManager.Instance.Write(
                ControlPacket.FromConfig(_configModel));
        }

        public void BackToMain()
        {
            SceneManager.LoadScene("Main");
        }

        private void OnDestroy()
        {
            _equilibriumController.SendPressed -= SendButton_Clicked;
            _minValueController.SendPressed -= SendButton_Clicked;
            _maxValueController.SendPressed -= SendButton_Clicked;
            _leftButton.onClick.RemoveAllListeners();
            _rightButton.onClick.RemoveAllListeners();
            _tailButton.onClick.RemoveAllListeners();
            _mainButton.onClick.RemoveAllListeners();
            StreamManager.StreamManager.Instance.DataCallback -= Instance_DataCallback;
        }
    }

}
