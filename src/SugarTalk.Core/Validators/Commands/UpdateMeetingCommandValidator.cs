using FluentValidation;
using SugarTalk.Core.Middlewares.FluentMessageValidator;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Validators.Commands;

public class UpdateMeetingCommandValidator : FluentMessageValidator<UpdateMeetingCommand>
{
    public UpdateMeetingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.TimeZone).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}