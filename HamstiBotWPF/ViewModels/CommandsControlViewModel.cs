using System.Collections.Generic;
using TBotHamsti.Core;
using TBotHamsti.LogicRepository;

namespace TBotHamsti.ViewModels
{
    public class CommandsControlViewModel
    {
        public List<BotCommand> ListCommands => RepCommands.botCommands;

        public CommandsControlViewModel() { }
    }
}
