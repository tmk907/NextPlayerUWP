using JamendoApi;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Model;
using System.Linq;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Jamendo
{
    public class JamendoRadiosData
    {
        private JamendoApiClient client;

        public JamendoRadiosData()
        {
            client = new JamendoApi.JamendoApiClient(AppConstants.JamendoClientId);
        }

        public async Task<JamendoApi.ApiEntities.Radios.StreamRadio> GetRadioStream(string radioName)
        {
            JamendoApi.ApiCalls.Radios.RadioStreamCall radiostream = new JamendoApi.ApiCalls.Radios.RadioStreamCall();
            radiostream.Name = new JamendoApi.ApiCalls.Parameters.NameParameter(radioName);
            var streamRadio = await client.CallAsync(radiostream);
            if (streamRadio !=null && streamRadio.Headers.Status == JamendoApi.ApiEntities.Headers.ResponseStatus.Success)
            {
                var stream = streamRadio.Results.FirstOrDefault();
                return stream;
            }
            return null;
        }

        public async Task<JamendoApi.ApiEntities.Radios.StreamRadio> GetRadioStream(int id)
        {
            JamendoApi.ApiCalls.Radios.RadioStreamCall radiostream = new JamendoApi.ApiCalls.Radios.RadioStreamCall();
            radiostream.Id = new JamendoApi.ApiCalls.Parameters.IdParameter((uint)id);
            var streamRadio = await client.CallAsync(radiostream);
            if (streamRadio != null && streamRadio.Headers.Status == JamendoApi.ApiEntities.Headers.ResponseStatus.Success)
            {
                var stream = streamRadio.Results.FirstOrDefault();
                return stream;

            }
            return null;
        }

        public RadioItem GetRadioItemFromStream(JamendoApi.ApiEntities.Radios.StreamRadio stream)
        {
            RadioItem radio = new RadioItem((int)stream.Id, Enums.RadioType.Jamendo, stream.Stream);
            radio.Name = stream.Name;
            radio.PlayingNowTitle = stream.PlayingNow.Name;
            radio.PlayingNowAlbum = stream.PlayingNow.AlbumName;
            radio.PlayingNowArtist = stream.PlayingNow.ArtistName;
            radio.PlayingNowImagePath = stream.PlayingNow.Image;
            return radio;
        }

        public int GetRemainingTime(JamendoApi.ApiEntities.Radios.StreamRadio stream)
        {
            return (int)stream.CallMeBack;
        }
    }
}
