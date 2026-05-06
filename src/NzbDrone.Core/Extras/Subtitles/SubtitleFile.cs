using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public Language Language { get; set; }
        public string LanguageTags { get; set; }
        public bool AiTranslated { get; set; }
        public bool MachineTranslated { get; set; }
    }
}
