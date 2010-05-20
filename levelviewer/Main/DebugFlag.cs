using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public enum DebugFlag
    {
        ShowStats,

        _Last
    }

    public static class DebugFlagManager
    {
        private class DebugFlagEntry
        {
            public string Name;
            public DebugFlag Type;
            public bool Value;
        }

        private static Dictionary<string, DebugFlagEntry> _stringMap;
        private static Dictionary<DebugFlag, DebugFlagEntry> _enumMap;

        static DebugFlagManager()
        {
            _stringMap = new Dictionary<string, DebugFlagEntry>(StringComparer.OrdinalIgnoreCase);
            _enumMap = new Dictionary<DebugFlag, DebugFlagEntry>();

            for (int i = 0; i < (int)DebugFlag._Last; i++)
            {
                DebugFlagEntry entry = new DebugFlagEntry();
                entry.Name = ((DebugFlag)i).ToString();
                entry.Type = (DebugFlag)i;
                entry.Value = false;

                _stringMap.Add(entry.Name, entry);
                _enumMap.Add(entry.Type, entry);
            }
        }

        public static bool GetDebugFlag(string name)
        {
            return _stringMap[name].Value;
        }

        public static bool GetDebugFlag(DebugFlag flag)
        {
            return _enumMap[flag].Value;
        }

        public static void SetDebugFlag(string name, bool value)
        {
            _stringMap[name].Value = value;
        }

        public static void SetDebugFlag(DebugFlag flag, bool value)
        {
            _enumMap[flag].Value = value;
        }
    }
}
