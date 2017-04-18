namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ChapterNegOne
{
    public class BlockNegOne : IStaticBookBlock
    {
        public IStaticCodeBook[][] Books = {
            new IStaticCodeBook[] {null},
            new IStaticCodeBook[] {null, null, new Page1()},
            new IStaticCodeBook[] {null, null, new Page2()},
            new IStaticCodeBook[] {null, null, new Page3()},
            new IStaticCodeBook[] {null, null, new Page4()},
            new IStaticCodeBook[] {null, null, new Page5()},
            new IStaticCodeBook[] {new Page6_0(), new Page6_1()},
            new IStaticCodeBook[] {new Page7_0(), new Page7_1()},
            new IStaticCodeBook[] {new Page8_0(), new Page8_1(), new Page8_2()}
        };
    }
}