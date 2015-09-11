﻿using System;
using System.Diagnostics;
using System.Linq;
using Args;
using Args.Help;
using Args.Help.Formatters;
using GitReleaseNotes.IssueTrackers;

namespace GitReleaseNotes
{
    public static class Program
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        static int Main(string[] args)
        {
            GitReleaseNotesEnvironment.Log = new ConsoleLog();

            var modelBindingDefinition = Configuration.Configure<GitReleaseNotesArguments>();

            if (args.Any(a => a == "/?" || a == "?" || a.Equals("/help", StringComparison.InvariantCultureIgnoreCase)))
            {
                var help = new HelpProvider().GenerateModelHelp(modelBindingDefinition);
                var f = new ConsoleHelpFormatter();
                f.WriteHelp(help, Console.Out);

                return 0;
            }

            var exitCode = 0;

            var arguments = modelBindingDefinition.CreateAndBind(args);
            var context = arguments.ToContext();
            if (!context.Validate())
            {
                return -1;
            }

            try
            {
                var releaseNotesGenerator = new ReleaseNotesGenerator(context, new FileSystem.FileSystem(), new GitTools.IssueTrackers.IssueTrackerFactory());
                var task = releaseNotesGenerator.GenerateReleaseNotesAsync();
                task.Wait();

                Log.WriteLine("Done");
            }
            catch (GitReleaseNotesException ex)
            {
                exitCode = -1;
                Log.WriteLine("An expected error occurred: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                exitCode = -2;
                Log.WriteLine("An unexpected error occurred: {0}", ex.Message);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }

            return exitCode;
        }
    }
}
