using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using TaleWorlds.CampaignSystem;

using DynaCulture.Data;
using System.Text.RegularExpressions;

namespace DynaCulture.Util
{
    internal class FileUtil
    {
        public static T TryLoadSerializedFile<T>(string characterName)
        {
            string path = GetConfigDirectory();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string filename = GetSerializedFileName(characterName);

            if (File.Exists(Path.Combine(path, filename)))
                return Serializator.Deserialize<T>(Path.Combine(path, filename));

            return default;
        }

        public static void SaveSerializedFile(string characterName, object o)
        {
            string path = GetConfigDirectory();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string filename = GetSerializedFileName(characterName);

            Serializator.Serialize(Path.Combine(path, filename), o);
        }

        public static string GetConfigDirectory()
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\Mount and Blade II Bannerlord\Configs\DynaCulture\";
        }

        public static string GetSerializedFileName(string characterName)
        {
            //removing Illegal characters
            string regexSearch = new string(Path.GetInvalidFileNameChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            characterName = r.Replace(characterName, "");

            return characterName + "_CultureConfig.dat";
        }
    }
}
