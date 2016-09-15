using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class BaseMetadata
    {
        [JsonProperty("parentfolderid")]
        public int ParentFolderId { get; set; }

        [JsonProperty("ismine")]
        public bool IsMine { get; set; }

        [JsonProperty("canread")]
        public bool CanRead { get; set; }

        [JsonProperty("canmodify")]
        public bool CanModify { get; set; }

        [JsonProperty("candelete")]
        public bool CanDelete { get; set; }

        [JsonProperty("cancreate")]
        public bool CanCreate { get; set; }

        [JsonProperty("isshared")]
        public bool IsShared { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("folderid")]
        public int FolderId { get; set; }

        [JsonProperty("fileid")]
        public int FileId { get; set; }
    }
}
