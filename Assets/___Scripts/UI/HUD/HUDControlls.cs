using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using DG.Tweening;
using PlaneTK.StreamManager.DataPacket;
using UnityEngine;
using UnityEngine.UI;

namespace PlaneTK.HUD
{
    public class HUDControlls : MonoBehaviour
    {
        [SerializeField] private Image _planeFront;

        [SerializeField] private Image _planeSide;

        [SerializeField] private Text _altitude;

        [SerializeField] private Text _temperature;

        [SerializeField] private Image _temperatureImage;

        [SerializeField] private Text _longitude;

        [SerializeField] private Text _latitude;

        [SerializeField] private GameObject _planeModel;

        [SerializeField] private RawImage _webCamImage;


        private PlaneSensorData _lastData;

        private Vector3 _rotationDelta;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _rotationDelta = _lastData.Rotation;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                _planeModel.SetActive(!_planeModel.activeSelf);
            }
        }

        public void UpdateSensorData(PlaneSensorData data)
        {
            _lastData = data;
            _altitude.text = $"Altitude: {data.Altitude}m";
            _temperature.text = $"Temp: {data.Temperature} C";
            if (data.HasGPS)
            {
                _longitude.text = $"Long: {data.Longitude}";
                _latitude.text = $"Lat: {data.Latitude}";
            }

            var relativePosition = Mathf.Clamp01(data.Temperature / 50f);
            _temperatureImage.fillAmount = relativePosition;

            data.Rotation = data.Rotation - _rotationDelta;

            _planeFront.rectTransform.DORotate(new Vector3(0, 0, data.Rotation.x), 0.2f);
            _planeSide.rectTransform.DORotate(new Vector3(0,0, data.Rotation.z), 0.2f);

            _planeModel.transform.DORotate(data.Rotation, 0.2f);
        }
    }
}

