using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter0;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter1;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter2;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter3;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter4;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter5;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter6;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter7;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter8;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter9;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ChapterNegOne;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ManagedChapter0;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ManagedChapter1;
using OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ManagedChapterNegOne;
using OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks;

namespace OggVorbisEncoder.Setup.Templates.Stereo44
{
    public static class Residue44
    {
        private static readonly Residue Residue44Low = new Residue(
            0, -1, -1, 9, -1, -1,
            new[] {0}, new[] {-1},
            new[] {0, 1, 2, 2, 4, 8, 16, 32},
            new[] {0, 0, 0, 999, 4, 8, 16, 32},
            ResidueType.Zero);

        private static readonly Residue Residue44Medium = new Residue(
            0, -1, -1, 10, -1, -1,
            new[] {0}, new[] {-1},
            new[] {0, 1, 1, 2, 2, 4, 8, 16, 32},
            new[] {0, 0, 999, 0, 999, 4, 8, 16, 32},
            ResidueType.Zero);

        private static readonly Residue Residue44High = new Residue(
            0, -1, -1, 9, -1, -1,
            new[] {0}, new[] {-1},
            new[] {0, 1, 2, 2, 4, 8, 16, 32},
            new[] {0, 0, 0, 999, 4, 8, 16, 32},
            ResidueType.Zero);

        private static readonly IStaticCodeBook HuffmanBookNegOneShort = new ChapterNegOneShort();
        private static readonly IStaticCodeBook HuffmanBookNegOneShortManaged = new ChapterNegOneShortManaged();
        private static readonly IStaticCodeBook HuffmanBookNegOneLong = new ChapterNegOneLong();
        private static readonly IStaticCodeBook HuffmanBookNegOneLongManaged = new ChapterNegOneLongManaged();
        private static readonly IStaticBookBlock BlockNeg1 = new BlockNegOne();
        private static readonly IStaticBookBlock BlockNeg1Managed = new ManagedBlockNegOne();

