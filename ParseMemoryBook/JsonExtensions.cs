using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ParseMemoryBook
{
    public static class JsonExtensions
    {
        public static T ParseJson<T>(this string value, T fallbackValue)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception)
            {
                return fallbackValue;
            }
        }
    }
}
