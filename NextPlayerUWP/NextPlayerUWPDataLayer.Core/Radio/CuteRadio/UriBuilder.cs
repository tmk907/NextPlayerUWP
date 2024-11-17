namespace NextPlayerUWPDataLayer.Radio.CuteRadio
{
    public class UriBuilder
    {
        public const string BaseUrl = "http://marxoft.co.uk/api/cuteradio";

        public string GetCountries(int offset = 0)
        {
            return $"{BaseUrl}/countries?limit=20&sort=name&sortDescending=false&offset={offset}";
        }

        public string GetGenres(int offset = 0)
        {
            return $"{BaseUrl}/genres?limit=20&sort=name&sortDescending=false&offset={offset}";
        }

        public string GetLanguages(int offset = 0)
        {
            return $"{BaseUrl}/languages?limit=20&sort=name&sortDescending=false&offset={offset}";
        }

        public string GetStations(int offset = 0)
        {
            return $"{BaseUrl}/stations?limit=20&sort=name&sortDescending=false&offset={offset}";
        }

        public string GetStation(int id)
        {
            return $"{BaseUrl}/stations/{id}";
        }

        public string SearchCountries(string name)
        {
            return $"{BaseUrl}/countries?search={name}";
        }

        public string SearchGenres(string name)
        {
            return $"{BaseUrl}/genres?search={name}";
        }

        public string SearchLanguages(string name)
        {
            return $"{BaseUrl}/languages?search={name}";
        }

        public string SearchStations(string name, int offset)
        {
            return $"{BaseUrl}/stations?limit=20&sort=title&sortDescending=false&approved=1&search={name}&offset={offset}";
        }

        public string GetStationsByCountry(string name, int offset)
        {
            return $"{BaseUrl}/stations?limit=20&sort=title&sortDescending=false&country={name}&approved=1&offset={offset}";
        }

        public string GetStationsByGenre(string name, int offset)
        {
            return $"{BaseUrl}/stations?limit=20&sort=title&sortDescending=false&genre={name}&approved=1&offset={offset}";
        }

        public string GetStationsByLanguage(string name, int offset)
        {
            return $"{BaseUrl}/stations?limit=20&sort=title&sortDescending=false&language={name}&approved=1&offset={offset}";
        }

    }
}
