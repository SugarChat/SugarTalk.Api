using FluentValidation;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Core.Middlewares.FluentMessageValidator;

namespace SugarTalk.Core.Validators.Commands;

public class JoinMeetingCommandValidator : FluentMessageValidator<JoinMeetingCommand>
{
    public JoinMeetingCommandValidator()
    {
        RuleFor(x => x.MeetingNumber).NotNull();
    }
}
