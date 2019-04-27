namespace GalacticWasteManagement
{
    public abstract class ScriptType : IScriptType
    {
        public abstract bool IsJournaled { get; }
        public static readonly ScriptType RunIfChanged = new RunIfChanged();
        public static readonly ScriptType Seed = new Seed();
        public static readonly ScriptType Drop = new Drop();
        public static readonly ScriptType Initialize = new Initialize();
        public static readonly ScriptType vNext = new vNext();
        public static readonly ScriptType Migration = new Migration();
        public static readonly ScriptType Deprecated = new Deprecated();

        public string Name
        {
            get
            {
                return GetType().Name;
            }
        }
    }

    public class RunIfChanged : ScriptType
    {
        public override bool IsJournaled => true;
    }
    public class Deprecated : ScriptType
    {
        public override bool IsJournaled => true;
    }
    public class Seed : ScriptType
    {
        public override bool IsJournaled => true;
    }
    public class Drop : ScriptType
    {
        public override bool IsJournaled => false;
    }
    public class Initialize : ScriptType
    {
        public override bool IsJournaled => false;
    }
    public class vNext : ScriptType
    {
        public override bool IsJournaled => true;
    }

    public class Migration : ScriptType
    {
        public override bool IsJournaled => true;
    }
}


