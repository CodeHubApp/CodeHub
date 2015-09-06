namespace System
{
    public class EnumDescription : Attribute
    {
        public string Value { get; private set; }
        public EnumDescription(string description)
        {
            Value = description;
        }
    }

    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var enumType = value.GetType();
            var field = enumType.GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(EnumDescription), false);
            return attributes.Length == 0 ? value.ToString() : ((EnumDescription)attributes[0]).Value;
        }
    }
}

