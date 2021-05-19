using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.Windows.Navigation;
using DSharpPlus.Entities;
using System.Reflection;
using System.Net;
using CorgiBotWPF.Models;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using SlidyBotWPF.Models;

//Original license
/**
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" (Revision 42):
 * <helix@sinair.ru> wrote this file.     As long as you retain this notice you
 * can do whatever you want with this stuff. If we meet some day, and you think
 * this stuff is worth it, you can buy me a beer in return.      Maxim Solovyov
 * ----------------------------------------------------------------------------
 */


namespace CorgiBotWPF
{
	public partial class App : Application, IDisposable
	{
		const string TOKEN_NAME = "DISCORD_CORGIBOT_TOKEN";
		DiscordClient Client;
		DiscordChannel Channel;
		public static string LogsPath;
		public static string StatsPath;
		public static string FolderPath;
		PlayerStats Stats;

		Config Config;

		public DiscordEmoji CorgiEmoji => DiscordEmoji.FromName(Client, ":corgi:");
		public DiscordEmoji CorgiButtEmoji => DiscordEmoji.FromName(Client, ":corgibutt:");
		public DiscordEmoji BatEmoji => DiscordEmoji.FromUnicode(Client, "\uD83E\uDD87");

		private void InitPath()
		{
			var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

			FolderPath = Path.Combine(basePath, "CorgiBot");
			if (!Directory.Exists(FolderPath))
			{
				Directory.CreateDirectory(FolderPath);
			}

			LogsPath = Path.Combine(FolderPath, "Logs.txt");
			if (!File.Exists(LogsPath))
			{
				File.Create(LogsPath).Dispose();
			}

			StatsPath = Path.Combine(FolderPath, "stats.json");
			if (!File.Exists(StatsPath))
			{
				File.Create(StatsPath).Dispose();
			}

			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
			string text = File.ReadAllText(path);
			var settingSettings = new JsonSerializerSettings()
			{
				ContractResolver = new SettingsResolver()
			};

			Config = JsonConvert.DeserializeObject<Config>(text, settingSettings);
			Stats = JsonConvert.DeserializeObject<PlayerStats>(File.ReadAllText(StatsPath));
			Stats ??= new PlayerStats();
		}



		public App()
		{
		}

		~App()
		{
			this.Dispose();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.Dispose();
			base.OnExit(e);
		}

		// Entry Point for our application
		protected override async void OnStartup(StartupEventArgs e)
		{
			
			try
			{
				InitPath();

				AddToLaunchWithWindows();



				var cfg = new DiscordConfiguration
				{
					Token = Environment.GetEnvironmentVariable(TOKEN_NAME)??Config.token,
					TokenType = TokenType.Bot,

					AutoReconnect = true
				};
				Client = new DiscordClient(cfg);
				Client.GuildAvailable += Client_GuildAvailable;
				Client.MessageCreated += Client_MessageCreated;
				Client.GuildMemberAdded += Client_GuildMemberAdded;
				Client.GuildMemberRemoved += Client_GuildMemberRemoved;
				Client.ClientErrored += Client_ClientErrored;
				await Client.ConnectAsync();
				base.OnStartup(e);
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw;
			}
		}

		private Task Client_ClientErrored(ClientErrorEventArgs e)
		{
			try
			{
				Logger.Log(e.Exception);
			}
			catch { }
			return Task.CompletedTask;
		}

		private Task Client_GuildMemberRemoved(GuildMemberRemoveEventArgs e)
		{
			try
			{
				Stats.Remove(e.Member.Id);

				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw;
			}
		}

		private Task Client_GuildMemberAdded(GuildMemberAddEventArgs e)
		{
			try
			{
				Join(e.Member);

				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw;
			}
		}

		private Task Client_MessageCreated(MessageCreateEventArgs e)
		{

			try
			{
				if(e.Guild is null)
				{
					switch (e.Message.Content)
					{
						case "!top":
						case "!marks":
						case "!stats":
							TopStats(e.Channel);
							
							break;
						case "!join":
							Join(e.Author);
							break;
						default:
							break;
					}
				}
				else if (e.Channel == Channel && e.Author is not null && !e.Author.IsBot)
				{
					bool gaveCorgi = false;
					//if(new Random().Next(0, 50) == 40)
					//{
					//	GiveFakeBat(e);
					//}
					if (new Random().Next(0, 100) == 50)
					{
						GiveCorgi(e);
						gaveCorgi = true;
					}
					if (new Random().Next(0, 1000) == 50)
					{
						GiveCorgiButt(e);
						gaveCorgi = true;
					}
					if (e.MentionedUsers.Any(t => t.Username == "CorgiBot"))
					{
						if(gaveCorgi)
						{
							e.Channel.SendMessageAsync("Here you go!");
						}
						else
						{
							e.Channel.SendMessageAsync("No, go ask SlidyBot.\n You now have 0 corgis...");
						}
					}
				}



				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw;
			}
		}

		private void GiveFakeBat(MessageCreateEventArgs e)
		{
			e.Message.CreateReactionAsync(BatEmoji);
		}

		private void GiveCorgi(MessageCreateEventArgs e)
		{
			e.Message.CreateReactionAsync(CorgiEmoji);
			Stats[e.Author]++;
		}

		private void GiveCorgiButt(MessageCreateEventArgs e)
		{
			e.Message.CreateReactionAsync(CorgiButtEmoji);
			Stats[e.Author] += 2;
		}

		private void Join(DiscordUser author)
		{
			var message = Config.Message.Replace("%1", $"<@!{author.Id}>").Replace("%2", "https://www.sourcemod.net/downloads.php?branch=stable");
			Channel.SendMessageAsync(message);
		}

		private void TopStats(DiscordChannel channel)
		{
			var builder = new DiscordEmbedBuilder()
			{
				Color = new DiscordColor(0x7289DA)
			};

			builder.AddField("Total reactions:", $"{Stats.Sum(t => t.Value)} {CorgiEmoji}");

			var sortedStats = Stats
								.OrderByDescending(k => k.Value)
								// returns a collection of formatted strings
								.Select(kv => $"**•** <@{kv.Key}> -{kv.Value} {CorgiEmoji}")
								.Take(15);

			string statsString = string.Join("\n", sortedStats);

			if(string.IsNullOrEmpty(statsString))
			{
				statsString = "**•** N/A";
			}

			builder.AddField("Top 15 users reacted to:", statsString);
			channel.SendMessageAsync(embed: builder);
		}

		// Called when we have connected to our discord and can start sending messages.
		// This will create our repeating timer at a safe point.
		private async Task Client_GuildAvailable(GuildCreateEventArgs e)
		{
			if(e.Guild.Name == "bhoptimer")
			{
				Channel = e.Guild.Channels.First(chan => chan.Name.ToLower() == Config.Channel);
			}

			(await Channel.GetMessagesAsync()).First();


		}

		public void Dispose()
		{
			Client?.Dispose();
			Logger.Dispose();
		}

		private static void AddToLaunchWithWindows()
		{
			try
			{
				using RegistryKey reg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

				var ass = Assembly.GetExecutingAssembly();


				var location = Path.ChangeExtension(ass.Location, ".exe");

				reg.SetValue("Launch CorgiBot", location);
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw;
			}
		}
	}
}
