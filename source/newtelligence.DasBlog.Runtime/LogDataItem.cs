#region Copyright (c) 2003, newtelligence AG. All rights reserved.
/*
// Copyright (c) 2003, newtelligence AG. (http://www.newtelligence.com)
// Original BlogX Source Code: Copyright (c) 2003, Chris Anderson (http://simplegeek.com)
// All rights reserved.
//  
// Redistribution and use in source and binary forms, with or without modification, are permitted 
// provided that the following conditions are met: 
//  
// (1) Redistributions of source code must retain the above copyright notice, this list of 
// conditions and the following disclaimer. 
// (2) Redistributions in binary form must reproduce the above copyright notice, this list of 
// conditions and the following disclaimer in the documentation and/or other materials 
// provided with the distribution. 
// (3) Neither the name of the newtelligence AG nor the names of its contributors may be used 
// to endorse or promote products derived from this software without specific prior 
// written permission.
//      
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// -------------------------------------------------------------------------
//
// Original BlogX source code (c) 2003 by Chris Anderson (http://simplegeek.com)
// 
// newtelligence is a registered trademark of newtelligence Aktiengesellschaft.
// 
// For portions of this software, the some additional copyright notices may apply 
// which can either be found in the license.txt file included in the source distribution
// or following this notice. 
//
*/
#endregion


using System;
using System.Xml;
using System.Xml.Serialization;

namespace newtelligence.DasBlog.Runtime
{
    [Serializable]
    [XmlRoot(Namespace=Data.NamespaceURI)]
    [XmlType(Namespace=Data.NamespaceURI)]
    public class LogDataItem
    {
        string _urlRequested;
        string _urlReferrer;
        string _userAgent;
        string _userDomain;
        DateTime _requested;

        public LogDataItem()
        {
        }

        public LogDataItem( string urlRequested, string urlReferrer, string userAgent, string userDomain )
        {
            RequestedUtc = DateTime.Now.ToUniversalTime();
            UrlRequested = urlRequested;
            UrlReferrer = urlReferrer;
            UserAgent = userAgent;
			UserDomain = userDomain;
        }

        public string UrlRequested { get { return _urlRequested; } set { _urlRequested = value; } }
        public string UrlReferrer { get { return _urlReferrer; } set { _urlReferrer = value; } }
        public string UserAgent { get { return _userAgent; } set { _userAgent = value; } }
        public string UserDomain { get { return _userDomain; } set { _userDomain = value; } }
        [XmlIgnore]
        public DateTime RequestedUtc { get { return _requested; } set { _requested = value; } }
        [XmlElement("Requested")]
        public DateTime RequestedLocalTime { get { return (RequestedUtc==DateTime.MinValue||RequestedUtc==DateTime.MaxValue)?RequestedUtc:RequestedUtc.ToLocalTime(); } set { RequestedUtc = (value==DateTime.MinValue||value==DateTime.MaxValue)?value:value.ToUniversalTime(); } }

        [XmlAnyElement]
        public XmlElement[] anyElements;
        [XmlAnyAttribute]
        public XmlAttribute[] anyAttributes;
    }
}
