using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public static class DialogManager
    {
        private static LinkedList<YakResource> _yaks = new LinkedList<YakResource>();
        private static LinkedListNode<YakResource> _lastYak = null;
        private static MomResource _lastMom = null;
        private static bool _waitingOnMom;
        private static string _lastLicensePlate;
        private static int _numLinesToPlay;
        private static int _linesPlayed;

        private class DialogWaitHandle : WaitHandle
        {
            public override bool Finished
            {
                get
                {
                    if (_waitingOnMom == true && _lastMom != null && _lastMom.IsFinished == false)
                        return false;
                    if (_lastYak != null && _lastYak.Value.IsFinished == false)
                        return false;

                    return true;
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static DialogWaitHandle _waitHandle = new DialogWaitHandle();

        public static WaitHandle PlayDialogue(string licensePlate, int numLines, bool plateHasLanguageCode, bool wait)
        {
            if (plateHasLanguageCode == false)
                licensePlate = "E" + licensePlate;

            YakResource yak = SceneManager.SceneContentManager.Load<YakResource>(string.Format("{0}.YAK", licensePlate));
            _lastYak = new LinkedListNode<YakResource>(yak);
            _yaks.AddLast(yak);
            _lastLicensePlate = licensePlate.Substring(1); // remove the language code
            _numLinesToPlay = numLines;
            _linesPlayed = 1;

            //yak.Play();
            Animator.Add(yak, false);

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

        public static WaitHandle PlayMom(string momFile, bool wait)
        {
            _lastMom = SceneManager.SceneContentManager.Load<Game.MomResource>("E" + momFile + ".MOM");
            _lastMom.Play();

            if (wait)
            {
                _waitingOnMom = true;
                return _waitHandle;
            }

            return null;
        }

        public static void Step()
        {
            // continue the Mom
            if (_lastMom != null)
            {
                _lastMom.Step();
            }

            // if the last played yak is finished then see if there are more yaks to play
            if (_lastYak != null && _lastYak.Value.IsFinished)
            {
                if (_numLinesToPlay > _linesPlayed)
                {
                    // load the new yak
                    _lastLicensePlate = incrementLicensePlate(_lastLicensePlate, 1);
                    
                    YakResource yak = SceneManager.SceneContentManager.Load<YakResource>("E" + _lastLicensePlate);
                    _lastYak = new LinkedListNode<YakResource>(yak);
                    _yaks.AddLast(_lastYak);

                    // play the yak
                    //yak.Play();
                    Animator.Add(yak, false);

                    _linesPlayed++;
                }
            }

            // remove any finished yaks
            for (LinkedListNode<YakResource> yakNode = _yaks.First; yakNode != null; )
            {
                if (yakNode.Value.IsPlaying == false)
                {
                    if (_lastYak == yakNode)
                    {
                        _lastYak = null;

                        if (_numLinesToPlay == _linesPlayed)
                        {
                            _numLinesToPlay = 0;
                            _linesPlayed = 0;
                        }
                    }

                    LinkedListNode<YakResource> next = yakNode.Next;
                    _yaks.Remove(yakNode);
                    yakNode = next;
                }
                else
                {
                    yakNode = yakNode.Next;
                }
            }

            // if the Mom is finished then get rid of it
            if (_lastMom != null && _lastMom.IsFinished)
            {
                _lastMom = null;
            }
        }

        private static string incrementLicensePlate(string licensePlate, int numLines)
        {
            const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            // get the last character
            char last = licensePlate[licensePlate.Length - 1];
            int digit = digits.IndexOf(char.ToUpper(last)) + numLines;

            // build the new plate #
            return licensePlate.Substring(0, licensePlate.Length - 1) + digits[digit];
        }
    }
}
