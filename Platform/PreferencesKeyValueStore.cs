using Cali.Core.Abstractions;
using Microsoft.Maui.Storage;

namespace Cali.Platform;

public sealed class PreferencesKeyValueStore : IKeyValueStore
{
    public bool GetBool(string k, bool d) => Preferences.Get(k, d);
    public void SetBool(string k, bool v) => Preferences.Set(k, v);
    public int GetInt(string k, int d) => Preferences.Get(k, d);
    public void SetInt(string k, int v) => Preferences.Set(k, v);
    public string GetString(string k, string d) => Preferences.Get(k, d);
    public void SetString(string k, string v) => Preferences.Set(k, v);
    public bool Contains(string k) => Preferences.ContainsKey(k);
}
