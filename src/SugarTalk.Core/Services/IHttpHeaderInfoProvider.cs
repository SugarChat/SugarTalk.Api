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
        public UserAccountIssuer? Issuer { get; set; }
    }

    public class HttpHeaderInfo : IHttpHeaderInfo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpHeaderInfo(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private UserAccountIssuer? _issuer;
        public UserAccountIssuer? Issuer
        {
            get
            {
                if (_issuer == null)
                {
                    var issuer = _httpContextAccessor.HttpContext?.Request?.Headers.SingleOrDefault(x =>
                        x.Key.Equals(RequestHeaderKeys.Issuer, StringComparison.InvariantCultureIgnoreCase)).Value;
                    _issuer = string.IsNullOrWhiteSpace(issuer) ? null : int.TryParse(issuer.Value, out var issuerInt) ? (UserAccountIssuer)issuerInt : null;
                }
                return _issuer;
            }
            set => _issuer = value;
        }
    }
}
