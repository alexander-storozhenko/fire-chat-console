using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
namespace FireStoreChatConsole
{
    class Db
    {
        private const int KEY_CHECK_DELAY = 2000;

        protected List<string[]> messagesList;
        protected CollectionReference messagesRef;
        protected CollectionReference usersRef;
        protected CollectionReference roomsRef;
        protected QuerySnapshot rooms;
        protected QuerySnapshot messages;
        protected QuerySnapshot users;
        protected FirestoreDb db;

        protected Dictionary<int, string[]> roomUsers = new Dictionary<int, string[]>();

        protected long lastTime = 0;
        public Db()
        {
            messagesList = new List<string[]>();
            users = Settings.users;
            messages = Settings.messages;
            usersRef = Settings.usersRef;
            messagesRef = Settings.messagesRef;
            rooms = Settings.rooms;
            roomsRef = Settings.roomsRef;
            db = Settings.fireDb;
        }
        public void ResetTime()
        {
            lastTime = 0;
        }
        public void ShowMessages()
        {

            messagesList.ForEach(x => Console.WriteLine(Settings.Encrypter.Decrypt(x[1])));

        }
        public void InsertMessage(string str,long time,int id = -1)
        {
            if (!string.IsNullOrEmpty(str))
            {
                //SetQueryForGetKey(User.secUser);

                Dictionary<string, object> messageData = new Dictionary<string, object>
                {
                    { "userName", User.login},
                    { "text", str },
                    { "time", time },
                    { "room", User.roomId},
                    { "id", id}
                };


                Settings.fireDb.Collection("messages").AddAsync(messageData);


                User.ListenMessages();

            }
        }
        public string GetOutKey(string opponent)
        {
            return usersRef.Select("key").WhereEqualTo("name", opponent).GetSnapshotAsync().Result.First().ToDictionary()["key"].ToString();
        }
        
        public void GetFriendsList()
        {
            var friends = (usersRef.Select("friends").WhereEqualTo("name", User.login).
               GetSnapshotAsync().Result.First().ToDictionary().Values.First() as Dictionary<string, object>).Keys.ToList();

            friends.ToList().ForEach(x => Console.WriteLine(x));

            friends.ForEach(x => {
                
                var online = (bool)usersRef.Select("online").WhereEqualTo("name", x).GetSnapshotAsync().Result.First().ToDictionary()["online"];
                
                if (online)
                {
                    Console.Write($"{x} - ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("online");
                    Console.ResetColor();
                }
                else Console.WriteLine($"{x} - offline");
                
            });
        }
        public void ConnectWithSecUser(string secUser,bool showRoom = false)
        {
            // select room from firends[secuser]

            var roomId =  int.Parse((usersRef.Select("friends").WhereEqualTo("name", User.login).
                GetSnapshotAsync().Result.First().ToDictionary().Values.First() as Dictionary<string,object>)[secUser].ToString());
            Settings.db.DeleteAllMessagesInRoom(roomId);
            Console.WriteLine("room id " + roomId);

            IDictionary<string, object> otherName = new Dictionary<string, object>()
            { ["secUser"] = secUser };
            
            Settings.fireDb.Collection("users").Document(User.login).UpdateAsync(otherName);

            Console.WriteLine(secUser);
            User.secUser = secUser;

            //set user for chat
            Console.WriteLine("-connect...");
            IDictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("secUser", secUser);
            Settings.fireDb.Collection("users").Document(User.login).UpdateAsync(dict);
            // send key to user
            Console.WriteLine("-send key...");
            IDictionary<string, object> key = new Dictionary<string, object>();

            key.Add("key", Settings.Encrypter.PublicKeyString());
            Settings.fireDb.Collection("users").Document(secUser).UpdateAsync(key);

            bool hasKey = false;
            Console.WriteLine("wait opponent...");

            while (!hasKey)
            {
                hasKey = !string.IsNullOrEmpty(usersRef.Select("key").WhereEqualTo("name", User.login).GetSnapshotAsync().Result.First().ToDictionary()["key"].ToString());
                Thread.Sleep(KEY_CHECK_DELAY);
            }
            if(showRoom)
            RoomController.ShowRoom(roomId);
            

        }


        public List<string[]> GetSecMessages(int room)
        {
            messagesList.Clear();
       
            var messages = messagesRef.Select("id","text", "time", "userName")
                .WhereEqualTo("room", room)
                .OrderBy("time")
                .WhereGreaterThan("time", lastTime)
                .GetSnapshotAsync().Result.ToList();

           // SortedList<int,string> m = new SortedList<int,string>();
            messages.ForEach(x =>
            {
                object text = "";
                DateTime myDate = new DateTime(Convert.ToInt64(x.ToDictionary()["time"]));

                if (x.ToDictionary()["userName"].ToString() == User.secUser)
                {
                    text = Settings.Encrypter.Decrypt(x.ToDictionary()["text"].ToString()).ToString();
                }
                else
                {
                    if (User.secMessages.ContainsKey(Convert.ToInt64(x.ToDictionary()["time"])))
                        text = User.secMessages[Convert.ToInt64(x.ToDictionary()["time"])];
                }

                messagesList.Add(new string[] { x.ToDictionary()["userName"].ToString(), "[" + myDate.ToString() + "]", text.ToString() });
                lastTime = Convert.ToInt64(messages.Last().ToDictionary()["time"]);

            });

            return messagesList;
        }

        public void SendQueryForMessage(string person)
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("sendFrom", User.login);
            Settings.fireDb.Collection("users").Document(person).UpdateAsync(dict);
        }
       
