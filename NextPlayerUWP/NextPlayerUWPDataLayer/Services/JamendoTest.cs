using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JamendoApi;
using NextPlayerUWPDataLayer.Constants;

namespace NextPlayerUWPDataLayer.Services
{
    public class JamendoTest
    {
        public static async Task Start()
        {
            JamendoApiClient client = new JamendoApiClient(AppConstants.JamendoClientId);
            JamendoApi.ApiCalls.Radios.RadiosCall radiodcall = new JamendoApi.ApiCalls.Radios.RadiosCall();
            
            var a  = await client.CallAsync(radiodcall);
            var b = a.Results.FirstOrDefault();


            JamendoApi.ApiCalls.Radios.RadioStreamCall radiostream = new JamendoApi.ApiCalls.Radios.RadioStreamCall();
            radiostream.Id = new JamendoApi.ApiCalls.Parameters.IdParameter(b.Id);
            var c = await client.CallAsync(radiostream);
            var d = c.Results.FirstOrDefault();
            
        }
    }
}
