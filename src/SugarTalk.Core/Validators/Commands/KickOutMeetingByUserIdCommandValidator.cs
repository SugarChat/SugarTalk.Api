using FluentValidation;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Core.Middlewares.FluentMessageValidator;

namespace SugarTalk.Core.Validators.Commands
{
    public class KickOutMeetingByUserIdCommandValidator : FluentMessageValidator<KickOutMeetingByUserIdCommand>
    {
        public KickOutMeetingByUserIdCommandValidator()
        {
            RuleFor(x => x.MeetingId).NotNull();
            RuleFor(x => x.KickOutUserId).NotNull();
        }
    }
}
