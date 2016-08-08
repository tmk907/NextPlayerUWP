namespace NextPlayerUWPDataLayer.CloudStorage
{
    public class CloudAccount
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public int DBId { get; set; }
        public CloudStorageType Type { get; set; }

        public CloudAccount(int DBId, string UserId, CloudStorageType Type, string UserName)
        {
            this.DBId = DBId;
            this.UserId = UserId;
            this.Type = Type;
            this.Username = UserName;
        }
    }
}
