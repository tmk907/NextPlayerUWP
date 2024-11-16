using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class BaseMetadata
    {
        [JsonProperty("parentfolderid")]
        public int ParentFolderId { get; set; }

        [JsonProperty("isfolder")]
        public bool IsFolder { get; set; }

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
        public string Id { get; set; }

        [JsonProperty("folderid")]
        public long FolderId { get; set; }

        [JsonProperty("fileid")]
        public long FileId { get; set; }

        [JsonProperty("deletedfileid")]
        public string DeletedFileId { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("modified")]
        public DateTime Modified { get; set; }

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
        public string Hash { get; set; }

        [JsonProperty("contents")]
        public IList<BaseMetadata> Contents { get; set; }

        [JsonProperty("isdeleted")]
        public string IsDeleted { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
