using TBotHamsti.Models.Commands;

namespace TBotHamsti.Models.Users
{
    public class User
    {
        /// <summary>
        /// Used on page "ChangeUserDataPage"
        /// </summary>
        public static int MaxLenghtIdUser => 9;

        /// <summary>
        /// Id user in Telegram
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User lock status
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Bookmark for set to top of the list
        /// </summary>
        public bool IsSetBookmark { get; set; }

        /// <summary>
        /// Local nickname for the bot
        /// </summary>
        public string Username { get; set; }
        public StatusUser Status { get; set; }
        public LevelCommand CurrentLevel { get; set; }
        public string Id_Username => Id.ToString() + " | " + (string.IsNullOrWhiteSpace(Username) ? "null" : Username);

        public User()
        {
            IsBlocked = false;
            IsSetBookmark = false;
            Username = null;
            Status = StatusUser.None;
            CurrentLevel = BotLevelCommand.RootLevel.NameOfLevel;
        }

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
