using nxtlvlOS.Windowing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Elements {
    public class ListView : Layout {
        private List<object> items = new();

        public IReadOnlyList<object> Items {
            get => items;
        }

        private uint backgroundColor = ColorUtils.Primary400;

        public uint BackroundColor {
            get => backgroundColor;
            set {
                backgroundColor = value;
                SetDirty(true);
            }
        }

        private uint itemColor = ColorUtils.Primary500;

        public uint ItemColor {
            get => itemColor;
            set {
                itemColor = value;
                // This does not set dirty as the item itself will be marked dirty
            }
        }

        private uint selectedItemColor = ColorUtils.Primary600;

        public uint SelectedItemColor {
            get => selectedItemColor;
            set {
                selectedItemColor = value;
                // This does not set dirty as the item itself will be marked dirty
            }
        }

        private ScrollView scrollView;
        private int selectedIndex = -1;

        public int SelectedIndex => selectedIndex;
        public object SelectedItem => selectedIndex == -1 ? null : items[selectedIndex];

        public ListView() {
            scrollView = new ScrollView {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = SizeX,
                SizeY = SizeY,
                ContainerSizeX = SizeX,
                ContainerSizeY = SizeY,
            };

            AddChild(scrollView);
            DoLayout();
        }

        // TODO: Maybe make DoLayout a flag, so it only happens each Update at most, and not 100 times if someone adds 100 items
        public override void DoLayout() {
            scrollView.SizeX = SizeX;
            scrollView.ContainerSizeX = SizeX;

            scrollView.SizeY = SizeY;

            int idx = 0;
            foreach (var item in items) {
                int __idx = idx;
                //Kernel.Instance.Logger.Log(LogLevel.Sill, "Putting item " + item + " at index " + idx);

                var sortedExistingButtons = SortButtonsByY(scrollView.Items);

                if (scrollView.Items.Count > idx) { // Reuse existing buttons
                    var btn = (TextButton)sortedExistingButtons[idx];
                    btn.Text = item.ToString();
                    btn.BackgroundColor = idx == selectedIndex ? selectedItemColor : itemColor;

                    btn.Click.UnsubscribeAll();
                    btn.Click += (_, _, _) => {
                        SelectItem(__idx);
                    };

                    idx++;
                    continue;
                } else {
                    var btn = new TextButton {
                        Text = item.ToString(),
                        SizeX = SizeX - 4,
                        SizeY = 24,
                        RelativePosY = 2 + idx * 24,
                        RelativePosX = 2,
                        BackgroundColor = idx == selectedIndex ? selectedItemColor : itemColor,
                        VerticalAlignment = VerticalAlignment.Middle,
                    };

                    btn.Click += (_, _, _) => {
                        SelectItem(__idx);
                    };

                    scrollView.AddItem(btn);
                }
                idx++;
            }

            // Remove unused buttons
            while (scrollView.Items.Count > idx) {
                scrollView.RemoveItem(scrollView.Items[idx]);
            }

            scrollView.ContainerSizeY = (uint)items.Count * 24 + 2;
        }

        public override void Draw() {
            SetDirty(false);

            DrawRectFilled(0, 0, SizeX, SizeY, backgroundColor);
        }

        public void SelectItem(int idx) {
            if (idx < 0 || idx >= items.Count) {
                Kernel.Instance.Logger.Log(LogLevel.Sill, "Tried to select item at index " + idx + " but there are only " + items.Count + " items");
                return;
            }

            selectedIndex = idx;
            DoLayout();
        }

        public void ClearItems(bool doLayout = true) {
            items.Clear();
            if(doLayout) DoLayout();
        }

        public void AddItem(object item, bool doLayout = true) {
            items.Add(item);
            if(doLayout) DoLayout();
        }

        public void RemoveItem(object item) {
            items.Remove(item);
            DoLayout();
        }

        public void RemoveItemAt(int idx) {
            items.RemoveAt(idx);
            DoLayout();
        }

        /// <summary>
        /// Quicksort implementation to sort the buttons by their Y position.
        /// Required because Array.Sort is currently not supported.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private List<TextButton> SortButtonsByY(List<BufferedElement> items) {
            List<TextButton> buttons = new();
            foreach (BufferedElement item in items) {
                buttons.Add((TextButton)item);
            }

            QuickSort(buttons, 0, buttons.Count - 1);
            return buttons;
        }

        private void QuickSort(List<TextButton> list, int left, int right) {
            if (left < right) {
                int pivotIndex = Partition(list, left, right);

                QuickSort(list, left, pivotIndex - 1);
                QuickSort(list, pivotIndex + 1, right);
            }
        }

        private int Partition(List<TextButton> list, int left, int right) {
            TextButton pivot = list[right];
            int i = left - 1;

            for (int j = left; j < right; j++) {
                if (list[j].RelativePosY <= pivot.RelativePosY) {
                    i++;
                    TextButton temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }

            TextButton temp1 = list[i + 1];
            list[i + 1] = list[right];
            list[right] = temp1;
            return i + 1;
        }

    }
}
