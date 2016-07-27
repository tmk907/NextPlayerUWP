using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Jamendo;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Common
{
    public class Test2
    {
        public async Task test1()
        {
            await GetJamendoRadios2();
        }

        private async Task<List<RadioItem>> GetJamendoRadios()
        {
            List<RadioItem> jamendoRadios = new List<RadioItem>();
            //JamendoApi.JamendoApiClient client = new JamendoApi.JamendoApiClient(AppConstants.JamendoClientId);
            //JamendoApi.ApiCalls.Radios.RadiosCall getRadios = new JamendoApi.ApiCalls.Radios.RadiosCall();
            //getRadios.ImageSize = new JamendoApi.ApiCalls.Parameters.ImageSizeParameter<JamendoApi.ApiCalls.Parameters.RadioImageSize>(JamendoApi.ApiCalls.Parameters.RadioImageSize.Px150);
            //var radiosList = await client.CallAsync(getRadios);
            //if (radiosList != null && radiosList.Headers.Status == JamendoApi.ApiEntities.Headers.ResponseStatus.Success)
            //{
            //    foreach (var item in radiosList.Results)
            //    {
            //        RadioItem r = new RadioItem((int)item.Id, NextPlayerUWPDataLayer.Enums.RadioType.Jamendo, "");
            //        r.Name = item.DisplayName;
            //        r.ImagePath = item.Image;
            //        r.PlayingNowTitle = "";
            //        r.PlayingNowAlbum = "";
            //        r.PlayingNowArtist = "";
            //        r.RemainingTime = 0;
            //        jamendoRadios.Add(r);
            //    }
            //}
            return jamendoRadios;
        }

        private async Task GetJamendoRadios2()
        {
            JamendoClient client = new JamendoClient();
            var r = await client.GetRadios();
        }
    }
}
