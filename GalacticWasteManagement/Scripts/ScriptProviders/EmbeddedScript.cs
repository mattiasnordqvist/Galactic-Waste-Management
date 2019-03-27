namespace GalacticWasteManagement.Scripts.ScriptProviders
{

    public class EmbeddedScript : ScriptBase
    {
        private ResourceFile _resourceFile;
        private ScriptType _type;
        private string _cachedContent;
        private string _cachedHashedContent;

        public EmbeddedScript(ResourceFile resourceFile, ScriptType type)
        {
            _resourceFile = resourceFile;
            _type = type;
        }

        public override string Sql
        {
            get
            {
                if (_cachedContent == null)
                {
                    _cachedContent = _resourceFile.Read();
                }
                return _cachedContent;
            }
        }

        public override string Name => _resourceFile.ResourceKey;

        public override ScriptType Type => _type;
    }
}
