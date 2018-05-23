using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FFPrivate;
using System;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 10/9/2015
// Purpose: FFMessageBoard is a type-based interface for
//      entry based events which you may want to send/
//      connect/disconnect from. If you want to send
//      an event locally use SendToLocal. If you want
//      to send an event to the server/other clients
//      use SendToNet.
//
//      There is also locallity already added to 
//      game objects which allows an event to be
//      passed to a gameobject/gameobject + childeren/
//      gameobject + parents/gameobject + everything 
//      connected using SendTo(Local/Net)Down/Up/
//      AllConnected events.
//      
//      FFMessageBoard is an entry-typebased system
//      which uses a FFMessageBox for its entries.
//      These can be directly interfaced with via
//      Box(string entry) function which can then be
//      used to get specific entry boxes which you may
//      want to use/manipulate.
//
///////////////////////////////////////////////////////

namespace FFPrivate
{
    public struct MessageBoardInfo
    {
        public int boxCount;
        public int lookupCount;
        public string[] boxEntry;
        public int[] callCount;
        public int[] listenerCount;
        public int[] visitorCount;
    }

    public class BaseMessageBoard
    {
        // When false messages canont be sent
        protected static bool activeGlobal = true;
        // total calls since the begining of the program
        protected static int lookupCountGlobal = 0;

        // Total calls from any FFMessageBoard
        protected static int callCountGlobal = 0;
        // total listeners of this message
        protected static int listenerCountGlobal = 0;
        // total Visitors, or the number of events which are not disconnected
        protected static int visitorCountGlobal = 0;

        private const string idBase = "FFNO:";
        public static string LocalIdEntry(int instanceId)
        {
            return idBase + instanceId;
        }

        public void AllMessageBoardsActive(bool b)
        {
            Debug.Log("All NetMessageBoards Activity set to " + b);
            activeGlobal = b;
        }

        #region VirtualStubs
        virtual public bool SendToLocalEntry(FFBasePacket package, string entry)
        {
            Debug.LogError("Message Didn't override SentToLocalEntry function?!?");
            return false;
        }
        virtual public void SendToLocalGameObject(FFBasePacket package, GameObject go)
        {
            Debug.LogError("Message Didn't override SentToLocalGameObject function?!?");
        }
        virtual public MessageBoardInfo GetInfo()
        {
            Debug.LogError("Message Didn't override GetInfo function?!?");
            MessageBoardInfo info;
            info.callCount = null;
            info.listenerCount = null;
            info.visitorCount = null;
            info.boxEntry = null;
            info.lookupCount = 0;
            info.boxCount = 0;
            return info;
        }
        virtual public void Active(bool b)
        {
            Debug.LogError("Message Didn't override Active function?!?");
        }
        virtual public void ClearMessageBoard()
        {
            Debug.LogError("Message Didn't override ClearAll function?!?");
        }
        virtual public int BoxCount()
        {
            Debug.LogError("Message Didn't override BoxCount function?!?");
            return 0;
        }
        #endregion VirtualStubs
    }
}

//TODO add lookup counter to all interfaces

public class FFMessageBoard<EventType> : FFPrivate.BaseMessageBoard
{
    // This is a singleton which is only created when a box/GameObject is requested/connected
    private FFMessageBoard(){}
    // should also prevent garbage collections
    private static FFMessageBoard<EventType> messageBoardSystem = null;
    // Lookup Dictionary for the board's boxes
    private static Dictionary<string, FFMessageBox<EventType>> messageBoard = new Dictionary<string, FFMessageBox<EventType>>();
    // When false messages canont be sent
    private static bool activeLocal = true;
    // private Initialization function
    private static void GetReady()
    {
        if (messageBoardSystem == null)
        {
            messageBoardSystem = new FFMessageBoard<EventType>();
            FFPrivate.FFMessageSystem.AddMessageBoard(messageBoardSystem, typeof(EventType).ToString());
        }
    }

    /// <summary>
    /// Returns a reference to the message board which contains helper functions
    /// which are potentially volitile to the any other listeners (reference will be null if this MessageType has never
    /// been connected too.) should be limited to Debug and FFMessageSystem as it give access to
    /// several maintenance functions.
    /// </summary>
    public static FFMessageBoard<EventType> MessageBoard
    {
        get { return messageBoardSystem; }
    }

