using NextPlayerUWPDataLayer.Enums;
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

        public async Task<TrackStream> GetRadioStream(string radioName)
        {
            var s = await client.GetStream(radioName);
            TrackStream ts = new TrackStream(s.PlayingNow.TrackName, s.PlayingNow.ArtistName, s.PlayingNow.AlbumName, s.PlayingNow.TrackImage, s.StreamUrl, s.CallMeBack, s.Id, s.Name);
            return ts;
        }

        public async Task<TrackStream> GetRadioStream(int id)
        {
            var s = await client.GetStream(id);
            TrackStream ts = new TrackStream(s.PlayingNow.TrackName, s.PlayingNow.ArtistName, s.PlayingNow.AlbumName, s.PlayingNow.TrackImage, s.StreamUrl, s.CallMeBack, id, s.Name);
            return ts;
        }

        public RadioItem GetRadioItemFromStream(TrackStream stream)
        {
            RadioItem radio = new RadioItem(stream.Id, RadioType.Jamendo);
            radio.Name = stream.RadioName;
            radio.PlayingNowTitle = stream.Title;
            radio.PlayingNowAlbum = stream.Album;
            radio.PlayingNowArtist = stream.Artist;
            radio.PlayingNowImagePath = stream.CoverUri;
            radio.StreamUrl = stream.Url;
            radio.RemainingTime = stream.RemainingSeconds;

            radio.StreamUpdatedAt = DateTime.Now;
            return radio;
        }

        public int GetRemainingSeconds(TrackStream stream)
        {
            return stream.RemainingSeconds;
        }
    }
}
