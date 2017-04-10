﻿// Copyright (c) 2014-2016 Jeff Gosling (jeffery.gosling@gmail.com)
// Copyright (c) 2016 Glenn R. Martin (glenn@intrepid.io)

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;

using log4net;
using log4net.Config;

using UrbanAirSharp.Dto;
using UrbanAirSharp.Request;
using UrbanAirSharp.Request.Base;
using UrbanAirSharp.Response;
using UrbanAirSharp.Type;
using System.Net;

namespace UrbanAirSharp
{
    /// <summary>
    /// A gateway for pushing notifications to the Urban Airship API V3
    /// 
    /// Supported:
    /// ---------
    /// api/push
    /// api/push/validate
    /// api/schedule 
    /// api/device_tokens 
    /// api/tags 
    /// 
    /// Not Supported Yet:
    /// -----------------
    /// api/feeds 
    /// api/reports 
    /// api/segments 
    /// api/location 
    /// </summary>
    /// <seealso href="http://docs.urbanairship.com/reference/api/v3/">Urban Airship API V3</seealso>
    public class UrbanAirSharpGateway
    {
        /// <summary>
        /// Should we execute code using <see cref="Awaitable{TResult}"/>.
        /// <remarks>This option makes utilization of the class simpler under the ASP.NET WebAPI Paradigm.</remarks>
        /// </summary>
        public static bool ConfigureAwait { get; set; }

        /// <summary>
        /// Log4Net Logger
        /// </summary>
		private static readonly ILog Log = LogManager.GetLogger(typeof(UrbanAirSharpGateway));

        /// <summary>
        /// Create a new gateway object utilizng a given key set.
        /// </summary>
        /// <param name="appKey">Application Key</param>
        /// <param name="appMasterSecret">Application Master Secret</param>
        public UrbanAirSharpGateway(string appKey, string appMasterSecret)
        {
            //UA has updated their endpoints to only allow TLS12 and TLS 11 connections, SSL 3 no longer works
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            XmlConfigurator.Configure();
            ServiceModelConfig.Create(appKey, appMasterSecret);
        }

        /// <summary>
        /// Send a Push request. This call can perform the following: 
        /// <list type="bullet"><item><description>Broadcast to all devices</description></item><item><description>Broadcast to one device type</description></item><item><description>Send to a targeted device</description></item><item><description>Broadcast to all devices with a different alert for each
        /// type</description></item></list></summary>
        /// <param name="alert">The message to be pushed</param>
        /// <param name="deviceTypes">use null for broadcast</param>
        /// <param name="deviceId">use null for broadcast or deviceTypes must contain 1
        /// element that distinguishes this deviceId</param>
        /// <param name="deviceAlerts">per device alert messages and extras</param>
        /// <param name="customAudience">a more specific way to choose the audience for the
        /// push. If this is set, deviceId is ignored</param>
        /// <returns>
        /// Service Response
        /// </returns>
        public PushResponse Push(string alert, IList<DeviceType> deviceTypes = null, string deviceId = null, IList<BaseAlert> deviceAlerts = null, Audience customAudience = null)
        {
            return SendRequest(new PushRequest(CreatePush(alert, deviceTypes, deviceId, deviceAlerts, customAudience)));
        }

        /// <summary>
        /// Validates a push request. Duplicates Push without actually sending the alert. See Push
        /// </summary>
        /// <param name="alert">The message to be pushed</param>
        /// <param name="deviceTypes">use null for broadcast</param>
        /// <param name="deviceId">use null for broadcast or deviceTypes must contain 1 element that distinguishes this deviceId</param>
        /// <param name="deviceAlerts">per device alert messages and extras</param>
        /// <param name="customAudience">a more specific way to choose the audience for the push. If this is set, deviceId is ignored</param>
        /// <returns>Service Response</returns>
        public PushResponse Validate(string alert, IList<DeviceType> deviceTypes = null, string deviceId = null,
            IList<BaseAlert> deviceAlerts = null, Audience customAudience = null)
        {
            return SendRequest(new PushValidateRequest(CreatePush(alert, deviceTypes, deviceId, deviceAlerts, customAudience)));
        }

        /// <summary>
        /// Create a Schedule
        /// </summary>
        /// <param name="schedule">Schedule</param>
        /// <returns>Service Response</returns>
		public ScheduleCreateResponse CreateSchedule(Schedule schedule)
        {
            return SendRequest(new ScheduleCreateRequest(schedule));
        }

        /// <summary>
        /// Change a Schedule
        /// </summary>
        /// <param name="scheduleId">The Schedule Id from previous requests.</param>
        /// <param name="schedule">Schedule</param>
        /// <returns>Service Response</returns>
		public ScheduleEditResponse EditSchedule(Guid scheduleId, Schedule schedule)
        {
            return SendRequest(new ScheduleEditRequest(scheduleId, schedule));
        }

