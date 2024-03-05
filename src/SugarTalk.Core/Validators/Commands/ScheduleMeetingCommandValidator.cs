using FluentValidation;
using SugarTalk.Core.Middlewares.FluentMessageValidator;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Validators.Commands;

public class ScheduleMeetingCommandValidator : FluentMessageValidator<ScheduleMeetingCommand>
{
    public ScheduleMeetingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x=>x.TimeZone).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
        When(x => x.UtilDate.HasValue, () =>
        {
            RuleFor(x => x.RepeatType).NotEqual(MeetingRepeatType.None);
            RuleFor(x => x.UtilDate).GreaterThan(x => x.EndDate);
        });
    }
}