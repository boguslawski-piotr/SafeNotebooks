using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pbXNet;

namespace SafeNotebooks
{
	public class NewtonsoftJsonSerializer : ISerializer
	{
		JsonSerializerSettings _settings;

		/// Settings to use with JsonConvert.Serialize i Deserialize methods.
		public virtual JsonSerializerSettings Settings
		{
			get {
				if (_settings == null)
				{
					_settings = new JsonSerializerSettings()
					{
						DateTimeZoneHandling = DateTimeZoneHandling.Utc,
						NullValueHandling = NullValueHandling.Ignore,
#if DEBUG
						//Formatting = Formatting.Indented,
#endif
					};
				}
				return _settings;
			}

			set {
				_settings = value;
			}
		}

		public string ToString(object o, string id = null)
		{
			string d = JsonConvert.SerializeObject(o, Settings);
			if (id != null)
			{
				d = "'" + id + "':" + d + ",";
			}
			return d;
		}

		public T FromString<T>(string d, string id = null)
		{
			if (id != null)
			{
				JObject pd = JObject.Parse("{" + d + "}");
				d = pd[id].ToString();
			}

			var o = JsonConvert.DeserializeObject<T>(d, Settings);
			return o;
		}

	}
}
