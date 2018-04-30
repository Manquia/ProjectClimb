using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 9/10/2015
// Purpose: FFMessageBox is a type-based disbatcher
//      which is mostly used by FFMessageBoard. In the
//      case whereby you want a type-based event to be
//      an object conected to a script/class versus a
//      gameobject or entry you may want to use this.
//
//      FFMessageBoxes which are not created via 
//      FFMessageBoard are not stat tracked nor will
//      they be controllable by FFMessageSystem.
//
//      WARNING: FFMESSAGEBOXES CREATED OUTSIDE OF
//      FFMESSAGEBOARD WILL NOT BE ABLE TO SENDTONET.
//
///////////////////////////////////////////////////////

public class FFMessageBox<EventType>
{
    private bool active = true;
    public bool isActive
    {
        get { return active; }
    }
    // total calls since the begining of the program
    private int callCountLocal = 0;
    // total listeners of this message
    private int listenerCountLocal = 0;
    // total Visitors, or the number of events which are not disconnected
    private int visitorCountLocal = 0;

    private List<EventListener> messageList = new List<EventListener>();
    /// <summary>
    /// The type of delegate used for this message
    /// </summary>
    public delegate void EventListener(EventType e);

    private string boxEntry;
    private FFMessageBoard<EventType> messageBoard;

    /// <summary>
    /// Warning, solo FFMessageBox's can't be networked
    /// can only be created through FFMessageBoards which are
    /// by default bound to gameObjects.
    /// </summary>
    public FFMessageBox()
    {
        messageBoard = null;
        boxEntry = null;
    }

    public FFMessageBoard<EventType> board
    {
        get { return messageBoard; }
    }
    public string entry
    {
        get { return boxEntry; }
    }

    public FFMessageBox(FFMessageBoard<EventType> board, string entry)
    {
        messageBoard = board;
        boxEntry = entry;
    }

    /// <summary>
    /// Connect to this box, function will be called when SendToLocal is called via
    /// FFMessageBoard or by other clients through a FFMessageBoard. Cannot Connect
    /// the same function more than once.
    /// </summary>
    public bool Connect(EventListener function)
    {
        if (!messageList.Contains(function)) // Cannot connect to a single function more than once
        {
            if (messageBoard != null)
                messageBoard.IncrementListenerCount();

            ++listenerCountLocal;
            messageList.Add(function);
            return true;
        }
        return false;
    }

    /// <summary>
    /// returns true if any listeners are called
    /// </summary>
    public bool SendToLocal(EventType e)
    {
        if (active)
        {
            if (messageBoard != null)
            {
                messageBoard.IncrementCallCount();
            }

            ++callCountLocal;

            var listenerList = new List<EventListener>(messageList);
            foreach (var listener in listenerList)
            {
                listener(e);
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// returns true if it could be sent to the net, if this box wasn't
    /// created via a FFMessageBoard it will not be able to send to Net
    /// </summary>
    public bool SendToNet(EventType e, bool varifiedPacket = false)
    {
        if (active && messageBoard != null)
        {
            messageBoard.IncrementCallCount();
            ++callCountLocal;
            FFMessageBoard<EventType>.SendToNet(e, entry, varifiedPacket);
            return true;
        }
        Debug.LogError("Warning, an FFMessageBox which is not connected to a FFMessageBoard tried to SendToNet.");
        return false;
    }

    /// <summary>
    /// Stop listening to this box, function will not be called when SendToLocal is called via
    /// FFMessageBoard or by other clients through a FFMessageBoard
    /// </summary>
    public bool Disconnect(EventListener function)
    {
        var deleted = messageList.Remove(function);

        if (deleted == true)
        {
            if (messageBoard != null)
            {
                messageBoard.IncrementVisitorCount();
                messageBoard.DecrementListenerCount();
            }
            --listenerCountLocal;
            ++visitorCountLocal;
            return true;
        }
        return false;
    }

    public void ClearMessageBox()
    {
        listenerCountLocal -= messageList.Count;
        visitorCountLocal += messageList.Count;
        messageList.Clear();
    }
    /// <summary>
    /// When passed true, messages can be sent, if false,
    /// no messages will be sent through this box.
    /// </summary>
    public void Active(bool b)
    {
        active = b;
    }


    /// <summary>
    /// Get some information about this FFMessageBox
    /// stat tracking can be useful to determine when
    /// and how often things are being called. For
    /// global stats on all FFMessage/FFMessageBoard
    /// and connected boxes see FFPrivate.FFMessageSystem.GetStats()
    /// </summary>
    public FFPrivate.MessageInfo GetInfo()
    {
        FFPrivate.MessageInfo info;
        info.callCount = callCountLocal;
        info.listenerCount = listenerCountLocal;
        info.visitorCount = visitorCountLocal;
        return info;
    }
}
