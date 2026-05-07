using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.AutoTagging.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Http.REST.Attributes;
using NzbDrone.SignalR;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.AutoTagging
{
    [V1ApiController]
    public class AutoTagController : RestControllerWithSignalR<AutoTagResource, AutoTag>,
                                     IHandle<AutoTagAddedEvent>,
                                     IHandle<AutoTagUpdatedEvent>,
                                     IHandle<AutoTagDeletedEvent>
    {
        private readonly IAutoTagService _autoTagService;
        private readonly List<IAutoTagSpecification> _specifications;

        public AutoTagController(IBroadcastSignalRMessage signalRBroadcaster,
                                 IAutoTagService autoTagService,
                                 List<IAutoTagSpecification> specifications)
            : base(signalRBroadcaster)
        {
            _autoTagService = autoTagService;
            _specifications = specifications;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
        }

        protected override AutoTagResource GetResourceById(int id)
        {
            return _autoTagService.GetAutoTag(id).ToResource();
        }

        [HttpGet]
        public List<AutoTagResource> GetAll()
        {
            return _autoTagService.All().ToResource();
        }

        [HttpGet("schema")]
        public List<AutoTagSpecificationSchema> GetSchema()
        {
            return _specifications.OrderBy(x => x.Order).Select(x => x.ToSchema()).ToList();
        }

        [RestPostById]
        public ActionResult<AutoTagResource> Create(AutoTagResource resource)
        {
            var model = resource.ToModel(_specifications);
            ValidateSpecifications(model);
            return Created(_autoTagService.Add(model).Id);
        }

        [RestPutById]
        public ActionResult<AutoTagResource> Update(AutoTagResource resource)
        {
            var model = resource.ToModel(_specifications);
            ValidateSpecifications(model);
            _autoTagService.Update(model);
            return Accepted(resource.Id);
        }

        [RestDeleteById]
        public void DeleteAutoTag(int id)
        {
            _autoTagService.Delete(id);
        }

        [NonAction]
        public void Handle(AutoTagAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Created, message.AutoTag.ToResource());
        }

        [NonAction]
        public void Handle(AutoTagUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.AutoTag.ToResource());
        }

        [NonAction]
        public void Handle(AutoTagDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.AutoTag.ToResource());
        }

        private void ValidateSpecifications(AutoTag autoTag)
        {
            foreach (var spec in autoTag.Specifications)
            {
                var result = new NzbDroneValidationResult(spec.Validate().Errors);
                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }
            }
        }
    }
}
