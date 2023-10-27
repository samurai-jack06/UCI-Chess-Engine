using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static ChessLogic;

public static class Magics
{
    public struct MagicEntry
    {
        public MagicEntry(ulong Mask, ulong Magic, int Shift, int Offset)
        {
            mask = Mask;
            magic = Magic;
            shift = Shift;
            offset = Offset;
        }

        public ulong mask;
        public ulong magic;
        public int shift;
        public int offset;
    }

    public static ulong[] RookTable = new ulong[102400];
    public static ulong[] BishopTable = new ulong[5248];

    public static MagicEntry[] RookMagics = new MagicEntry[64]
    {
        new MagicEntry (Mask: 0x000101010101017E, Magic: 0x5080008011400020, Shift: 52, Offset: 0),
        new MagicEntry (Mask: 0x000202020202027C, Magic: 0x0140001000402000, Shift: 53, Offset: 4096),
        new MagicEntry (Mask: 0x000404040404047A, Magic: 0x0280091000200480, Shift: 53, Offset: 6144),
        new MagicEntry (Mask: 0x0008080808080876, Magic: 0x0700081001002084, Shift: 53, Offset: 8192 ),
        new MagicEntry (Mask: 0x001010101010106E, Magic: 0x0300024408010030, Shift: 53, Offset: 10240 ),
        new MagicEntry (Mask: 0x002020202020205E, Magic: 0x510004004E480100, Shift: 53, Offset: 12288 ),
        new MagicEntry (Mask: 0x004040404040403E, Magic: 0x0400044128020090, Shift: 53, Offset: 14336 ),
        new MagicEntry (Mask: 0x008080808080807E, Magic: 0x8080004100012080, Shift: 52, Offset: 16384 ),
        new MagicEntry (Mask: 0x0001010101017E00, Magic: 0x0220800480C00124, Shift: 53, Offset: 20480 ),
        new MagicEntry (Mask: 0x0002020202027C00, Magic: 0x0020401001C02000, Shift: 54, Offset: 22528 ),
        new MagicEntry (Mask: 0x0004040404047A00, Magic: 0x000A002204428050, Shift: 54, Offset: 23552 ),
        new MagicEntry (Mask: 0x0008080808087600, Magic: 0x004E002040100A00, Shift: 54, Offset: 24576 ),
        new MagicEntry (Mask: 0x0010101010106E00, Magic: 0x0102000A00041020, Shift: 54, Offset: 25600 ),
        new MagicEntry (Mask: 0x0020202020205E00, Magic: 0x0A0880040080C200, Shift: 54, Offset: 26624 ),
        new MagicEntry (Mask: 0x0040404040403E00, Magic: 0x0002000600018408, Shift: 54, Offset: 27648 ),
        new MagicEntry (Mask: 0x0080808080807E00, Magic: 0x0025001200518100, Shift: 53, Offset: 28672 ),
        new MagicEntry (Mask: 0x00010101017E0100, Magic: 0x8900328001400080, Shift: 53, Offset: 30720 ),
        new MagicEntry (Mask: 0x00020202027C0200, Magic: 0x0848810020400100, Shift: 54, Offset: 32768 ),
        new MagicEntry (Mask: 0x00040404047A0400, Magic: 0xC001410020010153, Shift: 54, Offset: 33792 ),
        new MagicEntry (Mask: 0x0008080808760800, Magic: 0x4110C90020100101, Shift: 54, Offset: 34816 ),
        new MagicEntry (Mask: 0x00101010106E1000, Magic: 0x00A0808004004800, Shift: 54, Offset: 35840 ),
        new MagicEntry (Mask: 0x00202020205E2000, Magic: 0x401080801C000601, Shift: 54, Offset: 36864 ),
        new MagicEntry (Mask: 0x00404040403E4000, Magic: 0x0100040028104221, Shift: 54, Offset: 37888 ),
        new MagicEntry (Mask: 0x00808080807E8000, Magic: 0x840002000900A054, Shift: 53, Offset: 38912 ),
        new MagicEntry (Mask: 0x000101017E010100, Magic: 0x1000348280004000, Shift: 53, Offset: 40960 ),
        new MagicEntry (Mask: 0x000202027C020200, Magic: 0x001000404000E008, Shift: 54, Offset: 43008 ),
        new MagicEntry (Mask: 0x000404047A040400, Magic: 0x0424410300200035, Shift: 54, Offset: 44032 ),
        new MagicEntry (Mask: 0x0008080876080800, Magic: 0x2008C22200085200, Shift: 54, Offset: 45056 ),
        new MagicEntry (Mask: 0x001010106E101000, Magic: 0x0005304D00080100, Shift: 54, Offset: 46080 ),
        new MagicEntry (Mask: 0x002020205E202000, Magic: 0x000C040080120080, Shift: 54, Offset: 47104 ),
        new MagicEntry (Mask: 0x004040403E404000, Magic: 0x8404058400080210, Shift: 54, Offset: 48128 ),
        new MagicEntry (Mask: 0x008080807E808000, Magic: 0x0001848200010464, Shift: 53, Offset: 49152 ),
        new MagicEntry (Mask: 0x0001017E01010100, Magic: 0x6000204001800280, Shift: 53, Offset: 51200 ),
        new MagicEntry (Mask: 0x0002027C02020200, Magic: 0x2410004003C02010, Shift: 54, Offset: 53248 ),
        new MagicEntry (Mask: 0x0004047A04040400, Magic: 0x0181200A80801000, Shift: 54, Offset: 54272 ),
        new MagicEntry (Mask: 0x0008087608080800, Magic: 0x000C60400A001200, Shift: 54, Offset: 55296 ),
        new MagicEntry (Mask: 0x0010106E10101000, Magic: 0x0B00040180802800, Shift: 54, Offset: 56320 ),
        new MagicEntry (Mask: 0x0020205E20202000, Magic: 0xC00A000280804C00, Shift: 54, Offset: 57344 ),
        new MagicEntry (Mask: 0x0040403E40404000, Magic: 0x4040080504005210, Shift: 54, Offset: 58368 ),
        new MagicEntry (Mask: 0x0080807E80808000, Magic: 0x0000208402000041, Shift: 53, Offset: 59392 ),
        new MagicEntry (Mask: 0x00017E0101010100, Magic: 0xA200400080628000, Shift: 53, Offset: 61440 ),
        new MagicEntry (Mask: 0x00027C0202020200, Magic: 0x0021020240820020, Shift: 54, Offset: 63488 ),
        new MagicEntry (Mask: 0x00047A0404040400, Magic: 0x1020027000848022, Shift: 54, Offset: 64512 ),
        new MagicEntry (Mask: 0x0008760808080800, Magic: 0x0020500018008080, Shift: 54, Offset: 65536 ),
        new MagicEntry (Mask: 0x00106E1010101000, Magic: 0x10000D0008010010, Shift: 54, Offset: 66560 ),
        new MagicEntry (Mask: 0x00205E2020202000, Magic: 0x0100020004008080, Shift: 54, Offset: 67584 ),
        new MagicEntry (Mask: 0x00403E4040404000, Magic: 0x0008020004010100, Shift: 54, Offset: 68608 ),
        new MagicEntry (Mask: 0x00807E8080808000, Magic: 0x12241C0880420003, Shift: 53, Offset: 69632 ),
        new MagicEntry (Mask: 0x007E010101010100, Magic: 0x4000420024810200, Shift: 53, Offset: 71680 ),
        new MagicEntry (Mask: 0x007C020202020200, Magic: 0x0103004000308100, Shift: 54, Offset: 73728 ),
        new MagicEntry (Mask: 0x007A040404040400, Magic: 0x008C200010410300, Shift: 54, Offset: 74752 ),
        new MagicEntry (Mask: 0x0076080808080800, Magic: 0x2410008050A80480, Shift: 54, Offset: 75776 ),
        new MagicEntry (Mask: 0x006E101010101000, Magic: 0x0820880080040080, Shift: 54, Offset: 76800 ),
        new MagicEntry (Mask: 0x005E202020202000, Magic: 0x0044220080040080, Shift: 54, Offset: 77824 ),
        new MagicEntry (Mask: 0x003E404040404000, Magic: 0x2040100805120400, Shift: 54, Offset: 78848 ),
        new MagicEntry (Mask: 0x007E808080808000, Magic: 0x0129000080C20100, Shift: 53, Offset: 79872 ),
        new MagicEntry (Mask: 0x7E01010101010100, Magic: 0x0010402010800101, Shift: 52, Offset: 81920 ),
        new MagicEntry (Mask: 0x7C02020202020200, Magic: 0x0648A01040008101, Shift: 53, Offset: 86016 ),
        new MagicEntry (Mask: 0x7A04040404040400, Magic: 0x0006084102A00033, Shift: 53, Offset: 88064 ),
        new MagicEntry (Mask: 0x7608080808080800, Magic: 0x0002000870C06006, Shift: 53, Offset: 90112 ),
        new MagicEntry (Mask: 0x6E10101010101000, Magic: 0x0082008820100402, Shift: 53, Offset: 92160 ),
        new MagicEntry (Mask: 0x5E20202020202000, Magic: 0x0012008410050806, Shift: 53, Offset: 94208 ),
        new MagicEntry (Mask: 0x3E40404040404000, Magic: 0x2009408802100144, Shift: 53, Offset: 96256 ),
        new MagicEntry (Mask: 0x7E80808080808000, Magic: 0x821080440020810A, Shift: 52, Offset: 98304 )
    };
    public static MagicEntry[] BishopMagics = new MagicEntry[64]
    {
        new MagicEntry (Mask: 0x0040201008040200, Magic: 0x2020420401002200, Shift: 58, Offset: 0 ),
        new MagicEntry (Mask: 0x0000402010080400, Magic: 0x05210A020A002118, Shift: 59, Offset: 64 ),
        new MagicEntry (Mask: 0x0000004020100A00, Magic: 0x1110040454C00484, Shift: 59, Offset: 96 ),
        new MagicEntry (Mask: 0x0000000040221400, Magic: 0x1008095104080000, Shift: 59, Offset: 128 ),
        new MagicEntry (Mask: 0x0000000002442800, Magic: 0xC409104004000000, Shift: 59, Offset: 160 ),
        new MagicEntry (Mask: 0x0000000204085000, Magic: 0x0002901048080200, Shift: 59, Offset: 192 ),
        new MagicEntry (Mask: 0x0000020408102000, Magic: 0x0044040402084301, Shift: 59, Offset: 224 ),
        new MagicEntry (Mask: 0x0002040810204000, Magic: 0x2002030188040200, Shift: 58, Offset: 256 ),
        new MagicEntry (Mask: 0x0020100804020000, Magic: 0x0000C8084808004A, Shift: 59, Offset: 320 ),
        new MagicEntry (Mask: 0x0040201008040000, Magic: 0x1040040808010028, Shift: 59, Offset: 352 ),
        new MagicEntry (Mask: 0x00004020100A0000, Magic: 0x40040C0114090051, Shift: 59, Offset: 384 ),
        new MagicEntry (Mask: 0x0000004022140000, Magic: 0x40004820802004C4, Shift: 59, Offset: 416 ),
        new MagicEntry (Mask: 0x0000000244280000, Magic: 0x0010042420260012, Shift: 59, Offset: 448 ),
        new MagicEntry (Mask: 0x0000020408500000, Magic: 0x10024202300C010A, Shift: 59, Offset: 480 ),
        new MagicEntry (Mask: 0x0002040810200000, Magic: 0x000054013D101000, Shift: 59, Offset: 512 ),
        new MagicEntry (Mask: 0x0004081020400000, Magic: 0x0100020482188A0A, Shift: 59, Offset: 544 ),
        new MagicEntry (Mask: 0x0010080402000200, Magic: 0x0120090421020200, Shift: 59, Offset: 576 ),
        new MagicEntry (Mask: 0x0020100804000400, Magic: 0x1022204444040C00, Shift: 59, Offset: 608 ),
        new MagicEntry (Mask: 0x004020100A000A00, Magic: 0x0008000400440288, Shift: 57, Offset: 640 ),
        new MagicEntry (Mask: 0x0000402214001400, Magic: 0x0008060082004040, Shift: 57, Offset: 768 ),
        new MagicEntry (Mask: 0x0000024428002800, Magic: 0x0044040081A00800, Shift: 57, Offset: 896 ),
        new MagicEntry (Mask: 0x0002040850005000, Magic: 0x021200014308A010, Shift: 57, Offset: 1024 ),
        new MagicEntry (Mask: 0x0004081020002000, Magic: 0x8604040080880809, Shift: 59, Offset: 1152 ),
        new MagicEntry (Mask: 0x0008102040004000, Magic: 0x0000802D46009049, Shift: 59, Offset: 1184 ),
        new MagicEntry (Mask: 0x0008040200020400, Magic: 0x00500E8040080604, Shift: 59, Offset: 1216 ),
        new MagicEntry (Mask: 0x0010080400040800, Magic: 0x0024030030100320, Shift: 59, Offset: 1248 ),
        new MagicEntry (Mask: 0x0020100A000A1000, Magic: 0x2004100002002440, Shift: 57, Offset: 1280 ),
        new MagicEntry (Mask: 0x0040221400142200, Magic: 0x02090C0008440080, Shift: 55, Offset: 1408 ),
        new MagicEntry (Mask: 0x0002442800284400, Magic: 0x0205010000104000, Shift: 55, Offset: 1920 ),
        new MagicEntry (Mask: 0x0004085000500800, Magic: 0x0410820405004A00, Shift: 57, Offset: 2432 ),
        new MagicEntry (Mask: 0x0008102000201000, Magic: 0x8004140261012100, Shift: 59, Offset: 2560 ),
        new MagicEntry (Mask: 0x0010204000402000, Magic: 0x0A00460000820100, Shift: 59, Offset: 2592 ),
        new MagicEntry (Mask: 0x0004020002040800, Magic: 0x201004A40A101044, Shift: 59, Offset: 2624 ),
        new MagicEntry (Mask: 0x0008040004081000, Magic: 0x840C024220208440, Shift: 59, Offset: 2656 ),
        new MagicEntry (Mask: 0x00100A000A102000, Magic: 0x000C002E00240401, Shift: 57, Offset: 2688 ),
        new MagicEntry (Mask: 0x0022140014224000, Magic: 0x2220A00800010106, Shift: 55, Offset: 2816 ),
        new MagicEntry (Mask: 0x0044280028440200, Magic: 0x88C0080820060020, Shift: 55, Offset: 3328 ),
        new MagicEntry (Mask: 0x0008500050080400, Magic: 0x0818030B00A81041, Shift: 57, Offset: 3840 ),
        new MagicEntry (Mask: 0x0010200020100800, Magic: 0xC091280200110900, Shift: 59, Offset: 3968 ),
        new MagicEntry (Mask: 0x0020400040201000, Magic: 0x08A8114088804200, Shift: 59, Offset: 4000 ),
        new MagicEntry (Mask: 0x0002000204081000, Magic: 0x228929109000C001, Shift: 59, Offset: 4032 ),
        new MagicEntry (Mask: 0x0004000408102000, Magic: 0x1230480209205000, Shift: 59, Offset: 4064 ),
        new MagicEntry (Mask: 0x000A000A10204000, Magic: 0x0A43040202000102, Shift: 57, Offset: 4096 ),
        new MagicEntry (Mask: 0x0014001422400000, Magic: 0x1011284010444600, Shift: 57, Offset: 4224 ),
        new MagicEntry (Mask: 0x0028002844020000, Magic: 0x0003041008864400, Shift: 57, Offset: 4352 ),
        new MagicEntry (Mask: 0x0050005008040200, Magic: 0x0115010901000200, Shift: 57, Offset: 4480 ),
        new MagicEntry (Mask: 0x0020002010080400, Magic: 0x01200402C0840201, Shift: 59, Offset: 4608 ),
        new MagicEntry (Mask: 0x0040004020100800, Magic: 0x001A009400822110, Shift: 59, Offset: 4640 ),
        new MagicEntry (Mask: 0x0000020408102000, Magic: 0x2002111128410000, Shift: 59, Offset: 4672 ),
        new MagicEntry (Mask: 0x0000040810204000, Magic: 0x8420410288203000, Shift: 59, Offset: 4704 ),
        new MagicEntry (Mask: 0x00000A1020400000, Magic: 0x0041210402090081, Shift: 59, Offset: 4736 ),
        new MagicEntry (Mask: 0x0000142240000000, Magic: 0x8220002442120842, Shift: 59, Offset: 4768 ),
        new MagicEntry (Mask: 0x0000284402000000, Magic: 0x0140004010450000, Shift: 59, Offset: 4800 ),
        new MagicEntry (Mask: 0x0000500804020000, Magic: 0xC0408860086488A0, Shift: 59, Offset: 4832 ),
        new MagicEntry (Mask: 0x0000201008040200, Magic: 0x0090203E00820002, Shift: 59, Offset: 4864 ),
        new MagicEntry (Mask: 0x0000402010080400, Magic: 0x0820020083090024, Shift: 59, Offset: 4896 ),
        new MagicEntry (Mask: 0x0002040810204000, Magic: 0x1040440210900C05, Shift: 58, Offset: 4928 ),
        new MagicEntry (Mask: 0x0004081020400000, Magic: 0x0818182101082000, Shift: 59, Offset: 4992 ),
        new MagicEntry (Mask: 0x000A102040000000, Magic: 0x0200800080D80800, Shift: 59, Offset: 5024 ),
        new MagicEntry (Mask: 0x0014224000000000, Magic: 0x32A9220510209801, Shift: 59, Offset: 5056 ),
        new MagicEntry (Mask: 0x0028440200000000, Magic: 0x0000901010820200, Shift: 59, Offset: 5088 ),
        new MagicEntry (Mask: 0x0050080402000000, Magic: 0x0000014064080180, Shift: 59, Offset: 5120 ),
        new MagicEntry (Mask: 0x0020100804020000, Magic: 0xA001204204080186, Shift: 59, Offset: 5152 ),
        new MagicEntry (Mask: 0x0040201008040200, Magic: 0xC04010040258C048, Shift: 58, Offset: 5184 )
    };

