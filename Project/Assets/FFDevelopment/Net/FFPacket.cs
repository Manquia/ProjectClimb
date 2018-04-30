using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 9/10/2015
// Purpose: The Message protocal flags. Feel free to
//  change everything you see here to fit your needs
//  for doing any networking stuff throug the
//  messaging system.
///////////////////////////////////////////////////////

[Flags]
public enum FFPacketInstructionFlags
{
    /// <summary>
    /// Disbatch does not wait for main thread to disbatch message.
    /// This uses the async callback to run the event
    /// which unity disallows from using anything related to the gameobject.
    /// NOT MULTITHREAD SAFE
    /// </summary>
    Immediate = 1 | Message,

    /// <summary>
    /// Disbatch to all clients on FFNetMessage (Global Message)
    /// </summary>
    Message = 4,
    /// <summary>
    /// Disbatches to the FFMessageBoards on all clients with given entry or game object
    /// given the extension flags. This flag shouldn't be used to send packets for game
    /// objects. But rather FFMessageBoard string's which are defined by other means
    /// </summary>
    MessageBoard = 8, 

    /// <summary>
    /// Dispatch to all clients on FFMessageBoard of specified
    /// type in the given entry (or GameObject depending on the flags
    /// below.
    /// </summary>
    MessageBoardEntry = 16 | MessageBoard,

    // Game Object Interface
    MessageBoardGameObjectSend = 32 | MessageBoard,
    MessageBoardGameObjectSendDown = 64 | MessageBoardGameObjectSend,
    MessageBoardGameObjectSendUp = 128 | MessageBoardGameObjectSend,
    MessageBoardGameObjectSendToAllConnected = 256 | MessageBoardGameObjectSend,
}

[Serializable]
public class FFBasePacket
{
    public long senderId = -1; // DO NOT MODIFY, is changed in final stages of sending packet
    public FFPacketInstructionFlags packetInstructions;
    public string messageType;
    public string entry;
}

[Serializable]
public class FFPacket<EventType> : FFBasePacket
{
    #region Packet Constructors
    // MessageBoard
    public FFPacket(FFPacketInstructionFlags type, string eventType, EventType message, string eventEntry)
    {
        this.message = message;
        this.packetInstructions = type;
        this.messageType = eventType;
        this.entry = eventEntry;
    }

    //Message
    public FFPacket(FFPacketInstructionFlags type, string eventType, EventType message)
    {
        this.message = message;
        this.packetInstructions = type;
        this.messageType = eventType;
    }
    #endregion

    #region Encryption/Decryption

    public static void Encrypt(ref EventType message)
    {
        // Add your base encryption for all messages
    }

    public static void Encrypt(ref PlayerDiedEvent message)
    {
        // Specialization can be used for particularly important events
        // to increase difficulty of cheating to make hacking more difficult
        // and will be called automatically when that type is encountered
    }

    public static void Decrypt(ref EventType message)
    {
        // Add your base decryption for all messages
    }

    public static void Decrypt(ref PlayerDiedEvent message)
    {
        // Specialization can be used for particularly important events
        // to increase difficulty of cheating to make hacking more difficult
        // and will be called automatically when that type is encountered
    }

    #endregion Encryption/Decryption

    public EventType message;
}



