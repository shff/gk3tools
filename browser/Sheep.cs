using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace GK3BB
{
    class Sheep
    {
        public static string GetSheepInfo()
        {
            try
            {
                SHP_Version version = SHP_GetVersion();
                return string.Format("Sheep v{0}.{1}.{2}", version.Major, version.Minor, version.Revision);
            }
            catch (DllNotFoundException)
            {
                return "Sheep not found";
            }
        }

        public static string GetDisassembly(byte[] data)
        {
            try
            {
                IntPtr disassembly = SHP_GetDisassembly(data, data.Length);
                int textLength = SHP_GetDisassemblyLength(disassembly);

                System.Text.StringBuilder text = new System.Text.StringBuilder(textLength + 1);
                SHP_GetDisassemblyText(disassembly, text);

                SHP_FreeDisassembly(disassembly);

                return text.ToString();
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show(Strings.CouldntFindSheepError, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        [DllImport("sheep")]
        private extern static IntPtr SHP_GetDisassembly(byte[] data, int length);

        [DllImport("sheep")]
        private extern static int SHP_GetDisassemblyLength(IntPtr disassembly);

        [DllImport("sheep")]
        private extern static void SHP_GetDisassemblyText(IntPtr disassembly, System.Text.StringBuilder text);

        [DllImport("sheep")]
        private extern static void SHP_FreeDisassembly(IntPtr disassembly);

        struct SHP_Version
        {
            public byte Major, Minor, Revision;
        }

        [DllImport("sheep")]
        private extern static SHP_Version SHP_GetVersion();
    }
}
