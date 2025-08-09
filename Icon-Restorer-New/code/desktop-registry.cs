using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace IconsRestorer.Code
{
    internal class DesktopRegistry
    {
        private const string KeyName = @"Software\Microsoft\Windows\Shell\Bags\1\Desktop";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            IgnoreNullValues = true
        };

        public IDictionary<string, string> GetRegistryValues()
        {
            using (var registry = Registry.CurrentUser.OpenSubKey(KeyName))
            {
                return registry.GetValueNames().ToDictionary(n => n, n => GetValue(registry, n));
            }
        }

        private string GetValue(RegistryKey registry, string valueName)
        {
            var value = registry.GetValue(valueName);
            if (value == null)
            {
                return string.Empty;
            }

            try
            {
                return JsonSerializer.Serialize(value, _jsonOptions);
            }
            catch (NotSupportedException)
            {
                // Fallback for types that can't be serialized to JSON
                return value.ToString();
            }
        }

        public void SetRegistryValues(IDictionary<string, string> values)
        {
            using (var registry = Registry.CurrentUser.OpenSubKey(KeyName, true))
            {
                foreach (var item in values)
                {
                    registry.SetValue(item.Key, GetValue(item.Value));
                }
            }
        }

        private object GetValue(string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            try
            {
                // First try to deserialize as JSON
                return JsonSerializer.Deserialize<object>(stringValue, _jsonOptions);
            }
            catch (JsonException)
            {
                // If JSON deserialization fails, return the string as-is
                return stringValue;
            }
        }
    }
}
