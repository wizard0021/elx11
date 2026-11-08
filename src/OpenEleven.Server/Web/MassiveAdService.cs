// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 ELX11 Team & PES5 Indie Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace OpenEleven.Server.Web;

/// <summary>
/// Emulates the Microsoft Massive In-Game Advertising protocol (locate.madserver.net)
/// for PES 2011 stadium hoardings / billboards.
/// </summary>
public static class MassiveAdService
{
    private static string? _customAdsDir;

    public static string AdsFolder
    {
        get
        {
            if (!string.IsNullOrEmpty(_customAdsDir) && Directory.Exists(_customAdsDir))
                return _customAdsDir;

            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "reklamlar"),
                Path.Combine(Directory.GetCurrentDirectory(), "reklamlar"),
                "/home/ubuntu/elx11/reklamlar",
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "reklamlar")
            };

            foreach (var c in candidates)
            {
                if (Directory.Exists(c))
                {
                    _customAdsDir = Path.GetFullPath(c);
                    return _customAdsDir;
                }
            }

            var defaultDir = Path.Combine(Directory.GetCurrentDirectory(), "reklamlar");
            Directory.CreateDirectory(defaultDir);
            _customAdsDir = defaultDir;
            return _customAdsDir;
        }
    }

    public static byte[] BuildLocateResponse(string ip, int port = 80, ulong serverId = 1)
    {
        using var ms = new MemoryStream();
        // Tag 0x0D: Status (NOERR = 0)
        ms.WriteByte(0x0D);
        WriteUInt16BE(ms, 0);

        // Tag 0x3A: Host string
        ms.WriteByte(0x3A);
        WriteString(ms, ip);

        // Tag 0x3B: Port
        ms.WriteByte(0x3B);
        WriteUInt16BE(ms, (ushort)port);

        // Tag 0x3F: Server ID
        ms.WriteByte(0x3F);
        WriteUInt64BE(ms, serverId);

        return WrapPacket(0xCA, ms.ToArray());
    }

    public static byte[] BuildOpenSessionResponse(string sessionId = "SESS_PES2011_01")
    {
        using var ms = new MemoryStream();
        // Tag 0x47: Session token string
        ms.WriteByte(0x47);
        WriteString(ms, sessionId);

        // Tag 0x2B: Expiry (timestamp + 1 day)
        ms.WriteByte(0x2B);
        WriteUInt64BE(ms, (ulong)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 86400));

        // Tag 0x2A: Duration / TTL
        ms.WriteByte(0x2A);
        WriteUInt64BE(ms, 86400);

        // Tag 0x1E: Flags
        ms.WriteByte(0x1E);
        WriteUInt16BE(ms, 0);

        return WrapPacket(0xCC, ms.ToArray());
    }

    public static byte[] BuildEnterZoneResponse(string serverIp, int serverPort, string adsDirectory)
    {
        var files = Directory.Exists(adsDirectory)
            ? Directory.GetFiles(adsDirectory)
                .Where(f => {
                    var ext = Path.GetExtension(f).ToLowerInvariant();
                    return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".dds";
                })
                .OrderBy(f => f)
                .ToList()
            : [];

        if (files.Count == 0)
        {
            using var emptyMs = new MemoryStream();
            emptyMs.WriteByte(0x1E);
            WriteUInt16BE(emptyMs, 20);
            emptyMs.Write(new byte[20]);
            emptyMs.WriteByte(0x25);
            WriteUInt16BE(emptyMs, 0);
            emptyMs.WriteByte(0x55);
            WriteUInt16BE(emptyMs, 0);
            return WrapPacket(0xCE, emptyMs.ToArray());
        }

        var maoNames = Enumerable.Range(1, 9).Select(i => $"PES2011_Hoarding{i}").ToList();

        using var payload = new MemoryStream();
        payload.WriteByte(0x1E);
        WriteUInt16BE(payload, 20);
        payload.Write(new byte[20]);
        payload.WriteByte(0x25);
        WriteUInt16BE(payload, (ushort)maoNames.Count);
        payload.WriteByte(0x55);
        WriteUInt16BE(payload, 1);

        foreach (var mao in maoNames)
        {
            using var mPay = new MemoryStream();
            mPay.WriteByte(0x4B);
            WriteString(mPay, mao);
            mPay.WriteByte(0x4C);
            WriteUInt16BE(mPay, 0x10);
            mPay.WriteByte(0x0B);
            WriteUInt32BE(mPay, 1);
            mPay.WriteByte(0x24);
            WriteUInt16BE(mPay, 0);
            mPay.WriteByte(0x4A);
            WriteUInt32BE(mPay, 0);
            mPay.WriteByte(0x52);
            mPay.WriteByte(0);

            var mBytes = mPay.ToArray();
            payload.WriteByte(0xDD);
            WriteUInt32BE(payload, (uint)mBytes.Length);
            payload.Write(mBytes);
        }

        var orderAssets = new List<byte[]>();
        ulong idx = 1;
        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            var sha1 = SHA1.HashData(fileBytes);
            var (w, h) = GetImageDimensions(fileBytes);
            var portStr = serverPort != 80 ? $":{serverPort}" : "";
            var url = $"http://{serverIp}{portStr}/adsrv/assets/{fileName}";

            using var aPay = new MemoryStream();
            aPay.WriteByte(0x02);
            WriteUInt64BE(aPay, idx++);
            aPay.WriteByte(0x04);
            WriteUInt16BE(aPay, 20);
            aPay.Write(sha1);
            aPay.WriteByte(0x05);
            WriteUInt32BE(aPay, 1);
            aPay.WriteByte(0x06);
            WriteUInt32BE(aPay, (uint)w);
            aPay.WriteByte(0x07);
            WriteUInt32BE(aPay, (uint)h);
            aPay.WriteByte(0x0B);
            WriteUInt32BE(aPay, (uint)fileBytes.Length);
            aPay.WriteByte(0x0C);
            WriteString(aPay, url);
            aPay.WriteByte(0x15);
            WriteUInt32BE(aPay, 0);

            var aBytes = aPay.ToArray();
            using var astBlk = new MemoryStream();
            astBlk.WriteByte(0xDE);
            WriteUInt32BE(astBlk, (uint)aBytes.Length);
            astBlk.Write(aBytes);
            orderAssets.Add(astBlk.ToArray());
        }

        using var oPay = new MemoryStream();
        oPay.WriteByte(0x01);
        WriteUInt16BE(oPay, (ushort)orderAssets.Count);
        oPay.WriteByte(0x13);
        WriteUInt32BE(oPay, 1);
        foreach (var ab in orderAssets)
            oPay.Write(ab);

        var oBytes = oPay.ToArray();
        payload.WriteByte(0xE3);
        WriteUInt32BE(payload, (uint)oBytes.Length);
        payload.Write(oBytes);

        return WrapPacket(0xCE, payload.ToArray());
    }

    public static (int Width, int Height) GetImageDimensions(byte[] bytes)
    {
        try
        {
            if (bytes.Length >= 24 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                var w = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4));
                var h = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4));
                return (w, h);
            }
            if (bytes.Length >= 26 && bytes[0] == 0x42 && bytes[1] == 0x4D)
            {
                var w = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(18, 4));
                var h = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(22, 4));
                return (Math.Abs(w), Math.Abs(h));
            }
            if (bytes.Length >= 20 && bytes[0] == 0x44 && bytes[1] == 0x44 && bytes[2] == 0x53 && bytes[3] == 0x20)
            {
                var h = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(12, 4));
                var w = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(16, 4));
                return (w, h);
            }
        }
        catch { }
        return (512, 128);
    }

    private static byte[] WrapPacket(byte opcode, byte[] payload)
    {
        using var ms = new MemoryStream();
        ms.WriteByte(opcode);
        WriteUInt32BE(ms, (uint)payload.Length);
        ms.Write(payload);
        return ms.ToArray();
    }

    private static void WriteString(Stream s, string str)
    {
        var bytes = Encoding.ASCII.GetBytes(str);
        WriteUInt16BE(s, (ushort)bytes.Length);
        s.Write(bytes);
    }

    private static void WriteUInt16BE(Stream s, ushort val)
    {
        Span<byte> span = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(span, val);
        s.Write(span);
    }

    private static void WriteUInt32BE(Stream s, uint val)
    {
        Span<byte> span = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(span, val);
        s.Write(span);
    }

    private static void WriteUInt64BE(Stream s, ulong val)
    {
        Span<byte> span = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(span, val);
        s.Write(span);
    }
}
