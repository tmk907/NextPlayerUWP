﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Jamendo;
using NextPlayerUWPDataLayer.Enums;

namespace NextPlayerUWPDataLayer.Services
{
    public class JamendoRadioService : IRadioService
    {
        private JamendoClient client;

        public JamendoRadioService()
        {
            client = new JamendoClient();
        }

        public async Task<RadioItem> GetRadio(int id)
        {
            var r = await client.GetRadio(id);
            RadioItem radio = new RadioItem(id, RadioType.Jamendo);
            radio.Name = r.DisplayName;
            radio.ImagePath = r.Image;
            return radio;
        }

        public async Task<List<RadioItem>> GetRadios()
        {
            List<RadioItem> list = new List<RadioItem>();
            var radios = await client.GetRadios();
            foreach(var r in radios)
            {
                RadioItem item = new RadioItem(r.Id, RadioType.Jamendo);
                item.Name = r.DisplayName;
                item.ImagePath = r.Image;
                list.Add(item);
            }
            return list;
        }

        public async Task<TrackStream> GetRadioStream(int id)
        {
            var s = await client.GetStream(id);
            TrackStream ts = new TrackStream(s.PlayingNow.TrackName, s.PlayingNow.ArtistName, s.PlayingNow.AlbumName, s.PlayingNow.TrackImage, s.StreamUrl, s.CallMeBack);
            return ts;
        }
    }
}
