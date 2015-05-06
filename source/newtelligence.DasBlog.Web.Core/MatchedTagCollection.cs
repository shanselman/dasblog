using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace newtelligence.DasBlog.Web.Core
{
    /// <summary>
    /// Converts a match collection to a tag collection. 
    /// Takes care of illegal tags, non-closed, and non-balanced end-tags.
    /// </summary>
    public class MatchedTagCollection : Collection<MatchedTag>, IEnumerable<MatchedTag>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredTagCollection"/> class.
        /// </summary>
        /// <param name="allowedTags">The allowed tags.</param>
        public MatchedTagCollection(ValidTagCollection allowedTags)
        {
            // param validation
            if (allowedTags == null) { throw new ArgumentNullException("allowedTags"); }

            this.allowedTags = allowedTags;
        }

        /// <summary>
        /// Adds the tag matches in an input string.
        /// </summary>
        /// <param name="matches">The tag matches from an input string.</param>
        public void Init(MatchCollection matches)
        {

            // param validation
            if (matches == null) { throw new ArgumentNullException("matches"); }

            // holds the tags in the order they were matched
            store = new List<MatchedTag>(matches.Count);
            // pre-fill
            for (int i = 0; i < matches.Count; i++)
            {
                store.Add(null);
            }

            // holds the index of the matches that have not yet been matched
            Stack needMatching = new Stack();

            for (int i = 0; i < matches.Count; i++)
            {

                string name = matches[i].Groups["name"].Value;
                if (!allowedTags.IsValidTag(name))
                {
                    // illegal tag
                    store[i] = new MatchedTag(matches[i], false);
                    continue;
                }

                ValidTag validTag = allowedTags[name];

                // valid match
                // if its a self-closing tag, add to store
                bool self = matches[i].Groups["self"].Value.Length > 0;
                if (self)
                {

                    // this is the tag we use to write out the tag later
                    MatchedTag matchedTag = new MatchedTag(matches[i], true);
                    // check for invalid attributes
                    matchedTag.FilterAttributes(validTag);
                    // add to store
                    store[i] = matchedTag;

                    continue;
                }

                // end tag
                bool end = matches[i].Groups["end"].Value.Length > 0;
                if (end)
                {
                    if (needMatching.Count == 0)
                    {
                        // no opening tags for this closing tag, marks as invalid and go to the next
                        store[i] = new MatchedTag(matches[i], false);
                        continue;
                    }

                    // usually the opening tag will be followed by the closing tag
                    int peek = (int)needMatching.Peek();
                    if (String.Compare(matches[peek].Groups["name"].Value, name, true, CultureInfo.InvariantCulture) == 0)
                    {
                        // we have a match, add both to the store
                        store[i] = new MatchedTag(matches[i], true);

                        // filter the attr. for the opening tag
                        MatchedTag matchedTag = new MatchedTag(matches[peek], true);
                        matchedTag.FilterAttributes(validTag);

                        // add to store
                        store[peek] = matchedTag;
                        // remove from the queue
                        needMatching.Pop();
                        continue;
                    }

                    // enumerate through the stack to see if we can find a matching 
                    // opening tag
                    int foundIndex = -1;
                    foreach (int j in needMatching)
                    {
                        // found a match
                        if (String.Compare(matches[j].Groups["name"].Value, name, true, CultureInfo.InvariantCulture) == 0)
                        {
                            foundIndex = j;
                            break;
                        }
                    }

                    if (foundIndex > -1)
                    {
                        while (needMatching.Count > 0 && (int)needMatching.Peek() >= foundIndex)
                        {
                            int pop = (int)needMatching.Pop();
                            if (pop == foundIndex)
                            {
                                // we have a match, add both to the store
                                store[i] = new MatchedTag(matches[i], true);

                                // filter the attr. for the opening tag
                                MatchedTag matchedTag = new MatchedTag(matches[pop], true);
                                matchedTag.FilterAttributes(validTag);

                                // add to store
                                store[pop] = matchedTag;
                            }
                            else
                            {
                                // this tag needs to be closed, since its between our opening and closing tags
                                store[pop] = new MatchedTag(matches[pop], true, true);
                            }
                        }
                    }
                    else
                    {
                        // nothing we could do with this tag, mark as invalid
                        store[i] = new MatchedTag(matches[i], false);
                    }
                    continue;
                }

                // opening tag add to queue until we find the matchin closing tag
                needMatching.Push(i);
            }

            // we have unmatched valid opening tags, make them self closing
            if (needMatching.Count > 0)
            {
                foreach (int i in needMatching)
                {
                    // filter the attr. for the opening tag
                    MatchedTag matchedTag = new MatchedTag(matches[i], true, true);
                    matchedTag.FilterAttributes(allowedTags[matchedTag.TagName]);

                    // add to store
                    store[i] = matchedTag;
                }
            }
        }
        
        // FIELDS

        ValidTagCollection allowedTags;
        List<MatchedTag> store;
    }
}
