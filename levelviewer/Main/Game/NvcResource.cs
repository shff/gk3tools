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
        public Nouns Noun;
        public Verbs Verb;
        public string Case;
        public NvcApproachType Approach;
        public string Target;
        public string Script;

        public override string ToString()
        {
            // this will help with debugging (a little)
            return Noun + ":" + Verb + ":" + Case;
        }
    }

    struct NounVerbCombination
    {
        public NounVerbCombination(Nouns noun, Verbs verb, bool egoIsGabe)
        {
            Noun = noun;
            Verb = verb;
            EgoIsGabe = egoIsGabe;
        }

        public Nouns Noun;
        public Verbs Verb;
        public bool EgoIsGabe;
    }

    class NounVerbComparison : IEqualityComparer<NounVerbCombination>
    {
        public bool Equals(NounVerbCombination nv1, NounVerbCombination nv2)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(nv1.Noun, nv2.Noun) &&
                StringComparer.OrdinalIgnoreCase.Equals(nv1.Verb, nv2.Verb) &&
                nv1.EgoIsGabe == nv2.EgoIsGabe;
        }

        public int GetHashCode(NounVerbCombination nv)
        {
            // i have no idea how good a hash this returns
            return StringComparer.OrdinalIgnoreCase.GetHashCode(nv.Noun) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(nv.Verb);
        }
    }

    public class NvcResource : Resource.InfoResource
    {
        public NvcResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            // load the Logic aliases
            foreach (Resource.InfoSection section in _sections)
            {
                if (section.Name.Equals("Logic", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        _logic.Add(new KeyValuePair<string, string>(line.Attributes[0].Key, line.Attributes[0].Value.Replace("{", "").Replace("}", "")));
                    }
                }
            }

            foreach (Resource.InfoLine line in GlobalSection.Lines)
            {
                NounVerbCase nvc;
                nvc.Noun = NounUtils.ConvertStringToNoun(line.Value);
                nvc.Verb = VerbsUtils.ConvertStringToVerbs(line.Attributes[0].Key);
                nvc.Case = line.Attributes[1].Key;

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

                line.TryGetAttribute("target", out nvc.Target);
                line.TryGetAttribute("script", out nvc.Script);

                // remove any { } around the script
                // (normally they wouldn't mess anything up, but there are a few
                // that have no ; at the end, and those will mess up)
                int firstBracket = nvc.Script.IndexOf("{");
                int lastBracket = nvc.Script.LastIndexOf("}");
                if (firstBracket >= 0 && lastBracket >= 0)
                    nvc.Script = nvc.Script.Substring(firstBracket + 1, lastBracket - firstBracket - 1);
                else if (firstBracket >= 0)
                    nvc.Script = nvc.Script.Substring(firstBracket + 1);
                else if (lastBracket >= 0)
                    nvc.Script = nvc.Script.Substring(0, lastBracket);

                add(nvc);
            }
        }

        public List<NounVerbCase> NounVerbCases
        {
            get { return _nvcs; }
        }

        public List<KeyValuePair<string, string>> Logic
        {
            get { return _logic; }
        }

        private void add(NounVerbCase nvc)
        {
            // apparently GK3 mostly prioritizes NVCs alphabetically
            // by case, and sometimes that affects which action plays
            // when the player uses a noun/verb combo.
            for (int i = 0; i < _nvcs.Count; i++)
            {
                if (string.Compare(nvc.Case, _nvcs[i].Case, true) < 0)
                {
                    _nvcs.Insert(i, nvc);
                    return;
                }
            }

            // still here? add normally
            _nvcs.Add(nvc);
        }

        private List<NounVerbCase> _nvcs = new List<NounVerbCase>();
        private List<KeyValuePair<string, string>> _logic = new List<KeyValuePair<string, string>>();
    }

    public class NvcResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new NvcResource(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "NVC" };
    }

    public enum VerbType
    {
        Normal,
        Inventory,
        Topic,
        RecurringTopic,
        Chat
    }

    public struct VerbInfo
    {
        public Verbs Verb;
        public string Cursor;
        public string UpButton;
        public string DownButton;
        public string HoverButton;
        public string DisableButton;
        public VerbType Type;
    }

    public class VerbDefinitions : Resource.InfoResource
    {
        public VerbDefinitions(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (Resource.InfoSection section in Sections)
            {
                foreach (Resource.InfoLine line in section.Lines)
                {
                    VerbInfo verb;
                    verb.Verb = VerbsUtils.ConvertStringToVerbs(line.Value);

                    line.TryGetAttribute("cursor", out verb.Cursor);
                    line.TryGetAttribute("up", out verb.UpButton);
                    line.TryGetAttribute("down", out verb.DownButton);
                    line.TryGetAttribute("hover", out verb.HoverButton);
                    line.TryGetAttribute("disable", out verb.DisableButton);
                    verb.Type = VerbType.Normal;

                    string type;
                    if (line.TryGetAttribute("type", out type))
                    {
                        if (type.Equals("Normal", StringComparison.OrdinalIgnoreCase))
                            verb.Type = VerbType.Normal;
                        else if (type.Equals("Topic", StringComparison.OrdinalIgnoreCase))
                            verb.Type = VerbType.Topic;
                        else if (type.Equals("Inventory", StringComparison.OrdinalIgnoreCase))
                            verb.Type = VerbType.Inventory;
                        else if (type.Equals("RecurringTopic", StringComparison.OrdinalIgnoreCase))
                            verb.Type = VerbType.RecurringTopic;
                        else if (type.Equals("Chat", StringComparison.OrdinalIgnoreCase))
                            verb.Type = VerbType.Chat;
                        else
                            throw new Resource.InfoResourceException("Unknown verb button type: " + type);
                    }

                    _verbs.Add(verb.Verb, verb);
                }
            }
        }

        public VerbInfo this[string name]
        {
            get
            {
                Verbs verb = VerbsUtils.ConvertStringToVerbs(name);

                return this[verb];
            }
        }

        public VerbInfo this[Verbs verb]
        {
            get
            {
                // TODO: how to we properly handle ANY_INV_ITEM?
                if (verb == Verbs.V_ANY_INV_ITEM ||
                    verb == Verbs.V_TIMER_EXP)

                    return new VerbInfo();

                return _verbs[verb];
            }
        }

        private Dictionary<Verbs, VerbInfo> _verbs = new Dictionary<Verbs, VerbInfo>();
    }

    public class VerbFileLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new VerbDefinitions(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[0];
    }
}
