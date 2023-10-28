using System.Collections;

namespace ApiRoutes.Generator;

public class StringCache : IEnumerable<KeyValuePair<string, string>>
{
    private readonly IDictionary<string, string> _dictionary = new Dictionary<string, string>();

    public string Add(string key, string value)
    {
        var fixedKey = key.Replace(".", "_").Replace(":", "_").Replace("-", "_");

        if (!_dictionary.ContainsKey(fixedKey))
        {
            _dictionary.Add(fixedKey, value);
        }
        
        return $"StringCache.{fixedKey}";
    }

    IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_dictionary.Values).GetEnumerator();
    }
}