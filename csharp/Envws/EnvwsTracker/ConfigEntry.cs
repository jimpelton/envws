using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackProcess
{
    public class ConfigOpts

    {
        public enum Key
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

        private static readonly Dictionary<Key, string> OptionStrings =
            new Dictionary<Key, string>()
            {
                {Key.BaseDirectory, "BaseDirectory"},
                {Key.EnvExePath, "EnvExePath"},
                {Key.EnvisionOutputDirectoryName, "EnvisionOutputDirectoryName"},
                {Key.EnvLog, "EnvLog"},
                {Key.RemoteBaseDirectory, "RemoteBaseDirectory"},
                {Key.ResultsLogDirectory, "ResultsLogDirectory"},
                {Key.ResultsDirectory, "ResultsDirectory"},
                {Key.Log4NetConfigFile, "Log4NetConfigFile"}
            };

        public static string OptString(Key opt)
        {
            return OptionStrings[opt];
        }
    }
}
