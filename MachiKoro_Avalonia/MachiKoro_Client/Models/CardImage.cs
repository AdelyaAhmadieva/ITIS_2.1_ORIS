using Avalonia.Media.Imaging;
namespace MachiKoro_Client.Models;

public class CardImage
{
    public Bitmap Bitmap { get; set; }
    public CardImage(string path)
    {
        Bitmap = new Bitmap(path);
    }
}