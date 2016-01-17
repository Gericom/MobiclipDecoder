using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMobiclip.Utils;

namespace LibMobiclip.Codec.FastAudio
{
    public class FastAudioDecoder
    {
         //<1><1940a3>: Abbrev Number: 64 (DW_TAG_structure_type)
         //   <1940a4>   DW_AT_sibling     : <0x1940fe>	
         //   <1940a6>   DW_AT_name        : FastAudioUnpackData	
         //   <1940ba>   DW_AT_byte_size   : 464	
         //<2><1940bc>: Abbrev Number: 38 (DW_TAG_member)
         //   <1940bd>   DW_AT_name        : Src	
         //   <1940c1>   DW_AT_type        : DW_FORM_ref2 <0x19409b>	
         //   <1940c4>   DW_AT_data_member_location: 2 byte block: 23 0 	(DW_OP_plus_uconst: 0)
         //<2><1940c7>: Abbrev Number: 38 (DW_TAG_member)
         //   <1940c8>   DW_AT_name        : Dst	
         //   <1940cc>   DW_AT_type        : DW_FORM_ref2 <0x19409f>	
         //   <1940cf>   DW_AT_data_member_location: 2 byte block: 23 4 	(DW_OP_plus_uconst: 4)
         //<2><1940d2>: Abbrev Number: 3 (DW_TAG_array_type)
         //   <1940d3>   DW_AT_sibling     : <0x1940db>	
         //   <1940d5>   DW_AT_type        : DW_FORM_ref2 <0x194087>	
         //<3><1940d8>: Abbrev Number: 1 (DW_TAG_subrange_type)
         //   <1940d9>   DW_AT_upper_bound : 112	
         //<2><1940db>: Abbrev Number: 38 (DW_TAG_member)
         //   <1940dc>   DW_AT_name        : Internal	
         //   <1940e5>   DW_AT_type        : DW_FORM_ref2 <0x1940d2>	
         //   <1940e8>   DW_AT_data_member_location: 2 byte block: 23 8 	(DW_OP_plus_uconst: 8)
         //<2><1940eb>: Abbrev Number: 38 (DW_TAG_member)
         //   <1940ec>   DW_AT_name        : Increment	
         //   <1940f6>   DW_AT_type        : DW_FORM_ref2 <0x194087>	
         //   <1940f9>   DW_AT_data_member_location: 3 byte block: 23 cc 3 	(DW_OP_plus_uconst: 460)
        public byte[] Data;
        public int Offset = 0;
        private uint[] Internal = new uint[113];
        private uint Increment;
    }
}
