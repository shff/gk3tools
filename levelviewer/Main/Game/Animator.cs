using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public static class Animator
    {
        private static List<AnmResource> _anms = new List<AnmResource>();
        private static List<YakResource> _yaks = new List<YakResource>();

        internal static WaitHandle Add(AnmResource anm, bool wait)
        {
            anm.ReferenceCount++;

            add(_anms, anm);

            if (wait)
            {
                return anm.PlayAndWait();
            }

            anm.Play();
            return null;
        }

        internal static WaitHandle Add(YakResource yak, bool wait)
        {
            yak.ReferenceCount++;

            add(_yaks, yak);

            if (wait)
            {
                return yak.PlayAndWait();
            }

            yak.Play();
            return null;
        }

        public static void StopAll()
        {
            for (int i = 0; i < _anms.Count; i++)
            {
                if (_anms[i] != null)
                {
                    Resource.ResourceManager.Unload(_anms[i]);
                }
            }

            for (int i = 0; i < _yaks.Count; i++)
            {
                if (_yaks[i] != null)
                {
                    Resource.ResourceManager.Unload(_yaks[i]);
                }
            }

            _anms.Clear();
            _yaks.Clear();
        }

        public static void Advance(int elapsedTime)
        {
            int timeNow = GameManager.TickCount;

            // process yaks
            for (int i = 0; i < _yaks.Count; i++)
            {
                if (_yaks[i] != null)
                {
                    if (_yaks[i].IsPlaying == false)
                    {
                        // done playing the yak, so we can remove it
                        Resource.ResourceManager.Unload(_yaks[i]);
                        _yaks[i] = null;
                    }
                    else
                    {
                        int timeSinceStart = timeNow - _yaks[i].TimeAtPlayStart;
                        int startIndex, count;
                        AnimationResource.GetAllFramesSince(_yaks[i].Gk3Section, timeSinceStart,
                            _yaks[i].NumFrames * AnimationResource.MillisecondsPerFrame,
                            out startIndex, out count);

                        Actor actor = null;
                        for (int lineIndex = startIndex; lineIndex < startIndex + count; lineIndex++)
                        {
                            string param1 = _yaks[i].Gk3Section.Lines[lineIndex].Params[0].StringValue;

                            if (param1.Equals("LIPSYNCH", StringComparison.OrdinalIgnoreCase))
                            {
                                if (actor == null)
                                {
                                    actor = SceneManager.GetActor(_yaks[i].Speaker);
                                    if (actor == null)
                                        break; // couldn't find this actor for some reason, so give up
                                }

                                string param3 = _yaks[i].Gk3Section.Lines[lineIndex].Params[2].StringValue;
                                actor.SetMouth(param3);
                            }
                        }
                    }
                }
            }
        }

        private static void add<T>(List<T> list, T item)
        {
            // try to add in an empty spot first
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    list[i] = item;
                    return;
                }
            }

            // no empty spots found, so insert at the end
            list.Add(item);
        }
    }
}
