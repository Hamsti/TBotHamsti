using System;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// To work with a list of authorized users
    /// </summary>
    public static class RepUsers
    {
        /// <summary>
        /// Add all users to the list of authorized users.
        /// </summary>
        public static void AddAllUsers()
        {
            ////AdminUser
            //GlobalUnit.authUsers.Add(new Core.patternUserList {
            //    IdUser = Properties.Settings.Default.AdminId
            //});

            ////OtherUsers
            //GlobalUnit.authUsers.Add(new Core.patternUserList {
            //    IdUser = 492113551, IsBlocked = true, LocalNickname = "Эндрю"
            //});

            loadFromJson();
        }

        public static void loadFromJson()
        {
            try
            {
                GlobalUnit.authUsers = System.IO.File.Exists("AuthUsers.json") ? JsonConvert.DeserializeObject<System.Collections.Generic.List<Core.PatternUser>>(System.IO.File.ReadAllText("AuthUsers.json")) 
                    : new System.Collections.Generic.List<Core.PatternUser>() { new Core.PatternUser { IdUser = Properties.Settings.Default.AdminId } };
            }
            catch (Exception ex)
            {
                MessageBox.Show("При загрузке данных, произошла ошибка: " + ex.Message);
            }
        }

        public static void saveInJson()
        {
            try
            {
                if (GlobalUnit.authUsers == null) return;
               
                System.IO.File.WriteAllText("AuthUsers.json", JsonConvert.SerializeObject(GlobalUnit.authUsers));
            }
            catch (Exception ex)
            {
                MessageBox.Show("При сохранении данных, произошла ошибка: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks if this user is an administrator.
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool isHaveAccessAdmin(int userId) => GlobalUnit.authUsers.Exists(idExists => userId == Properties.Settings.Default.AdminId);

        /// <summary>
        /// Checks if this user is in the list of authorized users and not IsBlocked.
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool isAuthNotIsBlockedUser(int userId) => GlobalUnit.authUsers.Exists(user => user.IdUser == userId && user.IsBlocked == false);

        /// <summary>
        /// Checks if this user is in the list of authorized users.
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool isAuthUser(int userId) => GlobalUnit.authUsers.Exists(user => user.IdUser == userId);
    }
}
