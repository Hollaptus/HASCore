using System.Collections;
// Making an alias so we won't have to write the full class.
using SoundHotkey = HASCore.Soundboard.XMLSettings.SoundHotkey;

namespace HASCore.Helpers;

public class Comparers
{
    internal class ListViewItemComparer : IComparer
    {
        public ListViewItemComparer() => Column = 0;
        
        public ListViewItemComparer(Int32 column) => Column = column;
        
        private readonly Int32 Column;
        
        public Int32 Compare(Object? x, Object? y) => x is not null && y is not null
            ? String.Compare(((ListViewItem)x).SubItems[Column].Text, ((ListViewItem)y).SubItems[Column].Text)
            : 0;
    }

    internal class SoundHotkeyComparer : IComparer<SoundHotkey>
    {
        public int Compare(SoundHotkey? x, SoundHotkey? y)
        {
            // Handle null objects (if the list ever contains null)
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            // Handle null Keys
            if (x.Keys is null && y.Keys is null) return 0;
            if (x.Keys is null) return -1;
            if (y.Keys is null) return 1;

            // Both Keys are non‑null – compare their string representations
            return String.Compare(Conversions.KeysToString(x.Keys), Conversions.KeysToString(y.Keys), StringComparison.Ordinal);
        }
    } 

}
