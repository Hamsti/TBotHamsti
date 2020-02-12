namespace HamstiBotWPF.Core
{
    public class PatternUser
    {
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
        /// Local nickname for the bot
        /// </summary>
        public string LocalNickname { get; set; }

        public string IdUser_Nickname => IdUser.ToString() + " | " + (string.IsNullOrWhiteSpace(LocalNickname) ? "null" : LocalNickname); 

        public bool IsUserAdmin => IdUser == Properties.Settings.Default.AdminId;

        public PatternUser()
        {
            IsBlocked = false;
            LocalNickname = null;
        }

        //Waring, needed retype class in list: https://stackoverflow.com/questions/580202/how-do-i-override-listts-add-method-in-c
        //Overide add, delete and others methods for more good control users.
    }
}
