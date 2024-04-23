using ArchitectureLibrary.Model;
using GBATool.Models;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GBATool.Building;

public sealed class Header : Building<Header>
{
    /*
        Header Overview:
        Address Bytes Expl.
        004h    156   Nintendo Logo    (compressed bitmap, required!)
        0A0h    12    Game Title       (uppercase ascii, max 12 characters)
        0ACh    4     Game Code        (uppercase ascii, 4 characters)
        0B0h    2     Maker Code       (uppercase ascii, 2 characters)
        0B2h    1     Fixed value      (must be 96h, required!)
        0B3h    1     Main unit code   (00h for current GBA models)
        0B4h    1     Device type      (usually 00h) (bit7=DACS/debug related)
        0B5h    7     Reserved Area    (should be zero filled)
        0BCh    1     Software version (usually 00h)
        0BDh    1     Complement check (header checksum, required!)
        0BEh    2     Reserved Area    (should be zero filled)
        --- Additional Multiboot Header Entries ---
        0C0h    4     RAM Entry Point  (32bit ARM branch opcode, eg. "B ram_start")
        0C4h    1     Boot mode        (init as 00h - BIOS overwrites this value!)
        0C5h    1     Slave ID Number  (init as 00h - BIOS overwrites this value!)
        0C6h    26    Not used         (seems to be unused)
        0E0h    4     JOYBUS Entry Pt. (32bit ARM branch opcode, eg. "B joy_start")

        Information taken from: https://problemkaputt.de/gbatek-gba-cartridge-header.htm
     */

    private static readonly int HeaderSize = 224;
    private static readonly int HeaderSizeForChecksum = 29;

    protected override string FileName { get; } = "header.asm";

    private static readonly int[] GBANintendoLogo = {
        0x24, 0xFF, 0xAE, 0x51, 0x69, 0x9A, 0xA2, 0x21, 0x3D, 0x84, 0x82, 0x0A, 0x84, 0xE4, 0x09, 0xAD, 0x11, 0x24,
        0x8B, 0x98, 0xC0, 0x81, 0x7F, 0x21, 0xA3, 0x52, 0xBE, 0x19, 0x93, 0x09, 0xCE, 0x20, 0x10, 0x46, 0x4A, 0x4A,
        0xF8, 0x27, 0x31, 0xEC, 0x58, 0xC7, 0xE8, 0x33, 0x82, 0xE3, 0xCE, 0xBF, 0x85, 0xF4, 0xDF, 0x94, 0xCE, 0x4B,
        0x09, 0xC1, 0x94, 0x56, 0x8A, 0xC0, 0x13, 0x72, 0xA7, 0xFC, 0x9F, 0x84, 0x4D, 0x73, 0xA3, 0xCA, 0x9A, 0x61,
        0x58, 0x97, 0xA3, 0x27, 0xFC, 0x03, 0x98, 0x76, 0x23, 0x1D, 0xC7, 0x61, 0x03, 0x04, 0xAE, 0x56, 0xBF, 0x38,
        0x84, 0x00, 0x40, 0xA7, 0x0E, 0xFD, 0xFF, 0x52, 0xFE, 0x03, 0x6F, 0x95, 0x30, 0xF1, 0x97, 0xFB, 0xC0, 0x85,
        0x60, 0xD6, 0x80, 0x25, 0xA9, 0x63, 0xBE, 0x03, 0x01, 0x4E, 0x38, 0xE2, 0xF9, 0xA2, 0x34, 0xFF, 0xBB, 0x3E,
        0x03, 0x44, 0x78, 0x00, 0x90, 0xCB, 0x88, 0x11, 0x3A, 0x94, 0x65, 0xC0, 0x7C, 0x63, 0x87, 0xF0, 0x3C, 0xAF,
        0xD6, 0x25, 0xE4, 0x8B, 0x38, 0x0A, 0xAC, 0x72, 0x21, 0xD4, 0xF8, 0x07
    };

    private static readonly byte[] _arrayOfBytesForCheckSum = new byte[HeaderSizeForChecksum];

