using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MollkyCount.OneDrive
{
    public class OneDriveItem
    {
        public OneDriveItem(IDictionary<string, object> properties)
        {
            if (properties.ContainsKey("id"))
            {
                this.ID = properties["id"] as string;
            }

            if (properties.ContainsKey("name"))
            {
                this.Name = properties["name"] as string;
            }

            if (properties.ContainsKey("type"))
            {
                this.ItemType = properties["type"] as string;
            }
        }

        public string ID { get; private set; }

        public string Name { get; private set; }

        public string ItemType { get; private set; }

        public bool IsFolder
        {
            get
            {
                return !string.IsNullOrEmpty(this.ItemType) &&
                       (this.ItemType.Equals("folder") || this.ItemType.Equals("album"));
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
