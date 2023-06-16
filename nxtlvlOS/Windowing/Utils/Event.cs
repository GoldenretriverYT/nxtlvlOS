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

        public void Unsubscribe(Action<T> action) {
            if(handlers.Contains(action)) {
                handlers.Remove(action);
            }
        }

        public void Invoke(T data) {
            foreach (var handler in handlers) handler(data);
        }
    }
}
