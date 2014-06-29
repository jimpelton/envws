using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EnvwsLib.Util
{
    public class ConfigParser
    {
        private const int MAX_CONFIG_FILE_LINES = 500;
        private static readonly char[] COMMENT_CHARS = {';','#'};

        private log4net.ILog 
            logger = log4net.LogManager.GetLogger(typeof (ConfigParser));

        private static ConfigParser me = null;

        private IDictionary<string, IList<string>>
            confOpts = new Dictionary<string, IList<string>>();

        public string Filename { get; private set; }

        private ConfigParser() { }

        public static ConfigParser Instance()
        {
            return me ?? (me = new ConfigParser());
        }

        /// <summary>
        /// Get the first option for <code>key</code>.
        /// 
        /// If there is more than one value, then use GetAsList to 
        /// retrieve the entire list of strings.
        /// </summary>
        /// <param name="key">the key to get the option for</param>
        /// <returns>A string that is the value associated with key.</returns>
        public string this[string key]
        {
            get 
            {
                return confOpts[key].ElementAt(0); 
            }
			set
            {
                (confOpts[key] = new List<string>()).Add(value);
            }
        }

        /// <summary>
        /// Retrieve the option for key.
        /// If there is more than one option, then use GetAsList to 
        /// retrieve the entire list of strings.
        /// </summary>
        /// <param name="key">the key to get the option for</param>
        /// <returns>A string that is the value associated with key.</returns>
        public string Get(string key)
        {
            return this[key];
        }

        /// <summary>
        /// Get all values for <code>key</code>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IList<string> GetAsList(string key)
        {
            return confOpts[key];
        }

        /// <summary>
        /// Add an option to the parser. The parser will 
        /// </summary>
        /// <param name="key"></param>
        public ConfigParser AddOpt(string key)
        {
            if (! confOpts.ContainsKey(key))
                confOpts.Add(key, new List<string>());

            return this;
        }

        public void AddOpts(IDictionary<string, IList<string>> opts)
        {
            confOpts = opts;
        }

		public ConfigParser SetDefaultOptValue(string key, string value)
        {
            this[key] = value;

            return this;
        }

		public string GetFormatedOptionsString()
        {
            StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string,IList<string>> p in confOpts)
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