    #region Stats
    // total total lookups on this FFNetMessageBoard boxes
    private static int lookupCountLocal = 0;
    // Total calls from this FFNetMessageBoard
    private static int callCountLocal = 0;
    // total listeners for this FFNetMessageBoard
    private static int listenerCountLocal = 0;
    // total Visitors, or the number of events which are not disconnected of this FFNetMessageBoard
    private static int visitorCountLocal = 0;
    

    // Allows boxes to stat track for their boards if they are attached to one.
    public void IncrementCallCount()
    {
        ++callCountLocal;
        ++callCountGlobal;
    }
    public void IncrementVisitorCount()
    {
        ++visitorCountLocal;
        ++visitorCountGlobal;
    }
    public void IncrementListenerCount()
    {
        ++listenerCountLocal;
        ++listenerCountGlobal;
    }
    public void DecrementListenerCount()
    {
        --listenerCountLocal;
        --listenerCountGlobal;
    }
    #endregion

    #region Box Interface
    /// <summary>
    /// Returns an existing or new box for the given entry which can
    /// be used to connect/disconnect/send messages
    /// </summary>
    public static FFMessageBox<EventType> Box(string entry)
    {
        GetReady();
        ++lookupCountGlobal;
        ++lookupCountLocal;

        if (messageBoard.ContainsKey(entry))
        {
            var box = messageBoard[entry];
            box.Active(activeLocal && activeGlobal);
            return box;
        }
        else
        {
            var box = new FFMessageBox<EventType>(messageBoardSystem, entry);
            box.Active(activeLocal && activeGlobal);
            messageBoard.Add(entry, box);
            return box;
        }
    }
    public static FFMessageBox<EventType> Box(GameObject go)
    {
        string boxEntry = BaseMessageBoard.LocalIdEntry(go.GetInstanceID());
        FFMessageBox<EventType> box;
        if(messageBoard.TryGetValue(boxEntry, out box))
        {
            return box;
        }
        else
        {
            return null;
        }
    }
    #endregion
    
    #region QuickBoxEntry Interface
    /// <summary>
    /// Send to the message networked (to all other connected clients) to this board's sepecific box entry.
    /// </summary>
    private static void SendToNet(EventType message, string entry, bool varified = false)
    {
        Debug.Assert(false, "this codepath probably doens't work");

        if (activeGlobal && activeLocal)
        {
            FFMessageSystem.SendMessageToNet<EventType>(message, entry, varified);
        }
    }

    /// <summary>
    /// Send to the message locally (to this Unity Instance) to this board's sepecific box entry.
    /// </summary>
    public static bool SendToLocal(EventType message, string entry)
    {
        FFMessageBox<EventType> box;
        if (activeGlobal && activeLocal && messageBoard.TryGetValue(entry, out box))
        {
            box.SendToLocal(message);
            return true;
        }
        return false;
    }
    #endregion

    #region GameObject Interface
    // Note: In order for the GameObject Interface to work
    // Any object which want to recieve events from the net
    // much call the FFSystem.RegisterNetGameObject(gameObject)
    // in order for their object to recieve events from other clients.
    // RegisterNetGameObject cannot be called in: non-main Thread,
    // Component Constructor, Awake (maybe, TODO test) TODO Make this actually work
    
    /// <summary>
    /// Send the message locally (to this Unity Instance) to the go (GameObject)
    /// </summary>
    public static int Send(EventType message, GameObject go, int consumeQuantity = 1)
    {
        if (consumeQuantity <= 0)
            return consumeQuantity;

        string boxEntry = BaseMessageBoard.LocalIdEntry(go.GetInstanceID());
        FFMessageBox<EventType> box;

        if (messageBoard.TryGetValue(boxEntry, out box))
        {
            return box.SendToLocal(message, consumeQuantity);
        }
        else
        {
            return consumeQuantity;
        }
        // @TODO @NET if a networked object, we should send the message to the net as well...
        //FFMessageSystem.SendMessageToNet<EventType>(message, go.GetInstanceID(),
        //    FFPacketInstructionFlags.MessageBoardGameObjectSend, varifiedPacket);
    }

