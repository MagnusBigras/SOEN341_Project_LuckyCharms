using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Http;
using QRCoder;
using System.Drawing;
using System.Linq;
using ZXing.Windows.Compatibility;

namespace Lucky_Charm_Event_track.Helpers
{
    public class QRCodeGeneratorHelper
    {
        public static byte[] GenerateQRCode(string payload) 
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            var data = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCodeImage = new PngByteQRCode(data);
            byte[] QR = qrCodeImage.GetGraphic(30);
            return QR;
        }
        public static bool VerifyQRCode(WebAppDBContext webAppDBContext, IFormFile QRcodeImage) 
        {
            if (QRcodeImage == null) 
            {
                return false;
            }
            Bitmap bitmap = new Bitmap(QRcodeImage.OpenReadStream());
            BarcodeReader reader = new BarcodeReader();
            var result = reader.Decode(bitmap);
            if (result == null)
            {
                return false;
            }
            Ticket ticket = webAppDBContext.Tickets.FirstOrDefault(e => e.QRCodeText == result.Text);
            if (!ticket.CheckedIn) 
            {
                ticket.CheckedIn = true;
                webAppDBContext.Tickets.Update(ticket);
                webAppDBContext.SaveChanges();
                return true;
            }
            return false; 
        }
    }
}
