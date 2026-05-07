using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public class Pushcut : NotificationBase<PushcutSettings>
    {
        private readonly IPushcutProxy _proxy;
        private readonly Logger _logger;

        public Pushcut(IPushcutProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public override string Name => "Pushcut";
        public override string Link => "https://pushcut.io/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(BOOK_GRABBED_TITLE, grabMessage.Message, Settings);
        }

        public override void OnReleaseImport(BookDownloadMessage message)
        {
            _proxy.SendNotification(BOOK_DOWNLOADED_TITLE, message.Message, Settings);
        }

        public override void OnAuthorAdded(Author author)
        {
            _proxy.SendNotification(AUTHOR_ADDED_TITLE, author.Name, Settings);
        }

        public override void OnAuthorDelete(AuthorDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(AUTHOR_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnBookDelete(BookDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(BOOK_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnBookFileDelete(BookFileDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(BOOK_FILE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE, healthCheck.Message, Settings);
        }

        public override void OnDownloadFailure(DownloadFailedMessage message)
        {
            _proxy.SendNotification(DOWNLOAD_FAILURE_TITLE, message.Message, Settings);
        }

        public override void OnImportFailure(BookDownloadMessage message)
        {
            _proxy.SendNotification(IMPORT_FAILURE_TITLE, message.Message, Settings);
        }

        public override void OnRename(Author author, List<RenamedBookFile> renamedFiles)
        {
            _proxy.SendNotification("Books Renamed", author.Name, Settings);
        }

        public override void OnBookRetag(BookRetagMessage message)
        {
            _proxy.SendNotification(BOOK_RETAGGED_TITLE, message.Message, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(APPLICATION_UPDATE_TITLE, updateMessage.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Readarr";

                _proxy.SendNotification(title, body, Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                failures.Add(new ValidationFailure("", "Unable to send test message"));
            }

            return new ValidationResult(failures);
        }
    }
}
