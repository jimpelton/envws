using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EnvwsLib.Util
{
    public class ConfigOption : IEnumerable<string>
    {
        private IList<string> values;

        /// <summary>
        /// Get the value associated witht his config option.
        /// If you want all the values, then enumerate this ConfigOption and
        /// collect them as they are returned.
        /// </summary>
        public string Value
        {
            get
            {
                return values[0];
            }
        }

        public ConfigOption Add(string value)
        {
            values.Add(value);
            return this;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return (IEnumerator<string>) this.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable) values).GetEnumerator();
        }
    }

    /// <summary>
    /// Singleton class that parses a config file of key=value pairs.
    /// Multiple values for a single key can be specified by separating each
    /// value with a ',':
    ///     key=val1, val2, val3
    /// 
    /// Comments begin with a ';' or '#', but you can change that by changing the character
    /// array called COMMENT_CHARS (don't remove anything, or you'll break older configs!).
    /// 
    /// </summary>
    public class ConfigParser
    {
        private const int MAX_CONFIG_FILE_LINES = 500;
        private static readonly char[] COMMENT_CHARS = {';','#'};

        private log4net.ILog 
            logger = log4net.LogManager.GetLogger(typeof (ConfigParser));

        private static ConfigParser me = null;

        private IDictionary<string, ConfigOption>
            confOpts = new Dictionary<string, ConfigOption>();

        public string Filename { get; private set; }

        private ConfigParser() { }

        public static ConfigParser Instance()
        {
            return me ?? (me = new ConfigParser());
        }

        /// <summary>
        /// Get the first value for specified key <code>key</code>.
        /// 
        /// If there is more than one value, then use GetAsList to 
        /// retrieve the entire list of strings.
        /// </summary>
        /// <param name="key">the key to get the option for</param>
        /// <returns>A ConfigOption with one or more options asociated with the key.</returns>
        public ConfigOption this[string key]
        {
            get
            {
                return confOpts[key];
            }
//			set
//			{
//                // the last seen key-value pair takes precedence, so always make a new list.
//                ConfigOption opt = new ConfigOption().Add(value);
//			    confOpts[key] = 
//			}
        }

        /// <summary>
        /// Retrieve the option for key.
        /// If there is more than one option, then use GetAsList to 
        /// retrieve the entire list of strings.
        /// </summary>
        /// <param name="key">the key to get the option for</param>
        /// <returns>A string that is the value associated with key.</returns>
        public ConfigOption Get(string key)
        {
            return this[key];
        }

        /// <summary>
        /// Get all values for <code>key</code>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
//        public ConfigOption GetAsList(string key)
//        {
//            return confOpts[key];
//        }

        /// <summary>
        /// Add an option to the parser. The parser only accepts keys in the 
        /// config file if they have already added via AddOpt().
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <returns>This ConfigParser instance.</returns>
        public ConfigParser AddOpt(string key)
        {
            if (!confOpts.ContainsKey(key))
            { 
                confOpts[key] = new ConfigOption();
            }

            return this;
        }


//        public void AddOpts(IDictionary<string, IList<string>> opts)
//        {
//            confOpts = opts;
//        }

        /// <summary>
        /// Associate a key with a default value.
        /// </summary>
        /// <param name="key">The key to associate the default value with</param>
        /// <param name="value">The default value</param>
        /// <returns>This ConfigParser instance</returns>
		public ConfigParser SetDefaultOptValue(string key, string value)
        {
            confOpts[key] = new ConfigOption().Add(value);

            return this;
        }

		public string GetFormatedOptionsString()
        {
            StringBuilder sb = new StringBuilder();
			foreach (ConfigOption p in confOpts.Values)
            {
                sb.Append(p.Key + " ");
				sb.AppendFormat("%s", p.Value.ToArray());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /***************************************************************************
         * PARSIN' CODE!!!
         ***************************************************************************/
        /// <summary>
        /// Open the file at filename.
        /// The config file should have lines like this:
        ///     type = lval [,lval...]
        /// </summary>
        /// <param name="filename">the config file to open.</param>
        /// <returns></returns>
        public int Open(string filename)
        {
            Filename = filename;
            StreamReader tr;
            int nLines = 0;

            try
            {
                tr = File.OpenText(filename);
                // loop through lines of config file.
                while (!tr.EndOfStream && nLines < MAX_CONFIG_FILE_LINES)  
                {
                    string line = tr.ReadLine();
                    if (p_line(line, nLines) == 1)
                    {
                        nLines += 1;
                    }
                    else break;
                }
                tr.Close();
            }
            catch (Exception eek)
            {
                Console.WriteLine(eek.StackTrace);
                logger.Error(eek.Message);
            }
            return nLines;
        }

        /// <summary>
        /// A line looks like:
        ///     type = lval [,lval...]
        /// </summary>
        /// <param name="line">the line to parse</param>
        /// <param name="lnum">the line number we are on</param>
        /// <returns></returns>
        private int p_line(string line, int lnum)
        {
            int rval = 1;

            if (line == string.Empty)
                return rval;
            
            if (COMMENT_CHARS.Contains(line[0]))
                return rval;

            line = line.Trim();

            // lineSplits[0] is the option
            // lineSplits[i], for i>0, are the values for that option
            string[] lineSplits = line.Split('=');
            
            if (lineSplits.Length != 2)
            {
                rval = 0;
                logger.Error("Syntax error on line " + lnum);
            }
            else
            {
                for (int i = 0; i < lineSplits.Length; ++i) 
                { 
                    lineSplits[i] = lineSplits[i].Trim(); 
                }

                if (confOpts.ContainsKey(lineSplits[0]))
                {
                    if (lineSplits[1].Contains(","))
                    {
                        //many values for this option
                        string[] rhvals = lineSplits[1].Split(',');
                        for (int i = 0; i < rhvals.Length; ++i) 
                        { 
                            rhvals[i] = rhvals[i].Trim(); 
                        }
                        foreach (string v in rhvals) 
                        { 
                            confOpts[lineSplits[0]].Add(v); 
                        }
                    }
                    else 
                    {
                        //single value for this option
                        confOpts[lineSplits[0]].Add(lineSplits[1]);  
                    }
                }
                else
                {
                    logger.Error("Invalid option " + lineSplits[0] + " on line " + lnum);
                }
            }

            return rval;
        }
    }
}
