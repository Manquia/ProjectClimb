using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System;
using FFPrivate;
using FFNetEvents;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 9/10/2015
// Purpose: FFSystem is a central hub to keep track
//      of a number of miscalanious sets of data. Most
//      notable include time, netIds for
//      GameObjects.
//
//      FFSystem is also the primary disbatcher of
//      networked events which are not fully implimented
//      this is why a good chunk of this code is commented
//      out as it is only valuable if you add your own
//      networking details.
//
///////////////////////////////////////////////////////

namespace FFLocalEvents
{

    public struct TimeChangeEvent
    {
        public double newCurrentTime;
    }
}

public class FFSystem : MonoBehaviour {

    #region Monobehaviour Singleton
    private static FFSystem singleton = null;
    public static void GetReady()
    {
        if(singleton == null)
        {
            FFPrivate.FFMessageSystem.GetReady();
            GameObject newFFSystem;
            newFFSystem = new GameObject("FFSystem");
            singleton = newFFSystem.AddComponent<FFSystem>();
        }
    }
    private FFSystem()
    {
        if (singleton != null)
        {
            Debug.LogError("Error created a second Singleton of FFSystem");
            return;
        }
        singleton = this;
    }
    void Awake()
    {
        if (this == singleton)
        {
            _clientWatch.Start();
            //FFMessage<NetObjectCreatedEvent>.Connect(OnNetObjectCreatedEvent);
            //FFMessage<NetObjectDestroyedEvent>.Connect(OnNetObjectDestroyedEvent);
            //FFMessage<ClientConnectionReadyEvent>.Connect(OnClientConnectionReady);
            //FFMessage<GameObjectNetIdRecievedEvent>.Connect(OnGameObjectNetIdRecieved);
            //Debug.Log("FFSystem is awake!");

            FFLocalEvents.TimeChangeEvent TCE;
            TCE.newCurrentTime = FFSystem.time;
            FFMessage<FFLocalEvents.TimeChangeEvent>.SendToLocal(TCE);
        }
    }

    void OnDestroy()
    {
        _clientWatch.Reset();

        // Notify any FFAction of the time shift
        FFLocalEvents.TimeChangeEvent TCE;
        TCE.newCurrentTime = FFSystem.time;
        FFMessage<FFLocalEvents.TimeChangeEvent>.SendToLocal(TCE);


        singleton.recievedMessages.Clear();
        singleton = null;
        //FFMessage<ClientConnectionReadyEvent>.Disconnect(OnClientConnectionReady);
        //FFMessage<GameObjectNetIdRecievedEvent>.Disconnect(OnGameObjectNetIdRecieved);
        //FFMessage<NetObjectCreatedEvent>.Disconnect(OnNetObjectCreatedEvent);
        //FFMessage<NetObjectDestroyedEvent>.Disconnect(OnNetObjectDestroyedEvent);
        //Debug.Log("On Destroy of FFSystem, recieved Messages erased: " + recievedMessages.Count);
    }
    #endregion

    #region Time
    private System.Diagnostics.Stopwatch _clientWatch = new System.Diagnostics.Stopwatch();

    /// <summary>
    /// Returns the time elasped in seconds
    /// </summary>
    public static double time
    {
        //get { return FFClient.clientTime; }
        get { return timeCounter; }
    }
    public static float dt
    {
        get { return Time.unscaledDeltaTime; }
    }
    public static double clientWatchTime
    {
        get 
        {
            if (singleton == null) return 0;
            else return (double)(singleton._clientWatch.ElapsedMilliseconds) / 1000.0;
        }
    }

    #endregion

    #region MessageThread
    private Queue<FFBasePacket> recievedMessages = new Queue<FFBasePacket>();



    /// <summary>
    /// Disbatch a message
    /// </summary>
    public static void DisbatchMessage<EventType>(EventType message)
    {
        var packet = new FFPacket<EventType>(FFPacketInstructionFlags.Message, typeof(EventType).Name, message);
        DisbatchPacket(packet);
    }

    /// <summary>
    /// Disbatch a message to the MessageBoard of a specific eventEntry
    /// </summary>
    public static void DisbatchMessage<EventType>(EventType message, string eventEntry)
    {
        var packet = new FFPacket<EventType>(FFPacketInstructionFlags.MessageBoardEntry, typeof(EventType).Name, message, eventEntry);
        DisbatchPacket(packet);
    }