    protected override async Task<bool> DoGenerate(string filePath)
    {
        try
        {
            for (int i = 0; i < _arrayOfBytesForCheckSum.Length; i++)
            {
                _arrayOfBytesForCheckSum[i] = 0;
            }

            ProjectModel projectModel = ModelManager.Get<ProjectModel>();

            using (FileStream sourceStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                StringBuilder sb = new();
                _ = sb.AppendLine("; This file is auto generated!");
                _ = sb.AppendLine();

                int byteCounter = 0;

                byteCounter += WriteNintendoLogo(ref sb);
                byteCounter += WriteGameTitle(ref sb, projectModel);
                byteCounter += WriteGameCode(ref sb, projectModel);
                byteCounter += WriteMarkerCode(ref sb, projectModel);
                byteCounter += WriteFixedValue(ref sb);
                byteCounter += WriteMainUnitCode(ref sb);
                byteCounter += WriteDeviceType(ref sb);
                byteCounter += WriteEmptyArea(ref sb, 7);
                byteCounter += WriteSoftwareVersion(ref sb, projectModel);
                byteCounter += WriteComplementCheck(ref sb);
                byteCounter += WriteEmptyArea(ref sb, 2);
                byteCounter += WriteRAMEntryPoint(ref sb);
                byteCounter += WriteBootMode(ref sb);
                byteCounter += WriteSlaveIDNumber(ref sb);
                byteCounter += WriteEmptyArea(ref sb, 26);
                byteCounter += WriteJoybusEntryPoint(ref sb);

                if (byteCounter != HeaderSize)
                {
                    AddError("The header is malformed");

                    sb.AppendLine("=====> The header is malformed");
                }

                UTF8Encoding utf8 = new();
                byte[] encodedText = utf8.GetBytes(sb.ToString());

                await sourceStream.WriteAsync(encodedText).ConfigureAwait(false);
            };

            return GetErrors().Length == 0;
        }
        catch
        {
            return false;
        }
    }

    private static int WriteFixedValue(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; Fixed value");
        _ = sb.AppendLine("db 0x96");
        _ = sb.AppendLine();

        _arrayOfBytesForCheckSum[18] = 0x96;

        return 1;
    }

    private static int WriteNintendoLogo(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; Nintendo logo");

        int countHexValues = 0;
        foreach (int hex in GBANintendoLogo)
        {
            if (countHexValues % 26 == 0)
            {
                if (countHexValues > 0)
                    _ = sb.AppendLine();

                sb.Append($"db 0x{hex:X2},");
            }
            else
            {
                sb.Append($"0x{hex:X2}");

                if (countHexValues % 26 < 25)
                {
                    sb.Append(',');
                }
            }

            countHexValues++;
        }
        _ = sb.AppendLine();
        _ = sb.AppendLine();

        return countHexValues;
    }

    // Returns the number of bytes written
    private static int WriteStringAsBytes(ref StringBuilder sb, string input, int maxByteSize)
    {
        int numberOfBytes = 0;

        sb.Append("db ");

        input = input.PadRight(maxByteSize, '\0');

        for (int i = 0; i < maxByteSize; i++)
        {
            int character = input[i];

            sb.Append($"0x{character:X2}");

            numberOfBytes++;

            if (i < maxByteSize - 1)
                sb.Append(',');
        }

        return numberOfBytes;
    }

    private static void AddToChecksumArray(char[] charArray, int initIndex)
    {
        for (int i = 0; i < charArray.Length; i++)
        {
            _arrayOfBytesForCheckSum[initIndex + i] = (byte)charArray[i];
        }
    }

    private static int WriteGameTitle(ref StringBuilder sb, ProjectModel projectModel)
    {
        _ = sb.AppendLine("; Game title");

        int ret = WriteStringAsBytes(ref sb, projectModel.ProjectTitle.ToUpper(), 12);

        _ = sb.AppendLine();
        _ = sb.AppendLine();

        AddToChecksumArray(projectModel.ProjectTitle.ToUpper().PadRight(12, '\0').ToCharArray(), 0);

        return ret;
    }

