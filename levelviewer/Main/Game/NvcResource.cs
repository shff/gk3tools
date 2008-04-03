using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public enum NvcApproachType
    {
        None,
        WalkTo,
        TurnToModel
    }

    public struct NounVerbCase
    {
        public string Noun;
        public string Verb;
        public string Ego;
        public NvcApproachType Approach;
        public string Target;
        public string Script;
    }

    public class NvcResource : Resource.InfoResource
    {
        public NvcResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (Resource.InfoLine line in GlobalSection.Lines)
            {
                NounVerbCase nvc;
                nvc.Noun = line.Value;
                nvc.Verb = line.Attributes[0].Value;
                nvc.Ego = line.Attributes[1].Value;

                string approach;
                if (line.TryGetAttribute("approach", out approach))
                {
                    if (approach.ToUpper() == "WALKTO")
                        nvc.Approach = NvcApproachType.WalkTo;
                    else if (approach.ToUpper() == "TURNTOMODEL")
                        nvc.Approach = NvcApproachType.TurnToModel;
                    else
                        nvc.Approach = NvcApproachType.None;
                }
                else
                {
                    nvc.Approach = NvcApproachType.None;
                }

                
            }
        }
    }
}
