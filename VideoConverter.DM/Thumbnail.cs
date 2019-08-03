namespace VideoConverter.DM
{
    public class Thumbnail
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public decimal TimeInVideo { get; set; }
        public int VideoId { get; set; }
        public byte[] Content { get; set; }
        public int ContentLength { get; set; }


    }
}
