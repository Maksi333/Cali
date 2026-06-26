using Cali.Core.Abstractions;

namespace Cali.Tests.Fakes;

public sealed class InMemoryKeyValueStore : IKeyValueStore
{
    private readonly Dictionary<string, object> _d = new();
    public bool GetBool(string k, bool def) => _d.TryGetValue(k, out var v) ? (bool)v : def;
    public void SetBool(string k, bool v) => _d[k] = v;
    public int GetInt(string k, int def) => _d.TryGetValue(k, out var v) ? (int)v : def;
    public void SetInt(string k, int v) => _d[k] = v;
    public string GetString(string k, string def) => _d.TryGetValue(k, out var v) ? (string)v : def;
    public void SetString(string k, string v) => _d[k] = v;
    public bool Contains(string k) => _d.ContainsKey(k);
}
