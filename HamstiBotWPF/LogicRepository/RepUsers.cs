using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using HamstiBotWPF.Core;

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
        public static void Refresh()
        {
            try
            {
               GlobalUnit.authUsers = System.IO.File.Exists("AuthUsers.json") ? JsonConvert.DeserializeObject<ObservableCollection<PatternUser>>(System.IO.File.ReadAllText("AuthUsers.json")) 
                    : new ObservableCollection<PatternUser>() { new PatternUser { IdUser = Properties.Settings.Default.AdminId } };
            }
            catch (Exception ex)
            {
                MessageBox.Show("При загрузке данных, произошла ошибка: " + ex.Message);
            }
        }

        public static void Update()
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

        public static void Sort() => GlobalUnit.authUsers = new ObservableCollection<PatternUser>(GlobalUnit.authUsers.OrderByDescending(o => o.IsUserAdmin).ThenBy(t1 => t1.IsBlocked).ThenBy(t2 => t2.IdUser));

        /// <summary>
        /// Checks if this user is an administrator.
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool IsHaveAccessAdmin(int userId) => GlobalUnit.authUsers.Count(_ => userId == Properties.Settings.Default.AdminId) > 0;

        /// <summary>
        /// Checks if this user is in the list of authorized users and not IsBlocked.
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool IsAuthNotIsBlockedUser(int userId) => GlobalUnit.authUsers.Count(user => user.IdUser == userId && user.IsBlocked == false) > 0;

        /// <summary>
        /// Checks if this user is in the list of authorized users.
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool IsAuthUser(int userId) => GlobalUnit.authUsers.Count(user => user.IdUser == userId) > 0;
    }
}
