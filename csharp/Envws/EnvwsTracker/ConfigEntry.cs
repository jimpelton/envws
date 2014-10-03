using System.Collections.Generic;

namespace TrackProcess
{
    public enum ConfigKey
    {
        BaseDirectory,
        EnvExePath,
        EnvisionOutputDirectoryName,
        EnvLog,
        RemoteBaseDirectory,
        ResultsLogDirectory,
        ResultsDirectory,
        Log4NetConfigFile
    }

    public class ConfigOpts
    {
        private static readonly Dictionary<ConfigKey, string> OptionStrings =
            new Dictionary<ConfigKey, string>()
            {
                {ConfigKey.BaseDirectory, "BaseDirectory"},
                {ConfigKey.EnvExePath, "EnvExePath"},
                {ConfigKey.EnvisionOutputDirectoryName, "EnvisionOutputDirectoryName"},
                {ConfigKey.EnvLog, "EnvLog"},
                {ConfigKey.RemoteBaseDirectory, "RemoteBaseDirectory"},
                {ConfigKey.ResultsLogDirectory, "ResultsLogDirectory"},
                {ConfigKey.ResultsDirectory, "ResultsDirectory"},
                {ConfigKey.Log4NetConfigFile, "Log4NetConfigFile"}
            };

        public static string Get(ConfigKey opt)
        {
            return OptionStrings[opt];
        }
    }
}
