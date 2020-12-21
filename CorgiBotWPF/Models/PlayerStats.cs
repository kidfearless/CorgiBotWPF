using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorgiBotWPF.Models
{
	class PlayerStats : Dictionary<ulong, uint>
	{
		public new uint this[ulong index]
		{
			get
			{
				if(!this.ContainsKey(index))
				{
					this[index] = 0;
				}
				return base[index];
			}
			set
			{
				base[index] = value;
				Save();
			}
		}

		public uint this[DiscordUser user]
		{
			get => this[user.Id];
			set => this[user.Id] = value;
		}

		private void Save()
		{
			var jsonString = JsonConvert.SerializeObject(this);
			using var writer = new StreamWriter(App.StatsPath);
			writer.Write(jsonString);
			writer.Flush();
		}
	}
}
