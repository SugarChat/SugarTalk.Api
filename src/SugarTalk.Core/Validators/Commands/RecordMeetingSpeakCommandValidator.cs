using FluentValidation;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Core.Middlewares.FluentMessageValidator;

namespace SugarTalk.Core.Validators.Commands;

public class RecordMeetingSpeakCommandValidator : FluentMessageValidator<RecordMeetingSpeakCommand>
{
    public RecordMeetingSpeakCommandValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.RoomNumber).NotEmpty();
        RuleFor(x => x.MeetingRecordId).NotEmpty();

        When(x => x.Id.HasValue, () =>
        {
            RuleFor(x => x.SpeakEndTime).NotNull();
        });
        
        When(x => !x.Id.HasValue, () =>
        {
            RuleFor(x => x.SpeakStartTime).NotNull();
        });
    }
}