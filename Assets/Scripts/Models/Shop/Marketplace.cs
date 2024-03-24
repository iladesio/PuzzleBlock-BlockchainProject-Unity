using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;
using UnityEngine.Rendering;
using System.Diagnostics;
using System.Linq;

public class Marketplace : MonoBehaviour
{
    public List<Button> buttons;
    public List<GameObject> Panels;
    private GameObject currentPanel; // Keep track of current active panel
    private UserProfile userProfile = null;
    private List<Nft> nfts = null;
    public TextMeshProUGUI primaryBalance, temporaryBalance;
    public TextMeshProUGUI amethyst_price, grimoire_price, potion_price, amethyst_owned, grimoire_owned, potion_owned, buyInfo1, buyInfo2;
    public GameObject PaymentConfirmPanel, PaymentConfirmCard, NftInventory, LatestNftCard, YourNftInventory, ChoosePropicPanel;
    public Button BuyMainPage;
    private int idPowerUp = -1;
    private Nft NftToBuy = null;
    private string chosenPropic = "";
    private bool isOwnedNftSetup = false;

    public IUserService userService = new UserService();
    public INftService nftService = new NftService();

    /// <summary>
    /// Start is called before the first frame update. Setup scene data
    /// </summary>
    private async void Start()
    {
        if (!PlayerPrefs.HasKey("UserProfile") || !PlayerPrefs.HasKey("UserAddress"))
        {
            JavascriptRepository.Web3Alert("User not found.", "danger");
            userService.DoLogout();
        }

        userProfile = await userService.GetUserProfile(PlayerPrefs.GetString("UserAddress"));
        nfts = (await nftService.GetAllNft()).OrderBy(x => x.ReleaseDate).ToList();

        temporaryBalance.text = userProfile.SecondaryBalance.ToString() + " PB";
        primaryBalance.text = userProfile.PrimaryBalance.ToString() + " PB";

        PaymentConfirmPanel.SetActive(false);
        // Setup button events and panel activation
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => RedirectPage(button));
        }

        foreach (var nft in nfts)
        {
            var nftObj = Instantiate(Resources.Load<GameObject>("Prefabs/Nft"), NftInventory.transform);

            SetupNftCard(nftObj, nft);
        }

        SetupMainPage(nfts.FirstOrDefault());

        ActivatePanel("Market_MainPanel");

    }

    /// <summary>
    /// Setup Main page information.
    /// </summary>
    /// <param name="latestNft"></param>
    private void SetupMainPage(Nft latestNft)
    {
        LatestNftCard.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = latestNft.Sprite;
        LatestNftCard.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = latestNft.Amount.ToString();
        LatestNftCard.transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = latestNft.Name + " #" + latestNft.Id;
        LatestNftCard.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = latestNft.Price.ToString() + " PB";
        string spritePath;
        switch (latestNft.Rarity)
        {
            case 0:
                spritePath = "common";
                break;
            case 1:
                spritePath = "uncommon";
                break;
            case 2:
                spritePath = "rare";
                break;
            case 3:
                spritePath = "double-rare";
                break;
            case 4:
                spritePath = "ultra-rare";
                break;
            default:
                spritePath = "common";
                break;
        }
        LatestNftCard.transform.GetChild(6).GetComponent<Image>().sprite = Resources.Load<Sprite>($"Images/Shop/{spritePath}");

        if (latestNft.Amount <= 0)
        {
            var soldOut = new GameObject();
            soldOut.AddComponent<Image>().sprite = Resources.Load<Sprite>("Images/Common/out_of_stock");
            var soldOutObj = Instantiate(soldOut, LatestNftCard.transform.GetChild(1).GetChild(0).transform);
            soldOutObj.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(280, 180);
        }
        else
        {
            BuyMainPage.onClick.AddListener(() => BuyNft(latestNft.Id));
        }
    }

    /// <summary>
    /// Setup nft prefab information.
    /// </summary>
    /// <param name="nftObj"></param>
    /// <param name="nft"></param>
    private void SetupNftCard(GameObject nftObj, Nft nft)
    {
        nftObj.transform.GetChild(0).GetComponent<Image>().sprite = nft.Sprite;
        nftObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = nft.Name;
        nftObj.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = nft.Price.ToString() + " PB";
        nftObj.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = nft.Amount.ToString();
        nftObj.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "#" + nft.Id.ToString();

        string spritePath;
        switch (nft.Rarity)
        {
            case 0:
                spritePath = "common";
                break;
            case 1:
                spritePath = "uncommon";
                break;
            case 2:
                spritePath = "rare";
                break;
            case 3:
                spritePath = "double-rare";
                break;
            case 4:
                spritePath = "ultra-rare";
                break;
            default:
                spritePath = "common";
                break;
        }
        nftObj.transform.GetChild(5).GetComponent<Image>().sprite = Resources.Load<Sprite>($"Images/Shop/{spritePath}");

        if (nft.Amount <= 0)
        {
            var soldOut = new GameObject();
            soldOut.AddComponent<Image>().sprite = Resources.Load<Sprite>("Images/Common/out_of_stock");
            var soldOutObj = Instantiate(soldOut, nftObj.transform);
            soldOutObj.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(300, 200);
        }
        else
        {
            nftObj.GetComponent<Button>().onClick.AddListener(() => BuyNft(nft.Id));
        }
    }

    /// <summary>
    /// Setup owned nft prefab information.
    /// </summary>
    /// <param name="nftObj"></param>
    /// <param name="nft"></param>
    private void SetupOwnedNftCard(GameObject nftObj, Nft nft, int amountOwned)
    {
        nftObj.transform.GetChild(0).GetComponent<Image>().sprite = nft.Sprite;
        nftObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = nft.Name;
        nftObj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Amount";
        nftObj.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = amountOwned.ToString();
        nftObj.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = string.Empty;
        nftObj.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Empty;
        nftObj.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "#" + nft.Id.ToString();

        string spritePath;
        switch (nft.Rarity)
        {
            case 0:
                spritePath = "common";
                break;
            case 1:
                spritePath = "uncommon";
                break;
            case 2:
                spritePath = "rare";
                break;
            case 3:
                spritePath = "double-rare";
                break;
            case 4:
                spritePath = "ultra-rare";
                break;
            default:
                spritePath = "common";
                break;
        }
        nftObj.transform.GetChild(5).GetComponent<Image>().sprite = Resources.Load<Sprite>($"Images/Shop/{spritePath}");

        nftObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            chosenPropic = nft.Image;
            ChoosePropicPanel.SetActive(true);
        });

    }

    /// <summary>
    /// Handle propic choose cancelation
    /// </summary>
    public void OnCancelChoosePropic()
    {
        ChoosePropicPanel.SetActive(false);
    }

    /// <summary>
    /// Update user's profile picture
    /// </summary>
    public async void OnConfirmChoosePropic()
    {
        userProfile.PropicUrl = chosenPropic;
        await userService.UpdateUserProfile(userProfile, "GAME_TO_HOME");
    }

    /// <summary>
    /// Setup nft purchase panel
    /// </summary>
    /// <param name="id"></param>
    private async void BuyNft(int id)
    {
        var nft = await nftService.GetNftById(id);
        if (nft.Amount <= 0)
        {
            JavascriptRepository.Web3Alert("NFT out of stock", "warning");
            return;
        }
        var txt = "";
        txt += $"{nft.Name}\n\n\nPrimary Balance:\n{userProfile.PrimaryBalance} PB\n\nCurrent Price:\n{nft.Price} PB\n\nAmount available:\n{nft.Amount}\n\n";
        buyInfo1.text = txt;
        txt = $"Description:\n{nft.Description}\n\nFanciness: {nft.Fanciness}\n\nRarity: ";
        switch (nft.Rarity)
        {
            case 0:
                txt += "Common";
                break;
            case 1:
                txt += "Uncommon";
                break;
            case 2:
                txt += "Rare";
                break;
            case 3:
                txt += "Double Rare";
                break;
            case 4:
                txt += "Ultra Rare";
                break;
            default:
                txt += "Common";
                break;
        }
        buyInfo2.text = txt;

        var icon = new GameObject();
        icon.AddComponent<Image>().sprite = nft.Sprite;
        var iconObj = Instantiate(icon, PaymentConfirmCard.transform);
        iconObj.transform.localPosition = new Vector3(-1, 45, 0);
        iconObj.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(348, 348);

        PaymentConfirmPanel.SetActive(true);

        NftToBuy = nft;

    }

    /// <summary>
    /// Setup redirection logic on button trigger.
    /// </summary>
    /// <param name="clickedButton"></param>
    private void RedirectPage(Button clickedButton)
    {
        switch (clickedButton.name)
        {
            case ("ButtonCollection"):
                ActivatePanel("Collection_Panel");
                break;
            case ("ButtonBackto"):
                SceneManager.LoadScene("Home");
                break;
            case ("ButtonMarketplace"):
                ActivatePanel("Market_MainPanel");
                break;
            case ("ButtonPowerUp"):
                SetupPowerUp();
                ActivatePanel("PowerUp_Panel");
                break;
            case ("Button_more"):
                ActivatePanel("Collection_Panel");
                break;
            case ("ButtonYourNTFs"):
                SetupYourNFTsPanel();
                ActivatePanel("YourNFTs_Panel");
                break;
        }
    }

    /// <summary>
    /// Setup the panel showing user's nfts
    /// </summary>
    private async void SetupYourNFTsPanel()
    {
        int amountOwned;
        if (!isOwnedNftSetup)
        {
            foreach (var nft in nfts)
            {
                amountOwned = await nftService.GetOwnedNftAmount(userProfile.UserAddress, nft.Id);
                if (amountOwned > 0)
                {
                    var nftObj = Instantiate(Resources.Load<GameObject>("Prefabs/Nft"), YourNftInventory.transform);

                    SetupOwnedNftCard(nftObj, nft, amountOwned);
                }
            }
            isOwnedNftSetup = true;
        }

    }

    /// <summary>
    /// Handle logic of panels activation.
    /// </summary>
    /// <param name="panelName"></param>
    private void ActivatePanel(string panelName)
    {
        // Deactivate current panel, if exists
        if (currentPanel != null && currentPanel.name != panelName)
        {
            currentPanel.SetActive(false);
        }

        foreach (var panel in Panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(true);
                currentPanel = panel; // Update current panel
            }
            else
            {
                panel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Setup power up panel with user info, price etc...
    /// </summary>
    private void SetupPowerUp()
    {
        amethyst_owned.text = userProfile.AmethystNumber.ToString();
        grimoire_owned.text = userProfile.GrimoireNumber.ToString();
        potion_owned.text = userProfile.PotionNumber.ToString();
        amethyst_price.text = Utilities.AMETHYST_PRICE.ToString() + " PB";
        grimoire_price.text = Utilities.GRIMOIRE_PRICE.ToString() + " PB";
        potion_price.text = Utilities.POTION_PRICE.ToString() + " PB";
    }

    /// <summary>
    /// Setup purchase panel for selected power-up
    /// </summary>
    /// <param name="id"></param>
    public void BuyPowerUp(int id)
    {
        idPowerUp = id;
        int price, totalOwned;
        string type;
        var icon = new GameObject();
        if (id == 0)
        {
            if (userProfile.AmethystNumber >= 3)
            {
                JavascriptRepository.Web3Alert("Cannot buy more than 3 Amethysts!", "warning");
                return;
            }
            price = Utilities.AMETHYST_PRICE;
            totalOwned = userProfile.AmethystNumber;
            type = "Time Amethyst";
            icon.AddComponent<Image>().sprite = Resources.Load<Sprite>("Images/Common/Amethyst");

        }
        else if (id == 1)
        {
            if (userProfile.GrimoireNumber >= 3)
            {
                JavascriptRepository.Web3Alert("Cannot buy more than 3 Grimoires!", "warning");
                return;
            }
            type = "Hint Grimoire";
            price = Utilities.GRIMOIRE_PRICE;
            totalOwned = userProfile.GrimoireNumber;
            icon.AddComponent<Image>().sprite = Resources.Load<Sprite>("Images/Common/Grimoire");

        }
        else
        {
            if (userProfile.PotionNumber >= 3)
            {
                JavascriptRepository.Web3Alert("Cannot buy more than 3 Potions!", "warning");
                return;
            }
            type = "Potion x5";
            price = Utilities.POTION_PRICE;
            totalOwned = userProfile.PotionNumber;
            icon.AddComponent<Image>().sprite = Resources.Load<Sprite>("Images/Common/MagicPotion");

        }
        buyInfo1.text = $"{type}\n\n\nTemporary Balance:\n{userProfile.SecondaryBalance} PB\n\nCurrent Price:\n{price} PB\n\nUnits owned:\n{totalOwned} (max 3 allowed)";

        var iconObj = Instantiate(icon, PaymentConfirmCard.transform);
        iconObj.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(270, 250);

        PaymentConfirmPanel.SetActive(true);
    }

    /// <summary>
    /// Cancel purchase panel, rolling back modification
    /// </summary>
    public void CancelConfirmBuy()
    {
        buyInfo1.text = string.Empty;
        buyInfo2.text = string.Empty;
        idPowerUp = -1;
        NftToBuy = null;
        PaymentConfirmPanel.SetActive(false);
        DestroyImmediate(PaymentConfirmCard.transform.GetChild(0).gameObject);
    }

    /// <summary>
    /// Purchase and update profile
    /// </summary>
    public async void Purchase()
    {
        if (idPowerUp >= 0)
        {
            switch (idPowerUp)
            {
                case 0:
                    if (userProfile.SecondaryBalance >= Utilities.AMETHYST_PRICE)
                    {
                        userProfile.SecondaryBalance -= Utilities.AMETHYST_PRICE;
                        userProfile.AmethystNumber++;
                    }
                    else
                    {
                        JavascriptRepository.Web3Alert("Not enough coins!", "warning");
                        return;
                    }
                    break;
                case 1:
                    if (userProfile.SecondaryBalance >= Utilities.GRIMOIRE_PRICE)
                    {
                        userProfile.SecondaryBalance -= Utilities.GRIMOIRE_PRICE;
                        userProfile.GrimoireNumber++;
                    }
                    else
                    {
                        JavascriptRepository.Web3Alert("Not enough coins!", "warning");
                        return;
                    }
                    break;
                case 2:
                    if (userProfile.SecondaryBalance >= Utilities.POTION_PRICE)
                    {
                        userProfile.SecondaryBalance -= Utilities.POTION_PRICE;
                        userProfile.PotionNumber++;
                    }
                    else
                    {
                        JavascriptRepository.Web3Alert("Not enough coins!", "warning");
                        return;
                    }
                    break;

            }
        }
        else if (NftToBuy != null)
        {
            if (userProfile.PrimaryBalance >= NftToBuy.Price)
            {
                userProfile.PrimaryBalance -= NftToBuy.Price;
            }
            else
            {
                JavascriptRepository.Web3Alert("Not enough coins!", "warning");
                return;
            }
        }
        await nftService.NftTransfer(NftToBuy.Id, 1, userProfile, "PURCHASE_NFT");
        NftToBuy = null;
        idPowerUp = -1;
    }
}


