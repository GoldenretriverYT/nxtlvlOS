using nxtlvlOS.Processing;
using nxtlvlOS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Services {
    public class SystemPreferenceService : App {
        public static SystemPreferenceService Instance;

        private Dictionary<string, Preference> _prefs = new Dictionary<string, Preference>();
        static string PREFS_FILE = @"0:\System\prefs.nxt";

        public override void Exit() {

        }

        public override void Init(string[] args) {
            Instance = this;
            LoadPrefsFile();
        }
        
        public override void Update() {

        }

        public void LoadPrefsFile() {
            try {
                if (!Directory.Exists(@"0:\System")) {
                    Directory.CreateDirectory(@"0:\System");
                }

                Kernel.Instance.Logger.Log(LogLevel.Info, "ye.0 " + PREFS_FILE);
                if (!File.Exists(PREFS_FILE)) {
                    Kernel.Instance.Logger.Log(LogLevel.Info, "ye.1 " + PREFS_FILE);
                    File.WriteAllText(PREFS_FILE, "");
                    Kernel.Instance.mDebugger.Send("whar");
                    Kernel.Instance.Logger.Log(LogLevel.Info, "ye.2 " + PREFS_FILE);
                }
                Kernel.Instance.Logger.Log(LogLevel.Info, "ye.3 " + PREFS_FILE);

                var lines = File.ReadAllText(PREFS_FILE).Split("\n");
                foreach (var line in lines) {
                    var parts = line.Split('=');
                    if (parts.Length != 2) {
                        Kernel.Instance.Logger.Log(LogLevel.Warn, "Invalid preference: " + line);
                        continue;
                    }

                    var key = parts[0];
                    var value = parts[1];

                    if (value.StartsWith("int:")) {
                        var pref = new Preference<int>();
                        pref.Type = "int";
                        pref.Value = int.Parse(value.Substring(4));
                        _prefs.Add(key, pref);
                    } else if (value.StartsWith("string:")) {
                        var pref = new Preference<string>();
                        pref.Type = "string";
                        pref.Value = value.Substring(7);
                        _prefs.Add(key, pref);
                    } else if (value.StartsWith("bool:")) {
                        var pref = new Preference<bool>();
                        pref.Type = "bool";
                        pref.Value = bool.Parse(value.Substring(5));
                        _prefs.Add(key, pref);
                    } else {
                        Kernel.Instance.Logger.Log(LogLevel.Warn, "Invalid preference: " + line);
                        continue;
                    }
                }
            }catch (Exception ex) {
                if(ex == null) {
                    Kernel.Instance.Panic("Null exception on prefs load");
                }else {
                    Kernel.Instance.Panic("Exception on prefs load: " + ex.Message);
                }
            }
        }

        public void SavePrefsFile() {
            var text = "";
            foreach (var pref in _prefs) {
                if (pref.Value.Type == "int") {
                    text += (pref.Key + "=int:" + ((Preference<int>)pref.Value).Value) + "\n";
                } else if (pref.Value.Type == "string") {
                    text += (pref.Key + "=string:" + ((Preference<string>)pref.Value).Value) + "\n";
                } else if (pref.Value.Type == "bool") {
                    text += (pref.Key + "=bool:" + ((Preference<bool>)pref.Value).Value) + "\n";
                } else {
                    Kernel.Instance.Logger.Log(LogLevel.Warn, "Refusing to save preference due to unsupported type: " + pref.Value.Type);
                }
            }

            File.WriteAllText(PREFS_FILE, text);
        }

        public void SetPreference<T>(string key, T value) {
            if (value is not int 
                    and not string
                    and not bool) {
                throw new Exception("Unsupported preference type: " + typeof(T).Name);
            }

            if (_prefs.ContainsKey(key)) {
                _prefs[key] = new Preference<T>() {
                    Type = typeof(T).Name.ToLower(),
                    Value = value
                };
            } else {
                _prefs.Add(key, new Preference<T>() {
                    Type = typeof(T).Name.ToLower(),
                    Value = value
                });
            }

            SavePrefsFile();
        }

        public ErrorOr<T> GetPreference<T>(string key) {
            if (!_prefs.ContainsKey(key)) {
                return ErrorOr<T>.MakeError("Preference not found: " + key);
            }

            var pref = _prefs[key];
            
            if (pref.Type != typeof(T).Name.ToLower()) {
                return ErrorOr<T>.MakeError("Preference type mismatch: " + key);
            }

            return ErrorOr<T>.MakeResult(((Preference<T>)pref).Value);
        }

        public T GetPreferenceOrDefault<T>(string key, T defaultValue) {
            if (!_prefs.ContainsKey(key)) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, $"Preference not found: {key}");
                SetPreference(key, defaultValue);
                return defaultValue;
            }
            
            var pref = _prefs[key];
            
            if (pref.Type != typeof(T).Name.ToLower()) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, $"Preference type mismatch: {key} ({pref.Type} != {typeof(T).Name.ToLower()}); using default.");
                SetPreference(key, defaultValue);
                return defaultValue;
            }

            return ((Preference<T>)pref).Value;
        }
    }

    public class Preference {
        public string Type;
        public string Value;
    }

    public class Preference<T> : Preference {
        public new T Value;
    }
}
