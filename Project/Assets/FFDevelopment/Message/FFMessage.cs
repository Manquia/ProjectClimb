using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 10/9/2015
// Purpose: FFMessage is a type-based interface for
//      global events which you may want to send/
//      connect/disconnect from. If you want to send
//      an event locally use SendToLocal. If you want
//      to send an event to the server/other clients
//      use SendToNet.
//
///////////////////////////////////////////////////////

namespace FFPrivate
{
    public struct MessageInfo
    {
        public int callCount;
        public int listenerCount;
        public int visitorCount;
    }

    public class BaseMessage
    {
        protected static bool activeGlobal = true;
        // total calls since the begining of the program
        protected static int callCountGlobal = 0;
        // total listeners of this message
        protected static int listenerCountGlobal = 0;
        // total Visitors, or the number of events which are not disconnected
        protected static int visitorCountGlobal = 0;

        public void AllMessagesActive(bool b)
        {
            Debug.Log("All Messages Activity set to " + b.ToString());
            activeGlobal = b;
        }

        #region VirtualStubs
        virtual public MessageInfo GetInfo()
        {
            Debug.LogError("Message Didn't override GetInfo function?!?");
            MessageInfo info;
            info.callCount = 0;
            info.listenerCount = 0;
            info.visitorCount = 0;
            return info;
        }
        virtual public void Active(bool b)
        {
            Debug.LogError("Message Didn't override ActiveMesage function?!?");
        }
        virtual public void ClearMessage()
        {
            Debug.LogError("Message Didn't override ClearAll function?!?");
        }
        virtual public bool SendToLocal(FFBasePacket package)
        {
            Debug.LogError("Message Didn't override SentToLocal function?!?");
            return false;
        }
        virtual public void SendToNet(object message)
        {
            Debug.LogError("Message Didn't override SentToNet function?!?");
        }
        #endregion VirtualStubs
    }
}

public class FFMessage<EventType> : FFPrivate.BaseMessage
{
    // This is a singleton which is only created when connected to.
    private FFMessage() { }
    // should also prevent garbage collections
    private static FFMessage<EventType> messageSystem = null;
    // When false messages canont be sent
    private static bool activeLocal = true;

    /// <summary>
    /// returns true if messages can be sent through this FFMessage
    /// </summary>
    public static bool isActive
    {
        get { return activeLocal && activeGlobal; }
    }
    // total calls since the begining of the program
    private static int callCountLocal = 0;
    // total listeners of this message
    private static int listenerCountLocal = 0;
    // total Visitors, or the number of events which are not disconnected
    private static int visitorCountLocal = 0;

    private static List<EventListener> messageList = new List<EventListener>();
    /// <summary>
    /// The type of delegate used for this message
    /// </summary>
    public delegate void EventListener(EventType e);

    /// <summary>
    /// Returns a reference to the message which contains helper functions
    /// which are potentially volitile to the any other listeners (reference will be null if this MessageType has never
    /// been connected too.) should be limited to Debug and FFMessageSystem as it give access to
    /// several maintenance functions.
    /// </summary>
    public static FFMessage<EventType> Message
    {
        get { return messageSystem; }
    }

    /// <summary>
    /// Send the message locally (to this Unity Instance) to global type-based Message.
    /// </summary>
    public static bool SendToLocal(EventType e)
    {
        if (activeGlobal && activeLocal)
        {
            ++callCountLocal;
            ++callCountGlobal;

            var listenerList = new List<EventListener>(messageList);
            foreach (var listener in listenerList)
            {
                listener(e);
            }
            return true;
        }
        return false;
    }

    // Used to unpack messages from the net
    public override bool SendToLocal(FFBasePacket package)
    {
        FFPacket<EventType> sentPackage = (FFPacket<EventType>)package;
        FFPacket<EventType>.Decrypt(ref sentPackage.message);
        return FFMessage<EventType>.SendToLocal(sentPackage.message);
    }

    /// <summary>
    /// Send the message networked (to all other connected clients) to other clients' global type-based Message.
    /// </summary>
    public static void SendToNet(EventType e, bool varifiedPacket = false)
    {
        if (activeGlobal && activeLocal)
        {
            FFPrivate.FFMessageSystem.SendMessageToNet<EventType>(e, varifiedPacket);
        }
    }

    // Not sure how useful this is, TODO remove?
    public override void SendToNet(object message)
    {
        FFMessage<EventType>.SendToNet((EventType)message);
    }

    /// <summary>
    /// Connect to a global Local and Networked message. Connecting
    /// will make this function called when a message is sent to Local.
    /// Cnnot Connect the same function more than once.
    /// </summary>
    public static bool Connect(EventListener function)
    {
        if (messageSystem == null) // Connect MessageSystem to this Message
        {
            messageSystem = new FFMessage<EventType>();
            FFPrivate.FFMessageSystem.AddMessage(messageSystem, typeof(EventType).ToString());
        }
        if (!messageList.Contains(function)) // Cannot connect to a single function more than once
        {
            ++listenerCountLocal;
            ++listenerCountGlobal;
            messageList.Add(function);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Stops listening to Local and Networed message to global type-based Message. This
    /// can be done at any time and is useful for 1-off listeners or other time sensative listeners.
    /// </summary>
    public static bool Disconnect(EventListener function)
    {
        var deleted = messageList.Remove(function);
        if (deleted == true)
        {
            --listenerCountLocal;
            --listenerCountGlobal;
            ++visitorCountLocal;
            ++visitorCountGlobal;
            return true;
        }
        return false;
    }

    // Virtual Interface
    public override void ClearMessage()
    {
        listenerCountLocal -= messageList.Count;
        listenerCountGlobal -= messageList.Count;
        visitorCountLocal += messageList.Count;
        visitorCountGlobal += messageList.Count;
        Debug.Log(messageSystem.ToString() + " listeners cleared");
        messageList.Clear();
    }
    public override void Active(bool b)
    {
        Debug.Log(messageSystem.ToString() + " Activity set to " + b.ToString());
        activeLocal = b;
    }

    /// <summary>
    /// Get some information about this FFMessage
    /// stat tracking can be useful to determine when
    /// and how often things are being called. For
    /// global stats on all FFMessage/FFMessageBoard
    /// and connected boxes see FFPrivate.FFMessageSystem.GetStats()
    /// </summary>
    public override FFPrivate.MessageInfo GetInfo()
    {
        FFPrivate.MessageInfo info;
        info.callCount = callCountLocal;
        info.listenerCount = listenerCountLocal;
        info.visitorCount = visitorCountLocal;
        return info;
    }
}