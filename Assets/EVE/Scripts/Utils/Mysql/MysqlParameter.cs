using MySql.Data.MySqlClient;


namespace EVE.Scripts.Utils.Mysql
{
    public class MysqlParameter
    {
        public string name;
        public MySqlDbType type;
        public object value;

        public MysqlParameter(string name, MySqlDbType type, object value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }
    }
}
