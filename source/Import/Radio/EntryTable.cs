using System;
using System.Collections;

namespace DasBlog.Import.Radio
{
	class EntryTable
	{
		Hashtable data = new Hashtable();
		string name;

		public string Name { get { return name; } set { name = value; } }
		public string UniqueId { get { return int.Parse(name).ToString(); } }
		public IDictionary Data { get { return data; } }
		public DateTime When { get { return (DateTime)Data["when"]; } }
		public string Text { get { return (string)Data["text"]; } }
		public string Title { get { return (string)Data["title"]; } }
		public string Link {  get { return (string)Data["link"]; } }
		public string Categories { get { return (string)Data["categories"]; } }
		public bool NotOnHomePage { get { return (bool)Data["flNotOnHomePage"]; } }
	}
}
