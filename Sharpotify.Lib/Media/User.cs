using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;

namespace Sharpotify.Media
{
    public class User
    {
        #region Fields
        private string _name;
        private string _country;
        private string _notification;
        private Dictionary<string, string> _properties;
        #endregion
        #region Properties
        public string Name { get { return _name; } set { _name = value; } }
        public string Country { get { return _country; } set { _country = value; } }
        public string Notification { get { return _notification; } set { _notification = value; } }
        public Dictionary<string, string> Properties { get { return _properties; } set { _properties = value; } }
        public bool IsPremium
        {
            get
            {
                return this._properties["type"].Equals("premium");
            }
        }
        #endregion
        #region Factory
        public User(string name) : this(name, null, null)
        {
        }
        public User(string name, string country, string type)
        {
            this._name = name;
            this._country = country;
            this._notification = null;
            this._properties = new Dictionary<string, string>();
        }
        #endregion
        #region Methods
        public override string ToString()
        {
            return string.Format("[User: {0}, {1}, {2}]", this.Name, this.Country, this.Properties["type"]);
        }
        #endregion
    }
}
