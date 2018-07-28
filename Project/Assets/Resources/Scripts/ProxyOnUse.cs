using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxyOnUse : MonoBehaviour {

    public bool singleUse = false;
    internal bool used = false;
    public AudioClip useSound;

    public Transform proxyTarget;

    void Start()
    {
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
    }
    private void OnDestroy()
    {
        FFMessageBoard<PlayerInteract.Use>.Disconnect(OnUse, gameObject);
    }

    private int OnUse(PlayerInteract.Use e)
    {
        if (singleUse && used)
            return 0;

        if (singleUse)
            used = true;


        // Play effects
        if (GetComponent<AudioSource>() != null && useSound != null) GetComponent<AudioSource>().PlayOneShot(useSound);

        // Send out event to player
        return FFMessageBoard<PlayerInteract.Use>.Send(e, proxyTarget.gameObject);
    }
}
