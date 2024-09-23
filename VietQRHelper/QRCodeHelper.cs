using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using QRCoder;

namespace VietQRHelper
{
    public class QRCodeHelper
    {
        public static System.Drawing.Image TaoVietQRCodeImage(string input)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                
                var qrCodeData = qrGenerator.CreateQrCode(input, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCode(qrCodeData))
                {                   
                    Bitmap qrCodeImage = qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, true);
                    return qrCodeImage;
                }
            }
        }
    }
}
