using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public static class DialogManager
    {
        //private static LinkedList<YakResource> _yaks = new LinkedList<YakResource>();
        //private static LinkedListNode<YakResource> _lastYak = null;
        private static YakResource _lastYak;
        private static string _lastLicensePlateWithoutSuffix;
        private static int _startingLine;
        private static int _numLinesToPlay;
        private static int _linesPlayed;

        private class DialogWaitHandle : WaitHandle
        {
            public override bool Finished
            {
                get
                {
                    /*for (var itr = _yaks.First; itr != null; itr = itr.Next)
                    {
                        if (itr.Value.IsFinished == false)
                            return false;
                    }

                    return true;*/

                    return _lastYak == null || _lastYak.IsFinished;
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
            if (_lastYak != null)
            {
                Resource.ResourceManager.Unload(_lastYak);
            }

            _lastYak = (Game.YakResource)Resource.ResourceManager.Load(string.Format("E{0}.YAK", licensePlate));
            _startingLine = getLicensePlateEndingNumber(licensePlate, out _lastLicensePlateWithoutSuffix);
            _numLinesToPlay = numLines;
            _linesPlayed = 1;

            _lastYak.Play();

            if (wait)
                return _waitHandle;

            return null;
        }

        public static WaitHandle ContinueDialogue(int numLines, bool wait)
        {
            _numLinesToPlay += numLines;

            Step();

            if (wait)
                return _waitHandle;

            return null;
        }

        public static void Step()
        {
            if (_lastYak != null && _lastYak.IsFinished)
            {
                Resource.ResourceManager.Unload(_lastYak);
                _lastYak = null;
            }
            
            if (_lastYak == null)
            {
                if (_numLinesToPlay > _linesPlayed)
                {
                    // load the new yak
                    int yakNumToPlay = _startingLine + _linesPlayed;
                    _lastYak = (Game.YakResource)Resource.ResourceManager.Load(string.Format("E{0}{1}.YAK", _lastLicensePlateWithoutSuffix, yakNumToPlay));

                    // play the yak
                    _lastYak.Play();

                    _linesPlayed++;
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
