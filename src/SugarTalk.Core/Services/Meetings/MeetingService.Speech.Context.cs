using System;
using System.Collections.Generic;

namespace SugarTalk.Core.Services.Meetings;

public partial class MeetingService
{
    public static string GenerateContextId(string currentUserName, string meetingId)
    {
        var contextId = $"context_speech_history_{currentUserName}_{meetingId}";
        
        return contextId;
    }

    public class MeetingSpeechContext
    {
        public MeetingSpeechContext(string contextId)
        {
            ContextId = contextId;
        }

        public string ContextId { get; set; }

        public List<SpeechWithVoiceRecord> PreviousSpeechs { get; set; } = new();
    }
    
    public class SpeechWithVoiceRecord
    {
        public Guid SpeechId { get; set; }
        
        public Guid VoiceRecordId { get; set; }
    }
}