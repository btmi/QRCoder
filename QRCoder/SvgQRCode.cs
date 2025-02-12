#if NETFRAMEWORK || NETSTANDARD2_0 || NET5_0
using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using static QRCoder.QRCodeGenerator;
using static QRCoder.SvgQRCode;

namespace QRCoder
{
    public class SvgQRCode : AbstractQRCode, IDisposable
    {
        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public SvgQRCode() { }
        public SvgQRCode(QRCodeData data) : base(data) { }

        public string GetGraphic(int pixelsPerModule)
        {
            var viewBox = new Size(pixelsPerModule * this.QrCodeData.ModuleMatrix.Count, pixelsPerModule * this.QrCodeData.ModuleMatrix.Count);
            return this.GetGraphic(viewBox, Color.Black, Color.White);
        }
        public string GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor, bool drawQuietZones = true, SizingMode sizingMode = SizingMode.WidthHeightAttribute, SvgLogo logo = null)
        {
            var offset = drawQuietZones ? 0 : 4;
            var edgeSize = this.QrCodeData.ModuleMatrix.Count * pixelsPerModule - (offset * 2 * pixelsPerModule);
            var viewBox = new Size(edgeSize, edgeSize);
            return this.GetGraphic(viewBox, darkColor, lightColor, drawQuietZones, sizingMode, logo);
        }

        public string GetGraphic(int pixelsPerModule, string darkColorHex, string lightColorHex, bool drawQuietZones = true, SizingMode sizingMode = SizingMode.WidthHeightAttribute, SvgLogo logo = null)
        {
            var offset = drawQuietZones ? 0 : 4;
            var edgeSize = this.QrCodeData.ModuleMatrix.Count * pixelsPerModule - (offset * 2 * pixelsPerModule);
            var viewBox = new Size(edgeSize, edgeSize);
            return this.GetGraphic(viewBox, darkColorHex, lightColorHex, drawQuietZones, sizingMode, logo);
        }

        public string GetGraphic(Size viewBox, bool drawQuietZones = true, SizingMode sizingMode = SizingMode.WidthHeightAttribute, SvgLogo logo = null)
        {
            return this.GetGraphic(viewBox, Color.Black, Color.White, drawQuietZones, sizingMode, logo);
        }

        public string GetGraphic(Size viewBox, Color darkColor, Color lightColor, bool drawQuietZones = true, SizingMode sizingMode = SizingMode.WidthHeightAttribute, SvgLogo logo = null)
        {
            return this.GetGraphic(viewBox, ColorTranslator.ToHtml(Color.FromArgb(darkColor.ToArgb())), ColorTranslator.ToHtml(Color.FromArgb(lightColor.ToArgb())), drawQuietZones, sizingMode, logo);
        }

        public string GetGraphic(Size viewBox, string darkColorHex, string lightColorHex, bool drawQuietZones = true, SizingMode sizingMode = SizingMode.WidthHeightAttribute, SvgLogo logo = null)
        {
            int offset = drawQuietZones ? 0 : 4;
            int drawableModulesCount = this.QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : offset * 2);
            double pixelsPerModule = Math.Min(viewBox.Width, viewBox.Height) / (double)drawableModulesCount;
            double qrSize = drawableModulesCount * pixelsPerModule;
            string svgSizeAttributes = (sizingMode == SizingMode.WidthHeightAttribute) ? $@"width=""{viewBox.Width}"" height=""{viewBox.Height}""" : $@"viewBox=""0 0 {viewBox.Width} {viewBox.Height}""";
            ImageAttributes? logoAttr = null;
            if (logo != null)
                logoAttr = GetLogoAttributes(logo, viewBox, drawableModulesCount, pixelsPerModule);

            // Merge horizontal rectangles
            int[,] matrix = new int[drawableModulesCount, drawableModulesCount];
            for (int yi = 0; yi < drawableModulesCount; yi += 1)
            {
                BitArray bitArray = this.QrCodeData.ModuleMatrix[yi + offset];

                int x0 = -1;
                int xL = 0;
                for (int xi = 0; xi < drawableModulesCount; xi += 1)
                {
                    matrix[yi, xi] = 0;
                    if (bitArray[xi + offset]) // && !IsBlockedByLogo((xi + offset) * pixelsPerModule, (yi + offset) * pixelsPerModule, logoAttr, pixelsPerModule))
                    {
                        if (x0 == -1)
                        {
                            x0 = xi;
                        }
                        xL += 1;
                    }
                    else
                    {
                        if (xL > 0)
                        {
                            matrix[yi, x0] = xL;
                            x0 = -1;
                            xL = 0;
                        }
                    }
                }

                if (xL > 0)
                {
                    matrix[yi, x0] = xL;
                }
            }

