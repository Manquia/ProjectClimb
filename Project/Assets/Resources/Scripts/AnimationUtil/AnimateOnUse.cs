using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateOnUse : MonoBehaviour {

    public bool singleUse = true;
    bool used = false;
    public AudioClip animateSound;

    public bool needsKey = false;

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

        if (needsKey && e.actor.GetComponent<Inventory>().hasKey == false)
            return 0;
            

        if (singleUse)
            used = true;
        
        Animate();

        return 0;
    }
    

    public void Animate()
    {
        GetComponent<AudioSource>().PlayOneShot(animateSound);
        GetComponent<Animation>().Play();
    }
}
