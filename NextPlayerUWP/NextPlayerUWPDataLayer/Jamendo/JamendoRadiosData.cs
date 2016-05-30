using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Jamendo.Models;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Jamendo
{
    public class JamendoRadiosData
    {
        private JamendoClient client;

        public JamendoRadiosData()
        {
            client = new JamendoClient();
        }

        public async Task<RadioStream> GetRadioStream(string radioName)
        {
            var stream = await client.GetStream(radioName);
            return stream;
        }

        public async Task<RadioStream> GetRadioStream(int id)
        {
            var stream = await client.GetStream(id);
            return stream;
        }

        public RadioItem GetRadioItemFromStream(RadioStream stream)
        {
            RadioItem radio = new RadioItem(stream.Id, RadioType.Jamendo);
            radio.Name = stream.Name;
            radio.PlayingNowTitle = stream.PlayingNow.TrackName;
            radio.PlayingNowAlbum = stream.PlayingNow.AlbumName;
            radio.PlayingNowArtist = stream.PlayingNow.ArtistName;
            radio.PlayingNowImagePath = stream.PlayingNow.TrackImage;
            radio.StreamUrl = stream.StreamUrl;
            radio.RemainingTime = stream.CallMeBack;
            radio.StreamUpdatedAt = DateTime.Now;
            return radio;
        }

        public int GetRemainingTime(RadioStream stream)
        {
            return stream.CallMeBack;
        }
    }
}
