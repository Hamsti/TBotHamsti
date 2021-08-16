using TBotHamsti.Models.Commands;

namespace TBotHamsti.Models.Users
{
    public class User
    {
        /// <remarks>
        /// Used on the page <see cref="Views.ChangeUserDataPage"/> (check links before deleting)
        /// </remarks>
        /// <value>
        /// Maximum length <see cref="Telegram.Bot.Types.User.Id"/> in the Telegram
        /// </value>
        public static int IdMaxLenght => 9;

        /// <value>
        /// <see cref="Telegram.Bot.Types.User.Id"/> in the Telegram
        /// </value>
        public int Id { get; set; }

        /// <value>
        /// An <see cref="User"/>'s local lock status
        /// </value>
        public bool IsBlocked { get; set; }

        /// <value>
        /// Bookmark for set to top of the <see cref="UsersFunc.AuthUsers"/>
        /// </value>
        public bool IsSetBookmark { get; set; }

        /// <value>
        /// An <see cref="User"/>'s local nickname for the bot
        /// </value>
        public string Username { get; set; }

        /// <value>
        /// An <see cref="User"/>'s privilege level status
        /// </value>
        public StatusUser Status { get; set; }

        /// <value>
        /// The current <see cref="BotLevelCommand"/> used by the <see cref="User"/>
        /// </value>
        public LevelCommand CurrentLevel { get; set; }

        /// <value>
        /// Returns the concatenated formatted string <see cref="User.Id"/> and <see cref="User.Username"/>
        /// </value>
        public string Id_Username => Id.ToString() + " | " + (string.IsNullOrWhiteSpace(Username) ? "null" : Username);

        /// <summary>
        /// Creating a completely empty <see cref="User"/> with a default <see cref="BotLevelCommand"/> and <see cref="StatusUser"/>
        /// </summary>
        public User()
        {
            IsBlocked = false;
            IsSetBookmark = false;
            Username = null;
            Status = StatusUser.None;
            CurrentLevel = BotLevelCommand.RootLevel.NameOfLevel;
        }

        /// <summary>
        /// Creating a copy of the <paramref name="user"/> with a new <see cref="User"/> link
        /// </summary>
        /// <param name="user"><paramref name="user"/> to copy</param>
        public User(User user)
        {
            Id = user.Id;
            IsBlocked = user.IsBlocked;
            IsSetBookmark = user.IsSetBookmark;
            Username = user.Username;
            Status = user.Status;
            CurrentLevel = user.CurrentLevel;
        }
    }
}
