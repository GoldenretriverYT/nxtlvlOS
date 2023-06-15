using nxtlvlOS.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps {
    /// <summary>
    /// TODO: Properly secure this, probably using ACLs
    /// </summary>
    public class UACService : App {
        public static UACService Instance;

        private List<User> users = new();
        public User CurrentUser { get; private set; }
        public static string UserDir => @"0:\Users\" + Instance.CurrentUser.Username + "\\";

        public const string UserDatabasePath = @"0:\System\users.db";


        public override void Exit() {
            throw new Exception("UAC should not be killed.");
        }

        public override void Init() {
            if (Instance != null)
                throw new Exception("UAC should not be started twice.");

            Instance = this;

            if(!Directory.Exists(@"0:\System")) {
                Directory.CreateDirectory(@"0:\System");
            }

            if(!File.Exists(UserDatabasePath)) {
                File.Create(UserDatabasePath);
            }

            LoadUsers();
        }

        public override void Update() {
        }

        public bool Authenticate(string username, string password) {
            foreach(var user in users) {
                if(user.Username == username && user.Password == password) {
                    CurrentUser = user;
                    return true;
                }
            }

            return false;
        }

        public void CreateUser(string username, string password) {
            users.Add(new User() {
                Username = username,
                Password = password
            });

            SaveUsers();
        }

        public void LoadUsers() {
            users.Clear();

            string[] lines = File.ReadAllText(UserDatabasePath)
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach(var line in lines) {
                string[] parts = line.Split(';');

                if(parts.Length != 3)
                    continue;

                bool isRoot = parts[0] == "yes";
                string username = parts[1];
                string password = parts[2];

                users.Add(new User() {
                    IsRootUser = isRoot,
                    Username = username,
                    Password = password
                });
            }
        }

        public void SaveUsers() {
            string[] lines = new string[users.Count];

            for(var i = 0; i < users.Count; i++) {
                var user = users[i];

                lines[i] = $"{(user.IsRootUser ? "yes" : "no")};{user.Username};{user.Password}";
            }

            File.WriteAllText(UserDatabasePath,
                string.Join(Environment.NewLine, lines));
        }

        public class User {
            public bool IsRootUser = false;
            public string Username = "";
            public string Password = "";
        }
    }
}
