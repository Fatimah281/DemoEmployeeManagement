﻿using System.Collections;
using System.Text.Json;

namespace ClientLibrary.Helpers
{
    public class Serializations
    {
        public static string SerializObj<T>(T modelObject) => JsonSerializer.Serialize(modelObject);
        public static T DeserializeJsonString<T>(string jsonString) => JsonSerializer.Deserialize<T>(jsonString);
        public static IList<T> DeserializeJsonStringList<T>(string jsonString) => (IList<T>)JsonSerializer.Deserialize<IList>(jsonString);

    }
}
