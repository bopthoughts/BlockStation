using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockStation
{
    class Utils
    {
        public static string getMissingFile(string dir)
        {
            if (!File.Exists(dir + "white-list.txt"))
            {
                return "white-list.txt";
            }
            if (!File.Exists(dir + "ops.txt"))
            {
                return "ops.txt";
            }
            if (!File.Exists(dir + "server.properties"))
            {
                return "server.properties";
            }
            if (!File.Exists(dir + "pocketmine.yml"))
            {
                return "pocketmine.yml";
            }
            return "";
        }

        public static bool checkServerFolder(string dir)
        {
            if(!File.Exists(dir + "white-list.txt"))
            {
                return false;
            }
            if (!File.Exists(dir + "ops.txt"))
            {
                return false;
            }
            if (!File.Exists(dir + "server.properties"))
            {
                return false;
            }
            if (!File.Exists(dir + "pocketmine.yml"))
            {
                return false;
            }
            return true;
        }

        public static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is millisecods past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string ConvertStringArrayToString(string[] array)
        {
            StringBuilder builder = new StringBuilder();
            foreach(string value in array)
            {
                builder.Append(value);
                builder.Append(" ");
            }
            return builder.ToString();
        }
    }
}
