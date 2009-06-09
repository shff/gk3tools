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
        public string Case;
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
                nvc.Verb = line.Attributes[0].Key;
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

                _nvcs.Add(nvc);
            }
        }

        public List<NounVerbCase> NounVerbCases
        {
            get { return _nvcs; }
        }

        private List<NounVerbCase> _nvcs = new List<NounVerbCase>();
    }

    public class NvcResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
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
        public string Verb;
        public string Cursor;
        public string UpButton;
        public string DownButton;
        public string HoverButton;
        public string DisableButton;
        public VerbType Type;
    }

    public class Verbs : Resource.InfoResource
    {
        public Verbs(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (Resource.InfoSection section in Sections)
            {
                foreach (Resource.InfoLine line in section.Lines)
                {
                    VerbInfo verb;
                    verb.Verb = line.Value;

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

                    _verbs.Add(line.Value, verb);
                }
            }
        }

        public VerbInfo this[string name]
        {
            get { return _verbs[name]; }
        }

        private Dictionary<string, VerbInfo> _verbs = new Dictionary<string, VerbInfo>(StringComparer.OrdinalIgnoreCase);
    }

    public class VerbFileLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new Verbs(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[0];
    }
}
