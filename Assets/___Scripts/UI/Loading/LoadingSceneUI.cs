using System;
using System.Collections;
using System.Collections.Generic;
using PlaneTK.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PlaneTK.UI.Loading
{
    public class LoadingSceneUI : MonoBehaviour
    {
        [SerializeField] private InputField _portNumber;


        private void Awake()
        {
            _portNumber.text = ConfigurationManager.Instance.LastPort;
        }

        public void Init()
        {
            StreamManager.StreamManager.Instance.Init(_portNumber.text, () =>
            {
                ConfigurationManager.Instance.LastPort = _portNumber.text;
                SceneManager.LoadScene("Config");
            }, (e) =>
            {
                Debug.LogError("Did not connect");
            });
        }
    }
}
