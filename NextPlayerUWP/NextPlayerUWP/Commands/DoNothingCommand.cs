using System.Threading.Tasks;

namespace NextPlayerUWP.Commands
{
    public class DoNothingCommand : IGenericCommand
    {
        public async Task Excecute()
        {
            await Task.CompletedTask;
        }
    }
}
