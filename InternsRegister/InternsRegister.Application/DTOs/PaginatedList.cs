namespace InternsRegister.Application.DTOs
{
    public record PaginatedList<T>(
        List<T> Items,
        int TotalCount,
        int PageIndex,
        int PageSize
    )
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageIndex > 0;
        public bool HasNextPage => PageIndex < TotalPages - 1;
    }
}
