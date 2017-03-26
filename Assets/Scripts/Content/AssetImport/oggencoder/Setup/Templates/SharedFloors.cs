using OggVorbisEncoder.Setup.Templates.FloorBooks;

namespace OggVorbisEncoder.Setup.Templates
{
    public static class SharedFloors
    {
        private static readonly IStaticCodeBook[] Floor128X4Books =
        {
            new Line128X4Class0(),
            new Line128X4Sub0(),
            new Line128X4Sub1(),
            new Line128X4Sub2(),
            new Line128X4Sub3()
        };

        private static readonly IStaticCodeBook[] Floor256X4Books =
        {
            new Line256X4Class0(),
            new Line256X4Sub0(),
            new Line256X4Sub1(),
            new Line256X4Sub2(),
            new Line256X4Sub3()
        };

        private static readonly IStaticCodeBook[] Floor128X7Books =
        {
            new Line128X7Class0(),
            new Line128X7Class1(),
            new Line128X7_0Sub1(),
            new Line128X7_0Sub2(),
            new Line128X7_0Sub3(),
            new Line128X7_1Sub1(),
            new Line128X7_1Sub2(),
            new Line128X7_1Sub3()
        };

        private static readonly IStaticCodeBook[] Floor256X7Books =
        {
            new Line256X7Class0(),
            new Line256X7Class1(),
            new Line256X7_0Sub1(),
            new Line256X7_0Sub2(),
            new Line256X7_0Sub3(),
            new Line256X7_1Sub1(),
            new Line256X7_1Sub2(),
            new Line256X7_1Sub3()
        };

        private static readonly IStaticCodeBook[] Floor128X11Books =
        {
            new Line128X11Class1(),
            new Line128X11Class2(),
            new Line128X11Class3(),
            new Line128X11_0Sub0(),
            new Line128X11_1Sub0(),
            new Line128X11_1Sub1(),
            new Line128X11_2Sub1(),
            new Line128X11_2Sub2(),
            new Line128X11_2Sub3(),
            new Line128X11_3Sub1(),
            new Line128X11_3Sub2(),
            new Line128X11_3Sub3()
        };

        private static readonly IStaticCodeBook[] Floor128X17Books =
        {
            new Line128X17Class1(),
            new Line128X17Class2(),
            new Line128X17Class3(),
            new Line128X17_0Sub0(),
            new Line128X17_1Sub0(),
            new Line128X17_1Sub1(),
            new Line128X17_2Sub1(),
            new Line128X17_2Sub2(),
            new Line128X17_2Sub3(),
            new Line128X17_3Sub1(),
            new Line128X17_3Sub2(),
            new Line128X17_3Sub3()
        };

        private static readonly IStaticCodeBook[] Floor256X4LowBooks =
        {
            new Line256X4LowClass0(),
            new Line256X4LowSub0(),
            new Line256X4LowSub1(),
            new Line256X4LowSub2(),
            new Line256X4LowSub3()
        };

        private static readonly IStaticCodeBook[] Floor1024X27Books =
        {
            new Line1024X27Class1(),
            new Line1024X27Class2(),
            new Line1024X27Class3(),
            new Line1024X27Class4(),
            new Line1024X27_0Sub0(),
            new Line1024X27_1Sub0(),
            new Line1024X27_1Sub1(),
            new Line1024X27_2Sub0(),
            new Line1024X27_2Sub1(),
            new Line1024X27_3Sub1(),
            new Line1024X27_3Sub2(),
            new Line1024X27_3Sub3(),
            new Line1024X27_4Sub1(),
            new Line1024X27_4Sub2(),
            new Line1024X27_4Sub3()
        };

        private static readonly IStaticCodeBook[] Floor2048X27Books =
        {
            new Line2048X27Class1(),
            new Line2048X27Class2(),
            new Line2048X27Class3(),
            new Line2048X27Class4(),
            new Line2048X27_0Sub0(),
            new Line2048X27_1Sub0(),
            new Line2048X27_1Sub1(),
            new Line2048X27_2Sub0(),
            new Line2048X27_2Sub1(),
            new Line2048X27_3Sub1(),
            new Line2048X27_3Sub2(),
            new Line2048X27_3Sub3(),
            new Line2048X27_4Sub1(),
            new Line2048X27_4Sub2(),
            new Line2048X27_4Sub3()
        };

        private static readonly IStaticCodeBook[] Floor512X17Books =
        {
            new Line512X17Class1(),
            new Line512X17Class2(),
            new Line512X17Class3(),
            new Line512X17_0Sub0(),
            new Line512X17_1Sub0(),
            new Line512X17_1Sub1(),
            new Line512X17_2Sub1(),
            new Line512X17_2Sub2(),
            new Line512X17_2Sub3(),
            new Line512X17_3Sub1(),
            new Line512X17_3Sub2(),
            new Line512X17_3Sub3()
        };

        public static readonly IStaticCodeBook[][] FloorBooks =
        {
            Floor128X4Books,
            Floor256X4Books,
            Floor128X7Books,
            Floor256X7Books,
            Floor128X11Books,
            Floor128X17Books,
            Floor256X4LowBooks,
            Floor1024X27Books,
            Floor2048X27Books,
            Floor512X17Books
        };

