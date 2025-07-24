using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ComponentAce.Compression.Libs.zlib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SugarTalk.Messages.Dto.Tencent;

public class TencentTlsSigApIv2
{ 
    private readonly int _sdkappid;
    private readonly string _key;

    public TencentTlsSigApIv2(int sdkappid, string key)
    {
        _sdkappid = sdkappid;
        _key = key;
    }

    private static byte[] CompressBytes(byte[] sourceByte)
    {
        var inputStream = new MemoryStream(sourceByte);
        var outStream = CompressStream(inputStream);
        var outPutByteArray = new byte[outStream.Length];
        outStream.Position = 0;
        outStream.Read(outPutByteArray, 0, outPutByteArray.Length);
        return outPutByteArray;
    }

    private static Stream CompressStream(Stream sourceStream)
    {
        var streamOut = new MemoryStream();
        var streamZOut = new ZOutputStream(streamOut, zlibConst.Z_DEFAULT_COMPRESSION);
        CopyStream(sourceStream, streamZOut);
        streamZOut.finish();
        return streamOut;
    }

    public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
    {
        var buffer = new byte[2000];
        int len;
        while ((len = input.Read(buffer, 0, 2000)) > 0)
        {
            output.Write(buffer, 0, len);
        }
        output.Flush();
    }

    private string HMACSHA256(string identifier, long currTime, int expire, string base64UserBuf, bool userBufEnabled)
    {
        var rawContentToBeSigned = "TLS.identifier:" + identifier + "\n"
                                   + "TLS.sdkappid:" + _sdkappid + "\n"
                                   + "TLS.time:" + currTime + "\n"
                                   + "TLS.expire:" + expire + "\n";
        if (true == userBufEnabled)
        {
            rawContentToBeSigned += "TLS.userbuf:" + base64UserBuf + "\n";
        }
        using (HMACSHA256 hmac = new HMACSHA256())
        {
            var encoding = new UTF8Encoding();
            var textBytes = encoding.GetBytes(rawContentToBeSigned);
            var keyBytes = encoding.GetBytes(_key);
            Byte[] hashBytes;
            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    private string GenSig(string identifier, int expire, byte[] userbuf, bool userBufEnabled)
    {
        var epoch = new DateTime(1970, 1, 1); 
        var currTime = (Int64)(DateTime.UtcNow - epoch).TotalMilliseconds / 1000;

        string base64UserBuf;
        string jsonData;
        if (true == userBufEnabled)
        {
            base64UserBuf = Convert.ToBase64String(userbuf);
            var base64sig = HMACSHA256(identifier, currTime, expire, base64UserBuf, userBufEnabled);
            var jsonObj = new JObject();
            jsonObj["TLS.ver"] = "2.0";
            jsonObj["TLS.identifier"] = identifier;
            jsonObj["TLS.sdkappid"] = _sdkappid;
            jsonObj["TLS.expire"] = expire;
            jsonObj["TLS.time"] = currTime;
            jsonObj["TLS.sig"] = base64sig;
            jsonObj["TLS.userbuf"] = base64UserBuf;
            jsonData = JsonConvert.SerializeObject(jsonObj);
        }
        else
        {
            var base64sig = HMACSHA256(identifier, currTime, expire, "", false);
            var jsonObj = new JObject();
            jsonObj["TLS.ver"] = "2.0";
            jsonObj["TLS.identifier"] = identifier;
            jsonObj["TLS.sdkappid"] = _sdkappid;
            jsonObj["TLS.expire"] = expire;
            jsonObj["TLS.time"] = currTime;
            jsonObj["TLS.sig"] = base64sig;
            jsonData = JsonConvert.SerializeObject(jsonObj);
        }
        var buffer = Encoding.UTF8.GetBytes(jsonData);
        return Convert.ToBase64String(CompressBytes(buffer))
            .Replace('+', '*').Replace('/', '-').Replace('=', '_');
    }

    public string GenSig(string identifier, int expire = 180 * 86400)
    {
        return GenSig(identifier, expire, null, false);
    }
}