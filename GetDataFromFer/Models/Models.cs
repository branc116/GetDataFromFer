using System;
using System.Collections.Generic;
using System.Text;

namespace GetDataFromFer.Models
{

    public class ClassFolderRoot
    {
        public string name { get; set; }
        public string type { get; set; }
        public string path { get; set; }
        public string id { get; set; }
        public Item[] items { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string display { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string type { get; set; }
        public string subtype { get; set; }
        public object size { get; set; }
        public string visible { get; set; }
        public string time_posted { get; set; }
        public object lastedit_time { get; set; }
        public string public_name { get; set; }
        public string description { get; set; }
        public string _params { get; set; }
        public Item[] items { get; set; }
    }
}