    public static int Send<AComp>(EventType message, AComp[] objects, bool randomize = false, int consumeQuantity = 1) where AComp : Component
    {
        if(randomize)
        {
            objects.Randomize();
        }

        int objCount = objects.Length;
        for (int i = 0; i < objCount && consumeQuantity > 0; ++i)
        {
            consumeQuantity = Send(message, objects[i].gameObject, consumeQuantity);
        }
        return consumeQuantity;
    }
    public static int Send(EventType message, GameObject[] gos, bool randomize = false, int consumeQuantity = 1)
    {
        if (randomize)
        {
            gos.Randomize();
        }

        int objCount = gos.Length;
        for (int i = 0; i < objCount && consumeQuantity > 0; ++i)
        {
            consumeQuantity = Send(message, gos[i].gameObject, consumeQuantity);
        }
        return consumeQuantity;
    }

    public static int SendSpacial(EventType message, Vector3 center, float radius, int mask, bool randomize = false, int consumeQuantity = 1)
    {
        if (consumeQuantity <= 0)
            return consumeQuantity;

        var res = Physics.OverlapSphere(center, radius, mask);
        
        Send(message, res, randomize, consumeQuantity);
        return consumeQuantity;
    }
    public static int SendSpecial(EventType message, Vector3 center, Vector3 halfBoxExtends, Quaternion boxOrientation, int mask, bool randomize = false, int consumeQuantity = 1)
    {
        if (consumeQuantity <= 0)
            return consumeQuantity;

        var res = Physics.OverlapBox(center, halfBoxExtends, boxOrientation, mask);
        
        Send(message, res, randomize, consumeQuantity);

        return consumeQuantity;
    }

    /// <summary>
    /// Sends the message locally (to this Unity Instance) to all parents of go (GameObject) from bottom to top (go included)
    /// </summary>
    public static int SendUp(EventType message, GameObject go, int consumeQuantity = 1)
    {
        if (consumeQuantity <= 0)
            return consumeQuantity;

        Transform trans = go.transform;
        do
        {
            consumeQuantity = Send(message, trans.gameObject, consumeQuantity);
        } while ((trans = trans.parent) != null && consumeQuantity > 0);
        return consumeQuantity;
    }
    /// <summary>
    /// Sends the message locally (to this Unity Instance) to all childern of go (GameObject) from top to bottom (go included)
    /// </summary>
    public static int SendDown(EventType message, GameObject go, int consumeQuantity = 1)
    {
        if (consumeQuantity <= 0)
            return consumeQuantity;

        consumeQuantity = Send(message, go.gameObject, consumeQuantity);
        foreach (Transform child in go.transform)
        {
            if (consumeQuantity <= 0) break;
            consumeQuantity = SendDown(message, child.gameObject, consumeQuantity);
        }
        return consumeQuantity;
    }
    /// <summary>
    /// Sends the message locally (to this Unity Instance) to every parent and child reachable by the game object's transform (go included)
    /// </summary>
    public static int SendAllConnected(EventType message, GameObject go, int consumeQuantity = 1)
    {
        if (consumeQuantity <= 0)
            return consumeQuantity;

        // Get root object of cluster
        Transform trans = go.transform;
        while (trans.parent != null)
            trans = trans.parent;

        // Send down
        return SendDown(message, trans.gameObject, consumeQuantity);
    }
    