    /// <summary>
    /// Disbatch a message to a game object with instructions on locality of the disbatch
    /// </summary>
    public static void DisbatchMessage<EventType>(EventType message, FFPacketInstructionFlags instructions, GameObject go)
    {
        instructions |= FFPacketInstructionFlags.MessageBoardGameObjectSend;
        var packet = new FFPacket<EventType>(instructions, typeof(EventType).Name, message, BaseMessageBoard.LocalIdEntry(go.GetInstanceID()));
        DisbatchPacket(packet);
    }

    /// <summary>
    /// Disbatch a packet to the local client
    /// </summary>
    /// <param name="packet"></param>
    public static void DisbatchPacket(FFBasePacket packet)
    {
        if ((packet.packetInstructions & FFPacketInstructionFlags.Immediate).Equals(FFPacketInstructionFlags.Immediate))
        {
            //Debug.Log("Recieved Immediate Message from Net"); // debug
            FFPrivate.FFMessageSystem.SendPacketToLocal(packet);
        }
        else
        {
            //Debug.Log("Recieved Message from Net"); // debug
            singleton.recievedMessages.Enqueue(packet);
        }
    }

    void Update()
    {
        ExecuteMessages();


        timeCounter += Time.unscaledDeltaTime;
    }
    static double timeCounter = 0.0f;
    private void FixedUpdate()
    {
        ExecuteMessages();
    }
    void LateUpdate()
    {
        ExecuteMessages();
    }

    void ExecuteMessages()
    {
        while (singleton.recievedMessages.Count > 0)
        {
            FFBasePacket message = singleton.recievedMessages.Dequeue();
            FFPrivate.FFMessageSystem.SendPacketToLocal(message);   
        }
    }
    #endregion

    #region Networking
    
    //private Queue<GameObjectPreFabPair> _gameObjectsToRegisterToServer = new Queue<GameObjectPreFabPair>(16);

    // Shared GameObjectData values
    private Dictionary<long, GameObjectData> _netIdToGameObjectData = new Dictionary<long, GameObjectData>();
    private Dictionary<int, GameObjectData> _localIdToGameObjectData = new Dictionary<int, GameObjectData>();

    public class GameObjectData
    {
        public GameObjectData(GameObject go, long ownerClientId, long goNetId, int goInstanceId)
        {
            this.ownerClientId = ownerClientId;
            this.gameObject = go;
            this.gameObjectNetId = goNetId;
            this.gameObjectInstanceId = goInstanceId;
        }
        public GameObject gameObject;
        public long gameObjectNetId;
        public long ownerClientId;
        public int gameObjectInstanceId;
    }

    public struct GameObjectPreFabPair
    {
        public GameObject go;
        public string prefab;
    }

    /// <summary>
    /// Registers the Game Object to other clients. If this is called on the game object it will be created
    /// on all other connected clients. This should be called on Awake in order for OwnGameObject to work.
    /// </summary>
    public static bool RegisterNetGameObject(GameObject go, string prefabName)
    {
        if (go == null)
            throw new ArgumentNullException();

        FFSystem.GetReady();
        FFPrivate.FFMessageSystem.GetReady();

        // object registered/created by another client, or already registered once on this client
        GameObjectData localGoData;
        int id = go.GetInstanceID();
        if (singleton._GameObjecttRegistryIsLocked || singleton._localIdToGameObjectData.TryGetValue(id, out localGoData))
            return false;

        //if (FFClient.isReady)
        //{
        //    int instanceId = go.GetInstanceID();
        //    singleton._localIdToGameObjectData.Add(instanceId, new GameObjectData(go, FFClient.clientId, -1, id));
        //
        //    NetObjectCreatedEvent NOCE;
        //
        //    NOCE.pos = go.transform.position;
        //    NOCE.rot = go.transform.rotation;
        //    NOCE.scale = go.transform.localScale;
        //
        //    NOCE.creationTime = FFClient.clientTime;
        //    NOCE.gameObjectInstanceId = instanceId;
        //    NOCE.clientOwnerNetId = FFClient.clientId;
        //
        //    NOCE.gameObjectNetId = -1; // Set by server, -1 == Not Uninitialized
        //
        //    // Strip (clone)
        //    int cloneBegin = prefabName.IndexOf("(Clone)");
        //    if (cloneBegin == -1)
        //        NOCE.prefabName = prefabName;
        //    else
        //        NOCE.prefabName = prefabName.Substring(0, cloneBegin);
        //
        //    //Debug.Log("NewObjectCreatedEvent!"); // debug
        //    FFMessageSystem.SendMessageToNet<NetObjectCreatedEvent>(NOCE, true);
        //}
        //else
        //{
        //    GameObjectPreFabPair goPre;
        //    goPre.go = go;
        //    goPre.prefab = prefabName;
        //    singleton._gameObjectsToRegisterToServer.Enqueue(goPre);
        //}

        return true;
    }
    private bool _GameObjecttRegistryIsLocked = false;

