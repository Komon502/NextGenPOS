using System;
using System.Collections.Generic;
using System.IO;

namespace NextGenPOS.Services
{
    /// <summary>
    /// Reads key=value pairs from settings.txt (Notepad-editable config).
    /// Lines starting with # are comments. Blank lines are ignored.
    /// </summary>
    public class ConfigService
    {
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly string _filePath;

        public ConfigService(string filePath)
        {
            _filePath = filePath;
            Reload();
        }

        public void Reload()
        {
            _settings.Clear();
            if (!File.Exists(_filePath)) return;

            foreach (var line in File.ReadAllLines(_filePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                int idx = trimmed.IndexOf('=');
                if (idx <= 0) continue;

                string key = trimmed.Substring(0, idx).Trim();
                string val = trimmed.Substring(idx + 1).Trim();
                _settings[key] = val;
            }
        }

        public string Get(string key, string defaultValue = "")
        {
            return _settings.ContainsKey(key) ? _settings[key] : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (!_settings.ContainsKey(key)) return defaultValue;
            return _settings[key].Equals("true", StringComparison.OrdinalIgnoreCase)
                || _settings[key] == "1";
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (!_settings.ContainsKey(key)) return defaultValue;
            return int.TryParse(_settings[key], out int v) ? v : defaultValue;
        }

        public decimal GetDecimal(string key, decimal defaultValue = 0m)
        {
            if (!_settings.ContainsKey(key)) return defaultValue;
            return decimal.TryParse(_settings[key], out decimal v) ? v : defaultValue;
        }
    }
}
