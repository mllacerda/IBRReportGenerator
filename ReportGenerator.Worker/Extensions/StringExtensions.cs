namespace ReportGenerator.Worker.Extensions;

public static class StringExtensions
{
    public static bool IsBase64Image(this string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        try
        {
            var data = base64String;
            if (data.StartsWith("data:image"))
            {
                var index = data.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
                if (index == -1) return false;
                data = data.Substring(index + 7);
            }

            //Base64
            byte[] bytes = Convert.FromBase64String(data);

            if (bytes.Length < 4) return false;

            // PNG
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return true;

            // JPEG
            if (bytes[0] == 0xFF && bytes[1] == 0xD8)
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static byte[] ToImageBytes(this string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            throw new ArgumentException("Base64 string cannot be empty.", nameof(base64String));

        var data = base64String;
        if (data.StartsWith("data:image"))
        {
            var index = data.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
            if (index == -1)
                throw new ArgumentException("Invalid data URI format.", nameof(base64String));
            data = data.Substring(index + 7);
        }

        return Convert.FromBase64String(data);
    }
}