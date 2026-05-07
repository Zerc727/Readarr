using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.AutoTagging.Events;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.AutoTagging
{
    public interface IAutoTagService
    {
        AutoTag GetAutoTag(int id);
        List<AutoTag> All();
        AutoTag Add(AutoTag autoTag);
        AutoTag Update(AutoTag autoTag);
        void Delete(int id);
        void TagAuthor(Author author);
    }

    public class AutoTagService : IAutoTagService,
                                  IHandle<AuthorAddedEvent>,
                                  IHandle<AuthorRefreshCompleteEvent>
    {
        private readonly IAutoTagRepository _repo;
        private readonly IAuthorService _authorService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public AutoTagService(IAutoTagRepository repo,
                              IAuthorService authorService,
                              IEventAggregator eventAggregator,
                              Logger logger)
        {
            _repo = repo;
            _authorService = authorService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public AutoTag GetAutoTag(int id)
        {
            return _repo.Get(id);
        }

        public List<AutoTag> All()
        {
            return _repo.All().OrderBy(a => a.Name).ToList();
        }

        public AutoTag Add(AutoTag autoTag)
        {
            _repo.Insert(autoTag);
            _eventAggregator.PublishEvent(new AutoTagAddedEvent(autoTag));
            return autoTag;
        }

        public AutoTag Update(AutoTag autoTag)
        {
            _repo.Update(autoTag);
            _eventAggregator.PublishEvent(new AutoTagUpdatedEvent(autoTag));
            return autoTag;
        }

        public void Delete(int id)
        {
            var autoTag = _repo.Get(id);
            _repo.Delete(id);
            _eventAggregator.PublishEvent(new AutoTagDeletedEvent(autoTag));
        }

        public void TagAuthor(Author author)
        {
            var autoTags = _repo.All();

            if (!autoTags.Any())
            {
                return;
            }

            var tagsToAdd = new HashSet<int>();
            var tagsToRemove = new HashSet<int>();

            foreach (var autoTag in autoTags)
            {
                var allRequired = autoTag.Specifications.Where(s => s.Required).ToList();
                var anyRequired = autoTag.Specifications.Where(s => !s.Required).ToList();

                var allRequiredSatisfied = !allRequired.Any() || allRequired.All(s => s.IsSatisfiedBy(author));
                var anyRequiredSatisfied = !anyRequired.Any() || anyRequired.Any(s => s.IsSatisfiedBy(author));
                var matches = allRequiredSatisfied && anyRequiredSatisfied;

                if (matches)
                {
                    foreach (var tagId in autoTag.Tags)
                    {
                        tagsToAdd.Add(tagId);
                    }
                }
                else if (autoTag.RemoveTags)
                {
                    foreach (var tagId in autoTag.Tags)
                    {
                        if (!tagsToAdd.Contains(tagId))
                        {
                            tagsToRemove.Add(tagId);
                        }
                    }
                }
            }

            var newTags = new HashSet<int>(author.Tags);
            newTags.UnionWith(tagsToAdd);
            newTags.ExceptWith(tagsToRemove);

            if (!newTags.SetEquals(author.Tags))
            {
                _logger.Debug("AutoTagging updated tags for author '{0}': added [{1}] removed [{2}]",
                    author.Name,
                    string.Join(", ", tagsToAdd),
                    string.Join(", ", tagsToRemove));

                _authorService.UpdateTags(author.Id, newTags);
            }
        }

        public void Handle(AuthorAddedEvent message)
        {
            TagAuthor(message.Author);
        }

        public void Handle(AuthorRefreshCompleteEvent message)
        {
            TagAuthor(message.Author);
        }
    }
}
