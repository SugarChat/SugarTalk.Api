using FluentValidation;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Core.Middlewares.FluentMessageValidator;

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
