using System.Collections.Generic;
using TBotHamsti.Models;
using TBotHamsti.Models.Commands;

namespace TBotHamsti.ViewModels
{
    public class CommandsControlViewModel
    {
        public IList<ICommand> ListCommands => CollectionCommands.RootLevel.CommandsOfLevel;

        public CommandsControlViewModel() { }
    }
}
