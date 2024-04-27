using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.IO.Stores;

namespace FunkinSharp.Game.Core.Stores
{
    /// <summary>
    ///     ResourceStore designed for caching JSON Files.
    /// </summary>
    public class JSONStore : ResourceStore<byte[]>
    {
        protected readonly Dictionary<string, string> Cache = []; // Holds the file content

        public JSONStore(IResourceStore<byte[]> store = null) : base(store)
        {
            AddExtension("json");
        }

        public T Get<T>(string name)
        {
            if (!name.EndsWith(".json"))
                name += ".json";

            if (Cache.ContainsKey(name))
                return JsonConvert.DeserializeObject<T>(Cache[name]);

            byte[] rawB = Get(name);
            string content = Cache[name] = Encoding.UTF8.GetString(rawB);

            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