    /// <summary>
    /// Send objects ordered by their relative distances from a given world point
    /// </summary>
    public static int SendOrdered(EventType message, Transform[] objects, Vector3 fromPosition, FF.SendOrder_PT order, int consumeQuantity = 1)
    {
        if (consumeQuantity <= 0)
            return consumeQuantity;
        
        // Apply ordering to given objects
        switch (order)
        {
            case FF.SendOrder_PT.Nearest:
                // Sort with Array.Sort (ie quick sort)
                Array.Sort<Transform>(objects, delegate (Transform obj1, Transform obj2)
                {
                    float dist1 = (obj1.position - fromPosition).sqrMagnitude;
                    float dist2 = (obj2.position - fromPosition).sqrMagnitude;
                    return Math.Sign(dist1 - dist2);
                });
                break;
            case FF.SendOrder_PT.Furthest:
                // Sort with Array.Sort (ie quick sort)
                Array.Sort<Transform>(objects, delegate (Transform obj1, Transform obj2)
                {
                    float dist1 = (obj1.position - fromPosition).sqrMagnitude;
                    float dist2 = (obj2.position - fromPosition).sqrMagnitude;
                    return -Math.Sign(dist1 - dist2);
                });
                break;
        }

        // Send out events to objects
        return Send(message, objects, false, consumeQuantity);
    }
    /// <summary>
    /// Sends the message to the objects by their relative axis values.
    /// </summary>
    public static int SendOrdered(EventType message, Transform[] objects, FF.SendOrder_Axis order, int consumeQuantity = 1)
    {
        if (consumeQuantity <= 0)
            return consumeQuantity;

        // Apply ordering to given objects
        switch (order)
        {
            case FF.SendOrder_Axis.PosX:
                // Sort with Array.Sort (ie quick sort)
                Array.Sort<Transform>(objects, delegate (Transform obj1, Transform obj2)
                {
                    float x1 = obj1.position.x;
                    float x2 = obj2.position.x;
                    return Math.Sign(x1 - x2);
                });
                break;
            case FF.SendOrder_Axis.NegX:
                // Sort with Array.Sort (ie quick sort)
                Array.Sort<Transform>(objects, delegate (Transform obj1, Transform obj2)
                {
                    float x1 = obj1.position.x;
                    float x2 = obj2.position.x;
                    return -Math.Sign(x1 - x2);
                });
                break;
            case FF.SendOrder_Axis.PosY:
                // Sort with Array.Sort (ie quick sort)
                Array.Sort<Transform>(objects, delegate (Transform obj1, Transform obj2)
                {
                    float x1 = obj1.position.y;
                    float x2 = obj2.position.y;
                    return Math.Sign(x1 - x2);
                });
                break;
            case FF.SendOrder_Axis.NegY:
                // Sort with Array.Sort (ie quick sort)
                Array.Sort<Transform>(objects, delegate (Transform obj1, Transform obj2)
                {
                    float x1 = obj1.position.y;
                    float x2 = obj2.position.y;
                    return -Math.Sign(x1 - x2);
                });
                break;
            case FF.SendOrder_Axis.PoxZ:
                // Sort with Array.Sort (ie quick sort)
                Array.Sort<Transform>(objects, delegate (Transform obj1, Transform obj2)
                {
                    float x1 = obj1.position.z;
                    float x2 = obj2.position.z;
                    return Math.Sign(x1 - x2);
                });
                break;
            case FF.SendOrder_Axis.NegZ:
                // Sort with Array.Sort (ie quick sort)
                Array.Sort<Transform>(objects, delegate (Transform obj1, Transform obj2)
                {
                    float x1 = obj1.position.z;
                    float x2 = obj2.position.z;
                    return -Math.Sign(x1 - x2);
                });
                break;
        }

        // Send out events to objects
        return Send(message, objects, false, consumeQuantity);
    }

    #region Net Sending OLD
    /// <summary>
    /// Send the message networked (to all other connected clients) to other clients' go (GameObject). go (GameObject) must be a registered Object on other clients.
    /// </summary>
    private static void SendToNet(EventType message, GameObject go, bool varifiedPacket = false)
    {
        FFMessageSystem.SendMessageToNet<EventType>(message, go.GetInstanceID(),
            FFPacketInstructionFlags.MessageBoardGameObjectSend, varifiedPacket);
    }

    /// <summary>
    /// Send the message networked (to all other connected clients) to other clients' go (GameObject) and its parents. go (GameObject) must be a registered Object on other clients
    /// and its parents do not need to be registered to recieve the message
    /// </summary>
    private static void SendToNetUp(EventType message, GameObject go, bool varifiedPacket = false)
    {
        FFMessageSystem.SendMessageToNet<EventType>(message, go.GetInstanceID(),
            FFPacketInstructionFlags.MessageBoardGameObjectSendUp, varifiedPacket);
    }

    /// <summary>
    /// Send the message networked (to all other connected clients) to other clients' go (GameObject) and its childeren. go (GameObject) must be a registered Object on other clients
    /// and its childeren do not need to be registered to recieve the message
    /// </summary>
    private static void SendToNetDown(EventType message, GameObject go, bool varifiedPacket = false)
    {
        FFMessageSystem.SendMessageToNet<EventType>(message, go.GetInstanceID(),
            FFPacketInstructionFlags.MessageBoardGameObjectSendDown, varifiedPacket);
    }

    /// <summary>
    /// Send the message networked (to all other connected clients) to other clients' go (GameObject) and everything connected. go (GameObject) must be a registered Object on other clients
    /// and anything connected does not need to be registered
    /// </summary>
    private static void SendToNetToAllConnected(EventType message, GameObject go, bool varifiedpacket = false)
    {
        FFMessageSystem.SendMessageToNet<EventType>(message, go.GetInstanceID(),
            FFPacketInstructionFlags.MessageBoardGameObjectSendToAllConnected, varifiedpacket);
    }
    #endregion Net

