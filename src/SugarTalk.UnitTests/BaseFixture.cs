using Aliyun.OSS;
using AutoMapper;
using NSubstitute;
using SugarTalk.Core.Data;
using SugarTalk.Core.Mapping;
using Castle.Core.Configuration;
using Google.Cloud.Translation.V2;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Services.Http;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Aliyun;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Settings.LiveKit;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.AntMediaServer;
using SugarTalk.Core.Services.Aws;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Core.Services.Ffmpeg;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Core.Services.Smarties;
using SugarTalk.Core.Settings.Aws;
using SugarTalk.Core.Settings.Meeting;
using SugarTalk.Core.Settings.Meeting.Feedback;
using SugarTalk.Core.Settings.PostBoy;
using SugarTalk.Core.Settings.Smarties;
using SugarTalk.Core.Settings.TencentCloud;

namespace SugarTalk.UnitTests;

public partial class BaseFixture
{
    protected IRepository _repository;
    protected SugarTalkDbContext _dbContext;
    protected readonly IMeetingService _meetingService;
    protected readonly IMeetingDataProvider _meetingDataProvider;
    protected readonly IClock _clock;
    protected readonly IMapper _mapper;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IFClubClient _fclubClient;
    protected readonly ICurrentUser _currentUser;
    protected readonly IOpenAiService _openAiService;
    protected readonly IConfiguration _configuration;
    protected readonly IAccountDataProvider _accountDataProvider;
    protected readonly IAntMediaServerUtilService _antMediaServerUtilService;
    protected readonly ILiveKitServerUtilService _liveKitServerUtilService;
    protected readonly LiveKitServerSetting _liveKitServerSetting;
    protected readonly AwsS3Settings _awsS3Settings;
    protected readonly IAwsS3Service _awsS3Service;
    protected readonly ISpeechClient _speechClient;
    protected readonly ILiveKitClient _liveKitClient;
    protected readonly ISugarTalkHttpClientFactory _httpClientFactory;
    protected readonly IHttpContextAccessor _contextAccessor;
    protected readonly IMeetingProcessJobService _meetingProcessJobService;
    protected readonly ISugarTalkBackgroundJobClient _backgroundJobClient;
    protected readonly IMeetingUtilService _meetingUtilService;
    protected readonly TranslationClient _translationClient;
    protected readonly ICacheManager _cacheManager;
    protected readonly ISmartiesClient _smartiesClient;
    protected readonly ISugarTalkBackgroundJobClient _sugarTalkBackgroundJobClient;
    protected readonly IAliYunOssService _aliYunOssService = Substitute.For<IAliYunOssService>();
    private readonly MeetingInfoSettings _meetingInfoSettings;
    private readonly IFfmpegService _ffmpegService;
    private readonly IPostBoyClient _postBoyClient;
    private readonly FeedbackSettings _feedbackSettings;
    private readonly SmartiesSettings _smartiesSettings;
    private readonly ISmartiesDataProvider _smartiesDataProvider;
    private readonly PostBoySettings _postBoySettings;
    private readonly TencentCloudSetting _tencentCloudSetting;
    
