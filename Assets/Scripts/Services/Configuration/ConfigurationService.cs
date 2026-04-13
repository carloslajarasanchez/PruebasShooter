using System.Collections.Generic;

public class ConfigurationService : IConfigurationService
{
    private Dictionary<string, string> _keyValuePairs = new Dictionary<string, string>();

    public ConfigurationService()
    {
        // Lenguage por defecto
        _keyValuePairs.TryAdd("language", "es");
    }

    public string Get(string key, string _default= "")
    {
        if(this._keyValuePairs.TryGetValue(key, out var value))
            return value;
                
        return _default;       
    }
}
