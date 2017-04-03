using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace NextPlayerUWP.Extensions
{
    public enum ExtensionType
    {
        Lyrics,
        AlbumInfo,
        ArtistInfo,
    }

    public class AppExtensionInfo
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string ServiceName { get; set; }
        public string PackageName { get; set; }
        public ExtensionType Type { get; set; }
        public int Priority { get; set; }
        public bool Enabled { get; set; }
    }

    public interface IExtensionsHelper
    {
        Task<List<AppExtensionInfo>> GetExtensionsInfo();
    }

    public class ExtensionsService
    {
        public async Task<ValueSet> InvokeExtension(AppExtensionInfo info, Dictionary<string, object> parameters)
        {
            var message = new ValueSet();
            foreach (var kv in parameters)
            {
                message.Add(kv);
            }

            var serviceConnection = new AppServiceConnection();
            serviceConnection.AppServiceName = info.ServiceName;
            serviceConnection.PackageFamilyName = info.PackageName;
            var connectionStatus = await serviceConnection.OpenAsync();

            using (serviceConnection)
            {
                if (connectionStatus != AppServiceConnectionStatus.Success)
                {
                    System.Diagnostics.Debug.WriteLine("InvokeExtension connectionStatus:{0}", connectionStatus);
                    return null;
                }

                var response = await serviceConnection.SendMessageAsync(message);
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    string status = response.Message[Responses.Status] as string;
                    if (status == "OK")
                    {
                        return response.Message;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("InvokeExtension status:{0}", status);
                        return null;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("InvokeExtension status:{0}", response.Status);
                    return null;
                }
            }
        }
    }
}
