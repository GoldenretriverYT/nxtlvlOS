using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Utils {
    public class ErrorOr<T> {
        public bool IsError = false;
        public string Error = "";
        public T Data {
            get {
                if (_data == null) {
                    Kernel.Instance.Logger.Log(LogLevel.Fail, "Tried to access data of error object with error: " + Error);
                    throw new Exception("Tried to access data of error object!");
                }
                return _data;
            }
            set {
                _data = Data;
            }
        }

        private T _data;

        public static ErrorOr<T> MakeError(string error) {
            return new ErrorOr<T>() {
                IsError = true,
                Error = error
            };
        }

        public static ErrorOr<T> MakeResult(T data) {
            return new ErrorOr<T>() {
                IsError = false,
                _data = data
            };
        }
    }

    public class ErrorOr {
        public static bool Resolve<T>(ErrorOr<T> obj, out T data, T defaultVal = default(T)) {
            data = defaultVal;
            if (obj.IsError) return false;
            data = obj.Data;
            return true;
        }
    }

    public class ErrorOrNothing {
        public bool IsError = false;
        public string Error = "";

        public static ErrorOrNothing MakeError(string error) {
            return new ErrorOrNothing() {
                IsError = true,
                Error = error
            };
        }

        public static ErrorOrNothing MakeResult() {
            return new ErrorOrNothing() {
                IsError = false,
            };
        }
    }
}
