using System.Collections.Generic;
using HamstiBotWPF.Core;
using HamstiBotWPF.LogicRepository;

namespace HamstiBotWPF.ViewModels
{
    public class CommandsControlViewModel
    {
        public List<BotCommand> ListCommands => RepCommands.botCommands;

        public CommandsControlViewModel() { }
    }
}
