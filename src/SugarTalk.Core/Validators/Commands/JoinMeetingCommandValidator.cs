using FluentValidation;
using SugarTalk.Core.Middlewares.FluentMessageValidator;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Validators.Commands;

public class JoinMeetingCommandValidator : FluentMessageValidator<JoinMeetingCommand>
{
    public JoinMeetingCommandValidator()
    {
        RuleFor(x => x.StreamId).NotNull();
        RuleFor(x => x.MeetingNumber).NotNull();
    }
}
