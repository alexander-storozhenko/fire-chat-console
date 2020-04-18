using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FireStoreChatConsole
{
    static class CommandsEngine
    {
        public static void CheckString(string str)
        {
            str = str.Trim() + " ";
            ParseObject parseObject = new CommandParser().Parse(str);

            switch (parseObject.Action)
            {
                case "dk":
                    Settings.db.DeleteKeys();
                    break;
                case "q":
                    Console.Clear();
                    Settings.db.DeleteKeys();
                    RoomController.ShowLobby();
                    break;
                case "r":
                    Console.WriteLine("connect...");
                    string accessibleRooms = Settings.db.GetAccessibleRooms(User.login);
                    int.TryParse(parseObject.Values.First(), out User.roomId);
                    var isCorrectRoom = Settings.db.IsCorrectRoom(User.login, User.roomId);
                    if (User.roomId < 0)
                    {
                        Console.WriteLine("room id must be non-negative numeric value!");
                        User.roomId = -1;
                    }
                    else if (!isCorrectRoom)
                    {
                        Console.WriteLine("unsuccessible room!");
                        User.roomId = -1;
                    }
                    else
                    {
                        RoomController.ShowRoom(User.roomId);
                    }
                    break;
                case "ut":
                    if (User.IsAdmin())
                    {
                        var oldSpeed = RoomController.RefreshSpeed;
                        RoomController.RefreshSpeed = Convert.ToInt32(parseObject.Values.First());
                        Console.WriteLine("Change speed from " + oldSpeed + " to " + RoomController.RefreshSpeed);
                    }

                    break;
                case "help":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("/r [room id] - into room");
                    Console.WriteLine("/sr [user name] - into sec room");
                    Console.WriteLine("/q - into lobby");
                    Console.WriteLine("/fi - friends info list ");
                    Console.WriteLine("/ref - refresh room");
                    Console.WriteLine("/dk - delete keys");
                    Console.ResetColor();
                    break;
                case "fi":
                    Settings.db.GetFriendsList();
                    break;
                case "clear":
                    if (User.IsAdmin())
                    {
                        int _room = int.Parse(parseObject.Values.First());
                        Settings.db.DeleteAllMessagesInRoom(_room);
                    }
                    break;
                case "ref":
                    Console.Clear();
                    RoomController.ShowRoom(User.roomId);

                    break;
                case "sk":
                    if (User.IsAdmin())
                    {
                        string _user = parseObject.Values.First().ToString();
                        Settings.db.SetQueryForGetKey(_user);
                        //if (Settings.db.IHasKeyFromUser("admin")) Settings.db.InsertMessage(Settings.Encrypter.Encrypt("hello admin!"));
                        //Console.WriteLine("ss-"+ Settings.db.GetKey("admin"));
                    }
                    break;

                case "sr":

                    string name = parseObject.Values.First().ToString();
                    Settings.db.ConnectWithSecUser(name, true);
                    break;
                
                default:
                    if (User.roomId > 0)
                    {
                        var time = DateTime.Now.Ticks;
                        if (User.roomId > 100)
                        {
                            User.secMessages[time] = str;
                            int id = -1;

                            str.GetSplittedString()
                                .Select(x => Settings.Encrypter.Encrypt(x)).ToList()
                                .ForEach(x => Settings.db.InsertMessage(x, time, ++id));
                        }
                        else
                            Settings.db.InsertMessage(str, time);
                    }
                    else
                    {
                        Console.WriteLine("unknown command!");
                    }
                    break;

            }

        }

    }
}
