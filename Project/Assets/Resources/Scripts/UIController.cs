using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class UIController : MonoBehaviour {

    public Transform playerRoot;
    public UnityEngine.UI.Text coinText;
    public Transform KeyObject;
    public Transform PowerObject;

    
    public Inventory inventory;

	// Use this for initialization
	void Start ()
    {

        FFMessageBoard<GiveCoins>.Connect(OnGiveCoin, gameObject);
        FFMessageBoard<GiveObject>.Connect(OnGiveObject, gameObject);

        UpdateUI();
    }
    private void OnDestroy()
    {

        FFMessageBoard<GiveCoins>.Disconnect(OnGiveCoin, gameObject);
        FFMessageBoard<GiveObject>.Disconnect(OnGiveObject, gameObject);
    }

    private int OnGiveObject(GiveObject e)
    {
        // @Improve! @FIX @HACK
        if (e.ObjectName == "Key")
            inventory.hasKey = true;
        if (e.ObjectName == "PowerObject")
            inventory.hasPowerObject = true;


        UpdateObjects();
        return 0;
    }
    

    private int OnGiveCoin(GiveCoins e)
    {
        inventory.coinCount += e.coinCount;
        UpdateCoinText();
        return 0;
    }



    #region UpdateUI

    void UpdateUI()
    {
        UpdateObjects();
        UpdateCoinText();
    }

    private void UpdateObjects()
    {
        // @TEMP
        KeyObject.gameObject.SetActive(inventory.hasKey);
        PowerObject.gameObject.SetActive(inventory.hasPowerObject);
    }
    public void UpdateCoinText()
    {
        coinText.text = inventory.coinCount.ToString();
    }

#endregion

    // Update is called once per frame
    void Update () {
		
	}
}