    public static void PopulateMagicTables()
    {
        for (int square = 0; square < 64; square++)
        {
            ulong blockers = 0UL;

            do
            {
                ulong mask = RookMagics[square].mask;

                int index = GetRookIndex(square, blockers);
                RookTable[index] = GenRookSlow(square, blockers);

                blockers = (blockers - mask) & mask;

            } while (blockers != 0UL);

            blockers = 0UL;

            do
            {
                ulong mask = BishopMagics[square].mask;

                int index = GetBishopIndex(square, blockers);
                BishopTable[index] = GenBishopSlow(square, blockers);

                blockers = (blockers - mask) & mask;
            } while (blockers != 0UL);
        }
    }

    public static ulong GetMagicBitboard(byte square, ulong blockers, bool isRook)
    {
        if (isRook) return RookTable[GetRookIndex(square, blockers)];
        else return BishopTable[GetBishopIndex(square, blockers)];
    }

    public static int GetRookIndex(int square, ulong occupiedSquares)
    {
        ulong mask = RookMagics[square].mask;
        ulong magic = RookMagics[square].magic;
        int shift = RookMagics[square].shift;
        int offset = RookMagics[square].offset;

        return (int)(((mask & occupiedSquares) * magic) >> shift) + offset;
    }

    public static int GetBishopIndex(int square, ulong occupiedSquares)
    {
        ulong mask = BishopMagics[square].mask;
        ulong magic = BishopMagics[square].magic;
        int shift = BishopMagics[square].shift;
        int offset = BishopMagics[square].offset;

        return (int)(((mask & occupiedSquares) * magic) >> shift) + offset;
    }

