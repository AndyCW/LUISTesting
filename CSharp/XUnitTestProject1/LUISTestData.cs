using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace XUnitTestProject1
{
    public class Entity
    {
        [JsonProperty(PropertyName ="entity")]
        public string EntityName { get; set; }

        [JsonProperty(PropertyName = "startPos")]
        public int StartPosition { get; set; }

        [JsonProperty(PropertyName ="endPos")]
        public int EndPosition { get; set; }
    }

    public class LuisTestCaseData
    {
        [JsonProperty(PropertyName ="text")]
        public string Utterance { get; set; }

        [JsonProperty(PropertyName = "intent")]
        public string Intent { get; set; }

        [JsonProperty(PropertyName = "entities")]
        public List<Entity> Entities { get; set; }
    }
}
