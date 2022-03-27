namespace Huppy.Core.Entities
{
    public class PaginatedMessage
    {
        public ulong MessageId { get; set; }
        public ushort CurrentPage { get; set; }

        public PaginatedMessage(ulong messageId, ushort currentPage)
        {
            MessageId = messageId;
            CurrentPage = currentPage;
        }
    }
}