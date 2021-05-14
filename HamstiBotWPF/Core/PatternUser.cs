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
        public int IdUser { get; set; }  

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

        public string IdUser_Nickname => IdUser.ToString() + " | " + (string.IsNullOrWhiteSpace(LocalNickname) ? "null" : LocalNickname); 

        public PatternUser()
        {
            IsBlocked = false;
            IsSetBookmark = false;
            LocalNickname = null;
            Status = StatusUser.NotDefined;
        }

        public PatternUser(PatternUser user)
        {
            IdUser = user.IdUser;
            IsBlocked = user.IsBlocked;
            IsSetBookmark = user.IsSetBookmark;
            LocalNickname = user.LocalNickname;
            Status = user.Status;
        }
    }
}