    /*
        ==== =================================== =============================== ==== =====================
	                    U (game type)                   TT (shorthand game title)     D (destination/language)
	        ---------------------------------------- ------------------------------- --------------------------
	        Code Meaning                             Meaning                         Code Destination/language
	        ==== =================================== =============================== ==== =====================
	        A    Normal game (usually 2001 to 2003)  An arbitrary abbreviation for   D    German
	        B    Normal game (year 2003 onwards)     the game title (eg. "PM" for    E    USA/English
	        C    Normal game (not yet used)          "Pac Man").                     F    French
	        F    Famicom/NES                                                         I    Italian
	        K    Acceleration sensor                                                 J    Japan
	        P    For e-Reader (dot-code scanner)                                     P    Europe/elsewhere
	        R    Rumble and Z-axis gyro sensor                                       S    Spanish
	        U    RTC and solar sensor
	        V    Rumble motor         
     */
    private static int WriteGameCode(ref StringBuilder sb, ProjectModel projectModel)
    {
        _ = sb.AppendLine("; Game code");

        string projectInitials = projectModel.ProjectInitials;

        if (projectInitials.Length > 2)
            projectInitials = projectInitials[..2];
        else if (projectInitials.Length < 2)
            projectInitials = projectInitials.PadRight(2, '\0');

        StringBuilder gameCode = new();
        gameCode.Append('C');
        gameCode.Append(projectInitials);
        gameCode.Append('P');

        int ret = WriteStringAsBytes(ref sb, gameCode.ToString(), 4);

        _ = sb.AppendLine();
        _ = sb.AppendLine();

        AddToChecksumArray(gameCode.ToString().ToCharArray(), 12);

        return ret;
    }

    // Identifies the (commercial) developer
    private static int WriteMarkerCode(ref StringBuilder sb, ProjectModel projectModel)
    {
        _ = sb.AppendLine("; Marker Code");

        string markerCode = projectModel.DeveloperId;

        if (markerCode.Length > 2)
            markerCode = markerCode[..2];
        else if (markerCode.Length < 2)
            markerCode = markerCode.PadRight(2, '\0');

        int ret = WriteStringAsBytes(ref sb, markerCode, 2);

        _ = sb.AppendLine();
        _ = sb.AppendLine();

        AddToChecksumArray(markerCode.ToCharArray(), 16);

        return ret;
    }

    private static int WriteEmptyArea(ref StringBuilder sb, int areaSize)
    {
        _ = sb.AppendLine("; Empty area");

        string emptyArea = string.Empty;

        int ret = WriteStringAsBytes(ref sb, emptyArea, areaSize);

        _ = sb.AppendLine();
        _ = sb.AppendLine();

        return ret;
    }

    private static int WriteMainUnitCode(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; Main unit code");
        _ = sb.AppendLine("db 0x00");
        _ = sb.AppendLine();

        _arrayOfBytesForCheckSum[19] = 0x00;

        return 1;
    }

    private static int WriteDeviceType(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; Device type");
        _ = sb.AppendLine("db 0x00");
        _ = sb.AppendLine();

        _arrayOfBytesForCheckSum[20] = 0x00;

        return 1;
    }

    private static int WriteSoftwareVersion(ref StringBuilder sb, ProjectModel projectModel)
    {
        _ = sb.AppendLine("; Software version");
        _ = sb.AppendLine($"db 0x{projectModel.SoftwareVersion:X2}");
        _ = sb.AppendLine();

        _arrayOfBytesForCheckSum[28] = 0x00;

        return 1;
    }

    private static int WriteBootMode(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; Boot mode");
        _ = sb.AppendLine("db 0x00");
        _ = sb.AppendLine();

        return 1;
    }

    private static int WriteSlaveIDNumber(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; Slave ID number");
        _ = sb.AppendLine("db 0x00");
        _ = sb.AppendLine();

        return 1;
    }

    private static int WriteJoybusEntryPoint(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; Joy bus entry point");
        _ = sb.AppendLine("db 0x00,0x00,0x00,0x00");
        _ = sb.AppendLine();

        return 4;
    }

    private static int WriteRAMEntryPoint(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; RAM entry point");
        _ = sb.AppendLine("db 0x00,0x00,0x00,0x00");
        _ = sb.AppendLine();

        return 4;
    }

    /*
        # Python code to calculate GBA complement check.
		    checksum = 0

		    for b in rom_data[0xa0:0xbd]:
			    checksum = (checksum - b) & 0xff

		    return (checksum - 0x19) & 0xff
     */
    private static int WriteComplementCheck(ref StringBuilder sb)
    {
        _ = sb.AppendLine("; Complement check");

        int checksum = 0;

        for (int i = 0; i < HeaderSizeForChecksum; i++)
        {
            checksum = (checksum - _arrayOfBytesForCheckSum[i]) & 0xff;
        }

        checksum = (checksum - 0x19) & 0xff;

        _ = sb.AppendLine($"db 0x{checksum:X2}");
        _ = sb.AppendLine();

        return 1;
    }
}
