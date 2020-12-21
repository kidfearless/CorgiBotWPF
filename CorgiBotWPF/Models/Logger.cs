﻿using CorgiBotWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlidyBotWPF.Models
{
	class Logger
	{
		private static FileStream FileStream;
		private static StreamWriter StreamWriter;
		private static bool Initialized = false;

		static void Init()
		{
#pragma warning disable DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
#pragma warning disable DF0024 // Marks undisposed objects assinged to a field, originated in an object creation.
			FileStream = File.Open(App.LogsPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
			StreamWriter = new StreamWriter(FileStream)
			{
				AutoFlush = true
			};
			Initialized = true;
#pragma warning restore DF0024 // Marks undisposed objects assinged to a field, originated in an object creation.
#pragma warning restore DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
		}

		public static void Log(string text)
		{
			if(!Initialized)
			{
				Init();
			}
			StreamWriter.WriteLine(text);
		}

		public static void Log(Exception ex)
		{
			if (!Initialized)
			{
				Init();
			}

			StreamWriter.WriteLine(ex.ToString());
			StreamWriter.WriteLine(ex.Message);
			StreamWriter.WriteLine(ex.StackTrace);
		}

		public static void Dispose()
		{
			StreamWriter?.Flush();
			FileStream?.Dispose();
			StreamWriter?.Dispose();
		}
	}
}