            StringBuilder svgFile = new StringBuilder($@"<svg version=""1.1"" baseProfile=""full"" shape-rendering=""crispEdges"" {svgSizeAttributes} xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"">");
            svgFile.AppendLine($@"<rect x=""0"" y=""0"" width=""{CleanSvgVal(qrSize)}"" height=""{CleanSvgVal(qrSize)}"" fill=""{lightColorHex}"" />");
            for (int yi = 0; yi < drawableModulesCount; yi += 1)
            {
                double y = yi * pixelsPerModule;
                for (int xi = 0; xi < drawableModulesCount; xi += 1)
                {
                    int xL = matrix[yi, xi];
                    if (xL > 0)
                    {
                        // Merge vertical rectangles
                        int yL = 1;
                        for (int y2 = yi + 1; y2 < drawableModulesCount; y2 += 1)
                        {
                            if (matrix[y2, xi] == xL)
                            {
                                matrix[y2, xi] = 0;
                                yL += 1;
                            }
                            else
                            {
                                break;
                            }
                        }

                        // Output SVG rectangles
                        double x = xi * pixelsPerModule;
                        //if (!IsBlockedByLogo(x, y, logoAttr, pixelsPerModule))
                            svgFile.AppendLine($@"<rect x=""{CleanSvgVal(x)}"" y=""{CleanSvgVal(y)}"" width=""{CleanSvgVal(xL * pixelsPerModule)}"" height=""{CleanSvgVal(yL * pixelsPerModule)}"" fill=""{darkColorHex}"" />");

                    }
                }
            }

            //Render logo, if set
            if (logo != null)
            {
                if (logo.isEmbedded)
                {
                    if (logo.InvertImage()) svgFile.AppendLine($@"<rect x=""{CleanSvgVal(logoAttr.Value.X)}"" y=""{CleanSvgVal(logoAttr.Value.Y)}"" width=""{CleanSvgVal(logoAttr.Value.Width)}"" height=""{CleanSvgVal(logoAttr.Value.Height)}"" fill=""{darkColorHex}"" />");
                    else svgFile.AppendLine($@"<rect x=""{CleanSvgVal(logoAttr.Value.X)}"" y=""{CleanSvgVal(logoAttr.Value.Y)}"" width=""{CleanSvgVal(logoAttr.Value.Width)}"" height=""{CleanSvgVal(logoAttr.Value.Height)}"" fill=""{lightColorHex}"" />");
                    //transform values
                    XDocument svg = XDocument.Parse(logo.GetDataUri());
                    svg.Root.SetAttributeValue("x",CleanSvgVal(logoAttr.Value.X));
                    svg.Root.SetAttributeValue("y", CleanSvgVal(logoAttr.Value.Y));
                    svg.Root.SetAttributeValue("width", CleanSvgVal(logoAttr.Value.Width));
                    svg.Root.SetAttributeValue("height", CleanSvgVal(logoAttr.Value.Height));
                    svg.Root.SetAttributeValue("shape-rendering", "geometricPrecision");
                    if (logo.InvertImage()) svg.Root.SetAttributeValue("fill", lightColorHex);
                    else svg.Root.SetAttributeValue("fill", darkColorHex);
                    svgFile.AppendLine(svg.ToString().Replace("svg:",""));
                }
                else
                {
                    svgFile.AppendLine($@"<svg width=""100%"" height=""100%"" version=""1.1"" xmlns = ""http://www.w3.org/2000/svg"">");
                    svgFile.AppendLine($@"<image x=""{CleanSvgVal(logoAttr.Value.X)}"" y=""{CleanSvgVal(logoAttr.Value.Y)}"" width=""{CleanSvgVal(logoAttr.Value.Width)}"" height=""{CleanSvgVal(logoAttr.Value.Height)}"" xlink:href=""{logo.GetDataUri()}"" />");
                    svgFile.Append(@"</svg>");
                }
            }

