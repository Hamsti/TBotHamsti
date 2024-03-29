using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using TBotHamsti.Properties;
using Telegram.Bot.Types;
using static TBotHamsti.Models.ExecutionBot;
using BotCommand = TBotHamsti.Models.Commands.BotCommand;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Tests
{
    [TestFixture]
    public class ExecuteLaunchBot
    {
        private ObservableCollection<User> AuthUsers;
        private static IList<ICommand> CollectionCommands;

        [SetUp]
        public void Setup()
        {
            Models.CollectionCommands.Init();
            CollectionCommands = Models.CollectionCommands.RootLevel.CommandsOfLevel;

            string JsonFileName = Settings.Default.JsonFileName;
            Settings.Default.PropertyValues["JsonFileName"].PropertyValue = @"HamstiBotWPF\bin\Debug\" + JsonFileName;
            AuthUsers = UsersFunc.AuthUsers;
        }


        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(5, 0)]
        [TestCase(10, 0)]
        public async Task ExecCommand_IsNormalExecute(int indexCommand, int indexUser)
        {
            // Act
            Assert.DoesNotThrow(async () => await HandleTextCommand(CollectionCommands[indexCommand],
                                            AuthUsers[indexUser],
                                            new Message()
                                            {
                                                Text = CollectionCommands[indexCommand].Command,
                                                From = new User()
                                                {
                                                    Id = AuthUsers[indexUser].Id
                                                }
                                            }));
        }

        [Test]
        public void ExecCommand_OutOfRangeUsersList_ThrowsException()
        {
            // Arrange
            int indexCommand = CollectionCommands.Count > 0 ? 0 : throw new ArgumentOutOfRangeException(nameof(CollectionCommands));
            int indexUser = AuthUsers.Count;

            // Act
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ExecuteTextCommand(CollectionCommands[indexCommand],
                                                                              AuthUsers[indexUser],
                                                                              new Message()
                                                                              {
                                                                                  Text = CollectionCommands[indexCommand].Command,
                                                                                  From = new User()
                                                                                  {
                                                                                      Id = AuthUsers[indexUser].Id
                                                                                  }
                                                                              }));
        }

        [Test]
        public void ExecCommand_OutOfRangeCommandList_ThrowsException()
        {
            // Arrange
            int indexCommand = CollectionCommands.Count;
            int indexUser = AuthUsers.Count > 0 ? AuthUsers.Count : throw new ArgumentOutOfRangeException(nameof(AuthUsers));

            // Act
            Assert.ThrowsAsync<IndexOutOfRangeException>(() => ExecuteTextCommand(CollectionCommands[indexCommand],
                                                                           AuthUsers[indexUser],
                                                                           new Message()
                                                                           {
                                                                               Text = CollectionCommands[indexCommand].Command,
                                                                               From = new User()
                                                                               {
                                                                                   Id = AuthUsers[indexUser].Id
                                                                               }
                                                                           }));
        }


        //[TestCase(LevelCommand.None, LevelCommand.All)]
        //[TestCase(LevelCommand.PC, LevelCommand.All)]
        //[TestCase(LevelCommand.Bot, LevelCommand.All)]
        //[TestCase(LevelCommand.Users, LevelCommand.All)]
        //[TestCase(LevelCommand.Messages, LevelCommand.All)]
        //public async Task ExecCommand_LevelNotFound_NoChangesUser(LevelCommand userLevelBefore, LevelCommand userLevelAfter)
        //{
        //    // Arrange
        //    int indexUser = 0;
        //    BotLevelCommand botLevelCommandBefore = new BotLevelCommand(userLevelBefore);
        //    AuthUsers[indexUser].CurrentLevel = botLevelCommandBefore;
        //    ITCommand command = new BotLevelCommand(userLevelAfter); //CollectionCommands.Where(w => w.NameOfLevel == userLevelAfter).First();

        //    // Act
        //    await ExecCommand(command,
        //                      AuthUsers[indexUser],
        //                      new Message()
        //                      {
        //                          Text = command.Command,
        //                          From = new User()
        //                          {
        //                              Id = AuthUsers[indexUser].Id
        //                          }
        //                      });

        //    // Assert
        //    Assert.AreEqual(botLevelCommandBefore, AuthUsers[indexUser].CurrentLevel);
        //}


        //[TestCase(LevelCommand.None, LevelCommand.PC)]
        //[TestCase(LevelCommand.None, LevelCommand.Bot)]
        //[TestCase(LevelCommand.None, LevelCommand.Users)]
        //[TestCase(LevelCommand.None, LevelCommand.Messages)]
        //[TestCase(LevelCommand.None, LevelCommand.None)]
        //[TestCase(LevelCommand.Messages, LevelCommand.Messages)]
        //[TestCase(LevelCommand.PC, LevelCommand.PC)]
        //[TestCase(LevelCommand.Users, LevelCommand.Users)]
        //[TestCase(LevelCommand.Users, LevelCommand.Bot)]
        //[TestCase(LevelCommand.Users, LevelCommand.PC)]
        //[TestCase(LevelCommand.Users, LevelCommand.Messages)]
        //public async Task ExecCommand_IsChangeUserLevelCommands(LevelCommand userLevelBefore, LevelCommand userLevelAfter)
        //{
        //    // Arrange
        //    int indexUser = 0;
        //    AuthUsers[indexUser].CurrentLevel = userLevelBefore;
        //    ITCommand command = new BotLevelCommand(userLevelAfter); //CollectionCommands.Where(w => w.NameOfLevel == userLevelAfter).First();

        //    // Act
        //    await ExecCommand(command,
        //                      AuthUsers[indexUser],
        //                      new Message()
        //                      {
        //                          Text = command.Command,
        //                          From = new User()
        //                          {
        //                              Id = AuthUsers[indexUser].Id
        //                          }
        //                      });

        //    // Assert
        //    Assert.AreEqual(userLevelAfter, AuthUsers[indexUser].CurrentLevel);
        //}


        [TestCase(StatusUser.None, StatusUser.None)]
        [TestCase(StatusUser.None, StatusUser.User)]
        [TestCase(StatusUser.None, StatusUser.Moder)]
        [TestCase(StatusUser.None, StatusUser.Admin)]
        [TestCase(StatusUser.Admin, StatusUser.None)]
        [TestCase(StatusUser.Admin, StatusUser.User)]
        [TestCase(StatusUser.Admin, StatusUser.Moder)]
        [TestCase(StatusUser.Admin, StatusUser.Admin)]
        public async Task ExecCommand_IsHasAccessToCommand(StatusUser statusCommand, StatusUser statusUser)
        {
            // Arrange
            bool? isExec = null;
            var tempUser = new User(AuthUsers[0]) { Id = 0, Status = statusUser };
            AuthUsers.Add(tempUser);
            int indexUser = AuthUsers.IndexOf(tempUser);

            CollectionCommands[0] = new BotCommand(CollectionCommands[0].Command)
            {
                StatusUser = statusCommand,
                Execute = (model, user, message) => { isExec = true; return Task.CompletedTask; },
                OnError = (model, user, message) => { isExec = false; return Task.CompletedTask; },
            };

            // Act
            await ExecuteTextCommand(CollectionCommands[0],
                              AuthUsers[indexUser],
                              new Message()
                              {
                                  Text = CollectionCommands[0].Command,
                                  From = new User()
                                  {
                                      Id = AuthUsers[indexUser].Id
                                  }
                              });

            // Assert
            Assert.IsTrue(statusCommand > statusUser && isExec is null || statusCommand <= statusUser && isExec.Value);
        }
    }
}