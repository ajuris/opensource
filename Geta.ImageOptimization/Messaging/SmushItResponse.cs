namespace Geta.ImageOptimization.Messaging
{
    public class SmushItResponse
    {
        public string Dest { get; set; }

        public int Dest_Size { get; set; }

        public string Id { get; set; }

        public decimal Percent { get; set; }

        public string Src { get; set; }

        public int Src_Size { get; set; }

        public string Error { get; set; } 
    }
}