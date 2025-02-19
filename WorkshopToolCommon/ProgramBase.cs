﻿using Gwindalmir.Updater;
using Phoenix.WorkshopTool.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Phoenix.WorkshopTool
{
    // DO NOT REFERENCE GAME LIBRARIES HERE!
    public abstract class ProgramBase
    {
        internal static void CheckForUpdate(Action<string> logMethod = null, Action<string> errorMethod = null)
        {
            // Direct log to console, in an attempt to check for an update if there's
            // an initialization problem (ie. game updated, and many runtime errors).
            if (logMethod == null)
                logMethod = Console.WriteLine;

            if (errorMethod == null)
                errorMethod = Console.Error.WriteLine;

            try
            {
                var updateChecker = new UpdateChecker(prereleases: true);
                if (updateChecker.IsNewerThan(Assembly.GetEntryAssembly()?.GetName().Version))
                {
                    var release = updateChecker.GetLatestRelease();
                    var asset = release.GetMatchingAsset(Assembly.GetEntryAssembly().GetName().Name);

                    if (!string.IsNullOrEmpty(asset?.Url))
                    {
                        ConsoleWriteColored(ConsoleColor.Green, () =>
                            logMethod($"Update Check: UPDATE AVAILABLE: {release.TagName}, released {release.Published.ToLocalTime()}"));
                        logMethod($"Download at: {asset.Url}");

                        logMethod($"Changelog:\n{release.GetChangelog()}");
                        return;
                    }
                }
                logMethod($"Update Check: No update available");
            }
            catch (Exception ex)
            {
                // Don't cause problems if update checker failed. Just report it.
                ConsoleWriteColored(ConsoleColor.Red, () =>
                    errorMethod($"Error checking for update: {ex.Message}"));
            }
        }

        public static void ConsoleWriteColored(ConsoleColor color, Func<string> outputMethod)
        {
            ConsoleWriteColored(color, () => Console.WriteLine(outputMethod?.Invoke() ?? string.Empty));
        }

        public static void ConsoleWriteColored(ConsoleColor color, Action<string> outputMethod, string message)
        {
            ConsoleWriteColored(color, () => outputMethod?.Invoke(message));
        }

        public static void ConsoleWriteColored(ConsoleColor color, string message)
        {
            ConsoleWriteColored(color, () => Console.Out.WriteLine(message));
        }

        public static void ConsoleWriteColored(ConsoleColor color, Action outputMethod)
        {
            // Don't colorize output if the terminal is non-interactive, or any output streams are redirected.
            if(Console.Out.IsInteractive() && Console.Error.IsInteractive())
                Console.ForegroundColor = color;

            try
            {
                outputMethod?.Invoke();
            }
            finally
            {
                if (Console.Out.IsInteractive() && Console.Error.IsInteractive())
                    Console.ResetColor();
            }
        }
    }
}
