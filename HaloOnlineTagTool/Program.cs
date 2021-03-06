﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HaloOnlineTagTool.Commands;
using HaloOnlineTagTool.Commands.Tags;
using HaloOnlineTagTool.Serialization;

namespace HaloOnlineTagTool
{
	class Program
	{
		static void Main(string[] args)
		{
			// Get the file path from the first argument
			// If no argument is given, load tags.dat
			var filePath = (args.Length > 0) ? args[0] : "C:\\Halo Online\\maps\\tags.dat";

			// If there are extra arguments, use them to automatically execute a command
			List<string> autoexecCommand = null;
			if (args.Length > 1)
				autoexecCommand = args.Skip(1).ToList();

			if (autoexecCommand == null)
			{
				Console.WriteLine("Halo Online Tag Tool [{0}]", Assembly.GetExecutingAssembly().GetName().Version);
				Console.WriteLine("Written by Shockfire");
				Console.WriteLine();
				Console.WriteLine("Please report any bugs and feature requests at");
				Console.WriteLine("<https://github.com/ElDewrito/HaloOnlineTagTool/issues>.");
				Console.WriteLine();
				Console.Write("Reading tags...");
			}

			// Load the tag cache
			var fileInfo = new FileInfo(filePath);
			TagCache cache;
			using (var stream = fileInfo.Open(FileMode.Open, FileAccess.Read))
				cache = new TagCache(stream);

			if (autoexecCommand == null)
				Console.WriteLine("{0} tags loaded.", cache.Tags.Count);

			// Version detection
			EngineVersion closestVersion;
			var version = VersionDetection.DetectVersion(cache, out closestVersion);
			if (version != EngineVersion.Unknown)
			{
				if (autoexecCommand == null)
				{
					var buildDate = DateTime.FromFileTime(cache.Timestamp);
					Console.WriteLine("- Detected target engine version {0}.", VersionDetection.GetVersionString(closestVersion));
					Console.WriteLine("- This cache file was built on {0} at {1}.", buildDate.ToShortDateString(), buildDate.ToShortTimeString());
				}
			}
			else
			{
				Console.Error.WriteLine("WARNING: The cache file's version was not recognized!");
				Console.Error.WriteLine("Using the closest known version {0}.", VersionDetection.GetVersionString(closestVersion));
				version = closestVersion;
			}

			// Load stringIDs
			Console.Write("Reading stringIDs...");
			var stringIdPath = Path.Combine(fileInfo.DirectoryName ?? "", "string_ids.dat");
			var resolver = StringIdResolverFactory.Create(version);
			StringIdCache stringIds = null;
			try
			{
				using (var stream = File.OpenRead(stringIdPath))
					stringIds = new StringIdCache(stream, resolver);
			}
			catch (IOException)
			{
				Console.Error.WriteLine("Warning: unable to open string_ids.dat!");
				Console.Error.WriteLine("Commands which require stringID values will be unavailable.");
			}

			if (autoexecCommand == null && stringIds != null)
			{
				Console.WriteLine("{0} strings loaded.", stringIds.Strings.Count);
				Console.WriteLine();
			}

			var info = new OpenTagCache
			{
				Cache = cache,
				CacheFile = fileInfo,
				StringIds = stringIds,
				StringIdsFile = (stringIds != null) ? new FileInfo(stringIdPath) : null,
				Version = version,
				Serializer = new TagSerializer(version),
				Deserializer = new TagDeserializer(version),
			};

			// Create command context
			var contextStack = new CommandContextStack();
			var tagsContext = TagCacheContextFactory.Create(contextStack, info);
			contextStack.Push(tagsContext);

			// If autoexecuting a command, just run it and return
			if (autoexecCommand != null)
			{
				if (!ExecuteCommand(contextStack.Context, autoexecCommand))
					Console.Error.WriteLine("Unrecognized command: {0}", autoexecCommand[0]);
				return;
			}

			Console.WriteLine("Enter \"help\" to list available commands. Enter \"exit\" to quit.");
			while (true)
			{
				// Read and parse a command
				Console.WriteLine();
				Console.Write("{0}> ", contextStack.GetPath());
				var commandLine = Console.ReadLine();
				if (commandLine == null)
					break;
				string redirectFile;
				var commandArgs = ArgumentParser.ParseCommand(commandLine, out redirectFile);
				if (commandArgs.Count == 0)
					continue;

				// If "exit" or "quit" is given, pop the current context
				if (commandArgs[0] == "exit" || commandArgs[0] == "quit")
				{
					if (!contextStack.Pop())
						break; // No more contexts - quit
					continue;
				}

				// Handle redirection
				var oldOut = Console.Out;
				StreamWriter redirectWriter = null;
				if (redirectFile != null)
				{
					redirectWriter = new StreamWriter(File.Open(redirectFile, FileMode.Create, FileAccess.Write));
					Console.SetOut(redirectWriter);
				}

				// Try to execute it
				if (!ExecuteCommand(contextStack.Context, commandArgs))
				{
					Console.Error.WriteLine("Unrecognized command: {0}", commandArgs[0]);
					Console.Error.WriteLine("Use \"help\" to list available commands.");
				}

				// Undo redirection
				if (redirectFile != null)
				{
					Console.SetOut(oldOut);
					redirectWriter.Dispose();
					Console.WriteLine("Wrote output to {0}.", redirectFile);
				}
			}
		}

		private static bool ExecuteCommand(CommandContext context, List<string> commandAndArgs)
		{
			if (commandAndArgs.Count == 0)
				return true;

			// Look up the command
			var command = context.GetCommand(commandAndArgs[0]);
			if (command == null)
				return false;

			// Execute it
			commandAndArgs.RemoveAt(0);

				ExecuteCommand(command, commandAndArgs);

			return true;
		}

		private static void ExecuteCommand(Command command, List<string> args)
		{
			if (command.Execute(args))
				return;
			Console.Error.WriteLine("{0}: {1}", command.Name, command.Description);
			Console.Error.WriteLine();
			Console.Error.WriteLine("Usage:");
			Console.Error.WriteLine("{0}", command.Usage);
			Console.Error.WriteLine();
			Console.Error.WriteLine("Use \"help {0}\" for more information.", command.Name);
		}
	}
}