    /// <summary>
    /// Connect to Local and Networked message on this GameObject's box for this board type. Connecting
    /// will make this function called when it is sent a message directly or indirectly
    /// (via SendToLocal/Up/Down/ToAllConnected or by another client calling SendToNet/Up/Down/ToAllConnected)
    /// </summary>
    public static void Connect(FFMessage<EventType>.EventListener function, GameObject go)
    {
        GetReady();
        string id = BaseMessageBoard.LocalIdEntry(go.GetInstanceID());
        FFMessageBox<EventType> box;
        if (messageBoard.TryGetValue(id, out box))
        {
            box.Connect(function);
        }
        else
        {
            box = new FFMessageBox<EventType>(messageBoardSystem, id);
            box.Connect(function);
            messageBoard.Add(id, box);
        }
    }

    /// <summary>
    /// Stops listening to Local and Networed message on this GameObject's box for this board type. This
    /// can be done at any time and is useful for 1-off listeners or other time sensative listeners.
    /// </summary>
    public static void Disconnect(FFMessage<EventType>.EventListener function, GameObject go)
    {
        string id = BaseMessageBoard.LocalIdEntry(go.GetInstanceID());
        FFMessageBox<EventType> box;
        if (messageBoard.TryGetValue(id, out box))
        {
            box.Disconnect(function);
        }
    }
    #endregion

    #region Virtual NetMessageBoard Calls
    public override void Active(bool b)
    {
        Debug.Log(messageBoardSystem.ToString() + " Activity set to " + b.ToString());
        activeLocal = b;
    }

    /// <summary>
    /// Get some information about this FFMessageBoard
    /// stat tracking can be useful to determine when
    /// and how often things are being called. For
    /// global stats on all FFMessage/FFMessageBoard
    /// and connected boxes see FFPrivate.FFMessageSystem.GetStats()
    /// </summary>
    public override FFPrivate.MessageBoardInfo GetInfo()
    {
        FFPrivate.MessageBoardInfo info;
        info.lookupCount = lookupCountLocal;
        info.boxCount = messageBoard.Count;
        info.boxEntry = new string[messageBoard.Count];
        info.callCount = new int[messageBoard.Count];
        info.listenerCount = new int[messageBoard.Count];
        info.visitorCount = new int[messageBoard.Count];

        int i = 0;
        foreach (var box in messageBoard)
        {
            var boxInfo = box.Value.GetInfo();
            info.callCount[i] = boxInfo.callCount;
            info.visitorCount[i] = boxInfo.visitorCount;
            info.listenerCount[i] = boxInfo.listenerCount;
            info.boxEntry[i] = box.Key;
            ++i;
        }
        return info;
    }
    public override void ClearMessageBoard()
    {
        listenerCountGlobal -= listenerCountLocal;
        visitorCountGlobal += listenerCountLocal;
        visitorCountLocal += listenerCountLocal;
        listenerCountLocal = 0;

        messageBoard = new Dictionary<string, FFMessageBox<EventType>>();
    }
    public override int BoxCount()
    {
        return messageBoard.Count;
    }
    public override bool SendToLocalEntry(FFBasePacket package, string entry)
    {
        FFPacket<EventType> sentPackage = (FFPacket<EventType>)package;
        FFPacket<EventType>.Decrypt(ref sentPackage.message);
        return FFMessageBoard<EventType>.SendToLocal(sentPackage.message, entry);
    }
    public override void SendToLocalGameObject(FFBasePacket package, GameObject go)
    {
        FFPacket<EventType> sentPackage = (FFPacket<EventType>)package;
        FFPacket<EventType>.Decrypt(ref sentPackage.message);
        switch(package.packetInstructions)
        {
            case FFPacketInstructionFlags.MessageBoardGameObjectSend:
                FFMessageBoard<EventType>.Send(sentPackage.message, go);
                break;

            case FFPacketInstructionFlags.MessageBoardGameObjectSendDown:
                FFMessageBoard<EventType>.SendDown(sentPackage.message, go);
                break;

            case FFPacketInstructionFlags.MessageBoardGameObjectSendUp:
                FFMessageBoard<EventType>.SendUp(sentPackage.message, go);
                break;

            case FFPacketInstructionFlags.MessageBoardGameObjectSendToAllConnected:
                FFMessageBoard<EventType>.SendAllConnected(sentPackage.message, go);
                break;
            default:
                break;
        }
    }
    #endregion
}
