using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public abstract class Layout : BufferedElement {
        public abstract void DoLayout();

        /// <summary>
        /// Adds the element as a child and recalculates the layout
        /// </summary>
        public void AddItem(BufferedElement element) {
            AddChild(element);
            DoLayout();
        }
    }
}
