using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlaneTK.UI.Config.UIItems.SliderGroup
{

    public class SliderGroupController : MonoBehaviour
    {
        [SerializeField] private Text _value;

        [SerializeField] private Slider _slider;

        
        private void Awake()
        {
            _slider.onValueChanged.AddListener(ValueChanged);
        }

        public event Action<byte> SendPressed;

        public byte Value { get; private set; }

        public void ValueChanged(float value)
        {
            Value = (byte) value;
            _value.text = Value.ToString();
        }

        public void SetValue(byte data)
        {
            _slider.SetValueWithoutNotify(data);
            ValueChanged(data);
        }

        public virtual void OnSendPressed()
        {
            SendPressed?.Invoke(Value);
        }
    }
}
