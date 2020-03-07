using System.Collections.Generic;
using System.Linq;
using HamstiBotWPF.Core;

namespace HamstiBotWPF.ViewModels
{
    public class CommandsControlViewModel
    {
        public List<BotCommand> ListCommands => GlobalUnit.botCommands;

        public CommandsControlViewModel()
        {
            
        }
    }
}
