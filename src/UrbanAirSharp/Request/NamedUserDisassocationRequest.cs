using UrbanAirSharp.Dto;
using UrbanAirSharp.Request.Base;
using UrbanAirSharp.Response;
using UrbanAirSharp.Type;

namespace UrbanAirSharp.Request
{
    /// <summary>
    /// Used to form a Named User Assocation request
    /// http://docs.urbanairship.com/api/ua.html#disassociation
    /// </summary>
    public class NamedUserDisassociationRequest : PostRequest<BaseResponse, Association>
    {
        public NamedUserDisassociationRequest(Association association)
            :base(association)
        {
            RequestUrl = "api/named_users/disassociate/";
        }
    }
}
