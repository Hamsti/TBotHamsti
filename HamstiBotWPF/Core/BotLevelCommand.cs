﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace HamstiBotWPF.Core
{
    public class BotLevelCommand : BotCommand
    {
        /// <summary>
        /// Command for change to up level
        /// </summary>
        public const string TOPREVLEVEL = "/up";

        public enum LevelCommand
        {
            Root,
            Messages,
            ControlUsers,
            ControlBot,
            ControlPC
        }

        public new int CountArgsCommand => base.CountArgsCommand;
        public new string ExampleCommand => Command.ToUpper();

        /// <summary>
        /// Previos (parrent) level for commands
        /// </summary>
        public LevelCommand ParrentLevel { get; private set; }

        public BotLevelCommand(LevelCommand NameOfLevel, LevelCommand ParrentLevel = LevelCommand.Root)
        {
            Execute += (BotCommandStructure command, Message message) => ExecLevelUp(message);
            base.NameOfLevel = NameOfLevel;
            this.ParrentLevel = ParrentLevel;
            Command = "/" + NameOfLevel.ToString();
            base.CountArgsCommand = 0;
        }

        /// <summary>
        /// Processing commands for change current level
        /// </summary>
        public new Action<BotCommandStructure, Message> Execute { get; } = async (model, message) =>
        {
            //first execute fuction "ExecLevelUp" (added in initialization BotLevelCommand), for next exec constraction typed down.
            if (await WhenLevelIsRoot(message)) return;

            //Change currentLevel on LevelCommand
            foreach (LevelCommand level in Enum.GetValues(typeof(LevelCommand)))
            {
                if (("/" + level.ToString().ToLower()).Equals(model.Command))
                {
                    GlobalUnit.currentLevelCommand = level;
                    SendMessageWhenLevelChanges(message);
                }
            }
        };

        /// <summary>
        /// Execute if have error when change level commands
        /// </summary>
        public new Action<BotCommandStructure, Message> OnError { get; } = async (model, message) =>
        {
            await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Произошла ошибка при изменении уровня команд.\nСписок комманд: /help");
        };

        private static async void SendMessageWhenLevelChanges(Message message)
        {
            string messageWhenLevelChanges = "Current level: " + GlobalUnit.currentLevelCommand + "\n\nList of commands:\n";

            if (LogicRepository.RepUsers.IsHaveAccessAdmin(message.From.Id))
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, messageWhenLevelChanges + LogicRepository.RepBotActions.HelpForAdmin);
            else
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, messageWhenLevelChanges + LogicRepository.RepBotActions.HelpForUsers);
        }

        private static async Task<bool> WhenLevelIsRoot(Message message)
        {
            if (message.Text.ToLower() == TOPREVLEVEL && GlobalUnit.currentLevelCommand == LevelCommand.Root)
            {
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Вы находитесь на начальном уровне.");
                return true;
            }
            return false;
        }

        private void ExecLevelUp(Message message)
        {
            if (message.Text.ToLower() == TOPREVLEVEL && GlobalUnit.currentLevelCommand > LevelCommand.Root)
            {
                GlobalUnit.currentLevelCommand = ParrentLevel;
                SendMessageWhenLevelChanges(message);
            }
        }
    }
}
