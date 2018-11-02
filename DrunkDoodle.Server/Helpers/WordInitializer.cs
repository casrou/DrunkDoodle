using System.Collections.Generic;
using System.IO;
using DrunkDoodle.Server.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace DrunkDoodle.Server.Helpers
{
    public class WordInitializer
    {
        public static IEnumerable<Word> InitializeWords(IHostingEnvironment hostingEnvironment)
        {
            string webRootPath = hostingEnvironment.WebRootPath;
            string json = File.ReadAllText(webRootPath + "/words/words.json");
            List<Word> words = JsonConvert.DeserializeObject<List<Word>>(json);
            return words;
        }
    }
}
