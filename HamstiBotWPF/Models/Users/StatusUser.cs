using TBotHamsti.Models.Commands;

namespace TBotHamsti.Models.Users
{
    /// <summary>
    /// An <see cref="Users.User"/>'s privilege level status and a minimum <see cref="ICommand.StatusUser"/> for execution
    /// </summary>
    /// <remarks>
    /// <see cref="User.Status"/> = <see cref="StatusUser"/> for changing status of the <see cref="Users.User"/><para/>
    /// <see cref="ICommand.StatusUser"/> for setting minimum <see cref="User.Status"/> to execution
    /// </remarks>
    public enum StatusUser
    {
        /// <summary>
        /// An <see cref="User"/> without access rights, can only execute basic <see cref="ICommand"/>
        /// </summary>
        /// <remarks>
        /// Minimum status, setting by default to an unknown user
        /// </remarks>
        None,

        /// <summary>
        /// The standard <see cref="StatusUser"/> of all users of the bot
        /// </summary>
        /// <remarks>
        /// <see cref="Users.User"/> can execute <see cref="ICommand"/> for <see cref="User"/> (including <see cref="None"/> too)
        /// </remarks>
        User,

        /// <summary>
        /// The status of users who have almost full access to the bot commands
        /// </summary>
        /// <remarks>
        /// <see cref="Users.User"/> can execute <see cref="ICommand"/> for <see cref="Moder"/> (excluding <see cref="ICommand.StatusUser"/> = <see cref="Admin"/>)
        /// </remarks>
        Moder,

        /// <summary>
        /// Full access to the bot commands, <see cref="Users.User"/> can execute any <see cref="ICommand"/>
        /// </summary>
        Admin
    }
}
