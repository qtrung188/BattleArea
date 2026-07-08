using BattleArenaBackendAPI.Exceptions;

namespace BattleArenaBackendAPI.DTOs
{
    /// <summary>
    /// Standard query-string pagination parameters. Bound from the query string
    /// (e.g. ?page=2&amp;pageSize=50). Defaults to page 1, page size 20.
    /// </summary>
    public class PagedRequest
    {
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 20;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = DefaultPageSize;

        /// <summary>
        /// Validates the incoming values, throwing <see cref="BadRequestException"/>
        /// (handled by the global exception handler as a 400) if they are out of
        /// range. This prevents clients from requesting unbounded result sets.
        /// </summary>
        public void Validate()
        {
            if (Page < 1)
            {
                throw new BadRequestException("Page must be greater than or equal to 1.");
            }

            if (PageSize < 1 || PageSize > MaxPageSize)
            {
                throw new BadRequestException(
                    $"PageSize must be between 1 and {MaxPageSize}.");
            }
        }
    }
}
