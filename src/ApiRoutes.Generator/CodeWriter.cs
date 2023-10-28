namespace ApiRoutes.Generator;

using System.Text;

public class CodeWriter
{
	public struct Settings
	{
		public string StartBracket { get; set; }
		public string EndBracket { get; set; }
	}
	
	private class ScopeTracker : IDisposable
	{
		public ScopeTracker(Action<Settings> onDispose, Settings settings)
		{
			OnDispose = onDispose;
			Settings = settings;
		}
		public Action<Settings> OnDispose { get; }
		public Settings Settings { get; }

		public void Dispose()
		{
			OnDispose.Invoke(Settings);
		}
	}
	
	private StringBuilder _content { get; } = new();
	private int _indentLevel { get; set; }
	
	public void Append(string line) => _content.Append(line);

	public void AppendLine(string line) => _content.Append(new string('\t', _indentLevel)).AppendLine(line);
	public void AppendLine() => _content.AppendLine();

	private readonly Settings _default = new()
	{
		StartBracket = "{",
		EndBracket = "}"
	};
	
	public IDisposable BeginScope(string line)
	{
		AppendLine(line);
		return BeginScope(_default);
	}
	
	public IDisposable BeginScope(string line, Settings settings)
	{
		AppendLine(line);
		return BeginScope(settings);
	}

	public IDisposable BeginScope(Settings settings)
	{
		_content.Append(new string('\t', _indentLevel)).AppendLine(settings.StartBracket);
		
		_indentLevel += 1;
		return new ScopeTracker(EndScope, settings);
	}

	public void EndScope(Settings settings)
	{
		_indentLevel -= 1;
		_content.Append(new string('\t', _indentLevel)).AppendLine(settings.EndBracket);
	}

	public override string ToString() => _content.ToString();
}
