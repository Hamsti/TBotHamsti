﻿namespace HamstiBotWPF.Core
{
    public class patternUserList
    {
        /// <summary>
        /// Id user in Telegram
        /// </summary>
        public int idUser { get; set; } 

        /// <summary>
        /// User lock status
        /// </summary>
        public bool locked { get; set; } = false;

        /// <summary>
        /// Local nickname for the bot
        /// </summary>
        public string localNickname { get; set; } = null;

        public string idUser_Nickname { get { return idUser.ToString() + " | " + (string.IsNullOrWhiteSpace(localNickname) ? "null" : localNickname); } }

        public bool IsUserAdmin { get { return idUser == Properties.Settings.Default.AdminId; } }
    }
}
