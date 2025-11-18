using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI moneyText;
    public int moneyCount;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] public int rewardAmount;

    public void Start()
    {
        moneyCount = PlayerPrefs.GetInt("moneyCount", 0);
        moneyText.text = moneyCount.ToString();
        rewardText.text = rewardAmount.ToString();
    }

    public void UpdateMoney(int amount)
    {
        moneyCount = amount;
        PlayerPrefs.SetInt("moneyCount", moneyCount);
        PlayerPrefs.Save();
        moneyText.text = moneyCount.ToString();
        
        if (PlayerDataManager.Instance != null)
        {
            _ = PlayerDataManager.Instance.UpdateMoney(moneyCount);
        }
    }

    public void AddMoney(int amount)
    {
        UpdateMoney(moneyCount + amount);
    }

    public void SpendMoney(int amount)
    {
        if (moneyCount >= amount)
        {
            UpdateMoney(moneyCount - amount);
        }
    }

}
