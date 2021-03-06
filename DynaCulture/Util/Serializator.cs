﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DynaCulture.Util
{
    internal static class Serializator
    {
        private static BinaryFormatter _bin = new BinaryFormatter();

        public static void Serialize(string pathOrFileName, object objToSerialise)
        {
            using (Stream stream = File.Open(pathOrFileName, FileMode.Create))
                _bin.Serialize(stream, objToSerialise);
        }

        public static T Deserialize<T>(string pathOrFileName)
        {
            T items;

            using (Stream stream = File.Open(pathOrFileName, FileMode.Open))
                items = (T)_bin.Deserialize(stream);

            return items;
        }
    }
}