    public static ulong GenRookSlow(int square, ulong blockers)
    {
        ulong moves = 0UL;

        moves |= NORTH_RAYS[square];
        if ((NORTH_RAYS[square] & blockers) != 0)
        {
            int blockerSquare = BitOperations.TrailingZeroCount(NORTH_RAYS[square] & blockers);
            moves &= ~NORTH_RAYS[blockerSquare];
        }

        moves |= EAST_RAYS[square];
        if ((EAST_RAYS[square] & blockers) != 0)
        {
            int blockerSquare = BitOperations.TrailingZeroCount(EAST_RAYS[square] & blockers);
            moves &= ~EAST_RAYS[blockerSquare];
        }

        moves |= SOUTH_RAYS[square];
        if ((SOUTH_RAYS[square] & blockers) != 0)
        {
            int blockerSquare = 63 - BitOperations.LeadingZeroCount(SOUTH_RAYS[square] & blockers);
            moves &= ~SOUTH_RAYS[blockerSquare];
        }

        moves |= WEST_RAYS[square];
        if ((WEST_RAYS[square] & blockers) != 0)
        {
            int blockerSquare = 63 - BitOperations.LeadingZeroCount(WEST_RAYS[square] & blockers);
            moves &= ~WEST_RAYS[blockerSquare];
        }

        return moves;
    }

    public static ulong GenBishopSlow(int square, ulong blockers)
    {
        ulong moves = 0UL;

        moves |= NE_RAYS[square];
        if ((moves & blockers) != 0)
        {
            int blockerSquare = BitOperations.TrailingZeroCount(NE_RAYS[square] & blockers);
            moves &= ~NE_RAYS[blockerSquare];
        }

        moves |= NW_RAYS[square];
        if ((NW_RAYS[square] & blockers) != 0)
        {
            int blockerSquare = BitOperations.TrailingZeroCount(NW_RAYS[square] & blockers);
            moves &= ~NW_RAYS[blockerSquare];
        }

        moves |= SE_RAYS[square];
        if ((SE_RAYS[square] & blockers) != 0)
        {
            int blockerSquare = 63 - BitOperations.LeadingZeroCount(SE_RAYS[square] & blockers);
            moves &= ~SE_RAYS[blockerSquare];
        }

        moves |= SW_RAYS[square];
        if ((SW_RAYS[square] & blockers) != 0)
        {
            int blockerSquare = 63 - BitOperations.LeadingZeroCount(SW_RAYS[square] & blockers);
            moves &= ~SW_RAYS[blockerSquare];
        }

        return moves;
    }
}