    public BaseFixture()
    {
        _configuration = Substitute.For<IConfiguration>();
        _dbContext = MockDbContext.GetSugarTalkDbContext();
        _repository = MockDbContext.GetRepository(_dbContext);
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpClientFactory = Substitute.For<ISugarTalkHttpClientFactory>();
        _clock = Substitute.For<IClock>();
        _mapper = CreateMapper();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUser>();
        _openAiService = Substitute.For<IOpenAiService>();
        _currentUser.Id.Returns(1);
        _cacheManager = Substitute.For<ICacheManager>();
        _smartiesClient = Substitute.For<ISmartiesClient>();
        _liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
        _sugarTalkBackgroundJobClient = Substitute.For<ISugarTalkBackgroundJobClient>();
        _accountDataProvider = MockAccountDataProvider(_mapper, _repository, _unitOfWork, _currentUser);
        _meetingDataProvider = MockMeetingDataProvider(_clock, _mapper, _repository, _unitOfWork, _currentUser, _accountDataProvider);
        _meetingService = MockMeetingService(_clock, _mapper, _unitOfWork, _currentUser, _ffmpegService, _cacheManager, _openAiService, _speechClient, _postBoyClient,
            _liveKitClient, _fclubClient, _awsS3Settings, _awsS3Service, _smartiesClient, _postBoySettings, _translationClient, _meetingUtilService,
            _meetingDataProvider, _accountDataProvider, _liveKitServerSetting, _httpClientFactory, _backgroundJobClient,
            _liveKitServerUtilService, _antMediaServerUtilService, _sugarTalkBackgroundJobClient, _meetingInfoSettings, _feedbackSettings, _smartiesSettings, _smartiesDataProvider, _tencentCloudSetting); 
        _meetingProcessJobService = MockMeetingProcessJobService(_clock, _unitOfWork, _meetingDataProvider);
    }

    protected IMeetingProcessJobService MockMeetingProcessJobService(IClock clock, IUnitOfWork unitOfWork, IMeetingDataProvider meetingDataProvider)
    {
        return new MeetingProcessJobService(clock, unitOfWork, meetingDataProvider);
    }

    protected IAccountDataProvider MockAccountDataProvider(IMapper mapper, IRepository repository, IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        return new AccountDataProvider(repository, mapper, unitOfWork, currentUser);
    }

    protected IMeetingService MockMeetingService(
        IClock clock,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IFfmpegService ffmpegService,
        ICacheManager cacheManager,
        IOpenAiService openAiService,
        ISpeechClient speechClient,
        IPostBoyClient postBoyClient,
        ILiveKitClient liveKitClient,
        IFClubClient fclubClient,
        AwsS3Settings awsS3Settings,
        IAwsS3Service awsS3Service,
        ISmartiesClient smartiesClient,
        PostBoySettings postBoySettings,
        TranslationClient translationClient,
        IMeetingUtilService meetingUtilService,
        IMeetingDataProvider meetingDataProvider,
        IAccountDataProvider accountDataProvider,
        LiveKitServerSetting liveKitServerSetting,
        ISugarTalkHttpClientFactory httpClientFactory,
        ISugarTalkBackgroundJobClient backgroundJobClient,
        ILiveKitServerUtilService liveKitServerUtilService,
        IAntMediaServerUtilService antMediaServerUtilService,
        ISugarTalkBackgroundJobClient sugarTalkBackgroundJobClient, 
        MeetingInfoSettings meetingInfoSettings,
        FeedbackSettings feedbackSettings,
        SmartiesSettings smartiesSettings,
        ISmartiesDataProvider smartiesDataProvider,
        TencentCloudSetting tencentCloudSetting)
    {
        return new MeetingService(
            clock, mapper, unitOfWork, currentUser, ffmpegService, openAiService, speechClient, postBoyClient, liveKitClient, fclubClient, awsS3Settings, awsS3Service, smartiesClient, postBoySettings, smartiesSettings,
            translationClient, meetingUtilService, meetingDataProvider, accountDataProvider, liveKitServerSetting, 
            httpClientFactory, backgroundJobClient, liveKitServerUtilService, antMediaServerUtilService, sugarTalkBackgroundJobClient, null, cacheManager, feedbackSettings, smartiesDataProvider, tencentCloudSetting);
    }
    
    protected IMeetingDataProvider MockMeetingDataProvider(
        IClock clock,
        IMapper mapper, 
        IRepository repository,
        IUnitOfWork unitOfWork,     
        ICurrentUser currentUser,
        IAccountDataProvider accountDataProvider)
    {
        return new MeetingDataProvider(clock, mapper, repository, unitOfWork, currentUser, accountDataProvider);
    }

    protected IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MeetingMapping());
            cfg.AddProfile(new UserMapping());
        });

        return config.CreateMapper();
    }
}