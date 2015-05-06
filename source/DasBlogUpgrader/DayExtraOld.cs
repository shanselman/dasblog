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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using newtelligence.DasBlog.Util;
using System.Web;

namespace newtelligence.DasBlog.Runtime
{
	[Serializable, XmlRoot(ElementName="DayExtra",Namespace="urn:newtelligence-com:dasblog:runtime:data"), XmlType(Namespace="urn:newtelligence-com:dasblog:runtime:data")]
	public class DayExtraOld
	{
		private CommentCollection _comments = new CommentCollection();
		private TrackingCollection _trackings = new TrackingCollection();
		private DateTime _date;

		[XmlIgnore]
		public DateTime DateUtc
		{
			get	{ return _date;	}
			set	{ _date = value.Date; }
		}

		[XmlElement("Date")]
		public DateTime DateLocalTime
		{
			get { return _date; }
			set { _date = value; }
		}

		public CommentCollection Comments
		{
			get { return _comments; }
		}

		public TrackingCollection Trackings
		{
			get { return _trackings; }
		}

		[XmlAnyElement]
		public XmlElement[] anyElements;

		[XmlAnyAttribute]
		public XmlAttribute[] anyAttributes;

		public DayExtraOld()
		{
			
		}

		public DayExtraOld(string filePath)
		{
			LoadDayExtra(filePath);
		}

		internal void LoadDayExtra(string fullPath)
		{
			FileStream fileStream = FileUtils.OpenForRead(fullPath);
			if (fileStream != null)
			{
				try
				{
					XmlSerializer ser = new XmlSerializer(typeof (DayExtraOld), "urn:newtelligence-com:dasblog:runtime:data");
					using (StreamReader reader = new StreamReader(fileStream))
					{
                        //TODO: SDH: We need to have a better namespace upgrading solution for Medium Trust for folks with the OLD XML format
						//XmlNamespaceUpgradeReader upg = new XmlNamespaceUpgradeReader(reader, "", "urn:newtelligence-com:dasblog:runtime:data");
						DayExtraOld e = (DayExtraOld) ser.Deserialize(reader);
						this._date = e.DateLocalTime;
						this._comments = e.Comments;
						this._trackings = e.Trackings;
					}
				}
				catch (Exception e)
				{
					ErrorTrace.Trace(TraceLevel.Error, e);
				}
				finally
				{
					fileStream.Close();
				}
			}
		}
	}
}