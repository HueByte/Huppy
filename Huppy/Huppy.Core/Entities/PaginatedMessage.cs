namespace Huppy.Core.Entities
{
    public class PaginatedMessageState
    {
        public ulong MessageId { get; set; }
        public ushort CurrentPage { get; set; }
        public string EntryName { get; set; }
        public bool IsDynamic { get; set; }

        public PaginatedMessageState(ulong messageId, ushort currentPage, string entryName, bool isDynamic)
        {
            MessageId = messageId;
            CurrentPage = currentPage;
            EntryName = entryName;
            IsDynamic = isDynamic;
        }
    }
}