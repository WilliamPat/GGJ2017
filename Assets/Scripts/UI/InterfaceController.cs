﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TableauComponent
{
    public Tableau weaponTab;
    public Tableau targetTab;
    public Tableau movementTab;
}

public class InterfaceController : MonoBehaviour {
    public static InterfaceController instance;

    public GameObject panelPause;
    public GameObject panelEnd;
    public Text endGameText;
    public GameObject panelChoice;
    public GameObject waveRestartBtn;
    public TableauComponent tables;
    public Text currentMoney;
    public Text currentWave;
    public Slider lifeBar;

    [Header("Wave Bonus")]
    public Text bonusLife;
    public Text bonusDamage;

    float realMoney = 0;

    void Awake () {
        instance = this;
        StartCoroutine(initialiseTable());
    }

    IEnumerator initialiseTable()
    {
        while(GameManager.instance == null)
        {
            yield return 0;
        }
        currentMoney.text = "" + GameManager.instance.money;
        currentWave.text = "" + GameManager.instance.wave;

        openChoice(true);
        int index = 0;
        // Weapon
        tables.weaponTab.gameObject.SetActive(true);
        tables.weaponTab.addElement("Weapon", "Cost","Choose your weapon",ComponentType.None, -1, false);
        foreach (var wp in GameManager.instance.weaponComponents)
        {
            tables.weaponTab.addElement(wp.componentName, "" + wp.cost, wp.tooltip, ComponentType.Weapon, index);
            index++;
        }
        index = 0;
        // Target
        tables.targetTab.gameObject.SetActive(true);
        tables.targetTab.addElement("Target Priority", "Cost", "Target priority", ComponentType.None, -1, false);
        foreach (var tr in GameManager.instance.targetComponents)
        {
            tables.targetTab.addElement(tr.componentName, "" + tr.cost,tr.tooltip, ComponentType.Target, index);
            index++;
        }
        index = 0;
        // Movement
        tables.movementTab.gameObject.SetActive(true);
        tables.movementTab.addElement("Movement", "Cost", "How to move", ComponentType.None, -1, false);
        foreach (var mv in GameManager.instance.moveComponents)
        {
            tables.movementTab.addElement(mv.componentName, "" + mv.cost,mv.tooltip, ComponentType.Movement, index);
            index++;
        }
        //openChoice(false);
    }

    public void Pause(bool value)
    {
        if (value)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
        panelPause.SetActive(value);
    }

    public void loadMenu()
    {
        Time.timeScale = 1;
        SceneLoader.loadSceneName("Menu");
    }

    public void endGame(bool isWin)
    {
        Time.timeScale = 0;
        panelEnd.SetActive(true);
        if (isWin)
        {
            endGameText.text = "You Win";
        }
        else
        {
            endGameText.text = "Game Over";
        }
    }

    public void restartGame()
    {
        Time.timeScale = 1;
        SceneLoader.loadSceneIndex(SceneLoader.activeSceneIndex());
    }

    public void openChoice(bool value)
    {
        if (value)
        {
            Time.timeScale = 0;
            currentWave.text = "" + GameManager.instance.wave;
        }
        else
        {
            GameManager.instance.Reset();
            Time.timeScale = 1;
        }
        lifeBar.transform.parent.gameObject.SetActive(!value);
        waveRestartBtn.SetActive(!value);
        panelChoice.SetActive(value);
        UpdateMoney();
    }

    public void UpdateMoney()
    {
        realMoney = GameManager.instance.money;
        if (GameManager.selectedMovement != null) realMoney -= GameManager.selectedMovement.cost;
        if (GameManager.selectedTarget != null) realMoney -= GameManager.selectedTarget.cost;
        if (GameManager.selectedWeapon != null) realMoney -= GameManager.selectedWeapon.cost;

        currentMoney.text = "" + realMoney;
        if(realMoney < 0)
        {
            currentMoney.color = Color.red;
        }
        else
        {
            currentMoney.color = Color.white;
        }
    }

    public void isReady()
    {
        if(realMoney >= 0)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().init();
            openChoice(false);
            IAManager.instance.startWave();
        }
    }

    public void restartWave()
    {
        IAManager.instance.Restart();
        GameManager.instance.Reset();
        UpdateMoney();
        openChoice(true);
    }

    public void UpdateLife(float currentLife, float maxlife)
    {
        lifeBar.value = currentLife / maxlife;
    }

    public void Heal()
    {
        Destroyable dest = GameManager.player.GetComponent<Destroyable>();
        if(dest && dest.life != dest.maxLife && GameManager.instance.money > 0)
        {
            GameManager.instance.money--;
            UpdateMoney();
            dest.reset();
            UpdateLife(dest.life, dest.maxLife);
        }
    }

    public void switchTooltip(bool value)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Tile");
        foreach (var tile in objs)
        {
            if(tile.GetComponent<TooltipTrigger>())
            {
                tile.GetComponent<TooltipTrigger>().enabled = value;
            }
        }
    }

    public void updateWaveBonus(float life, float dmg)
    {
        bonusLife.text = "" + life;
        bonusDamage.text = "" + dmg;
    }
}
