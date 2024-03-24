using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Class containing all the information of an NFT
/// </summary>
[Serializable]
public class Nft
{
    public Nft()
    {

    }

    public Nft(int id, string name, int rarity, string description, int fanciness, string image, int amount, int price, double releaseDate)
    {
        Id = id;
        Name = name;
        Rarity = rarity;
        Description = description;
        Fanciness = fanciness;
        Image = image;
        Amount = amount;
        Price = price;
        ReleaseDate = releaseDate;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int Rarity { get; set; }
    public string Description { get; set; }
    public int Fanciness { get; set; }
    public string Image { get; set; }
    public int Amount { get; set; }
    public int Price { get; set; }
    public double ReleaseDate { get; set; }
    public Sprite Sprite
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Image))
            {
                return Resources.Load<Sprite>("Images/Common/image_not_found");
            }
            else
            {
                return Utilities.Base64ToSprite(Image);
            }
        }
    }

}
