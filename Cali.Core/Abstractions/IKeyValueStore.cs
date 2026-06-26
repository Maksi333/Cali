namespace Cali.Core.Abstractions;

public interface IKeyValueStore
{
    bool GetBool(string key, bool def);
    void SetBool(string key, bool value);
    int GetInt(string key, int def);
    void SetInt(string key, int value);
    string GetString(string key, string def);
    void SetString(string key, string value);
    bool Contains(string key);
}
