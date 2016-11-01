using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace newtelligence.DasBlog.Web.Core
{
    public class AmpifyPageFilter : MemoryStream
    {
        private Stream _stream = null;
        
        const string imgRegEx = @"<img.+? src=[""'](.+?)[""'].*? width=[""'](.+?)[""'].*? height=[""'](.+?)[""'].*?>";
        const string frameRegEx = @"<iframe.*? src=[""'](.+?)[""'].*?></iframe>";
        const string twitterRegEx = @"<script .*?platform.twitter.com.*?></script>";

        const string tagReplacementTemplate = "<tag src=\"{0}\" layout=\"responsive\" width=\"{1}\" height=\"{2}\" ></tag>";
        const string twitterReplacement = "<amp-twitter width = \"390\" height=\"330\" layout=\"responsive\" data-tweetid=\"{0}\" data-cards=\"hidden\"></amp-twitter>";

        public AmpifyPageFilter(Stream stream)
        {
            _stream = stream;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            string content = UTF8Encoding.UTF8.GetString(buffer);

            content = AmpifyBlogContent(content);

            _stream.Write(UTF8Encoding.UTF8.GetBytes(content), offset, UTF8Encoding.UTF8.GetByteCount(content));

            base.Write(UTF8Encoding.UTF8.GetBytes(content), offset, UTF8Encoding.UTF8.GetByteCount(content));
        }

        private string AmpifyBlogContent(string blogcontent)
        {
            blogcontent = ReplaceImgTag(blogcontent);

            blogcontent = ReplaceFrameTag(blogcontent);

            blogcontent = ReplaceTwitterTag(blogcontent);

            return blogcontent;
        }

        private string ReplaceImgTag(string content)
        {
            Regex urlRx = new Regex(imgRegEx, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return urlRx.Replace(content, new MatchEvaluator(ImageTagMatch));
        }

        private string ReplaceFrameTag(string content)
        {
            Regex urlRx = new Regex(frameRegEx, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return urlRx.Replace(content, new MatchEvaluator(FrameTagMatch));
        }

        private string ReplaceTwitterTag(string content)
        {
            Regex urlRx = new Regex(twitterRegEx, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return urlRx.Replace(content, new MatchEvaluator(TwitterTagMatch));
        }

        private string ImageTagMatch(Match match)
        {
            string imageTag = match.ToString();

            if (match != null && match.Groups != null && match.Groups.Count > 1)
            {
                imageTag = tagReplacementTemplate.Replace("tag", "amp-img");
                imageTag = string.Format(imageTag, match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
            }

            return imageTag;
        }

        private string FrameTagMatch(Match match)
        {
            string htmlTag = match.ToString();

            if (match != null && match.Groups != null && match.Groups.Count > 1)
            {
                htmlTag = tagReplacementTemplate.Replace("tag", "amp-iframe");
                htmlTag = string.Format(htmlTag, match.Groups[1].Value, 560, 315);
            }

            return htmlTag;
        }

        private string TwitterTagMatch(Match match)
        {
            string twitterTag = match.ToString();

            if (match != null && match.Groups != null && match.Groups.Count > 1)
            {
                twitterTag = string.Format(twitterReplacement, match.Groups[1].Value);
            }

            return string.Empty;
        }
    }

}
