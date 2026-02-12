namespace SuiPOS.ViewModels
{
    public class AttributeVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<AttributeValueVM> Values { get; set; } = new();
    }

    public class AttributeValueVM
    {
        public Guid Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class AttributeInputVM
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
