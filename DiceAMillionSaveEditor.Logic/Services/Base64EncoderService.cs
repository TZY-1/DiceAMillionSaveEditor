using System;
using System.Text;
using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.Logic.Services;

public class Base64EncoderService : IBase64Encoder
{
    public string Decode(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String)) return string.Empty;
        var bytes = Convert.FromBase64String(base64String.Trim());
        return Encoding.UTF8.GetString(bytes).Trim();
    }

    public string Encode(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText)) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }
}
