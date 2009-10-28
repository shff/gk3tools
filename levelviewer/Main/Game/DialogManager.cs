using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public static class DialogManager
    {
        private static LinkedList<YakResource> _yaks = new LinkedList<YakResource>();

        private class DialogWaitHandle : WaitHandle
        {
            public override bool Finished
            {
                get
                {
                    for (var itr = _yaks.First; itr != null; itr = itr.Next)
                    {
                        if (itr.Value.IsFinished == false)
                            return false;
                    }

                    return true;
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static DialogWaitHandle _waitHandle = new DialogWaitHandle();

        public static WaitHandle PlayDialogue(string licensePlate, int numLines, bool wait)
        {
            Game.YakResource first = (Game.YakResource)Resource.ResourceManager.Load(string.Format("E{0}.YAK", licensePlate));
            _yaks.AddLast(first);

            if (numLines > 1)
            {
                string licenseWithoutNumber;
                int licenseSuffix = getLicensePlateEndingNumber(licensePlate, out licenseWithoutNumber);

                for (int i = 1; i < numLines; i++)
                {
                    Game.YakResource yak = (Game.YakResource)Resource.ResourceManager.Load(string.Format("E{0}{1}.YAK", licenseWithoutNumber, licenseSuffix + i));
                    _yaks.AddLast(yak);
                }
            }

            first.Play();
            if (wait)
                return _waitHandle;

            return null;
        }

        public static void Step()
        {
            // remove dialogs that are finsihed
            for (var itr = _yaks.First; itr != null; )
            {
                if (itr.Value.IsFinished)
                {
                    Resource.ResourceManager.Unload(itr.Value);

                    // remove
                    var next = itr.Next;
                    _yaks.Remove(itr);
                    itr = next;

                    // play the next one
                    if (itr != null)
                        itr.Value.Play();
                }
                else
                {
                    break;
                }
            }
        }

        private static int getLicensePlateEndingNumber(string licensePlate, out string licenseWithoutNumber)
        {
            int index;
            for (index = licensePlate.Length - 1; index >= 0; index--)
            {
                if (char.IsDigit(licensePlate, index) == false)
                    break;
            }
            index++;

            if (index >= 0 && index < licensePlate.Length)
            {
                licenseWithoutNumber = licensePlate.Substring(0, index);
                return int.Parse(licensePlate.Substring(index));
            }

            licenseWithoutNumber = null;
            return 0;
        }
    }
}
