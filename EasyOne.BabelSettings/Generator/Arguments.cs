namespace EasyOne.BabelSettings
{
    public class Arguments
    {
        public string SettingsCode { get; set; }
        public LanguageEnum Language { get; set; }
        public bool Override { get; set; }
    }

    public enum LanguageEnum
    {
        it = 0,
        en = 1,
        dt = 1 << 1,
        fr = 1 << 2,
        pt = 1 << 3
    }
}