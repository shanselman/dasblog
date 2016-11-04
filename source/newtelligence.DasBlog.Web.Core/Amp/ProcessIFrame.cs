using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace newtelligence.DasBlog.Web.Core.Amp
{
    public class ProcessIFrame : BaseAmpProcess
    {
        public ProcessIFrame() : base(@"<iframe.*? src=[""'](.+?)[""'].*?></iframe>") { }

        protected override string TagMatchEvaluator(Match match)
        {
            string htmlTag = match.ToString();

            if (match != null && match.Groups != null && match.Groups.Count > 1)
            {
                htmlTag = tagReplacementTemplate.Replace("tag", "amp-iframe");
                htmlTag = string.Format(htmlTag, match.Groups[1].Value, 560, 315);
            }

            return htmlTag;
        }
    }
}
