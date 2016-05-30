using NextPlayerUWPDataLayer.Constants;

namespace NextPlayerUWPDataLayer.Jamendo
{
    public class UriBuilder
    {
        public const string BaseUrl = "https://api.jamendo.com/v3.0";

        public string GetRadios()
        {
            return $"{BaseUrl}/radios/?client_id={AppConstants.JamendoClientId}&format=json";
        }

        public string GetRadio(int id)
        {
            return $"{BaseUrl}/radios/?client_id={AppConstants.JamendoClientId}&format=json&id={id}";
        }

        public string GetStream(int id)
        {
            return $"{BaseUrl}/radios/stream/?client_id={AppConstants.JamendoClientId}&format=json&id={id}";
        }

        public string GetStream(string name)
        {
            return $"{BaseUrl}/radios/stream/?client_id={AppConstants.JamendoClientId}&format=json&name={name}";
        }
    }
}
