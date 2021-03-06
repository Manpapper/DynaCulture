﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DynaCulture.Data;

using System.IO;
using TaleWorlds.CampaignSystem;

namespace DynaCulture.Util
{
    internal class FileUtil
    {
        public static T TryLoadSerializedFile<T>(string characterName)
        {
            try
            {
                string path = GetConfigDirectory();

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string filename = GetSerializedFileName(characterName);

                if (File.Exists(Path.Combine(path, filename)))
                    return Serializator.Deserialize<T>(Path.Combine(path, filename));
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Windows.Forms.MessageBox.Show("Failed to load or deserialize saved culture file: " + ex.Message);
            }

            return default;
        }

        public static void SaveSerializedFile(string characterName, object o)
        {
            try
            {
                string path = GetConfigDirectory();

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string filename = GetSerializedFileName(characterName);

                Serializator.Serialize(Path.Combine(path, filename), o);
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Windows.Forms.MessageBox.Show("Failed to save or serialize culture file: " + ex.Message);
            }
        }

        public static string GetConfigDirectory()
        {
            return $@"C:\Users\{Environment.UserName}\Documents\Mount and Blade II Bannerlord\Configs\DynaCulture\";
        }

        public static string GetSerializedFileName(string characterName)
        {
            return characterName + "_CultureConfig.dat";
        }
    }
}
