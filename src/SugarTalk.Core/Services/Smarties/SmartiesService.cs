using System;
using Serilog;
using System.Linq;
using System.Text;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Services.Aws;
using SugarTalk.Core.Services.Http;
using SugarTalk.Core.Domain.Meeting;
using Microsoft.IdentityModel.Tokens;
using SugarTalk.Core.Services.Ffmpeg;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Dto.SpeechMatics;
using SugarTalk.Messages.Commands.SpeechMatics;

namespace SugarTalk.Core.Services.Smarties;

public interface ISmartiesService : IScopedDependency
{
    Task HandleTranscriptionCallbackAsync(TranscriptionCallBackCommand command, CancellationToken cancellationToken);
}

public class SmartiesService : ISmartiesService
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IFfmpegService _ffmpegService;
    private readonly IOpenAiService _openAiService;
    private readonly IMeetingDataProvider _meetingDataProvider;
    private readonly ISmartiesDataProvider _smartiesDataProvider;
    private readonly ISugarTalkHttpClientFactory _sugarTalkHttpClientFactory;

    public SmartiesService(IFfmpegService ffmpegService, IOpenAiService openAiService, IAwsS3Service awsS3Service, IMeetingDataProvider meetingDataProvider, ISmartiesDataProvider smartiesDataProvider, ISugarTalkHttpClientFactory sugarTalkHttpClientFactory)
    {
        _awsS3Service = awsS3Service;
        _ffmpegService = ffmpegService;
        _openAiService = openAiService;
        _meetingDataProvider = meetingDataProvider;
        _smartiesDataProvider = smartiesDataProvider;
        _sugarTalkHttpClientFactory = sugarTalkHttpClientFactory;
    }

    public async Task HandleTranscriptionCallbackAsync(TranscriptionCallBackCommand command, CancellationToken cancellationToken)
    {
        Log.Information("Sugartalk speech matics call back: {@comman}", command);
        
        if (command == null || command.Results.IsNullOrEmpty() || command.Job == null || command.Job.Id.IsNullOrEmpty()) return;
        
        var originalSpeakInfos = StructureDiarizationResults(command.Results);
        
        Log.Information("SpeechMatics callback originalSpeakInfos: {@originalSpeakInfos}", originalSpeakInfos);
        
        var speechMaticsRecord = await _smartiesDataProvider.GetSpeechMaticsRecordAsync(command.Job.Id, cancellationToken).ConfigureAwait(false);
        
        Log.Information("SpeechMatics record: {@speechMaticsRecord}", speechMaticsRecord);
        
        var originalSpeakDetails = await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
            meetingNumber: speechMaticsRecord.MeetingNumber, recordId: speechMaticsRecord.MeetingRecordId, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("SpeechMatics originalSpeakDetails: {@originalSpeakDetails}", originalSpeakDetails);
        
        var meetingRecord = (await _meetingDataProvider.GetMeetingRecordsAsync(speechMaticsRecord.MeetingRecordId, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        Log.Information("SpeechMatics meetingRecord: {@meetingRecord}", meetingRecord);
        
        var speakDetails = new List<MeetingSpeakDetail>();
        
        foreach (var speakInfo in originalSpeakInfos)
        {
            var speakDetail = originalSpeakDetails.OrderBy(x => Math.Abs(x.SpeakStartTime - speakInfo.StartTime)).FirstOrDefault();

            var replaceSameSpeaker = originalSpeakInfos.Where(x => x.Speaker.Equals(speakInfo.Speaker)).Select(x =>
            {
                if (speakDetail == null) return null;
                
                x.Speaker = speakDetail?.Username;
                
                return new MeetingSpeakDetail
                {
                    UserId = speakDetail.UserId,
                    TrackId = speakDetail.TrackId,
                    Username = speakDetail.Username,
                    CreatedDate = speakDetail.CreatedDate,
                    SpeakStatus = speakDetail.SpeakStatus,
                    SpeakEndTime = Convert.ToInt64(x.EndTime),
                    MeetingNumber = speakDetail.MeetingNumber,
                    MeetingRecordId = speakDetail.MeetingRecordId,
                    SpeakStartTime = Convert.ToInt64(x.StartTime),
                    FileTranscriptionStatus = speakDetail.FileTranscriptionStatus,
                };
            }).ToList();
            
            Log.Information("SpeechMatics replaceSameSpeaker: {@replaceSameSpeaker}", replaceSameSpeaker);
            
            speakDetails.AddRange(replaceSameSpeaker);
            
            if (speakDetails.Count == originalSpeakInfos.Count)
                break;
        }

        Log.Information("New speak details: {@speakDetails}", speakDetails);
        
        var recordUrl = await _awsS3Service.GeneratePresignedUrlAsync(meetingRecord?.Url ?? "", 30).ConfigureAwait(false);
        
        var audioContent = await _sugarTalkHttpClientFactory.GetAsync<byte[]>(recordUrl, cancellationToken).ConfigureAwait(false);
        
        speakDetails = await TranscriptionSpeakInfoAsync(speakDetails, audioContent, cancellationToken).ConfigureAwait(false);
        
        Log.Information("Transcription new speak details: {@speakDetails}", speakDetails);
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailsAsync(speakDetails, true, cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<MeetingSpeakDetail>> TranscriptionSpeakInfoAsync(
        List<MeetingSpeakDetail> originalSpeakDetails, byte[] audioContent, CancellationToken cancellationToken)
    {
        foreach (var speakDetail in originalSpeakDetails)
        {
            Log.Information("Start time of speak in video: {SpeakStartTimeVideo}, End time of speak in video: {SpeakEndTimeVideo}", speakDetail.SpeakStartTime * 1000, speakDetail.SpeakEndTime * 1000);

            try
            {
                if (speakDetail.SpeakStartTime != 0 && speakDetail.SpeakEndTime != 0)
                    speakDetail.OriginalContent = await SplitAudioAsync(
                        audioContent, Convert.ToInt64(speakDetail.SpeakStartTime * 1000), Convert.ToInt64(speakDetail.SpeakEndTime * 1000),
                        TranscriptionFileType.Wav, cancellationToken).ConfigureAwait(false);
                else
                    speakDetail.OriginalContent = "";
            }
            catch (Exception ex)
            {
                Log.Information("transcription error: {ErrorMessage}", ex.Message);
            }
        }
        
        return originalSpeakDetails;
    }
    
    public async Task<string> SplitAudioAsync(byte[] file , long speakStartTimeVideo, long speakEndTimeVideo, TranscriptionFileType fileType = TranscriptionFileType.Wav, CancellationToken cancellationToken = default)
    {
        if (file == null) return null;
        
        var audioBytes = await _ffmpegService.ConvertFileFormatAsync(file, fileType, cancellationToken).ConfigureAwait(false);
    
        var splitAudios = await _ffmpegService.SpiltAudioAsync(audioBytes, speakStartTimeVideo, speakEndTimeVideo, cancellationToken).ConfigureAwait(false);
        
        var transcriptionResult = new StringBuilder();
        
        foreach (var reSplitAudio in splitAudios)
        {
            var transcriptionResponse = await _openAiService.TranscriptionAsync(
                reSplitAudio, TranscriptionLanguage.Chinese, speakStartTimeVideo, speakEndTimeVideo,
                TranscriptionFileType.Mp3, TranscriptionResponseFormat.Text,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            
            transcriptionResult.Append(transcriptionResponse);
        }
        
        Log.Information("Transcription result {Transcription}", transcriptionResult.ToString());
        
        return transcriptionResult.ToString();
    }

    private List<SpeechMaticsSpeakInfoDto> StructureDiarizationResults(List<SpeechMaticsResultDto> results)
    {
        string currentSpeaker = null;
        var startTime = 0.0;
        var endTime = 0.0;
        var speakInfos = new List<SpeechMaticsSpeakInfoDto>();

        foreach (var result in results.Where(result => !result.Alternatives.IsNullOrEmpty()))
        {
            if (currentSpeaker == null)
            {
                currentSpeaker = result.Alternatives[0].Speaker;
                startTime = result.StartTime;
                endTime = result.EndTime;
                continue;
            }

            if (result.Alternatives[0].Speaker.Equals(currentSpeaker))
            {
                endTime = result.EndTime;
            }
            else
            {
                speakInfos.Add(new SpeechMaticsSpeakInfoDto { EndTime = endTime, StartTime = startTime, Speaker = currentSpeaker });
                currentSpeaker = result.Alternatives[0].Speaker;
                startTime = result.StartTime;
                endTime = result.EndTime;
            }
        }

        speakInfos.Add(new SpeechMaticsSpeakInfoDto { EndTime = endTime, StartTime = startTime, Speaker = currentSpeaker });

        Log.Information("Structure diarization results : {@speakInfos}", speakInfos);
        
        return speakInfos;
    }
}