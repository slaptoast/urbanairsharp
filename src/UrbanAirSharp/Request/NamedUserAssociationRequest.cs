using UrbanAirSharp.Dto;
using UrbanAirSharp.Request.Base;
using UrbanAirSharp.Response;
using UrbanAirSharp.Type;

namespace UrbanAirSharp.Request
{
    /// <summary>
    /// Used to form a Named User Assocation request
    /// http://docs.urbanairship.com/api/ua.html#association
    /// </summary>
    public class NamedUserAssociationRequest : PostRequest<BaseResponse, Association>
    {
        public NamedUserAssociationRequest(Association association)
            :base(association)
        {
            RequestUrl = "api/named_users/associate/";
        }
    }
}
