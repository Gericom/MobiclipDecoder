# Mobiclip Decoder
Mobiclip decoder is a decoder for the mobiclip video codec used by Nintendo based on a lot of disassembly.

Compiled version (17/01/2016): http://florian.nouwt.com/MobiclipDecoder.zip

### Supported formats
- 3DS Moflex (only IMA-ADPCM audio supported)
- DS Mods (only IMA-ADPCM and SX audio supported)
- Wii MOC5 files (no audio and not full speed)

### What is yet to be done
- Support for the fastaudio codec
- Support for .vx files and their old (and much different) version of the mobiclip codec
- Find out how to correctly read the audio from MOC5 files
- More refactoring of the code (aka understand it better)
- Implement encoding in mobiconverter
