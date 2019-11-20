using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThreadUpload
{
	//[JsonObject("connectApi")]
	public class ConnectAPI
	{
		//[JsonProperty("uri")]
		public string uri { get; set; }

		//[JsonProperty("userName")]
		public string username { get; set; }

		//[JsonProperty("passWord")]
		public string password { get; set; }
	}
}