        /// <summary>
        /// Remove a Schedule
        /// </summary>
        /// <param name="scheduleId">The Schedule Id from previous requests.</param>
        /// <returns>Service Response</returns>
		public BaseResponse DeleteSchedule(Guid scheduleId)
        {
            return SendRequest(new ScheduleDeleteRequest(scheduleId));
        }

        /// <summary>
        /// Fetch a Schedule
        /// </summary>
        /// <param name="scheduleId">The Schedule Id from previous requests.</param>
        /// <returns>Service Response</returns>
        public ScheduleGetResponse GetSchedule(Guid scheduleId)
        {
            return SendRequest(new ScheduleGetRequest(scheduleId));
        }

        /// <summary>
        /// Get a List of Schedules
        /// </summary>
        /// <returns>Service Response</returns>
        public ScheduleListResponse ListSchedules()
        {
            return SendRequest(new ScheduleListRequest());
        }

        /// <summary>
        /// Get list of all named users. Can provide a named user id to start list from.
        /// </summary>
        /// <returns>Service Response</returns>
        public NamedUsersListResponse GetNamedUsersList(string StartNamedUserId = null)
        {
            return SendRequest(new NamedUsersListRequest(StartNamedUserId));
        }

        /// <summary>
        /// Get a named user
        /// </summary>
        /// <returns>Service Response</returns>
        public NamedUserResponse GetNamedUser(string NamedUserId)
        {
            return SendRequest(new NamedUserRequest(NamedUserId));
        }

        /// <summary>
        /// Associate named user with a device
        /// </summary>
        /// <returns>Service Response</returns>
        public BaseResponse AssociateNamedUser(string ChannelId, DeviceType DeviceType, string NamedUserId)
        {
            return SendRequest(new NamedUserAssociationRequest(new Association(ChannelId, DeviceType, NamedUserId)));
        }

        /// <summary>
        /// Disassociate named user from a device
        /// </summary>
        /// <returns>Service Response</returns>
        public BaseResponse DisassociateNamedUser(string ChannelId, DeviceType DeviceType, string NamedUserId)
        {
            return SendRequest(new NamedUserDisassociationRequest(new Association(ChannelId, DeviceType, NamedUserId)));
        }

        /// <summary>
        /// Register a device
        /// 
        /// Registers a device token only with the Urban Airship site, this can be used for new device tokens and for existing tokens.
        /// The existing settings (badge, tags, alias, quiet times) will be overriden. If a token has become inactive reregistering it
        /// will make it active again.
        /// </summary>
        /// <returns>Service Response</returns>
        public BaseResponse RegisterDeviceToken(string deviceToken)
        {
            return RegisterDeviceToken(new DeviceToken { Token = deviceToken });
        }

        /// <summary>
        /// Register / Update a Device Registration
        /// Registers a device token with extended properties with the Urban Airship site, this can be used for new device 
        /// tokens and for existing tokens. If a token has become inactive reregistering it will make it active again. 
        /// </summary>
        /// <returns>Service Response</returns>
        /// <exception cref="ArgumentException">A device Tokens Token field is Required</exception>
        public BaseResponse RegisterDeviceToken(DeviceToken deviceToken)
        {
            if (string.IsNullOrEmpty(deviceToken.Token))
                throw new ArgumentException("A device Tokens Token field is Required", "deviceToken");

            return SendRequest(new DeviceTokenRequest(deviceToken));
        }

        /// <summary>
        /// Create a System Wide Tag
        /// </summary>
        /// <param name="tag">Tag Value</param>
        /// <returns>Service Response</returns>
        /// <exception cref="ArgumentException">A tag name is Required</exception>
        public BaseResponse CreateTag(Tag tag)
        {
            if (string.IsNullOrEmpty(tag.TagName))
                throw new ArgumentException("A tag name is Required", "tag");

            return SendRequest(new TagCreateRequest(tag));
        }

        /// <summary>
        /// Remove a System Wide Tag
        /// </summary>
        /// <param name="tag">Tag name</param>
        /// <returns>Service Response</returns>
        /// <exception cref="ArgumentException">A tag is Required</exception>
        public BaseResponse DeleteTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException("A tag is Required", "tag");

            return SendRequest(new TagDeleteRequest(tag));
        }

        /// <summary>
        /// Get a List of Tags
        /// </summary>
	    /// <returns>Service Response</returns>
		public TagListResponse ListTags()
        {
            return SendRequest(new TagListRequest());
        }

