using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class LevelTwo : MonoBehaviour
{
    public TextMeshProUGUI transactionCounterText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI inputs;
    public TextMeshProUGUI signatures;
    public TextMeshProUGUI outputs;
    public TextMeshProUGUI ids;
    public Image user1;
    public Image user2;
    public Image grimoirePanel;
    private int transactionIndex = 0;
    private List<int> maxTransactions = new List<int>();
    private int difficulty;
    private List<List<List<List<(string, int)>>>> transactionMap = new List<List<List<List<(string, int)>>>>();
    private List<List<bool>> validationMap = new List<List<bool>>();

    /// <summary>
    /// Initialize level two on screen according to the difficulty
    /// </summary>
    public void Start()
    {
        difficulty = PlayerPrefs.GetInt("Difficulty");

        maxTransactions.AddRange(new int[] { 2, 3, 5 });

        transactionCounterText.text = "1/" + maxTransactions[difficulty];
        grimoirePanel.gameObject.SetActive(false);

        InitializeTransactions();
        NextTransaction();

    }

    /// <summary>
    /// Set behaviour of Valid button. Set game status according to validation map. If the last transaction get correctly validated set status to "Win", setup next transaction otherwise.
    /// </summary>
    public void OnValid()
    {
        if (validationMap[difficulty][transactionIndex - 1])
        {
            if (transactionIndex + 1 > maxTransactions[difficulty])
            {
                timerText.gameObject.tag = "Win";
            }
            else
            {
                NextTransaction();
            }
        }
        else
        {
            timerText.gameObject.tag = "Loose";
        }
    }

    /// <summary>
    /// Set behaviour of NotValid button. Set game status according to validation map. If the last transaction get correctly validated set status to "Win", setup next transaction otherwise.
    /// </summary>
    public void OnNotValid()
    {
        if (!validationMap[difficulty][transactionIndex - 1])
        {
            if (transactionIndex + 1 > maxTransactions[difficulty])
            {
                timerText.gameObject.tag = "Win";
            }
            else
            {
                NextTransaction();
            }
        }
        else
        {
            timerText.gameObject.tag = "Loose";
        }
    }

    /// <summary>
    /// Setup next transaction on screen
    /// </summary>
    private void NextTransaction()
    {
        grimoirePanel.gameObject.SetActive(false);

        GameObject.FindGameObjectsWithTag("Clone").ToList().ForEach(x => Destroy(x));

        var data0 = transactionMap[difficulty][transactionIndex][0];
        var data1 = transactionMap[difficulty][transactionIndex][1];
        var data2 = transactionMap[difficulty][transactionIndex][2];
        var data3 = transactionMap[difficulty][transactionIndex][3];

        var currentInputObj = inputs.transform.GetChild(0);
        int index = 0;
        foreach (var input in data0)
        {
            if (index > 0)
            {
                currentInputObj = GameObject.Instantiate(currentInputObj, currentInputObj.transform.parent);
                currentInputObj.transform.localPosition += new Vector3(0, -50, 0);
                currentInputObj.gameObject.tag = "Clone";

            }
            currentInputObj.GetComponent<TextMeshProUGUI>().text = input.Item1;
            index++;
        }

        var currentSignImageObj = signatures.transform.GetChild(0);
        var currentSignTextObj = signatures.transform.GetChild(1);
        index = 0;
        foreach (var sign in data1)
        {
            if (index > 0)
            {

                currentSignImageObj = GameObject.Instantiate(currentSignImageObj, currentSignImageObj.transform.parent);
                currentSignImageObj.transform.localPosition += new Vector3(0, -50, 0);
                currentSignImageObj.gameObject.tag = "Clone";


                currentSignTextObj = GameObject.Instantiate(currentSignTextObj, currentSignTextObj.transform.parent);
                currentSignTextObj.transform.localPosition += new Vector3(0, -50, 0);
                currentSignTextObj.gameObject.tag = "Clone";


            }
            currentSignTextObj.GetComponent<TextMeshProUGUI>().text = sign.Item1;
            if (sign.Item2 <= 0)
            {
                currentSignImageObj.gameObject.SetActive(false);
            }
            index++;
        }



        var currentOutputObj = outputs.transform.GetChild(0);
        index = 0;
        foreach (var outData in data2)
        {
            if (index > 0)
            {
                currentOutputObj = GameObject.Instantiate(currentOutputObj, currentOutputObj.transform.parent);
                currentOutputObj.transform.localPosition += new Vector3(0, -50, 0);
                currentOutputObj.gameObject.tag = "Clone";

            }
            currentOutputObj.GetComponent<TextMeshProUGUI>().text = outData.Item1;
            index++;
        }

        var currentIdObj = ids.transform.GetChild(0);
        index = 0;
        foreach (var id in data3)
        {
            if (index > 0)
            {
                currentIdObj = GameObject.Instantiate(currentIdObj, currentIdObj.transform.parent);
                currentIdObj.transform.localPosition += new Vector3(0, -50, 0);
                currentIdObj.gameObject.tag = "Clone";

            }
            currentIdObj.GetComponent<TextMeshProUGUI>().text = id.Item1;
            index++;
        }
        transactionIndex++;
        transactionCounterText.text = transactionIndex + "/" + maxTransactions[difficulty];

    }

    /// <summary>
    /// Setup transaction content
    /// </summary>
    private void InitializeTransactions()
    {
        validationMap.Add(new List<bool> { true, false });

        transactionMap.Add(new List<List<List<(string, int)>>> {
            new List<List<(string, int)>> {
                new List<(string, int)> { ("0x3354 (2)", 0), ("0xa932 (4)", 0)},
                new List<(string, int)> { ("[2]", 1), ("[3]", 1)},
                new List<(string, int)> { ("VerdaneBlack [4]", 0), ("CloudPunk [0.7]", 0)},
                new List<(string, int)> { ("0x123f", 0) },
                new List<(string, int)> { ("Everything looks correct!", 0) }
                },

            new List<List<(string, int)>> {
                new List<(string, int)> { ("0x3af4 (2)", 0), ("0xcc32 (4)", 0)},
                new List<(string, int)> { ("[2]", 1), ("[3]", 0)},
                new List<(string, int)> { ("VerdaneBlack [4]", 0), ("CloudPunk [0.7]", 0)},
                new List<(string, int)> { ("0x153f", 0) },
                new List<(string, int)> { ("Look carefully at the signatures...", 0) }
                }
            }
        );

        validationMap.Add(new List<bool> { true, true, false });

        transactionMap.Add(new List<List<List<(string, int)>>> {
            new List<List<(string, int)>> {
                new List<(string, int)> { ("0xabcd (2)", 0), ("0xa932 (0)", 0)},
                new List<(string, int)> { ("[2]", 1), ("[3]", 1)},
                new List<(string, int)> { ("VerdaneBlack [4]", 0), ("CloudPunk [0.7]", 0)},
                new List<(string, int)> { ("0x123f", 0) },
                new List<(string, int)> { ("Everything looks correct!", 0) }
                },

            new List<List<(string, int)>> {
                new List<(string, int)> { ("0xfaf44 (1)", 0), ("0xc232f (3)", 0)},
                new List<(string, int)> { ("[2]", 1), ("[3]", 1)},
                new List<(string, int)> { ("VerdaneBlack [4.5]", 0), ("CloudPunk [0.1]", 0)},
                new List<(string, int)> { ("0x5555", 0) },
                new List<(string, int)> { ("Everything looks correct!", 0) }
                },

            new List<List<(string, int)>> {
                new List<(string, int)> { ("0xabcd (2)", 0), ("0xa33bb (1)", 0)},
                new List<(string, int)> { ("[2]", 1), ("[3]", 1)},
                new List<(string, int)> { ("VerdaneBlack [7]", 0), ("CloudPunk [0.7]", 0)},
                new List<(string, int)> { ("0xeeff", 0) },
                new List<(string, int)> { ("Look carefully at the amount spent...", 0) }
                }
            }
        );

        validationMap.Add(new List<bool> { true, true, false, false, false });

        transactionMap.Add(new List<List<List<(string, int)>>> {
            new List<List<(string, int)>> {
                new List<(string, int)> { ("0xabcd (2)", 0), ("0xa932 (0)", 0)},
                new List<(string, int)> { ("[4]", 1), ("[3]", 1)},
                new List<(string, int)> { ("VerdaneBlack [5]", 0), ("CloudPunk [1]", 0)},
                new List<(string, int)> { ("0x1234", 0) },
                new List<(string, int)> { ("Everything looks correct!", 0) }
                },

            new List<List<(string, int)>> {
                new List<(string, int)> { ("0x1234 (0)", 0), ("0xc232f (3)", 0)},
                new List<(string, int)> { ("[1]", 1), ("[3]", 1)},
                new List<(string, int)> { ("VerdaneBlack [3]", 0)},
                new List<(string, int)> { ("0x153f", 0) },
                new List<(string, int)> { ("Everything looks correct!", 0) }
                },

            new List<List<(string, int)>> {
                new List<(string, int)> { ("0xabcd (2)", 0), ("0xa33bb (1)", 0)},
                new List<(string, int)> { ("[2]", 1), ("[3]", 1)},
                new List<(string, int)> { ("VerdaneBlack [4]", 0), ("CloudPunk [0.7]", 0)},
                new List<(string, int)> { ("", 0) },
                new List<(string, int)> { ("Look carefully at the id...", 0) }
                },

            new List<List<(string, int)>> {
                new List<(string, int)> { ("0xabcd (2)", 0), ("0xa33bb (1)", 0)},
                new List<(string, int)> { ("[4]", 1), ("[5]", 1)},
                new List<(string, int)> { ("VerdaneBlack [3]", 0), ("CloudPunk [3]", 0)},
                new List<(string, int)> { ("0xeeff", 0) },
                new List<(string, int)> { ("Look carefully at the input addresses...", 0) }
                },

            new List<List<(string, int)>> {
                new List<(string, int)> { ("0x65d8d (2)", 0), ("0xeeff(1)", 0)},
                new List<(string, int)> { ("[2]", 1), ("[3]", 1)},
                new List<(string, int)> { ("VerdaneBlack [5.5]", 0), ("CloudPunk [2]", 0)},
                new List<(string, int)> { ("0xe34f", 0) },
                new List<(string, int)> { ("Look carefully at the amount spent...", 0) }
                }
            }
        );
    }

    /// <summary>
    /// Set behaviour on grimoire usage. Check availability and decrease owned grimoire number.
    /// Show hint on screen
    /// </summary>
    public void OnGrimoire()
    {
        var grimoireNumber = PlayerPrefs.GetInt("GrimoireNumber");


        if (grimoireNumber > 0)
        {


            grimoirePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = transactionMap[difficulty][transactionIndex - 1][4][0].Item1;

            grimoireNumber--;
            PlayerPrefs.SetInt("GrimoireNumber", grimoireNumber);

            grimoirePanel.gameObject.SetActive(true);
            Debug.Log("Used Level 2 Grimoire");

        }
        else
        {
            JavascriptRepository.Web3Alert("Not enough Grimoires!", "warning");

        }



    }

}
