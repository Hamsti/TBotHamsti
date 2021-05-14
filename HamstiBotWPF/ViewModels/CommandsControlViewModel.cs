using System.Collections.Generic;
using System.Linq;
using TBotHamsti.Core;
using TBotHamsti.LogicRepository;

namespace TBotHamsti.ViewModels
{
    public class CommandsControlViewModel
    {
        public List<BotCommand> ListCommands => RepCommands.botCommands;
        //public List<BotLevelCommand> ListLevels => (BotLevelCommand)RepCommands.botCommands.Where(s => s is BotLevelCommand).ToList();

        public CommandsControlViewModel() { }
    }
}
