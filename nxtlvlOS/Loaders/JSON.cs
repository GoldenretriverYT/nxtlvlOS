using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Loaders {
    public class JSONParser {
        private string code;
        private int pos;

        public char Current => Peek(0);

        public char Peek(int off) {
            if (pos + off >= code.Length || pos + off < 0) return '\0';
            return code[pos + off];
        }

        public char Next() {
            pos++;
            return Current;
        }

        public void MatchChar(char c) {
            if (Current == c) {
                Next();
                return;
            }

            throw new Exception($"Expected character '{c}' at offset {pos}");
        }

        public bool MatchSeq(string seq) {
            for (var i = 0; i < seq.Length; i++) {
                if (Current == seq[i])
                    Next();
                else
                    throw new Exception($"Expected character '{seq[i]}' at offset {pos}");
            }

            return true;
        }

        public JSONParser(string code) {
            this.code = code;
        }

        public Dictionary<string, object> Parse() {
            return ParseObject();
        }

        public Dictionary<string, object> ParseObject() {
            var obj = new Dictionary<string, object>();

            MatchChar('{');
            ParseWhitespace();

            do {
                if (Current == ',') MatchChar(',');

                var key = ParseString();
                ParseWhitespace();
                MatchChar(':');
                var val = ParseValue();

                obj.Add(key, val);
            } while (Current == ',');

            ParseWhitespace();
            MatchChar('}');

            return obj;
        }

        public object ParseValue() {
            ParseWhitespace();

            if (Current == '"')
                return ParseString();
            else if (char.IsDigit(Current))
                return ParseNumber();
            else if (Current == '{')
                return ParseObject();
            else if (Current == '[')
                return ParseArray();
            else if (Current == 't')
                return MatchSeq("true");
            else if (Current == 'f')
                return !MatchSeq("false");
            else if (Current == 'n')
                return MatchSeq("null") ? null : null;
            else
                throw new Exception($"Unexpected character {Current} at position {pos}");
        }

        public object[] ParseArray() {
            var list = new List<object>();
            MatchChar('[');

            ParseWhitespace();
            do {
                if (Current == ',') MatchChar(',');
                if (Current == ']') break; // HACK: this might be incorrect and could be considered a hack (this is to support empty array)

                ParseWhitespace();
                var val = ParseValue();
                ParseWhitespace();

                list.Add(val);
            } while (Current == ',');

            ParseWhitespace();
            MatchChar(']');
            return list.ToArray();
        }

        public string ParseString() {
            ParseWhitespace();
            MatchChar('"');

            string r = "";

            while (true) {
                if (Current == '\0') throw new Exception($"Unexpected end of file in string sequence at position {pos}");

                if (Current == '\\') {
                    var esc = Next();

                    switch (esc) {
                        case '"': r += "\""; break;
                        case '\\': r += "\\"; break;
                        case '/': r += "/"; break;
                        case 'b': r += "\b"; break;
                        case 'f': r += "\f"; break;
                        case 'n': r += "\n"; break;
                        case 'r': r += "\r"; break;
                        case 't': r += "\t"; break;
                        case 'u': throw new NotImplementedException("Unimplemented: Parse unicode codepoint escape sequence");
                        default: throw new Exception($"Unexpected escape sequence '\\{esc}' at position {pos}");
                    }

                    Next();
                    continue;
                }

                if (Current == '"') {
                    MatchChar('"');
                    break;
                }

                r += Current;
                Next();
            }

            return r;
        }

        public void ParseWhitespace() {
            while (char.IsWhiteSpace(Current)) Next();
        }

        public object ParseNumber() {
            var str = "";
            var hasFract = false;

            while (Current != '\0') {
                if (char.IsDigit(Current) || Current is 'e' or '+' or '-' or '.') {
                    if (Current == '.') {
                        if (hasFract) throw new Exception($"Number has 2 fraction symbols at position {pos}");
                        hasFract = true;
                    }

                    str += Current;
                    Next();
                    continue;
                }

                break;
            }

            if (hasFract) {
                if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float resF))
                    throw new Exception($"Float parsing failed at position {pos}");
                return resF;
            } else {
                if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int resI))
                    throw new Exception($"Integer parsing failed at position {pos}");
                return resI;
            }
        }
    }

    public class JSONSerializer {
        public static string Serialize(Dictionary<string, object> obj) {
            string s = "{";

            foreach (var key in obj.Keys) {
                s += "\"" + key + "\": " + SerializeValue(obj[key]) + ", ";
            }

            if (s.Length > 1) s = s.Substring(0, s.Length - 2);

            return s + "}";
        }

        public static string SerializeValue(object value) {
            if (value is string) return "\"" + value + "\"";
            if (value is bool b) return b.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
            if (value is int i) return i.ToString(CultureInfo.InvariantCulture);
            if (value is float f) return f.ToString(CultureInfo.InvariantCulture);
            if (value is double d) return d.ToString(CultureInfo.InvariantCulture);
            if (value is Dictionary<string, object>) return Serialize((Dictionary<string, object>)value);
            if (value is object[]) return SerializeArray((object[])value);
            if (value is List<object>) return SerializeArray(((List<object>)value).ToArray<object>());

            throw new Exception("Unsupported type: " + value.GetType().Name);
        }

        public static string SerializeArray(object[] arr) {
            string s = "[";

            foreach (var value in arr) {
                s += SerializeValue(value) + ", ";
            }

            if (s.Length > 1) s = s.Substring(0, s.Length - 2);

            return s + "]";
        }
    }
}
