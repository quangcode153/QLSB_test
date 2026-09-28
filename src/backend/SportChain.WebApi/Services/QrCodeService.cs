using QRCoder;

namespace SportChain.WebApi.Services;

public interface IQrCodeService
{
    string GenerateQrBase64(string payload);
}

public class QrCodeService : IQrCodeService
{
    public string GenerateQrBase64(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }
}
