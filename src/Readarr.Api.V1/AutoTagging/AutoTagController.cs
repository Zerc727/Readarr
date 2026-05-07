using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.AutoTagging.Events;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
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

        public AutoTagController(IBroadcastSignalRMessage signalRBroadcaster,
                                 IAutoTagService autoTagService)
            : base(signalRBroadcaster)
        {
            _autoTagService = autoTagService;

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

        [RestPostById]
        public ActionResult<AutoTagResource> Create(AutoTagResource resource)
        {
            return Created(_autoTagService.Add(resource.ToModel()).Id);
        }

        [RestPutById]
        public ActionResult<AutoTagResource> Update(AutoTagResource resource)
        {
            _autoTagService.Update(resource.ToModel());
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
    }
}
