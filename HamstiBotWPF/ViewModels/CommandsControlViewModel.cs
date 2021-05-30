using System.Collections.Generic;
using TBotHamsti.Core;
using TBotHamsti.LogicRepository;

namespace TBotHamsti.ViewModels
{
    public class CommandsControlViewModel
    {
        public IList<ITCommand> ListCommands => CollectionCommands.RootLevel.CommandsOfLevel;

        public CommandsControlViewModel() { }
    }
}
