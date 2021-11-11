﻿#if !NETCOREAPP1_1

using Xunit;
using QRCoder;
using Shouldly;
using System.Drawing;
using QRCoderTests.Helpers.XUnitExtenstions;
using QRCoderTests.Helpers;

namespace QRCoderTests
{

    public class ArtQRCodeRendererTests
    {


        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_create_standard_qrcode_graphic()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var bmp = new ArtQRCode(data).GetGraphic(10);

            var result = HelperFunctions.BitmapToHash(bmp);
#if NET35_OR_GREATER || NET40_OR_GREATER
            result.ShouldBe("ea1a9008dbfe7825a6e17ed228a83a6f");
#else
            result.ShouldBe("9e18d03a3f2a4ca0fb603350f9764d6d");
#endif
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_create_standard_qrcode_graphic_with_custom_finder()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var finder = new Bitmap(15, 15);
            var bmp = new ArtQRCode(data).GetGraphic(10, Color.Black, Color.White, Color.Transparent, finderPatternImage: finder);

            var result = HelperFunctions.BitmapToHash(bmp);
#if NET35_OR_GREATER || NET40_OR_GREATER
            result.ShouldBe("a6895846ed6b56de997bfcb2c4a72736");
#else
            result.ShouldBe("8185b055473b170afa56152ec46c4be4");
#endif
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_create_standard_qrcode_graphic_without_quietzone()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var bmp = new ArtQRCode(data).GetGraphic(10, Color.Black, Color.White, Color.Transparent, drawQuietZones: false);

            var result = HelperFunctions.BitmapToHash(bmp);
#if NET35_OR_GREATER || NET40_OR_GREATER
            result.ShouldBe("550f31b988ff12d5f8429ef19d9d5a0c");
#else
            result.ShouldBe("632315c8695416fc82fe06a202688433");
#endif
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_create_standard_qrcode_graphic_with_background()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var bmp = new ArtQRCode(data).GetGraphic((Bitmap)Image.FromFile(HelperFunctions.GetAssemblyPath() + "\\assets\\noun_software engineer_2909346.png"));
            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346

            var result = HelperFunctions.BitmapToHash(bmp);

#if NET35_OR_GREATER || NET40_OR_GREATER
            result.ShouldBe("9f4f860d0c15ed4b1258dbbea89c4b4b");
#else
            result.ShouldBe("16f3ad423cc0690cc2e259d3731400cd");
#endif
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void should_throw_pixelfactor_oor_exception()
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode("This is a quick test! 123#?", QRCodeGenerator.ECCLevel.H);
            var aCode = new ArtQRCode(data);
            
            var exception = Record.Exception(() => aCode.GetGraphic(10, Color.Black, Color.White, Color.Transparent, pixelSizeFactor: 2));
            Assert.NotNull(exception);
            Assert.IsType<System.Exception>(exception);
            exception.Message.ShouldBe("The parameter pixelSize must be between 0 and 1. (0-100%)");
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_instantate_parameterless()
        {
            var asciiCode = new ArtQRCode();
            asciiCode.ShouldNotBeNull();
            asciiCode.ShouldBeOfType<ArtQRCode>();
        }

        [Fact]
        [Category("QRRenderer/ArtQRCode")]
        public void can_render_artqrcode_from_helper()
        {
            //Create QR code
            var bmp = ArtQRCodeHelper.GetQRCode("A", 10, Color.Black, Color.White, Color.Transparent, QRCodeGenerator.ECCLevel.L);

            var result = HelperFunctions.BitmapToHash(bmp);
#if NET35_OR_GREATER || NET40_OR_GREATER
            result.ShouldBe("fea51114bc4ff893542a1c0574c82a07");
#else
            result.ShouldBe("57ecaa9bdeadcdcbeac8a19d734907ff");
#endif
        }
    }
}
#endif