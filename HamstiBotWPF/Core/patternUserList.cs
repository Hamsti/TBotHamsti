namespace HamstiBotWPF.Core
{
    public class PatternUserList
    {
        private string localNickname;
        /// <summary>
        /// Id user in Telegram
        /// </summary>
        public int IdUser { get; set; } 

        /// <summary>
        /// User lock status
        /// </summary>
        public bool IsBlocked { get; set; }
        
        /// <summary>
        /// Local nickname for the bot
        /// </summary>
        public string LocalNickname { get => localNickname ?? "null";
                                      set => localNickname = value; }

        public string IdUser_Nickname => IdUser.ToString() + " | " + (string.IsNullOrWhiteSpace(LocalNickname) ? "null" : LocalNickname); 

        public bool IsUserAdmin => IdUser == Properties.Settings.Default.AdminId;

        public PatternUserList()
        {
            IsBlocked = false;
            localNickname = null;
        }

        //Waring, needed retype class in list: https://stackoverflow.com/questions/580202/how-do-i-override-listts-add-method-in-c
        //Overide add, delete and others methods for more good control users.
    }
}