        private static readonly IResidueTemplate[] ResidueNegative1 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 32,
                Residue44Low,
                HuffmanBookNegOneShort,
                HuffmanBookNegOneShortManaged,
                BlockNeg1,
                BlockNeg1Managed),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 32,
                Residue44Low,
                HuffmanBookNegOneLong,
                HuffmanBookNegOneLongManaged,
                BlockNeg1,
                BlockNeg1Managed)
        };

        private static readonly IStaticCodeBook HuffmanBook0Short = new Chapter0Short();
        private static readonly IStaticCodeBook HuffmanBook0ShortManaged = new Chapter0ShortManaged();
        private static readonly IStaticCodeBook HuffmanBook0Long = new Chapter0Long();
        private static readonly IStaticCodeBook HuffmanBook0LongManaged = new Chapter0LongManaged();
        private static readonly IStaticBookBlock Block0 = new Block0();
        private static readonly IStaticBookBlock Block0Managed = new ManagedBlock0();

        private static readonly IResidueTemplate[] Residue0 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 16,
                Residue44Low,
                HuffmanBook0Short,
                HuffmanBook0ShortManaged,
                Block0,
                Block0Managed),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 32,
                Residue44Low,
                HuffmanBook0Long,
                HuffmanBook0LongManaged,
                Block0,
                Block0Managed)
        };

        private static readonly IStaticCodeBook HuffmanBook1Short = new Chapter1Short();
        private static readonly IStaticCodeBook HuffmanBook1ShortManaged = new Chapter1ShortManaged();
        private static readonly IStaticCodeBook HuffmanBook1Long = new Chapter1Long();
        private static readonly IStaticCodeBook HuffmanBook1LongManaged = new Chapter1LongManaged();
        private static readonly IStaticBookBlock Block1 = new Block1();
        private static readonly IStaticBookBlock Block1Managed = new ManagedBlock1();

        private static readonly IResidueTemplate[] Residue1 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 16,
                Residue44Low,
                HuffmanBook1Short,
                HuffmanBook1ShortManaged,
                Block1,
                Block1Managed),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 32,
                Residue44Low,
                HuffmanBook1Long,
                HuffmanBook1LongManaged,
                Block1,
                Block1Managed)
        };

        private static readonly IStaticCodeBook HuffmanBook2Short = new Chapter2Short();
        private static readonly IStaticCodeBook HuffmanBook2Long = new Chapter2Long();
        private static readonly IStaticBookBlock Block2 = new Block2();

        private static readonly IResidueTemplate[] Residue2 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 16,
                Residue44Medium,
                HuffmanBook2Short,
                HuffmanBook2Short,
                Block2,
                Block2),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 32,
                Residue44Medium,
                HuffmanBook2Long,
                HuffmanBook2Long,
                Block2,
                Block2)
        };

        private static readonly IStaticCodeBook HuffmanBook3Short = new Chapter3Short();
        private static readonly IStaticCodeBook HuffmanBook3Long = new Chapter3Long();
        private static readonly IStaticBookBlock Block3 = new Block3();

        private static readonly IResidueTemplate[] Residue3 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass,
                16,
                Residue44Medium,
                HuffmanBook3Short,
                HuffmanBook3Short,
                Block3,
                Block3),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass,
                32,
                Residue44Medium,
                HuffmanBook3Long,
                HuffmanBook3Long,
                Block3,
                Block3)
        };

        private static readonly IStaticCodeBook HuffmanBook4Short = new Chapter4Short();
        private static readonly IStaticCodeBook HuffmanBook4Long = new Chapter4Long();
        private static readonly IStaticBookBlock Block4 = new Block4();

        private static readonly IResidueTemplate[] Residue4 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass,
                16,
                Residue44Medium,
                HuffmanBook4Short,
                HuffmanBook4Short,
                Block4,
                Block4),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass,
                32,
                Residue44Medium,
                HuffmanBook4Long,
                HuffmanBook4Long,
                Block4,
                Block4)
        };

        private static readonly IStaticCodeBook HuffmanBook5Short = new Chapter5Short();
        private static readonly IStaticCodeBook HuffmanBook5Long = new Chapter5Long();
        private static readonly IStaticBookBlock Block5 = new Block5();

        private static readonly IResidueTemplate[] Residue5 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass,
                16,
                Residue44Medium,
                HuffmanBook5Short,
                HuffmanBook5Short,
                Block5,
                Block5),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass,
                32,
                Residue44Medium,
                HuffmanBook5Long,
                HuffmanBook5Long,
                Block5,
                Block5)
        };

        private static readonly IStaticCodeBook HuffmanBook6Short = new Chapter6Short();
        private static readonly IStaticCodeBook HuffmanBook6Long = new Chapter6Long();
        private static readonly IStaticBookBlock Block6 = new Block6();

        private static readonly IResidueTemplate[] Residue6 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass,
                16,
                Residue44High,
                HuffmanBook6Short,
                HuffmanBook6Short,
                Block6,
                Block6),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass,
                32,
                Residue44High,
                HuffmanBook6Long,
                HuffmanBook6Long,
                Block6,
                Block6)
        };

        private static readonly IStaticCodeBook HuffmanBook7Short = new Chapter7Short();
        private static readonly IStaticCodeBook HuffmanBook7Long = new Chapter7Long();
        private static readonly IStaticBookBlock Block7 = new Block7();

        private static readonly IResidueTemplate[] Residue7 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 16,
                Residue44High,
                HuffmanBook7Short,
                HuffmanBook7Short,
                Block7,
                Block7),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 32,
                Residue44High,
                HuffmanBook7Long,
                HuffmanBook7Long,
                Block7,
                Block7)
        };

        private static readonly IStaticCodeBook HuffmanBook8Short = new Chapter8Short();
        private static readonly IStaticCodeBook HuffmanBook8Long = new Chapter8Long();
        private static readonly IStaticBookBlock Block8 = new Block8();

        private static readonly IResidueTemplate[] Residue8 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 16,
                Residue44High,
                HuffmanBook8Short,
                HuffmanBook8Short,
                Block8,
                Block8),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 32,
                Residue44High,
                HuffmanBook8Long,
                HuffmanBook8Long,
                Block8,
                Block8)
        };

        private static readonly IStaticCodeBook HuffmanBook9Short = new Chapter9Short();
        private static readonly IStaticCodeBook HuffmanBook9Long = new Chapter9Long();
        private static readonly IStaticBookBlock Block9 = new Block9();

        private static readonly IResidueTemplate[] Residue9 =
        {
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 16,
                Residue44High,
                HuffmanBook9Short,
                HuffmanBook9Short,
                Block9,
                Block9),
            new ResidueTemplate(
                ResidueType.Two,
                ResidueLimitType.LowPass, 32,
                Residue44High,
                HuffmanBook9Long,
                HuffmanBook9Long,
                Block9,
                Block9)
        };

        private static readonly Mapping[] MapNominal =
        {
            new Mapping(new[] {0, 0}, new[] {0}, new[] {0}, new[] {0}, new[] {1}),
            new Mapping(new[] {0, 0}, new[] {1}, new[] {1}, new[] {0}, new[] {1})
        };

        public static readonly IMappingTemplate[] MapRes44Stereo =
        {
            new MappingTemplate(MapNominal, ResidueNegative1),
            new MappingTemplate(MapNominal, Residue0),
            new MappingTemplate(MapNominal, Residue1),
            new MappingTemplate(MapNominal, Residue2),
            new MappingTemplate(MapNominal, Residue3),
            new MappingTemplate(MapNominal, Residue4),
            new MappingTemplate(MapNominal, Residue5),
            new MappingTemplate(MapNominal, Residue6),
            new MappingTemplate(MapNominal, Residue7),
            new MappingTemplate(MapNominal, Residue8),
            new MappingTemplate(MapNominal, Residue9)
        };
    }
}