        public List<string> GetRoomUsers()
        {
            if (roomUsers.ContainsKey(User.roomId) && roomUsers[User.roomId].Count() != 0)
            {
                return roomUsers[User.roomId].ToList();
            }
            else
            {
                roomUsers[User.roomId] = usersRef.Select("name").WhereArrayContains("rooms", User.roomId).GetSnapshotAsync().Result.Select(x => x.ToDictionary()["name"].ToString()).ToArray();

                return roomUsers[User.roomId].ToList();
            }

        }
        public List<string[]> GetMessages(int room)
        {
            messagesList.Clear();

            var messages = messagesRef.Select("text", "time", "userName").WhereEqualTo("room", room).OrderBy("time").WhereGreaterThan("time", lastTime).GetSnapshotAsync().Result.ToList();

            messages.ForEach(x =>
            {
                DateTime myDate = new DateTime(Convert.ToInt64(x.ToDictionary()["time"]));
                messagesList.Add(new string[] { x.ToDictionary()["userName"].ToString(), "[" + myDate.ToString() + "]", x.ToDictionary()["text"].ToString() });
                lastTime = Convert.ToInt64(messages.Last().ToDictionary()["time"]);
            });

            return messagesList;
        }
        public List<string> GetAdmins()
        {
            return usersRef.Select("name").WhereEqualTo("admin", true).GetSnapshotAsync().Result.Select(x => x.ToDictionary()["name"].ToString()).ToList();
        }
        public void DeleteAllMessagesInRoom(int room)
        {
            var messages = messagesRef.Select("text", "time", "userName").WhereEqualTo("room", room).GetSnapshotAsync().Result.ToList();

            messages.ForEach(x => x.Reference.DeleteAsync());
        }

        public string GetAccessibleRooms(string login)
        {
            return usersRef.Select("rooms").WhereEqualTo("name", login).GetSnapshotAsync().Result.First().ToDictionary()["rooms"].ToString();
        }

        public bool IsCorrectUser(string login, string pass)
        {
            return usersRef.Select("name", "pass").WhereEqualTo("name", login).WhereEqualTo("pass", pass).GetSnapshotAsync().Result.Count == 1;
        }
        public List<string> GetAllOnlineUsers()
        {
            return usersRef.Select("name").WhereEqualTo("online", true).GetSnapshotAsync().Result.Select(x => x.ToDictionary()["name"].ToString()).ToList();
        }
        public bool IsCorrectRoom(string login, int id)
        {
            return usersRef.WhereArrayContains("rooms", id).WhereEqualTo("name", login).GetSnapshotAsync().Result.Count() > 0;
        }
        //public bool IsNeedToSendPublicKey()
        //{
        //    return !string.IsNullOrEmpty(usersRef.WhereArrayContains("name", User.login).GetSnapshotAsync().Result.First().ToString());
        //}

        // poluch
        public void SendKeyToSender(string user, string publicKey)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(User.login, publicKey);
            Settings.fireDb.Collection("users").Document(user).Collection("keys").AddAsync(dict);
        }
        //otpr
        public void SetQueryForGetKey(string user)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("name", User.login);
            Settings.fireDb.Collection("users").Document(user).Collection("senders").AddAsync(dict);
        }

        //otpr
        //public void SendKeys()
        //{

        //    var senders = Settings.fireDb.Collection("users").Document(User.login).Collection("senders").Select("name").GetSnapshotAsync().Result.ToList();
        //    //senders.ForEach(x => Console.WriteLine(x.ToDictionary()["name"]));

        //    senders.ForEach(x =>
        //    {
        //        Console.WriteLine(x);
        //        Dictionary<string, string> dict = new Dictionary<string, string>();
        //        dict.Add(User.login, Settings.Encrypter.PublicKeyString());
        //        Settings.fireDb.Collection("users").Document(x.ToDictionary()["name"].ToString()).Collection("keys").AddAsync(dict);
        //    });


        //}
        public void DeleteKeys()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("key", "");
            Settings.fireDb.Collection("users").Document(User.login).UpdateAsync(dict);
        }
        public void SetOnlineStatus(string user,bool status)
        {
            IDictionary<string, object> online = new Dictionary<string, object>();
            online.Add("online", status);
            
           Settings.fireDb.Collection("users").Document(user).UpdateAsync(online).Wait();
        }

        public void ResetSecUserKey()
        {
            IDictionary<string, object> key = new Dictionary<string, object>();
            key.Add("key", "");

            Settings.fireDb.Collection("users").Document(User.secUser).UpdateAsync(key).Wait();
        }

        public void ResetUserSecUser()
        {
            IDictionary<string, object> secUser = new Dictionary<string, object>();
            secUser.Add("secUser", "");

            Settings.fireDb.Collection("users").Document(User.login).UpdateAsync(secUser).Wait();
        }
    }
}
