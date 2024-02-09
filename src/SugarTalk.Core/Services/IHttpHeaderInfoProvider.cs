using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Data.Claims;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Enums.Account;
using System;
using System.Linq;

namespace SugarTalk.Core.Services
{
    public interface IHttpHeaderInfoProvider : IScopedDependency
    {
        IHttpHeaderInfo GetHttpHeaderInfo();
    }

    public class HttpHeaderInfoProvider : IHttpHeaderInfoProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpHeaderInfoProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IHttpHeaderInfo GetHttpHeaderInfo()
        {
            return new HttpHeaderInfo(_httpContextAccessor);
        }
    }

    public interface IHttpHeaderInfo
    {
        public UserAccountType? UserAccountType { get; set; }
    }

    public class HttpHeaderInfo : IHttpHeaderInfo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpHeaderInfo(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private UserAccountType? _userAccountType;
        public UserAccountType? UserAccountType
        {
            get
            {
                if (_userAccountType == null)
                {
                    var userAccountType = _httpContextAccessor.HttpContext?.Request?.Headers.SingleOrDefault(x =>
                        x.Key.Equals(RequestHeaderKeys.UserAccountType, StringComparison.InvariantCultureIgnoreCase)).Value;
                    _userAccountType = string.IsNullOrWhiteSpace(userAccountType) ? null : int.TryParse(userAccountType.Value, out var userAccountTypeInt) ? (UserAccountType)userAccountTypeInt : null;
                }
                return _userAccountType;
            }
            set => _userAccountType = value;
        }
    }
}
