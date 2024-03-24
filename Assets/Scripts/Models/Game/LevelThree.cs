using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class LevelThree : MonoBehaviour
{

    private int difficulty;
    private int grimoireCounter = 0;
    public GameObject root;
    public GameObject targetLeaf;
    public GameObject blocks;
    public TextMeshProUGUI timerText;
    private List<List<int>> attestationsMap = new List<List<int>>();
    private int blockIndex = 0;
    private List<List<int>> solutions = new List<List<int>>();
    public GameObject particlePrefab;

    /// <summary>
    /// Setup level three content.
    /// </summary>
    public void Start()
    {
        difficulty = PlayerPrefs.GetInt("Difficulty");

        attestationsMap.Add(new List<int> { -1, 8, 3, 8, -1, 2, 1, 6, 1 });
        attestationsMap.Add(new List<int> { 8, 1, 3, -1, 7, -1, 2, -1, 5 });
        attestationsMap.Add(new List<int> { -1, 8, 3, -1, -1, -1, -1, 8, 1 });

        solutions.Add(new List<int> { 0, 3 });
        solutions.Add(new List<int> { 4, 2, 1 });
        solutions.Add(new List<int> { 0, 1, 4, 3, 2 });

        SetupTree(root);

    }

    /// <summary>
    /// Build level tree recursively starting from root game object.
    /// </summary>
    /// <param name="root"></param>
    private void SetupTree(GameObject root)
    {
        int numberOfChildren = root.transform.childCount;

        if (root.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.ToLower() != "Root".ToLower())
        {
            if (attestationsMap[difficulty][blockIndex] > 0)
            {
                root.tag = "Untagged";
                root.transform.GetChild(0).GetComponent<TextMeshProUGUI>().enabled = true;
            }
            else
            {
                root.tag = "Available";
            }
            root.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = attestationsMap[difficulty][blockIndex++].ToString();

        }

        if (numberOfChildren > 2)
        {
            SetupTree(root.transform.GetChild(1).gameObject);
            SetupTree(root.transform.GetChild(2).gameObject);

        }
        else if (numberOfChildren > 1)
        {
            SetupTree(root.transform.GetChild(1).gameObject);
        }
    }

    /// <summary>
    /// Set behaviour on grimoire use. Evidentiate one suggested couple slot-block.
    /// </summary>
    public void OnGrimoire()
    {

        var grimoireNumber = PlayerPrefs.GetInt("GrimoireNumber");

        //TODO: check number of grimoire
        if (grimoireNumber > 0)
        {
            if ((difficulty == Utilities.EASY && grimoireCounter < 2) || difficulty == Utilities.MEDIUM || difficulty == Utilities.HARD)
            {

                var slotList = GameObject.FindGameObjectsWithTag("Available").Concat(GameObject.FindGameObjectsWithTag("Busy")).OrderBy(x => Variables.Object(x).Get("id")).ToList();
                var correctBlocks = solutions[difficulty].Select(x => blocks.transform.GetChild(x).gameObject).ToList();
                var finalList = slotList.Zip(correctBlocks, (slot, block) => new Tuple<GameObject, GameObject>(slot, block)).ToList();

                if (grimoireCounter < finalList.Count)
                {
                    var pair = finalList[grimoireCounter];

                    float scale = 0.5f;

                    var colorList = new List<GradientColorKey[]>() {
                    new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
                    new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.gray, 1.0f) },
                    new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.magenta, 1.0f) },
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.black, 1.0f) },
                    new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.cyan, 1.0f) }
                };

                    CreateParticleSystem(particlePrefab, pair.Item1.transform, scale, colorList[grimoireCounter]);
                    CreateParticleSystem(particlePrefab, pair.Item2.transform, scale, colorList[grimoireCounter]);


                    grimoireCounter++;
                    Debug.Log("Grimoire used");

                    grimoireNumber--;
                    PlayerPrefs.SetInt("GrimoireNumber", grimoireNumber);
                    //TODO: decrease number of grimoire on chain
                }
            }
        }
        else
        {
            JavascriptRepository.Web3Alert("Not enough Grimoires!", "warning");
        }



    }

    /// <summary>
    /// Create a particle system with variable color to point out an object.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <param name="scale"></param>
    /// <param name="colors"></param>
    private void CreateParticleSystem(GameObject prefab, Transform parent, float scale, GradientColorKey[] colors)
    {
        var particleComponent = Instantiate(prefab, parent);
        particleComponent.layer = 5;

        ParticleSystem.MainModule mainModule = particleComponent.GetComponent<ParticleSystem>().main;
        mainModule.scalingMode = ParticleSystemScalingMode.Local;

        particleComponent.transform.localScale = new Vector3(scale, scale, scale);

        for (int i = 0; i < 4; i++)
        {
            particleComponent.transform.GetChild(i).gameObject.layer = 5;
            particleComponent.transform.GetChild(i).GetComponent<ParticleSystemRenderer>().sortingOrder = 1;
            particleComponent.transform.GetChild(i).GetComponent<ParticleSystemRenderer>().sortingLayerName = "UI";

            mainModule = particleComponent.transform.GetChild(i).GetComponent<ParticleSystem>().main;
            mainModule.scalingMode = ParticleSystemScalingMode.Local;

            particleComponent.transform.GetChild(i).transform.localScale = new Vector3(scale, scale, scale);

            if (i > 0)
            {
                Gradient grad = new Gradient();
                grad.SetKeys(colors, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
                var colParent = particleComponent.transform.GetChild(i).GetComponent<ParticleSystem>().colorOverLifetime;
                colParent.color = grad;

                var col = particleComponent.transform.GetChild(i).transform.GetChild(0).GetComponent<ParticleSystem>().colorOverLifetime;
                col.color = grad;

                particleComponent.transform.GetChild(i).transform.GetChild(0).gameObject.layer = 5;
                particleComponent.transform.GetChild(i).transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sortingOrder = 1;
                particleComponent.transform.GetChild(i).transform.GetChild(0).GetComponent<ParticleSystemRenderer>().sortingLayerName = "UI";

                mainModule = particleComponent.transform.GetChild(i).transform.GetChild(0).GetComponent<ParticleSystem>().main;
                mainModule.scalingMode = ParticleSystemScalingMode.Local;

                particleComponent.transform.GetChild(i).transform.GetChild(0).transform.localScale = new Vector3(scale, scale, scale);

            }
        }
    }

    /// <summary>
    /// Set behaviour of Confirm button. Set game status according to validation.
    /// </summary>
    public void OnConfirm()
    {
        var slotsArray = GameObject.FindGameObjectsWithTag("Available");
        if (slotsArray.Any())
        {
            timerText.gameObject.tag = "Loose";
        }
        else
        {
            GameObject leaf = ValidateTree(root);

            if (targetLeaf == leaf)
            {
                timerText.gameObject.tag = "Win";
            }
            else
            {
                timerText.gameObject.tag = "Loose";
            }

        }

    }

    /// <summary>
    /// Recursively validate tree according to LMD-GHOST like algorhythm
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    private GameObject ValidateTree(GameObject root)
    {
        int numberOfChildren = root.transform.childCount;

        if (root.transform.GetChild(numberOfChildren - 1).GetComponent<ParticleSystem>() != null)
        {
            numberOfChildren--;
        }

        if (numberOfChildren > 2)
        {
            return Int32.Parse(root.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text)
                >= Int32.Parse(root.transform.GetChild(2).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text) ?
                ValidateTree(root.transform.GetChild(1).gameObject) : ValidateTree(root.transform.GetChild(2).gameObject);

        }
        else if (numberOfChildren > 1)
        {
            return ValidateTree(root.transform.GetChild(1).gameObject);
        }
        else
        {
            return root;
        }
    }


}
