namespace DiceAMillionSaveEditor.Logic.Interfaces;

public interface IBase64Encoder
{
    string Decode(string base64String);
    string Encode(string plainText);
}
