using System;
using System.Xml.Serialization;

namespace Assets.EVE.Scripts.XML
{
    /// <summary>
    /// Stores database settings
    /// </summary>
    [Serializable]
    public class DatabaseSettings
    {
        public string Server, Schema, User, Password;
    }
}