    /// <summary>
    /// Returns true if this Unity Instance Owns the GameObject. Owns: Was created/Registered by, or given ownership by Server/Client. Assuming
    /// RegisterNetGameObject was called in Awake, you can call this after Start
    /// </summary>
    public static bool OwnGameObject(GameObject go)
    {
        // Net Object
        GameObjectData data;
        if(singleton._localIdToGameObjectData.TryGetValue(go.GetInstanceID(), out data))
        {
            //var clientId = FFClient.clientId;
            //// We own the object
            //if (data.ownerClientId == clientId)
            //    return true;
            //else
            //    return false;
        }
        // Non-registered object is a Local
        return true;
    }

    public static long GetGameObjectClientOwner(GameObject go)
    {
        GameObjectData data;
        if (singleton._localIdToGameObjectData.TryGetValue(go.GetInstanceID(), out data))
        {
            return data.ownerClientId;
        }
        Debug.LogWarning("Warning, GameObject passed to GetGameObjectClientOwener but GameObject was not found by LocalId");
        return -1;
    }
    public static bool TryGetGameObjectDataByNetId(long goNetId, out GameObjectData gameObjectData)
    {
        return (singleton != null || (gameObjectData = null) == null) && singleton._netIdToGameObjectData.TryGetValue(goNetId, out gameObjectData);
    }
    public static bool TryGetGameObjectDataByInstanceId(int goInstanceId, out GameObjectData gameObjectData)
    {
        // All the fun!
        return (singleton != null || (gameObjectData = null) == null) && singleton._localIdToGameObjectData.TryGetValue(goInstanceId, out gameObjectData);
    }

    /// <summary>
    /// This should only be called by FFClient durring a ClientSyncTimeEvent
    /// </summary>
    public static void ResetClientWatch()
    {
        singleton._clientWatch.Reset();
        singleton._clientWatch.Start();
    }

    //private static void OnClientConnectionReady(ClientConnectionReadyEvent e)
    //{
    //    Debug.Log("ClientConnectionReady Event!"); // debug
    //    for (int i = 0, count = singleton._gameObjectsToRegisterToServer.Count; i < count; ++i)
    //    {
    //        GameObjectPreFabPair goPre = singleton._gameObjectsToRegisterToServer.Dequeue();
    //        if (goPre.go != null)
    //            RegisterNetGameObject(goPre.go, goPre.prefab);
    //    }
    //}

    // TODO ?
    //private static void OnClieintConnectionEnded(ClientConnectionEndedEvent e)
    //{
    //    // TODO cleanup? not sure if I want a connection Trouble event like ClientConnectionSuspendedEvent
    //}
    // TODO ?
    //private static void OnClieintConnectionSuspended(ClieintConnectionSuspended e)
    //{
    //    // TODO cleanup? not sure if I want a connection Trouble event like ClientConnectionSuspendedEvent
    //}

    // Recived netId for locally registered object
    //private static void OnGameObjectNetIdRecieved(GameObjectNetIdRecievedEvent e)
    //{
    //    Debug.Log("GameObjectNetIdRecieved Event!"); // debug
    //    GameObjectData goData;
    //    int id = e.gameInstanceId;
    //    if (singleton._localIdToGameObjectData.TryGetValue(id, out goData))
    //    {
    //        goData.gameObjectNetId = e.netId;
    //        singleton._netIdToGameObjectData.Add(e.netId, goData);
    //    }
    //    else
    //    {
    //        Debug.LogError("Error, GameObjectNetIdRecieved For an non-registered game object");
    //    }
    //}
    //// Net Object comming in from another client
    //private static void OnNetObjectCreatedEvent(NetObjectCreatedEvent e)
    //{
    //    GameObject go = null;
    //    if (e.prefabName != null)
    //    {
    //        singleton._GameObjecttRegistryIsLocked = true;
    //        go = GameObject.Instantiate(FFResource.Load_Prefab(e.prefabName));
    //        singleton._GameObjecttRegistryIsLocked = false;
    //
    //        if (go == null)
    //        {
    //            Debug.Log("Error in FFMessageSystem.OnNewObjectCreatedEvent, failed to Instantiate prefabName as gameobject");
    //            return;
    //        }
    //        int id = go.GetInstanceID();
    //
    //        var trans = go.transform;
    //        trans.position = e.pos;
    //        trans.rotation = e.rot;
    //        trans.localScale = e.scale;
    //
    //        //Debug.LogError("New Net Object Created");//debug
    //        GameObjectData ffgoData = new GameObjectData(go, e.clientOwnerNetId, e.gameObjectNetId, id);
    //        singleton._localIdToGameObjectData.Add(id, ffgoData);
    //        singleton._netIdToGameObjectData.Add(e.gameObjectNetId, ffgoData);
    //    }
    //}

