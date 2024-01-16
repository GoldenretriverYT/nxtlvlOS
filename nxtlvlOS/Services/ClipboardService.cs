using Cosmos.System;
using nxtlvlOS.Processing;
using nxtlvlOS.Utils;
using System;
using System.Collections.Generic;

namespace nxtlvlOS.Services {
    public class ClipboardService : App {
        public static ClipboardService Instance;
        private List<ClipboardStorageProvider> storages;

        public override void Exit() {
            throw new Exception("ClipboardService should not be killed.");
        }

        public override void Init(string[] args) {
            if (Instance != null)
                throw new Exception("ClipboardService should not be started twice.");

            Instance = this;
        }

        public override void Update() {
           
        }

        public void RegisterStorage(ClipboardStorageProvider storage) {
            storages.Add(storage);
        }

        public ClipboardStorageProvider GetStorageProvider<T>() {
            foreach(var storage in storages) {
                if (storage is T) return storage;
            }

            return null;
        }

        public ErrorOr<TRet> GetValue<TRet, TStorageProvider>() {
            var storageProvider = GetStorageProvider<TStorageProvider>();

            if (storageProvider == null) ErrorOr<TRet>.MakeError("Storage provider not found.");
            var value = storageProvider.Read();

            if (value is not TRet ret) return ErrorOr<TRet>.MakeError("Storage provider returned different type.");
            
            return ErrorOr<TRet>.MakeResult(ret);
        }

        public ErrorOrNothing SetValue<TStorageProvider>(object value) {
            var storageProvider = GetStorageProvider<TStorageProvider>();

            if (storageProvider == null) ErrorOrNothing.MakeError("Storage provider not found.");
            return storageProvider.Write(value);
        }
    }

    /// <summary>
    /// Simplified access to basic clipboard functionality.
    /// </summary>
    public static class Clipboard {
        public static ErrorOr<string> GetText() {
            return ClipboardService.Instance.GetValue<string, TextClipboardStorageProvider>();
        }

        public static ErrorOrNothing SetText(string text) {
            return ClipboardService.Instance.SetValue<TextClipboardStorageProvider>(text);
        }
    }

    public abstract class ClipboardStorageProvider {
        public abstract string TypeName { get; }
        public abstract object Read();
        public abstract ErrorOrNothing Write(object v);
    }

    public class TextClipboardStorageProvider : ClipboardStorageProvider {
        public override string TypeName => "Text";

        private string _value;

        public override object Read() {
            return _value;
        }

        public override ErrorOrNothing Write(object v) {
            if(v is not string s) {
                return ErrorOrNothing.MakeError("Value must be of type string");
            }

            _value = s;
            return ErrorOrNothing.MakeResult();
        }
    }
}
