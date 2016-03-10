// Copyright (c) 2014-2015 Jeff Gosling (jeffery.gosling@gmail.com)
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UrbanAirSharp.Dto
{
	public class IosAlert : BaseAlert
	{
		/// <summary>
		/// TODO only "auto" supported for now
		/// </summary>
		[JsonProperty("badge")] 
		public string Badge = "auto";

		[JsonProperty("sound")]
		public string Sound { get; set; }

		[JsonProperty("content_available")]
		public bool ContentAvailable { get; set; }

		[JsonProperty("expiry")]
		public int ApnsTimeToLive { get; set; }

		[JsonProperty("priority")] 
		public int Priority = 10;

		[JsonProperty("extra")]
		public Dictionary<string, string> Extras { get; set; }
	}
}