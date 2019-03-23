# Big Gustave #

An attempt at PNG decoding according to the PNG spec only using .NET standard libraries.

## Usage ##

Still being written but the idea is calling:

    Png.Open(Stream stream)

Will return a PNG including the ```ImageHeader``` information and the pixels of the image.

Currently does not support Adam7 interlacing.