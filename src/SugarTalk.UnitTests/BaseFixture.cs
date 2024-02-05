using AutoMapper;
using NSubstitute;
using SugarTalk.Core.Data;
using SugarTalk.Core.Mapping;
using Castle.Core.Configuration;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Services.Http;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Settings.LiveKit;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.AntMediaServer;

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
    protected readonly ICurrentUser _currentUser;
    protected readonly IConfiguration _configuration;
    protected readonly IAccountDataProvider _accountDataProvider;
    protected readonly IAntMediaServerUtilService _antMediaServerUtilService;
    protected readonly ILiveKitServerUtilService _liveKitServerUtilService;
    protected readonly LiveKitServerSetting _liveKitServerSetting;
    protected readonly ISpeechClient _speechClient;
    protected readonly ISugarTalkHttpClientFactory _httpClientFactory;
    protected readonly IHttpContextAccessor _contextAccessor;
    protected readonly IMeetingProcessJobService _meetingProcessJobService;

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
        _liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
        _accountDataProvider = MockAccountDataProvider(_mapper, _repository, _unitOfWork);
        _meetingDataProvider = MockMeetingDataProvider(_mapper, _repository, _unitOfWork, _accountDataProvider, _currentUser);
        _meetingService = MockMeetingService(_clock, _mapper, _unitOfWork, _currentUser, _meetingDataProvider, _accountDataProvider, _antMediaServerUtilService, _liveKitServerUtilService, _liveKitServerSetting, _speechClient);
        _meetingProcessJobService = MockMeetingProcessJobService(_clock, _unitOfWork, _meetingDataProvider);
    }

    protected IMeetingProcessJobService MockMeetingProcessJobService(IClock clock, IUnitOfWork unitOfWork, IMeetingDataProvider meetingDataProvider)
    {
        return new MeetingProcessJobService(clock, unitOfWork, meetingDataProvider);
    }

    protected IAccountDataProvider MockAccountDataProvider(IMapper mapper, IRepository repository, IUnitOfWork unitOfWork)
    {
        return new AccountDataProvider(repository, mapper, unitOfWork);
    }

    protected IMeetingService MockMeetingService(
        IClock clock,
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IMeetingDataProvider meetingDataProvider,
        IAccountDataProvider accountDataProvider,
        IAntMediaServerUtilService antMediaServerUtilService, 
        ILiveKitServerUtilService liveKitServerUtilService,
        LiveKitServerSetting liveKitServerSetting, 
        ISpeechClient speechClient)
    {
        return new MeetingService(clock, mapper, unitOfWork,
            currentUser,
            speechClient,
            meetingDataProvider,
            accountDataProvider,
            liveKitServerSetting,
            liveKitServerUtilService,
            antMediaServerUtilService);
    }
    
    protected IMeetingDataProvider MockMeetingDataProvider(
        IMapper mapper, 
        IRepository repository,
        IUnitOfWork unitOfWork,
        IAccountDataProvider accountDataProvider,
        ICurrentUser currentUser)
    {
        return new MeetingDataProvider(mapper, repository, unitOfWork, accountDataProvider, currentUser);
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