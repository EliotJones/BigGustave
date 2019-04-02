# Big Gustave #

[![Build status](https://ci.appveyor.com/api/projects/status/nh12x7vg36qxunp0?svg=true)](https://ci.appveyor.com/project/EliotJones/biggustave)

An attempt at PNG decoding according to the PNG spec only using .NET standard libraries.

## Usage ##

Still being written but the idea is calling:

    Png png = Png.Open(Stream stream)

Will return a PNG object.

The PNG object has methods to inspect the header and get the pixel values. The header has properties for:

    png.Header.Width
    png.Header.Height
    png.Header.BitDepth
    png.Header.ColorType
    png.Header.CompressionMethod
    png.Header.FilterMethod
    png.Header.InterlaceMethod

The PNG also has width and height as convenience properties from the header information:

    png.Width == png.Header.Width
    png.Height == png.Header.Height

And a property that indicates if the image uses transparency:

    png.HasAlphaChannel

To get a pixel use:

    Pixel pixel = png.GetPixel(0, 7);

Where the first argument is x (column) and the second is y (row). The `Pixel` is used for all image types, e.g. Grayscale, Colour, with/without transparency.