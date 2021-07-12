using System;

namespace TBotHamsti.Models.Commands
{
    [Flags]
    public enum LevelCommand
    {
        None = 0,
        Root = 1,
        Messages = 1 << 1,
        Users = 1 << 2,
        Bot = 1 << 3,
        PC = 1 << 4,
        All = ~(~0 << 5)
    }
}