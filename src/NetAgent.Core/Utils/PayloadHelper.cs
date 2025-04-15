using NetAgent.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Core.Utils
{
    public class PayloadHelper
    {
        public static string ConvertSearchResultsToString(IReadOnlyList<SemanticSearchResult> results)
        {
            // Duyệt qua danh sách kết quả và chuyển mỗi kết quả thành chuỗi
            var resultStrings = results.Select(result =>
                $"ID: {result.Id}, Score: {result.Score}, Memory: {result.ToMemoryString()}"
            ).ToList();

            // Nối tất cả các chuỗi lại với nhau
            return string.Join("\n", resultStrings);
        }
    }
}
