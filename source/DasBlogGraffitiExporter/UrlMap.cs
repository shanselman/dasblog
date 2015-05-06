using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using log4net;

namespace DasBlogGraffitiExporter
{
    [XmlRoot("UrlMap")]
    public class UrlMap
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _from;
        private string _to;
        public UrlMap()
        { 
        }

        public UrlMap(string from, string to)
        {
            this._from = from;
            this._to = to;
        }

        [XmlAttribute("To")]
        public string To
        {
            get { return _to; }
            set { _to = value; }
        }

        [XmlAttribute("From")]
        public string From
        {
            get { return _from; }
            set { _from = value; }
        }

        public static void Save(List<UrlMap> urlMapping, string fullPath)
        {
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite))
            {
                if (fileStream != null)
                {
                    try
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(List<UrlMap>));
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            ser.Serialize(writer, urlMapping);
                        }

                        logger.InfoFormat("Saved url map file to:{0}", fullPath);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Error writing url map file", e);
                    }
                }
            }
        }

        public static List<UrlMap> Load(string fullPath)
        {
            using (FileStream reader = new FileStream(fullPath, FileMode.Open))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<UrlMap>));
                List<UrlMap> urlMapping = (List<UrlMap>)ser.Deserialize(reader);
                return urlMapping;
            }
        }
    }
}

                        

   
   