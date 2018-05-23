using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PickupEvent
{
    public string name;

}

public class PickupOnUse : FFComponent {

    public bool singleUse = true;
    bool used = false;
    public AudioClip pickupSound;


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

        Pickup(e);
        return 1;
    }



    private void Pickup(PlayerInteract.Use e)
    {
        // Play effects
        if(GetComponent<AudioSource>() != null && pickupSound != null) GetComponent<AudioSource>().PlayOneShot(pickupSound);

        // Send out event to player
        PickupEvent pe;
        pe.name = gameObject.name;
        FFMessageBoard<PickupEvent>.SendAllConnected(pe, e.actor.gameObject);

        // Destroy self, @TODO Fancy pickup Animation?
        var seq = action.Sequence();
        float pickupTime = Mathf.Max(1.0f, pickupSound != null ? pickupSound.length : 1.0f);

        seq.Property(ffposition, e.actor.position, FFEase.E_SmoothStart, pickupTime);
        seq.Delay(pickupSound != null ? pickupSound.length : 1.0f);
        seq.Sync();
        seq.Call(DestroySelf);
    }

    
    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