    private static void OnNetObjectDestroyedEvent(NetObjectDestroyedEvent e)
    {
        Debug.Log("NetObjectDestroyedEvent!");

        GameObjectData goData;
        if (singleton._netIdToGameObjectData.TryGetValue(e.gameObjectNetId, out goData))
        {
            DestroyNetGameObject(e.gameObjectNetId);
            GameObject.Destroy(goData.gameObject);
        }
    }
    #endregion

    #region FFSystemData
    
    /// <summary>
    /// Removes Local Data go, DOES NOT destroy object
    /// </summary>
    /// <param name="go"></param>
    public static void UnRegisterNetGameObject(GameObject go, bool destroyGameObject = true)
    {
        int id = go.GetInstanceID();
        GameObjectData goData_C0;
        if (singleton._localIdToGameObjectData.TryGetValue(id, out goData_C0))
        {
            GameObjectData goData_C1;
            if(singleton._netIdToGameObjectData.TryGetValue(goData_C0.gameObjectNetId, out goData_C1))
            {
                if (goData_C0.Equals(goData_C1) == false) // should be the same
                    Debug.LogError("ERROR, GameObjectData is messed up!");


                if(destroyGameObject)
                {
                    // if we own the object or are the server, we can destroy
                    // it accross all other clients
                    // We own the object
                    //if (goData_C0.ownerClientId == FFClient.clientId ||
                    //    FFServer.isLocal)
                    //{
                    //    //Debug.Log("Sent NetObjectDestroyedEvent");//debug
                    //    NetObjectDestroyedEvent e;
                    //    e.destructiontime = FFSystem.time;
                    //    e.gameObjectNetId = goData_C1.gameObjectNetId;
                    //    FFMessage<NetObjectDestroyedEvent>.SendToNet(e, true);
                    //}
                }
                
                singleton._netIdToGameObjectData.Remove(goData_C1.gameObjectNetId);
                singleton._localIdToGameObjectData.Remove(goData_C1.gameObjectInstanceId);
            }

        }

        if(destroyGameObject)
            GameObject.Destroy(go);
    }
    /// <summary>
    /// Removes Net data, and destroys object
    /// </summary>
    /// <param name="netId"></param>
    public static void DestroyNetGameObject(long netId)
    {
        GameObjectData goData_C0;
        if (singleton._netIdToGameObjectData.TryGetValue(netId, out goData_C0))
        {
            GameObjectData goData_C1;
            if (singleton._netIdToGameObjectData.TryGetValue(goData_C0.gameObjectInstanceId, out goData_C1))
            {
                if (goData_C0.Equals(goData_C1) == false) // should be the same
                    Debug.LogError("ERROR, GameObjectData is messed up!");

                // if we own the object, we can destroy it accross call clients
                if (FFSystem.OwnGameObject(goData_C0.gameObject))
                {
                    //Debug.Log("Sent NetObjectDestroyedEvent");//debug
                    NetObjectDestroyedEvent e;
                    e.destructiontime = FFSystem.time;
                    e.gameObjectNetId = goData_C1.gameObjectNetId;
                    FFMessage<NetObjectDestroyedEvent>.SendToNet(e, true);
                }

                singleton._netIdToGameObjectData.Remove(goData_C1.gameObjectNetId);
                singleton._localIdToGameObjectData.Remove(goData_C1.gameObjectInstanceId);
            }

        }
    }

    /// <summary>
    /// Options used game-wide for binary Formatter in the networking
    /// </summary>
    /// <param name="bf"></param>
    public static void InitBinaryFormatter(BinaryFormatter bf)
    {
        //bf.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
        //bf.FilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Low;
        //bf.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesWhenNeeded;
    }
    #endregion

}
