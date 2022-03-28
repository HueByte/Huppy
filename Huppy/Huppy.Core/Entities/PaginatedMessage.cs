namespace Huppy.Core.Entities
{
    public class PaginatedMessage
    {
        public ulong MessageId { get; set; }
        public ushort CurrentPage { get; set; }
        public string EntryName { get; set; }

        public PaginatedMessage(ulong messageId, ushort currentPage, string entryName)
        {
            MessageId = messageId;
            CurrentPage = currentPage;
            EntryName = entryName;
        }
    }
}