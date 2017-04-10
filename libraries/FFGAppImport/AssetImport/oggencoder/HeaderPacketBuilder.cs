using System;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{
    public class HeaderPacketBuilder
    {
        private const string VorbisString = "vorbis";
        private const string VendorString = "OggVorbisEncoder";

        public OggPacket BuildInfoPacket(VorbisInfo info)
        {
            var buffer = new EncodeBuffer();

            PackInfo(buffer, info);

            var bytes = buffer.GetBytes();
            return new OggPacket(bytes, false, 0, 0);
        }

        public OggPacket BuildCommentsPacket(Comments comments)
        {
            var buffer = new EncodeBuffer();

            PackComment(buffer, comments);

            var bytes = buffer.GetBytes();
            return new OggPacket(bytes, false, 0, 1);
        }

        public OggPacket BuildBooksPacket(VorbisInfo info)
        {
            var buffer = new EncodeBuffer(4096);

            PackBooks(buffer, info);

            var bytes = buffer.GetBytes();
            return new OggPacket(bytes, false, 0, 2);
        }

        private void PackBooks(EncodeBuffer buffer, VorbisInfo info)
        {
            var codecSetup = info.CodecSetup;

            buffer.Write(0x05, 8);
            buffer.WriteString(VorbisString);

            buffer.Write((uint) (codecSetup.BookParams.Count - 1), 8);
            foreach (var book in codecSetup.BookParams)
                PackStaticBook(buffer, book);

            // times; hook placeholders 
            buffer.Write(0, 6);
            buffer.Write(0, 16);

            buffer.Write((uint) (codecSetup.FloorParams.Count - 1), 6);
            foreach (var floor in codecSetup.FloorParams)
            {
                buffer.Write(1, 16); // For now we're only using floor type 1
                PackFloor(buffer, floor);
            }

            buffer.Write((uint) (codecSetup.ResidueParams.Count - 1), 6);
            foreach (var residue in codecSetup.ResidueParams)
            {
                buffer.Write((uint) residue.ResidueType, 16);
                PackResidue(buffer, residue);
            }

            buffer.Write((uint) (codecSetup.MapParams.Count - 1), 6);
            foreach (var mapping in codecSetup.MapParams)
            {
                buffer.Write(0, 16); // Mapping type is always zero
                PackMapping(buffer, info, mapping);
            }

            buffer.Write((uint) (codecSetup.ModeParams.Count - 1), 6);
            for (var i = 0; i < codecSetup.ModeParams.Count; i++)
                PackModes(buffer, codecSetup, i);

            buffer.Write(1, 1);
        }

        private static void PackModes(EncodeBuffer buffer, CodecSetup codecSetup, int i)
        {
            buffer.Write((uint) codecSetup.ModeParams[i].BlockFlag, 1);
            buffer.Write((uint) codecSetup.ModeParams[i].WindowType, 16);
            buffer.Write((uint) codecSetup.ModeParams[i].TransformType, 16);
            buffer.Write((uint) codecSetup.ModeParams[i].Mapping, 8);
        }

        private static void PackResidue(EncodeBuffer buffer, Residue residue)
        {
            buffer.Write((uint) residue.Begin, 24);
            buffer.Write((uint) residue.End, 24);

            buffer.Write((uint) (residue.Grouping - 1), 24);
                // residue vectors to group and code with a partitioned book 
            buffer.Write((uint) (residue.Partitions - 1), 6); // possible partition choices 
            buffer.Write((uint) residue.GroupBook, 8); // group huffman book 

            var acc = 0;

            // secondstages is a bitmask; as encoding progresses pass by pass, a
            // bitmask of one indicates this partition class has bits to write this pass 
            for (var j = 0; j < residue.Partitions; j++)
            {
                if (Encoding.Log(residue.SecondStages[j]) > 3)
                {
                    // yes, this is a minor hack due to not thinking ahead 
                    buffer.Write((uint) residue.SecondStages[j], 3);
                    buffer.Write(1, 1);
                    buffer.Write((uint) residue.SecondStages[j] >> 3, 5);
                }
                else
                {
                    buffer.Write((uint) residue.SecondStages[j], 4); // trailing zero 
                }

                acc += Count(residue.SecondStages[j]);
            }

            for (var j = 0; j < acc; j++)
                buffer.Write((uint) residue.BookList[j], 8);
        }

        private static int Count(int value)
        {
            var ret = 0;
            while (value != 0)
            {
                ret += value & 1;
                value >>= 1;
            }
            return ret;
        }

        private static void PackFloor(EncodeBuffer buffer, Floor floor)
        {
            var count = 0;
            var maxposit = floor.PostList[1];
            var maxclass = -1;

            // save out partitions 
            buffer.Write((uint) floor.PartitionClass.Length, 5); // only 0 to 31 legal 
            foreach (var partitionClass in floor.PartitionClass)
            {
                buffer.Write((uint) partitionClass, 4); // only 0 to 15 legal 
                if (maxclass < partitionClass)
                    maxclass = partitionClass;
            }

            // save out partition classes 
            for (var j = 0; j < maxclass + 1; j++)
            {
                buffer.Write((uint) (floor.ClassDimensions[j] - 1), 3); // 1 to 8 
                buffer.Write((uint) floor.ClassSubs[j], 2); // 0 to 3 
                if (floor.ClassSubs[j] != 0)
                    buffer.Write((uint) floor.ClassBook[j], 8);

                for (var k = 0; k < 1 << floor.ClassSubs[j]; k++)
                    buffer.Write((uint) (floor.ClassSubBook[j][k] + 1), 8);
            }

            // save out the post list, only 1,2,3,4 legal now 
            buffer.Write((uint) (floor.Mult - 1), 2);

            // maxposit cannot legally be less than 1; this is encode-side, we can assume our setup is OK 
            buffer.Write((uint) Encoding.Log(maxposit - 1), 4);
            var rangebits = Encoding.Log(maxposit - 1);

            for (int j = 0, k = 0; j < floor.PartitionClass.Length; j++)
            {
                count += floor.ClassDimensions[floor.PartitionClass[j]];
                for (; k < count; k++)
                    buffer.Write((uint) floor.PostList[k + 2], rangebits);
            }
        }

        private static void PackMapping(EncodeBuffer buffer, VorbisInfo info, Mapping mapping)
        {
            /* another 'we meant to do it this way' hack...  up to beta 4, we
               packed 4 binary zeros here to signify one submapping in use.  We
               now redefine that to mean four bitflags that indicate use of
               deeper features; bit0:submappings, bit1:coupling,
               bit2,3:reserved. This is backward compatible with all actual uses
               of the beta code. */
            if (mapping.SubMaps > 1)
            {
                buffer.Write(1, 1);
                buffer.Write((uint) mapping.SubMaps - 1, 4);
            }
            else
            {
                buffer.Write(0, 1);
            }

            if (mapping.CouplingSteps > 0)
            {
                buffer.Write(1, 1);
                buffer.Write((uint) mapping.CouplingSteps - 1, 8);

                var couplingBits = Encoding.Log(info.Channels - 1);
                for (var i = 0; i < mapping.CouplingSteps; i++)
                {
                    buffer.Write((uint) mapping.CouplingMag[i], couplingBits);
                    buffer.Write((uint) mapping.CouplingAng[i], couplingBits);
                }
            }
            else
            {
                buffer.Write(0, 1);
            }

            buffer.Write(0, 2); // 2,3:reserved 

            // we don't write the channel submappings if we only have one... 
            if (mapping.SubMaps > 1)
                for (var i = 0; i < info.Channels; i++)
                    buffer.Write((uint) mapping.ChannelMuxList[i], 4);

            for (var i = 0; i < mapping.SubMaps; i++)
            {
                buffer.Write(0, 8); // time submap unused 
                buffer.Write((uint) mapping.FloorSubMap[i], 8);
                buffer.Write((uint) mapping.ResidueSubMap[i], 8);
            }
        }

        private void PackStaticBook(EncodeBuffer buffer, IStaticCodeBook book)
        {
            var ordered = false;

            // first the basic parameters
            buffer.Write(0x564342, 24);
            buffer.Write((uint) book.Dimensions, 16);
            buffer.Write((uint) book.LengthList.Length, 24);

            // pack the codewords.  There are two packing types; length ordered and length random. 
            int i;
            for (i = 1; i < book.LengthList.Length; i++)
                if ((book.LengthList[i - 1] == 0) || (book.LengthList[i] < book.LengthList[i - 1]))
                    break;

            if (i == book.LengthList.Length)
                ordered = true;

            if (ordered)
            {
                // length ordered.  We only need to say how many codewords of each length.  The actual codewords are generated deterministically 
                buffer.Write(1, 1);

                buffer.Write((uint) (book.LengthList[0] - 1), 5); // 1 to 32

                var count = 0;
                for (i = 1; i < book.LengthList.Length; i++)
                {
                    var current = book.LengthList[i];
                    var previous = book.LengthList[i - 1];

                    if (current <= previous)
                        continue;

                    for (var j = previous; j < current; j++)
                    {
                        buffer.Write((uint) (i - count), Encoding.Log(book.LengthList.Length - count));
                        count = i;
                    }
                }

                buffer.Write((uint) (i - count), Encoding.Log(book.LengthList.Length - count));
            }
            else
            {
                // length unordered. Again, we don't code the codeword itself, just the length. This time, though, we have to encode each length 
                buffer.Write(0, 1);

                /* algorithmic mapping has use for 'unused entries', which we tag
                   here.  The algorithmic mapping happens as usual, but the unused
                   entry has no codeword. */
                for (i = 0; i < book.LengthList.Length; i++)
                    if (book.LengthList[i] == 0)
                        break;

                if (i == book.LengthList.Length)
                {
                    buffer.Write(0, 1); // no unused entries
                    for (i = 0; i < book.LengthList.Length; i++)
                        buffer.Write((uint) (book.LengthList[i] - 1), 5);
                }
                else
                {
                    buffer.Write(1, 1); // we have unused entries; thus we tag 
                    for (i = 0; i < book.LengthList.Length; i++)
                        if (book.LengthList[i] == 0)
                        {
                            buffer.Write(0, 1);
                        }
                        else
                        {
                            buffer.Write(1, 1);
                            buffer.Write((uint) (book.LengthList[i] - 1), 5);
                        }
                }
            }

            buffer.Write((uint) book.MapType, 4);
            if (book.MapType == CodeBookMapType.None)
                return;

            // is the entry number the desired return value, or do we have a mapping? If we have a mapping, what type? 
            if ((book.MapType != CodeBookMapType.Implicit) && (book.MapType != CodeBookMapType.Listed))
                throw new InvalidOperationException("Unknown CodeBookMapType: {book.MapType}");

            if (book.QuantList == null)
                throw new InvalidOperationException("QuantList cannot be null");

            // values that define the dequantization 
            buffer.Write((uint) book.QuantMin, 32);
            buffer.Write((uint) book.QuantDelta, 32);
            buffer.Write((uint) (book.Quant - 1), 4);
            buffer.Write((uint) book.QuantSequenceP, 1);

            var quantVals = 0;
            switch (book.MapType)
            {
                case CodeBookMapType.Implicit:
                    // a single column of (c.entries/c.dim) quantized values for building a full value list algorithmically (square lattice) 
                    quantVals = book.GetQuantVals();
                    break;
                case CodeBookMapType.Listed:
                    // every value (c.entries*c.dim total) specified explicitly 
                    quantVals = book.LengthList.Length*book.Dimensions;
                    break;
            }

            // quantized values 
            for (i = 0; i < quantVals; i++)
                buffer.Write((uint) Math.Abs(book.QuantList[i]), book.Quant);
        }

        private static void PackComment(EncodeBuffer buffer, Comments vorbisComment)
        {
            // Preamble
            buffer.Write(0x03, 8);
            buffer.WriteString(VorbisString);

            // Vendor
            buffer.Write((uint) VendorString.Length, 32);
            buffer.WriteString(VendorString);

            // Comments
            buffer.Write((uint) vorbisComment.UserComments.Count, 32);

            foreach (var comment in vorbisComment.UserComments)
                if (!string.IsNullOrEmpty(comment))
                {
                    buffer.Write((uint) comment.Length, 32);
                    buffer.WriteString(comment);
                }
                else
                {
                    buffer.Write(0, 32);
                }

            buffer.Write(1, 1);
        }

        private static void PackInfo(EncodeBuffer buffer, VorbisInfo info)
        {
            var codecSetup = info.CodecSetup;

            // preamble
            buffer.Write(0x01, 8);
            buffer.WriteString(VorbisString);

            // basic information about the stream 
            buffer.Write(0x00, 32);
            buffer.Write((uint) info.Channels, 8);
            buffer.Write((uint) info.SampleRate, 32);

            buffer.Write(0, 32); // Bit rate upper not used
            buffer.Write((uint) info.BitRateNominal, 32);
            buffer.Write(0, 32); // Bit rate lower not used

            buffer.Write((uint) Encoding.Log(codecSetup.BlockSizes[0] - 1), 4);
            buffer.Write((uint) Encoding.Log(codecSetup.BlockSizes[1] - 1), 4);
            buffer.Write(1, 1);
        }
    }
}