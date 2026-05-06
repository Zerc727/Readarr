using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Extras.Subtitles
{
    public interface ISubtitleFileService : IExtraFileService<SubtitleFile>
    {
    }

    public class SubtitleFileService : ExtraFileService<SubtitleFile>, ISubtitleFileService
    {
        public SubtitleFileService(IExtraFileRepository<SubtitleFile> repository,
                                   IAuthorService authorService,
                                   IDiskProvider diskProvider,
                                   IRecycleBinProvider recycleBinProvider,
                                   Logger logger)
            : base(repository, authorService, diskProvider, recycleBinProvider, logger)
        {
        }
    }
}
