using NextPlayerWinUIDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerWinUIDataLayer.Services
{
    public interface IRadioService
    {
        Task<RadioItem> GetRadio(int id);
        Task<List<RadioItem>> GetRadios();
        Task<TrackStream> GetRadioStream(int id);
    }
}
