using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Utils {
    public class Event<T> {
        private List<Action<T>> handlers = new();

        public void Subscribe(Action<T> action) {
            handlers.Add(action);
        }

        public static Event<T> operator +(Event<T> a, Action<T> b) {
            a.Subscribe(b);
            return a;
        }

        public static Event<T> operator -(Event<T> a, Action<T> b) {
            a.Unsubscribe(b);
            return a;
        }

        public void Unsubscribe(Action<T> action) {
            if(handlers.Contains(action)) {
                handlers.Remove(action);
            }
        }

        public unsafe void Invoke(T data) {
            foreach (var handler in handlers) {
                if (handler == null) continue;
                handler(data);
            }
        }
    }

    public class Event<T1, T2> {
        private List<Action<T1, T2>> handlers = new();

        public void Subscribe(Action<T1, T2> action) {
            handlers.Add(action);
        }

        public static Event<T1, T2> operator +(Event<T1, T2> a, Action<T1, T2> b) {
            a.Subscribe(b);
            return a;
        }

        public static Event<T1, T2> operator -(Event<T1, T2> a, Action<T1, T2> b) {
            a.Unsubscribe(b);
            return a;
        }

        public void Unsubscribe(Action<T1, T2> action) {
            if (handlers.Contains(action)) {
                handlers.Remove(action);
            }
        }

        public unsafe void Invoke(T1 data1, T2 data2) {
            foreach (var handler in handlers) {
                if (handler == null) continue;
                handler(data1, data2);
            }
        }
    }

    public class Event<T1, T2, T3> {
        private List<Action<T1, T2, T3>> handlers = new();

        public void Subscribe(Action<T1, T2, T3> action) {
            handlers.Add(action);
        }

        public static Event<T1, T2, T3> operator +(Event<T1, T2, T3> a, Action<T1, T2, T3> b) {
            a.Subscribe(b);
            return a;
        }

        public static Event<T1, T2, T3> operator -(Event<T1, T2, T3> a, Action<T1, T2, T3> b) {
            a.Unsubscribe(b);
            return a;
        }

        public void Unsubscribe(Action<T1, T2, T3> action) {
            if (handlers.Contains(action)) {
                handlers.Remove(action);
            }
        }

        public unsafe void Invoke(T1 data1, T2 data2, T3 data3) {
            foreach (var handler in handlers) {
                if (handler == null) continue;
                handler(data1, data2, data3);
            }
        }
    }

    public class Event<T1, T2, T3, T4> {
        private List<Action<T1, T2, T3, T4>> handlers = new();

        public void Subscribe(Action<T1, T2, T3, T4> action) {
            handlers.Add(action);
        }

        public static Event<T1, T2, T3, T4> operator +(Event<T1, T2, T3, T4> a, Action<T1, T2, T3, T4> b) {
            a.Subscribe(b);
            return a;
        }

        public static Event<T1, T2, T3, T4> operator -(Event<T1, T2, T3, T4> a, Action<T1, T2, T3, T4> b) {
            a.Unsubscribe(b);
            return a;
        }

        public void Unsubscribe(Action<T1, T2, T3, T4> action) {
            if (handlers.Contains(action)) {
                handlers.Remove(action);
            }
        }

        public unsafe void Invoke(T1 data1, T2 data2, T3 data3, T4 data4) {
            foreach (var handler in handlers) {
                if (handler == null) continue;
                handler(data1, data2, data3, data4);
            }
        }
    }

    public class Event {
        private List<Action> handlers = new();

        public void Subscribe(Action action) {
            handlers.Add(action);
        }

        public static Event operator +(Event a, Action b) {
            a.Subscribe(b);
            return a;
        }

        public static Event operator -(Event a, Action b) {
            a.Unsubscribe(b);
            return a;
        }

        public void Unsubscribe(Action action) {
            if (handlers.Contains(action)) {
                handlers.Remove(action);
            }
        }

        public unsafe void Invoke() {
            foreach (var handler in handlers) {
                if (handler == null) continue;
                handler();
            }
        }
    }
}
