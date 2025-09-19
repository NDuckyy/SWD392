using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace SWD392_backend.Utilities
{
    public class SlugHelper
    {
        /// <summary>
        /// Function chuyển name của product thành slug
        /// </summary>
        /// <param name="input">Input đầu vào</param>
        /// <returns>Trả về string slug, nếu không trả về null</returns>
        public static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            // Normalize + remove accents (dấu tiếng việt)
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string noAccents = sb.ToString().Normalize(NormalizationForm.FormC);

            // Lowercase
            string lower = noAccents.ToLowerInvariant();

            // Replace anything not a-z, 0-9 with hyphen
            string slug = Regex.Replace(lower, @"[^a-z0-9\s-]", "");

            // Replace whitespace with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");

            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from ends
            slug = slug.Trim('-');

            return slug;
        }
    }
}
