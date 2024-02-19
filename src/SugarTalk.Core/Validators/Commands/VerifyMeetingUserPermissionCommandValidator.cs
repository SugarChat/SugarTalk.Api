using FluentValidation;
using SugarTalk.Core.Middlewares.FluentMessageValidator;
using SugarTalk.Messages.Commands.Meetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Core.Validators.Commands
{
    public class VerifyMeetingUserPermissionCommandValidator : FluentMessageValidator<VerifyMeetingUserPermissionCommand>
    {
        public VerifyMeetingUserPermissionCommandValidator()
        {
            RuleFor(x => x.MeetingId).NotNull();
            RuleFor(x => x.UserId).NotNull();
        }
    }
}
