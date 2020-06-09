using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace PlaneTK.Configuration
{
    public class ConfigurationManager
    {


        private static ConfigurationManager _instance;
        public static ConfigurationManager Instance => _instance ?? (_instance = new ConfigurationManager()); 

        private const string DEFAULT_CONFIG_KEY = "DefaultConfig";

        private const string LAST_PORT_KEY = "LastPort";

        public bool HasConfig => PlayerPrefs.HasKey(DEFAULT_CONFIG_KEY);

        public ConfigModel GetConfig()
        {
            if(!HasConfig)
                return new ConfigModel();
            return JsonConvert.DeserializeObject<ConfigModel>(PlayerPrefs.GetString(DEFAULT_CONFIG_KEY));
        }

        public void SaveConfig(ConfigModel config)
        {
            PlayerPrefs.SetString(DEFAULT_CONFIG_KEY,JsonConvert.SerializeObject(config));
        }

        public string LastPort
        {
            get { return PlayerPrefs.GetString(LAST_PORT_KEY, ""); }
            set
            {
                PlayerPrefs.SetString(LAST_PORT_KEY,value);
            }
        }

    }

    public class ConfigModel
    {
        public byte SyncNumber;
        public ConfigRange LeftWing;
        public ConfigRange RightWing;
        public ConfigRange Tail;
        public ConfigRange MainEngine;

        public ConfigModel()
        {
            LeftWing = new ConfigRange();
            RightWing = new ConfigRange();
            Tail = new ConfigRange();
            MainEngine = new ConfigRange();
        }
    }

    public class ConfigRange
    {
        public byte equillibrium_position;
        public byte max_position;
        public byte min_position;

        public byte GetMappedValue(float min, float max, float value, bool log=false)
        {
            var newValue = value - min;
            var relativeDistance = newValue / (max - min);

            var val = (byte) ((relativeDistance) * (max_position - min_position) + min_position);
            if(log)
                Debug.Log($"{value} from {min} to {max} mapped to {max_position}, {min_position} is {val}");
            return val;
        }
    }
}