            svgFile.Append(@"</svg>");
            return svgFile.ToString();
        }

        private bool IsBlockedByLogo(double x, double y, ImageAttributes? attr, double pixelPerModule)
        {
            if (attr == null)
                return false;
            return x + pixelPerModule >= attr.Value.X && x <= attr.Value.X + attr.Value.Width && y + pixelPerModule >= attr.Value.Y && y <= attr.Value.Y + attr.Value.Height;
        }

        private ImageAttributes GetLogoAttributes(SvgLogo logo, Size viewBox, double drawableModulesCount, double pixelsPerModule)
        {
            double imgWidth;
            double imgHeight;
            double imgPosX;
            double imgPosY;
            if (logo.isEmbedded)
            {
                imgPosX = (drawableModulesCount - 7d) / 2d * pixelsPerModule;
                imgPosY = imgPosX;
                imgWidth = 7d * pixelsPerModule;
                imgHeight = imgWidth;
            }
            else
            {
                imgWidth = logo.GetIconSizePercent() / 100d * viewBox.Width;
                imgHeight = logo.GetIconSizePercent() / 100d * viewBox.Height;
                imgPosX = viewBox.Width / 2d - imgWidth / 2d;
                imgPosY = viewBox.Height / 2d - imgHeight / 2d;
            }
            return new ImageAttributes()
            {
                Width = imgWidth,
                Height = imgHeight,
                X = imgPosX,
                Y = imgPosY
            };
        }

        private struct ImageAttributes
        {
            public double Width;
            public double Height;
            public double X;
            public double Y;
        }

        private string CleanSvgVal(double input)
        {
            //Clean double values for international use/formats
            return input.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public enum SizingMode
        {
            WidthHeightAttribute,
            ViewBoxAttribute
        }

        public class SvgLogo
        {
            private string _logoData;
            private string _mediaType;
            private int _iconSizePercent;
            private bool _invertIcon;
            public bool isEmbedded;

            /// <summary>
            /// Create a logo object to be used in SvgQRCode renderer
            /// </summary>
            /// <param name="iconRasterized">Logo to be rendered as Bitmap/rasterized graphic</param>
            /// <param name="iconSizePercent">Degree of percentage coverage of the QR code by the logo</param>
            public SvgLogo(Bitmap iconRasterized, int iconSizePercent = 15)
            {
                _iconSizePercent = iconSizePercent;
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var bitmap = new Bitmap(iconRasterized))
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        _logoData = Convert.ToBase64String(ms.GetBuffer(), Base64FormattingOptions.None);
                    }
                }
                _mediaType = "image/png";
                isEmbedded = false;
            }

            /// <summary>
            /// Create a logo object to be used in SvgQRCode renderer
            /// </summary>
            /// <param name="iconVectorized">Logo to be rendered as SVG/vectorized graphic/string</param>
            /// <param name="iconSizePercent">Degree of percentage coverage of the QR code by the logo</param>
            /// 

            public SvgLogo(string iconVectorized, int iconSizePercent = 15, bool invertIcon = false, bool iconEmbedded = false)
            {
                _iconSizePercent = iconSizePercent;
                isEmbedded = iconEmbedded;
                _logoData = isEmbedded? iconVectorized : Convert.ToBase64String(Encoding.UTF8.GetBytes(iconVectorized), Base64FormattingOptions.None);
                _mediaType = "image/svg+xml";
                _invertIcon = invertIcon;
            }

            public string GetDataUri()
            {
                if (isEmbedded) return _logoData;
                else return $"data:{_mediaType};base64,{_logoData}";
            }

            public int GetIconSizePercent()
            {
                return _iconSizePercent;
            }
            public bool InvertImage()
            {
                return _invertIcon;
            }
        }
    }

    public static class SvgQRCodeHelper
    {
        public static string GetQRCode(string plainText, int pixelsPerModule, string darkColorHex, string lightColorHex, ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, EciMode eciMode = EciMode.Default, int requestedVersion = -1, bool drawQuietZones = true, SizingMode sizingMode = SizingMode.WidthHeightAttribute, SvgLogo logo = null)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion))
            using (var qrCode = new SvgQRCode(qrCodeData))
                return qrCode.GetGraphic(pixelsPerModule, darkColorHex, lightColorHex, drawQuietZones, sizingMode, logo);
        }
    }
}

#endif