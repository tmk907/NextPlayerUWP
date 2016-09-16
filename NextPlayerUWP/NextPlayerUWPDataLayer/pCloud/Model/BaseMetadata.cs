using Newtonsoft.Json;
using System.Collections.Generic;

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

        [JsonProperty("deletedfileid")]
        public string DeletedFileId { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("category")]
        public int Category { get; set; }

        [JsonProperty("thumb")]
        public bool Thumb { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("contenttype")]
        public string ContentType { get; set; }

        [JsonProperty("hash")]
        public string hash { get; set; }

        [JsonProperty("contents")]
        public IList<BaseMetadata> Contents { get; set; }

        [JsonProperty("isdeleted")]
        public string IsDeleted { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
