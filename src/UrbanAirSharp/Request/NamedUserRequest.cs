using UrbanAirSharp.Request.Base;
using UrbanAirSharp.Response;

namespace UrbanAirSharp.Request
{
    /// <summary>
    /// Used to form a Named User Lookup request
    /// http://docs.urbanairship.com/api/ua.html#lookup
    /// </summary>
    public class NamedUserRequest : GetRequest<NamedUserResponse>
    {
        public NamedUserRequest(string NamedUserId)
        {
            RequestUrl = "api/named_users/?id=" + NamedUserId;
        }
    }
}
