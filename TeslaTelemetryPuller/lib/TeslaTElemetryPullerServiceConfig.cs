using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeslaTelemetryPuller
{
    public class TeslaTelemetryPullerServiceConfig 
    {
        private Dictionary<string, string> _config;

        public TeslaTelemetryPullerServiceConfig()
        {
            _config = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                if (_config.ContainsKey(key))
                {
                    return _config[key];
                }
                return null;
            }
        }

        public void AddConfig(string key, string value)
        {
            _config.Add(key, value);
        }
    }
}
