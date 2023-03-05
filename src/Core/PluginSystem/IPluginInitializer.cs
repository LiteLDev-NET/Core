namespace LiteLoader.NET.PluginSystem
{
    public interface IPluginInitializer
    {
        public Version Version { get; }

        public string Introduction { get; }

        public (string, string)[] MetaData { get; }

        public void OnInitialize();
    }
}
