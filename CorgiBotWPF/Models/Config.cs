using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CorgiBotWPF.Models
{
	struct Config
	{
		private string channel;
		private string message;
		[JsonIgnore]
		public string Channel
		{
			get => channel;
			set
			{
				channel = value;
			}
		}
		[JsonIgnore]
		public string Message
		{
			get => message;
			set
			{
				message = value;
			}
		}

#nullable enable
		public string? token;
#nullable disable
	}
}
