namespace GalacticWasteManagement.Scripts.EmbeddedScripts
{

    public class EmbeddedScript : ScriptBase
    {
        private ResourceFile _resourceFile;
        private IScriptType _type;
        private string _cachedContent;

        public EmbeddedScript(ResourceFile resourceFile, IScriptType type)
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

        public override IScriptType Type => _type;
    }
}
