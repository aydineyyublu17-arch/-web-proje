namespace PrintMarket.Models
{
    public class MachineImage
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsMain { get; set; }

        public int MachineId { get; set; }
        public virtual Machine Machine { get; set; } = null!;
    }
}
