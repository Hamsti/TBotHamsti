using System.Collections.Generic;
using System.Linq;
using TBotHamsti.Core;
using TBotHamsti.LogicRepository;

namespace TBotHamsti.ViewModels
{
    public class CommandsControlViewModel
    {
        public IList<ITCommand> ListCommands => CollectionCommands.RootLevel.CommandsOfLevel;
        //public List<BotLevelCommand> ListLevels => (BotLevelCommand)CollectionCommands.Commands.Where(s => s is BotLevelCommand).ToList();

        public CommandsControlViewModel() { }
    }
}