        /// <summary>
        /// Create a Push Object
        /// </summary>
        /// <param name="alert">Alert Text</param>
        /// <param name="deviceTypes">Device Types</param>
        /// <param name="deviceId">Singular Device ID</param>
        /// <param name="deviceAlerts">Device/Platform Specific Alerts</param>
        /// <param name="customAudience">Audience</param>
        /// <returns>A push object capable of representing the options provided.</returns>
        /// TODO: Move to DTOs?
        /// <exception cref="InvalidOperationException">when deviceId is not null, deviceTypes must contain 1 element which identifies the deviceId type</exception>
        /// <exception cref="ArgumentNullException">Linq source or predicate is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public static Push CreatePush(string alert, IList<DeviceType> deviceTypes = null, string deviceId = null, IList<BaseAlert> deviceAlerts = null, Audience customAudience = null)
        {
            var push = new Push()
            {
                Notification = new Notification()
                {
                    DefaultAlert = alert
                }
            };

            if (customAudience != null)
            {
                deviceId = null;

                push.Audience = customAudience;
            }

            if (deviceTypes != null)
            {
                push.DeviceTypes = deviceTypes;

                if (deviceId != null)
                {
                    if (deviceTypes.Count != 1)
                    {
                        throw new InvalidOperationException("when deviceId is not null, deviceTypes must contain 1 element which identifies the deviceId type");
                    }

                    var deviceType = deviceTypes[0];

                    switch (deviceType)
                    {
                        case DeviceType.Android:
                            push.SetAudience(AudienceType.Android, deviceId);
                            break;
                        case DeviceType.Ios:
                            push.SetAudience(AudienceType.Ios, deviceId);
                            break;
                        case DeviceType.Wns:
                            push.SetAudience(AudienceType.Windows, deviceId);
                            break;
                        case DeviceType.Mpns:
                            push.SetAudience(AudienceType.WindowsPhone, deviceId);
                            break;
                        case DeviceType.Blackberry:
                            push.SetAudience(AudienceType.Blackberry, deviceId);
                            break;
                    }
                }
            }

            if (deviceAlerts == null || deviceAlerts.Count <= 0)
            {
                return push;
            }

            push.Notification.AndroidAlert = (AndroidAlert)deviceAlerts.FirstOrDefault(x => x is AndroidAlert);
            push.Notification.IosAlert = (IosAlert)deviceAlerts.FirstOrDefault(x => x is IosAlert);
            push.Notification.WindowsAlert = (WindowsAlert)deviceAlerts.FirstOrDefault(x => x is WindowsAlert);
            push.Notification.WindowsPhoneAlert = (WindowsPhoneAlert)deviceAlerts.FirstOrDefault(x => x is WindowsPhoneAlert);
            push.Notification.BlackberryAlert = (BlackberryAlert)deviceAlerts.FirstOrDefault(x => x is BlackberryAlert);

            return push;
        }

        //=====================================================================================================================

        /// <summary>
        /// Private awaitable changer. To support WebAPIs threading environment. Which seems to be a <see href="http://stackoverflow.com/questions/20882697/connecting-to-urban-airship-rest-api-with-c-sharp/24804223#24804223">known issue</see>.
        /// <seealso href="http://blog.stephencleary.com/2012/02/async-and-await.html">Blog: Async and Await</seealso>
        /// <seealso href="https://msdn.microsoft.com/en-us/magazine/gg598924.aspx">MSDN Mag: Parallel Computing - It's All About the SynchronizationContext</seealso>
        /// <seealso href="http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html">Blog: Don't Block on Async Code</seealso>
        /// <seealso href="http://stackoverflow.com/questions/20882697/connecting-to-urban-airship-rest-api-with-c-sharp/24804223#24804223">SO: Connecting to Urban Airship REST API with c#</seealso>
        /// </summary>
        /// <typeparam name="TResult">Return data.</typeparam>
        /// <param name="fct">Function to invoke in wrapper.</param>
        /// <param name="awaitConfig">Config value to use.</param>
        /// <returns>Task</returns>
        [SuppressMessage(@"ReSharper", @"ConsiderUsingAsyncSuffix")]
        [SuppressMessage(@"ReSharper", @"IdentifierWordIsNotInDictionary")]
        private static async Task<TResult> Awaitable<TResult>([NotNull] Func<TResult> fct, bool awaitConfig)
        {
            return await Task.Run(fct).ConfigureAwait(awaitConfig);
        }

        /// <summary>
        /// Send a request. Get a response.
        /// </summary>
        /// <typeparam name="TResponse">The expected Response Type</typeparam>
        /// <param name="baseRequest">Request Data</param>
        /// <returns>Service Response</returns>
        private static TResponse SendRequest<TResponse>([NotNull] BaseRequest<TResponse> baseRequest) where TResponse : BaseResponse, new()
        {
            Func<BaseRequest<TResponse>, TResponse> innerAction = (request) =>
            {
                try
                {
                    var requestTask = request.ExecuteAsync();

                    return requestTask.Result;
                }
                catch (Exception e)
                {
                    Log.Error(request.GetType().FullName, e);

                    return new TResponse()
                    {
                        Error = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Ok = false
                    };
                }
            };

            return Awaitable(() => innerAction.Invoke(baseRequest), ConfigureAwait).Result;
        }
    }
}
