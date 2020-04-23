using System;

namespace EVE.Scripts.XML
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