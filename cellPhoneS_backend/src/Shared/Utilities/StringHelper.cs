using System;
using System.Globalization;
using System.Text;

namespace cellPhoneS_backend.Utils;

public static class StringHelper
{
    public static string RemoveSign(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // 1. Chuẩn hóa chuỗi sang dạng FormD (tách ký tự gốc và dấu thành 2 phần riêng biệt)
        // Ví dụ: chữ "á" sẽ thành "a" và dấu sắc
        text = text.Normalize(NormalizationForm.FormD);
        
        var chars = text.ToCharArray();
        var sb = new StringBuilder();

        foreach (var c in chars)
        {
            // 2. Lọc bỏ các ký tự thuộc loại NonSpacingMark (chính là các dấu)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        // 3. Chuẩn hóa lại về FormC để gộp các ký tự còn lại (nếu có)
        string result = sb.ToString().Normalize(NormalizationForm.FormC);

        // 4. Xử lý riêng trường hợp chữ Đ/đ (vì trong Unicode, Đ là ký tự riêng chứ không phải D + dấu)
        return result.Replace('đ', 'd').Replace('Đ', 'D');
    }
    public static int ComputeLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return string.IsNullOrEmpty(target) ? 0 : target.Length;
        if (string.IsNullOrEmpty(target)) return source.Length;

        int n = source.Length;
        int m = target.Length;
        int[,] d = new int[n + 1, m + 1];

        // Khởi tạo ma trận
        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        // Tính toán
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1,      // Xóa
                             d[i, j - 1] + 1),     // Chèn
                             d[i - 1, j - 1] + cost); // Thay thế
            }
        }
        return d[n, m];
    }
}
