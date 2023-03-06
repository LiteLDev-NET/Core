namespace LiteLoader.NET.PluginSystem;

public interface IPluginInitializer
{
    public Version Version { get => new(0); }

    public string Introduction { get => string.Empty; }

    public (string, string)[]? MetaData { get => null; }

    public void OnInitialize();
}
