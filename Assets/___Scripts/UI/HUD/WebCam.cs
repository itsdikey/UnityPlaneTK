using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PlaneTK.HUD
{
    public class WebCam : MonoBehaviour
    {

        [SerializeField] private RawImage _image;

        private string[] _names;
        private int _currentIndex;
        private WebCamTexture _camTexture;
        void Awake()
        {
            var devices = WebCamTexture.devices;
            _names = devices.Select(x => x.name).ToArray();
            ActivateCurrentWebCam();
        }

        private void ActivateCurrentWebCam()
        {
            if(_camTexture!=null)
                _camTexture.Stop();

            _camTexture = new WebCamTexture(_names[_currentIndex]);
            Debug.Log($"Active Cam is {_names[_currentIndex]}");
            _image.texture = _camTexture;
            _camTexture.Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                _currentIndex++;
                _currentIndex %= _names.Length;
                ActivateCurrentWebCam();
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                _currentIndex--;
                _currentIndex %= _names.Length;
                ActivateCurrentWebCam();
            }
        }
    }
}