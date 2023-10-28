namespace ApiRoutes.Generator;

public struct GeneratorContext
{
    private Action<string, string> _addSource;
    private string _assemblyName;
    private bool _isNet8;
    private IGeneratorResult _result;

    private StringCache _stringCache;

    public GeneratorContext(Action<string, string> addSource, string assemblyName, bool isNet8, IGeneratorResult result)
    {
        _addSource = addSource;
        _assemblyName = assemblyName;
        _isNet8 = isNet8;
        _result = result;
        _stringCache = new StringCache();
    }

    public Action<string, string> AddSource => _addSource;

    public string AssemblyName => _assemblyName;

    public bool IsNet8 => _isNet8;

    public IGeneratorResult Result => _result;

    public StringCache StringCache => _stringCache;
}