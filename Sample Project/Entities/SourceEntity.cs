namespace FdoMapper
{
    public class SourceEntity
    {
        public int Id {  get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Url { get; set; }
        public required int? CategoryId { get; set; }
    }
}
