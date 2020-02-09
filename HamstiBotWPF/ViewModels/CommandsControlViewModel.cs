using System.Collections.Generic;
using HamstiBotWPF.Core;

namespace HamstiBotWPF.ViewModels
{
    public class CommandsControlViewModel
    {
        public List<BotCommand> ListCommands { get { return GlobalUnit.botCommands; }}

        public CommandsControlViewModel()
        {
            
            //GlobalUnit.botCommands.ForEach(botCom => ListCommands.Add(new MenuItem()
            //{
            //    Header = botCom.ExampleCommand,
            //    Foreground = botCom.VisibleForUsers ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 216, 162))
            //}));
        }
    }
}
