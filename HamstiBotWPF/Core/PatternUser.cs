using LevelCommand = TBotHamsti.Core.BotLevelCommand.LevelCommand;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;

namespace TBotHamsti.Core
{
    public class PatternUser
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
        public string LocalNickname { get; set; }

        public StatusUser Status { get; set; }
        public LevelCommand CurrentLevel { get; set; }

        public string IdUser_Nickname => Id.ToString() + " | " + (string.IsNullOrWhiteSpace(LocalNickname) ? "null" : LocalNickname); 

        public PatternUser()
        {
            IsBlocked = false;
            IsSetBookmark = false;
            LocalNickname = null;
            Status = StatusUser.None;
            CurrentLevel = BotLevelCommand.RootLevel.NameOfLevel;
        }

        public PatternUser(PatternUser user)
        {
            Id = user.Id;
            IsBlocked = user.IsBlocked;
            IsSetBookmark = user.IsSetBookmark;
            LocalNickname = user.LocalNickname;
            Status = user.Status;
            CurrentLevel = user.CurrentLevel;
        }
    }
}
