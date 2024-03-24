using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class containing constants and useful reusable static functions
/// </summary>
public class Utilities
{
    public const int LEVEL1 = 0, LEVEL2 = 1, LEVEL3 = 2;
    public const int EASY = 0, MEDIUM = 1, HARD = 2;
    public const double MULTIPLIER_LEVEL1 = 0.2, MULTIPLIER_LEVEL2 = 0.5, MULTIPLIER_LEVEL3 = 0.9;
    public const double POTION_MULTIPLIER = 5;
    public const int AMETHYST_PRICE = 5, GRIMOIRE_PRICE = 8, POTION_PRICE = 20;
    public const string LOCALSERVER = "http://localhost:3000/";

    /// <summary>
    /// Convert a base64 format string to a Unity Sprite
    /// </summary>
    /// <param name="str"></param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public static Sprite Base64ToSprite(string str, int height = 348, int width = 348)
    {
        byte[] imageBytes = Convert.FromBase64String(str);
        var tex = new Texture2D(height, width);
        tex.LoadImage(imageBytes);
        return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Convert an hexadecimal string to the default encoding of the system codes table, usually UTF8Encoding
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static string HexToString(string hex)
    {
        var bytes = Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
        return System.Text.Encoding.Default.GetString(bytes).Replace("\0", String.Empty);
    }

    /// <summary>
    /// Convert an UTF8 encoded string to an hexadecimal string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string StringToHex(string str)
    {
        byte[] ba = Encoding.UTF8.GetBytes(str);
        var hexString = BitConverter.ToString(ba).Replace("-", "");

        return hexString;
    }

    /// <summary>
    /// Convert an hexadecimal string to an integer number
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static int HexToInt(string hex)
    {
        int intValue = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        return intValue;
    }

    /// <summary>
    /// Split a string in N chuncks of a specified size
    /// </summary>
    /// <param name="str"></param>
    /// <param name="chunkSize"></param>
    /// <returns></returns>
    public static List<string> SplitString(string str, int chunkSize)
    {
        return Enumerable.Range(0, str.Length / chunkSize)
            .Select(i => str.Substring(i * chunkSize, chunkSize)).ToList();
    }

}
