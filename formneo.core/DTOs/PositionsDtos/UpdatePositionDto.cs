namespace formneo.core.DTOs.PositionsDtos
{
    public class UpdatePositionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; } = null;
        public Guid? ParentPositionId { get; set; } = null;
    }
}
