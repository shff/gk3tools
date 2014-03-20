using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public static class NvcManager
    {
        private static List<NounVerbCase> _globalNvcs = new List<NounVerbCase>();
        private static List<NounVerbCase> _localNvcs = new List<NounVerbCase>();
        private static Dictionary<string, string> _globalNvcLogic = new Dictionary<string, string>();
        private static Dictionary<string, string> _localNvcLogic = new Dictionary<string, string>();
        private static Dictionary<Nouns, List<NounVerbCase>> _nounNvcMap = new Dictionary<Nouns, List<NounVerbCase>>();

        public static void Reset()
        {
            _localNvcs.Clear();
            _localNvcLogic.Clear();
            _nounNvcMap.Clear();
        }

        public static void AddNvc(NvcResource nvclist, bool isGlobal)
        {
            foreach (NounVerbCase nvc in nvclist.NounVerbCases)
            {
                if (isGlobal)
                    _globalNvcs.Add(nvc);
                else
                    _localNvcs.Add(nvc);
            }

            foreach (KeyValuePair<string,string> logic in nvclist.Logic)
            {
                if (isGlobal)
                    _globalNvcLogic.Add(logic.Key, logic.Value);
                else
                    _localNvcLogic.Add(logic.Key, logic.Value);
            }
        }

        /// <summary>
        /// "Compiles" the NVC data. This should be called once all the NVCs
        /// for a scene have been added to the manager.
        /// </summary>
        public static void Compile()
        {
            foreach (NounVerbCase nvc in _localNvcs)
            {
                if (_nounNvcMap.ContainsKey(nvc.Noun) == false)
                    _nounNvcMap.Add(nvc.Noun, new List<NounVerbCase>());

                _nounNvcMap[nvc.Noun].Add(nvc);
            }

            foreach (NounVerbCase nvc in _globalNvcs)
            {
                if (_nounNvcMap.ContainsKey(nvc.Noun) == false)
                    _nounNvcMap.Add(nvc.Noun, new List<NounVerbCase>());

                _nounNvcMap[nvc.Noun].Add(nvc);
            }

            // TODO: in the future it might be a cool idea to compile the sheep script for the cases
        }

        public static bool NounHasTopicsLeft(Nouns noun)
        {
            List<NounVerbCase> nvcs;
            if (_nounNvcMap.TryGetValue(noun, out nvcs))
            {
                foreach(NounVerbCase nvc in nvcs)
                {
                    if (VerbsUtils.IsTopicVerb(nvc.Verb) &&
                        evaluateNvcLogic(nvc.Noun, nvc.Verb, nvc.Case))
                        return true;
                }
            }

            return false;
        }

        public static List<Game.NounVerbCase> GetNounVerbCases(Nouns noun, bool evaluate)
        {
            List<Game.NounVerbCase> results = new List<Gk3Main.Game.NounVerbCase>();
            List<Game.NounVerbCase> nounNvcs;

            if (_nounNvcMap.TryGetValue(noun, out nounNvcs))
            {
                foreach (NounVerbCase nvc in nounNvcs)
                {
                    if (!evaluate || evaluateNvcLogic(noun, nvc.Verb, nvc.Case))
                        addUnique(nvc, results, nvc.Case.Equals("ALL", StringComparison.OrdinalIgnoreCase));
                }
            }

            // not done yet... there may be aliases/groups
            List<Nouns> groups = NounUtils.GetNounGroupsFromMember(noun);
            if (groups != null)
            {
                foreach (Nouns n in groups)
                {
                    if (_nounNvcMap.TryGetValue(n, out nounNvcs))
                    {
                        foreach (NounVerbCase nvc in nounNvcs)
                        {
                            if (!evaluate || evaluateNvcLogic(n, nvc.Verb, nvc.Case))
                                addUnique(nvc, results, nvc.Case.Equals("ALL", StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
            }

            return results;
        }

        public static List<Game.NounVerbCase> GetNounVerbCases(Nouns noun, Verbs verb, bool evaluate)
        {
            List<Game.NounVerbCase> results = new List<Gk3Main.Game.NounVerbCase>();
            List<Game.NounVerbCase> nounNvcs;

            if (_nounNvcMap.TryGetValue(noun, out nounNvcs))
            {
                foreach (NounVerbCase nvc in nounNvcs)
                {
                    if (nvc.Verb == verb && 
                        (!evaluate || evaluateNvcLogic(noun, nvc.Verb, nvc.Case)))
                        addUnique(nvc, results, nvc.Case.Equals("ALL", StringComparison.OrdinalIgnoreCase));
                }
            }

            // not done yet... there may be aliases/groups
            List<Nouns> groups = NounUtils.GetNounGroupsFromMember(noun);
            if (groups != null)
            {
                foreach (Nouns n in groups)
                {
                    if (_nounNvcMap.TryGetValue(n, out nounNvcs))
                    {
                        foreach (NounVerbCase nvc in nounNvcs)
                        {
                            if (nvc.Verb == verb &&
                                (!evaluate || evaluateNvcLogic(n, nvc.Verb, nvc.Case)))
                                addUnique(nvc, results, nvc.Case.Equals("ALL", StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
            }

            return results;
        }

        private static bool evaluateNvcLogic(Nouns noun, Verbs verb, string conditionName)
        {
            if (conditionName.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                return true;
            if (conditionName.Equals("GRACE_ALL", StringComparison.OrdinalIgnoreCase))
                return GameManager.CurrentEgo == Ego.Grace;
            if (conditionName.Equals("GABE_ALL", StringComparison.OrdinalIgnoreCase))
                return GameManager.CurrentEgo == Ego.Gabriel;
            if (conditionName.Equals("1ST_TIME", StringComparison.OrdinalIgnoreCase))
                return GameManager.GetNounVerbCount(noun, verb) == 0;
            if (conditionName.Equals("2CD_TIME", StringComparison.OrdinalIgnoreCase) ||
                conditionName.Equals("2ND_TIME", StringComparison.OrdinalIgnoreCase))
                return GameManager.GetNounVerbCount(noun, verb) == 1;
            if (conditionName.Equals("3RD_TIME", StringComparison.OrdinalIgnoreCase))
                return GameManager.GetNounVerbCount(noun, verb) == 2;
            if (conditionName.Equals("OTR_TIME", StringComparison.OrdinalIgnoreCase))
                return GameManager.GetNounVerbCount(noun, verb) > 0;
            if (conditionName.Equals("TIME_BLOCK", StringComparison.OrdinalIgnoreCase))
                return true; // TODO: what does this case mean?
            if (conditionName.Equals("TIME_BLOCK_OVERRIDE", StringComparison.OrdinalIgnoreCase))
                return true; // TODO: what does this case mean?
            if (conditionName.Equals("DIALOGUE_TOPICS_INTRO", StringComparison.OrdinalIgnoreCase))
                return false; // TODO
            if (conditionName.Equals("DIALOGUE_TOPICS_NOT_INTRO", StringComparison.OrdinalIgnoreCase))
                return false; // TODO
            if (conditionName.Equals("DIALOGUE_TOPICS_LEFT", StringComparison.OrdinalIgnoreCase))
                return NounHasTopicsLeft(noun);
            if (conditionName.Equals("NOT_DIALOGUE_TOPICS_LEFT", StringComparison.OrdinalIgnoreCase))
                return !NounHasTopicsLeft(noun);
            if (conditionName.Equals("CLOSE_UP", StringComparison.OrdinalIgnoreCase))
                return false; // TODO
            if (conditionName.Equals("NOT_CLOSEUP", StringComparison.OrdinalIgnoreCase))
                return false; // TODO
            if (conditionName.Equals("IN_INVENTORY", StringComparison.OrdinalIgnoreCase))
                return false; // TODO

            // guess it was something else
            string condition;
            if (_localNvcLogic.ContainsKey(conditionName))
                condition = _localNvcLogic[conditionName];
            else if (_globalNvcLogic.ContainsKey(conditionName))
                condition = _globalNvcLogic[conditionName];
            else
            {
                if (_localNvcLogic.TryGetValue("G_" + conditionName, out condition) == false)
                    return false; // apparently some cases just don't exist anywhere
            }

            return Sheep.SheepMachine.RunSnippet(condition, noun, verb) > 0;
        }

        private static void addUnique(NounVerbCase nvc, List<NounVerbCase> nvcs, bool isAll)
        {
            for (int i = 0; i < nvcs.Count; i++)
            {
                if (nvcs[i].Noun == nvc.Noun &&
                    nvcs[i].Verb == nvc.Verb)
                {
                    // the "ALL" case seems to be like an "else", where
                    // it is overridden if there are other valid cases.
                    // So since we already have a valid case, ignore this one if it's "ALL".
                    if (isAll) return;

                    // compare priorities and keep the higher of the two
                    int oldPriority = CaseUtils.GetCasePriority(nvcs[i].CaseType);
                    int newPriority = CaseUtils.GetCasePriority(nvc.CaseType);

                    if (newPriority > oldPriority)
                    {
                        nvcs[i] = nvc;
                    }
                    
                    return;
                }
            }

            // still here? must be a new case.
            nvcs.Add(nvc);
        }
    }
}
