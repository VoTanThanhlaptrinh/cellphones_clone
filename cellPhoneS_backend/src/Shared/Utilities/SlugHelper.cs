using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Lớp tiện ích tĩnh hỗ trợ tạo Slug (Normal Name) cho URL.
/// </summary>
public static class SlugHelper
{
    /// <summary>
    /// Chuyển đổi một chuỗi tiếng Việt có dấu thành chuỗi không dấu, cách nhau bằng dấu gạch ngang (Slug).
    /// </summary>
    /// <param name="phrase">Chuỗi đầu vào (Ví dụ: "Điện thoại iPhone 15!")</param>
    /// <returns>Chuỗi Slug (Ví dụ: "dien-thoai-iphone-15")</returns>
    public static string GenerateSlug(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
            return string.Empty;

        // 1. Chuyển toàn bộ thành chữ thường
        string str = phrase.ToLowerInvariant();

        // 2. Xử lý riêng chữ 'đ' (vì hàm Normalize ở bước 3 không xử lý được chữ đ của tiếng Việt)
        str = str.Replace("đ", "d");

        // 3. Loại bỏ dấu tiếng Việt (diacritics)
        str = RemoveDiacritics(str);

        // 4. Loại bỏ các ký tự đặc biệt, chỉ giữ lại chữ cái a-z, số 0-9, khoảng trắng và dấu gạch ngang
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

        // 5. Gom các khoảng trắng hoặc dấu gạch ngang liên tiếp thành 1 dấu gạch ngang duy nhất
        str = Regex.Replace(str, @"[\s-]+", "-");

        // 6. Xóa dấu gạch ngang ở đầu và cuối chuỗi (nếu có)
        return str.Trim('-');
    }

    private static string RemoveDiacritics(string text)
    {
        // Phân tách các ký tự có dấu thành ký tự cơ bản và dấu (FormD)
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            // Nếu không phải là ký tự dấu (NonSpacingMark) thì giữ lại
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Ép lại thành chuỗi chuẩn (FormC)
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}