        public static readonly Floor[] Floor =
        {
            /* 0: 128 x 4 */
            new Floor(new[] {0}, new[] {4}, new[] {2}, new[] {0},
                new[] {new[] {1, 2, 3, 4}},
                4, new[] {0, 128, 33, 8, 16, 70},
                60, 30, 500, 1, 18, 128),
            /* 1: 256 x 4 */
            new Floor(new[] {0}, new[] {4}, new[] {2}, new[] {0},
                new[] {new[] {1, 2, 3, 4}},
                4, new[] {0, 256, 66, 16, 32, 140},
                60, 30, 500, 1, 18, 256
            ),
            /* 2: 128 x 7 */
            new Floor(new[] {0, 1}, new[] {3, 4}, new[] {2, 2}, new[] {0, 1},
                new[] {new[] {-1, 2, 3, 4}, new[] {-1, 5, 6, 7}},
                4, new[] {0, 128, 14, 4, 58, 2, 8, 28, 90},
                60, 30, 500, 1, 18, 128
            ),
            /* 3: 256 x 7 */
            new Floor(new[] {0, 1}, new[] {3, 4}, new[] {2, 2}, new[] {0, 1},
                new[] {new[] {-1, 2, 3, 4}, new[] {-1, 5, 6, 7}},
                4, new[] {0, 256, 28, 8, 116, 4, 16, 56, 180},
                60, 30, 500, 1, 18, 256
            ),
            /* 4: 128 x 11 */
            new Floor(new[] {0, 1, 2, 3}, new[] {2, 3, 3, 3}, new[] {0, 1, 2, 2}, new[] {-1, 0, 1, 2},
                new[] {new[] {3}, new[] {4, 5}, new[] {-1, 6, 7, 8}, new[] {-1, 9, 10, 11}},
                2, new[] {0, 128, 8, 33, 4, 16, 70, 2, 6, 12, 23, 46, 90},
                60, 30, 500, 1, 18, 128
            ),
            /* 5: 128 x 17 */
            new Floor(new[] {0, 1, 1, 2, 3, 3}, new[] {2, 3, 3, 3}, new[] {0, 1, 2, 2}, new[] {-1, 0, 1, 2},
                new[] {new[] {3}, new[] {4, 5}, new[] {-1, 6, 7, 8}, new[] {-1, 9, 10, 11}},
                2, new[] {0, 128, 12, 46, 4, 8, 16, 23, 33, 70, 2, 6, 10, 14, 19, 28, 39, 58, 90},
                60, 30, 500, 1, 18, 128
            ),
            /* 6: 256 x 4 (low bitrate version) */
            new Floor(new[] {0}, new[] {4}, new[] {2}, new[] {0},
                new[] {new[] {1, 2, 3, 4}},
                4, new[] {0, 256, 66, 16, 32, 140},
                60, 30, 500, 1, 18, 256
            ),
            /* 7: 1024 x 27 */
            new Floor(new[] {0, 1, 2, 2, 3, 3, 4, 4}, new[] {3, 4, 3, 4, 3}, new[] {0, 1, 1, 2, 2},
                new[] {-1, 0, 1, 2, 3},
                new[] {new[] {4}, new[] {5, 6}, new[] {7, 8}, new[] {-1, 9, 10, 11}, new[] {-1, 12, 13, 14}},
                2, new[]
                {
                    0, 1024, 93, 23, 372, 6, 46, 186, 750, 14, 33, 65, 130, 260, 556,
                    3, 10, 18, 28, 39, 55, 79, 111, 158, 220, 312, 464, 650, 850
                },
                60, 30, 500, 3, 18, 1024
            ),
            /* 8: 2048 x 27 */
            new Floor(new[] {0, 1, 2, 2, 3, 3, 4, 4}, new[] {3, 4, 3, 4, 3}, new[] {0, 1, 1, 2, 2},
                new[] {-1, 0, 1, 2, 3},
                new[] {new[] {4}, new[] {5, 6}, new[] {7, 8}, new[] {-1, 9, 10, 11}, new[] {-1, 12, 13, 14}},
                2, new[]
                {
                    0, 2048, 186, 46, 744, 12, 92, 372, 1500, 28, 66, 130, 260, 520, 1112,
                    6, 20, 36, 56, 78, 110, 158, 222, 316, 440, 624, 928, 1300, 1700
                },
                60, 30, 500, 3, 18, 2048
            ),
            /* 9: 512 x 17 */
            new Floor(new[] {0, 1, 1, 2, 3, 3}, new[] {2, 3, 3, 3}, new[] {0, 1, 2, 2}, new[] {-1, 0, 1, 2},
                new[] {new[] {3}, new[] {4, 5}, new[] {-1, 6, 7, 8}, new[] {-1, 9, 10, 11}},
                2, new[]
                {
                    0, 512, 46, 186, 16, 33, 65, 93, 130, 278,
                    7, 23, 39, 55, 79, 110, 156, 232, 360
                },
                60, 30, 500, 1, 18, 512
            ),
            /* 10: X x 0 (LFE floor; edge posts only) */
            new Floor(new[] {0}, new[] {0}, new[] {0}, new[] {-1},
                new[] {new[] {-1}},
                2, new[] {0, 12},
                60, 30, 500, 1, 18, 10
            )
        };
    }
}