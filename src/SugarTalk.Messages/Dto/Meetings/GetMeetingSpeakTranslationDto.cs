using System.Collections.Generic;
using SugarTalk.Messages.Dto.Meetings.Speak;

namespace SugarTalk.Messages.Dto.Meetings;

public class GetMeetingSpeakTranslationDto
{
    public List<MeetingSpeakDetailDto> MeetingSpeakDetail { get; set; }

    public List<MeetingSpeakTranslationDetailDto> MeetingSpeakTranslationDetail { get; set; }
}