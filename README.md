# Mobiclip Decoder
Mobiclip decoder is a decoder for the mobiclip video codec used by Nintendo. I dissassembled this from Bowling 3DS.

Compiled version (21/12/2015): http://florian.nouwt.com/MobiclipDecoder.zip

### Supported formats
- 3DS Moflex (only IMA-ADPCM audio supported)
- DS Mods (no audio)
- Wii MOC5 files (no audio and not full speed)

### What is yet to be done
- Support for the fastaudio codec
- Find out how to correctly read the audio from Mods and MOC5 files
- More refactoring of the code (aka understand it better)
- An encoder. I made a quite bad poc one, but I did not make a muxer yet. Without a muxer we can not create full moflex files for example.
