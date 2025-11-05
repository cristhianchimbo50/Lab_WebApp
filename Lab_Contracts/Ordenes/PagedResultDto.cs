using System.Collections.Generic;

namespace Lab_Contracts.Ordenes
{
    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
