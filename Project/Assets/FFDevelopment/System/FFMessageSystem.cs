using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using System;
using FFNetEvents;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 10/9/2015
// Purpose: FFMessageSystem is the central controller
//      for all type-based events in FF. It can be used
//      to get stats on any FFMesssage and
//      FFMessageBoard. It is also the main point of
//      entry and exit for all events to the FFClient.
//
//      This central controller can disable all events,
//      clear all events and has a lot of functionaity
//      used by FFMessage/FFMessageBoard. It is in the
//      Private namespace because for most everything
//      you will never need to call FFMessageSystem
//      functions. (aside from stat tracking)
//
///////////////////////////////////////////////////////


namespace FFNetEvents
 {
    /// <summary>
    /// FFMessage: Used in RegisterNetGameObject to tell all other client about this object.
    /// </summary>
    [Serializable]
    public struct NetObjectCreatedEvent
    {
        public double creationTime;
        public int gameObjectInstanceId;
        public long gameObjectNetId; // needs to be set by server, check if server in OnObjectCreatdEvent below after server is created, 
        public long clientOwnerNetId;

        public FFVector3 pos;
        public FFVector4 rot;
        public FFVector3 scale;

        // by prefabName
        public string prefabName;
    }

    /// <summary>
    /// FFMessage: Used when an object is destroyed to remove it from the server's list of objects to spawn to each client
    /// upon connection.
    /// </summary>
    [Serializable]
    public struct NetObjectDestroyedEvent
    {
        public double destructiontime;
        public long gameObjectNetId; // needs to be set by server, check if server in OnObjectCreatdEvent below after server is created, 
    }
}

namespace FFPrivate{
    // used for communication between FFMessageSystem and all base
    // Messages' member functions, done to reduce clutter
    public struct ___blank___{}

    // TODO Maybe: add a way to Q messages to be sent via FFSystem. For work to be done on
    // other threads then send the result(s) as an event later (HACK in GetPublicIP in FFClient)
    public class FFMessageSystem
    {
        #region System
        private FFMessageSystem(){}
        ~FFMessageSystem()
        {
            if (system != null)
            {
                system = null;
                Debug.Log("FFMessageSystem Destroyed");
            }
        }
        public static void GetReady()
        {
            if (system == null)
            {
                system = new FFMessageSystem();
                Debug.Log("FFMessageSystem Ready");
            }
        }
        private static FFMessageSystem system = null;

        
        #endregion

        #region Message References
        // Lists for all Messages/MesageBoards/NetMessages/NetMessageBoards
        private static List<BaseMessage> messages = new List<BaseMessage>();
        private static List<BaseMessageBoard> messageBoards = new List<BaseMessageBoard>();
        // Dictionary for all NetMessages/NetMessageBoards
        private static Dictionary<string, BaseMessage> messageDictionary = new Dictionary<string, BaseMessage>();
        private static Dictionary<string, BaseMessageBoard> messageBoardDictionary = new Dictionary<string, BaseMessageBoard>();
        #endregion Message References

        #region NetConnection

        // TODO optimization, change FFPacket's message/eventEntry to type object for gameObject Interface for sending netId as a number versus string
        // GameObject Interface
        public static void SendMessageToNet<EventType>(EventType message, int gameObjectInstanceId, FFPacketInstructionFlags instructions, bool varifiedPacket)
        {
            FFSystem.GameObjectData gameObjectData;
            if(FFSystem.TryGetGameObjectDataByInstanceId(gameObjectInstanceId, out gameObjectData))
            {
                FFPacket<EventType> netpacket = new FFPacket<EventType>(instructions, typeof(EventType).ToString(), message, gameObjectData.gameObjectNetId.ToString());
                SendToNet<EventType>(netpacket, varifiedPacket);
            }
            else
            {
                Debug.Log("Tried to send a Message through Net via non-registered GameObject" +
                    "\nInstance ID: " + gameObjectInstanceId +
                    "\nObjects which send events to other NetObject need to be Registered via FFSystem.RegisterNetGameObject");
            }
        }

        // MessageBoards Outgoing (packing/encryption stage)
        public static void SendMessageToNet<EventType>(EventType message, string entry, bool varifiedPacket)
        {
            FFPacket<EventType> netpacket = new FFPacket<EventType>(FFPacketInstructionFlags.MessageBoardEntry, typeof(EventType).ToString(), message, entry);
            SendToNet<EventType>(netpacket, varifiedPacket);
        }

        // Messages Outgoing (packing/encryption stage)
        public static void SendMessageToNet<EventType>(EventType message, bool varifiedPacket)
        {
            FFPacket<EventType> netpacket = new FFPacket<EventType>(FFPacketInstructionFlags.Message, typeof(EventType).ToString(), message);
            SendToNet<EventType>(netpacket, varifiedPacket);
        }

        public static void SendToNet<EventType>(FFPacket<EventType> netPacket, bool varifiedPacket)
        {
            // any Data scrambling which is based on type needs to happen here. It is unscrambled
            // in 
            FFPacket<EventType>.Encrypt(ref netPacket.message);
            //FFClient.SendPacket(netPacket, varifiedPacket);
            return;
        }

        // MessageBoards/Messages executed by main thread in FFNetSystem
        // MessageBoards/Messages Unpacking of packetdata
        // MessageBoards/Messages direction and distribution
        public static void SendPacketToLocal(FFBasePacket baseNetPacket)
        {
            #region FFMessage
            if ((baseNetPacket.packetInstructions & FFPacketInstructionFlags.Message).Equals(FFPacketInstructionFlags.Message))
            {
                BaseMessage netMessage;
                if (messageDictionary.TryGetValue(baseNetPacket.messageType, out netMessage))
                {
                    netMessage.SendToLocal(baseNetPacket);
                }
                else
                {
                    Debug.Log("Recieved message of non-registered NetMessage: " + baseNetPacket.messageType);
                }
            }
            #endregion
            #region FFMessageBoard
            else if ((baseNetPacket.packetInstructions & FFPacketInstructionFlags.MessageBoard).Equals(FFPacketInstructionFlags.MessageBoard))
            {
                BaseMessageBoard netMessageBoard;
                if (messageBoardDictionary.TryGetValue(baseNetPacket.messageType, out netMessageBoard))
                {
                    // goes to an entry
                    if ((baseNetPacket.packetInstructions & FFPacketInstructionFlags.MessageBoardEntry).Equals(FFPacketInstructionFlags.MessageBoardEntry))
                    {
                        netMessageBoard.SendToLocalEntry(baseNetPacket, baseNetPacket.entry);
                    }
                    // goes to a GameObject
                    else if ((baseNetPacket.packetInstructions & FFPacketInstructionFlags.MessageBoardGameObjectSend).Equals(FFPacketInstructionFlags.MessageBoardGameObjectSend))
                    {
                        FFSystem.GameObjectData goToSendToData;
                        long netId = Convert.ToInt64(baseNetPacket.entry);
                        if(FFSystem.TryGetGameObjectDataByNetId(netId, out goToSendToData))
                        {
                            netMessageBoard.SendToLocalGameObject(baseNetPacket, goToSendToData.gameObject);
                        }
                        else
                        {
                            Debug.Log("Recieved messageBoard packet for netId which is not registered" +
                            "\nMessageType: " + baseNetPacket.messageType +
                            "\nFlag: " + baseNetPacket.packetInstructions +
                            "\nEntry: " + baseNetPacket.entry +
                            "\nNetId: " + netId);
                        }
                    }
                    else
                    {
                        Debug.Log("Recieved Invalid FFPacketInstructionFlags" +
                            "\nMessageType: " + baseNetPacket.messageType +
                            "\nFlag: " + baseNetPacket.packetInstructions + 
                            "\nEntry: " + baseNetPacket.entry);
                    }
                }
                else
                {
                    Debug.Log("Recieved message for a non-registered NetMessageBoard" +
                            "\nMessageType: " + baseNetPacket.messageType +
                            "\nFlag: " + baseNetPacket.packetInstructions +
                            "\nEntry: " + baseNetPacket.entry);
                }
            }
            #endregion
        }
        #endregion NetConnections

        #region Add
        /// <summary>
        /// Should only be called by FFMessage
        /// </summary>
        public static void AddMessage(BaseMessage message, string messageType)
        {
            messages.Add(message);
            messageDictionary.Add(messageType, message);
        }
        /// <summary>
        /// Should only be called by FFMessageBoard
        /// </summary>
        public static void AddMessageBoard(BaseMessageBoard messageBoard, string messageType)
        {
            messageBoards.Add(messageBoard);
            messageBoardDictionary.Add(messageType, messageBoard);
        }
        #endregion Add

        #region Clear
        /// <summary>
        /// Clears all global FFMessages
        /// </summary>
        public static void ClearAllMessages()
        {
            Debug.Log("Clearing AllMessages");
            foreach(var mes in messages)
            {
                mes.ClearMessage();
            }
        }

        /// <summary>
        /// Clears all FFMessageBoards with any entry/gameobject
        /// </summary>
        public static void ClearAllMessageBoards()
        {
            Debug.Log("Clearing AllMessageBoards");
            foreach(var mesBoard in messageBoards)
            {
                mesBoard.ClearMessageBoard();
            }
        }
        #endregion Clear

        #region Activity
        public static void AllMessagesActive(bool b)
        {
            FFMessage<___blank___>.Message.AllMessagesActive(b);
        }
        public static void AllMessageBoardsActive(bool b)
        {
            FFMessageBoard<___blank___>.MessageBoard.AllMessageBoardsActive(b);
        }
        #endregion Activity

        #region Statistics
        /// <summary>
        /// Returns an array of strings containing all of the known stats on events.
        /// </summary>
        /// <returns></returns>
        public static string[] GetStats()
        {
            int sizeOfStatsArray = GetSizeOfStatsArray();
            string[] stats = new string[sizeOfStatsArray];

            #region Message
            stats[0] += "Stats for MessageSystem";

            int count = 0;
            int totalCalls = 0;
            int totalListeners = 0;
            int totalVisitors = 0;

            for (count = 0; count < messages.Count; ++count)
            {
                var mesInfo = messages[count].GetInfo();
                stats[count + 1] +=
                    "Message:  " + messages[count].ToString()
                    + " Calls: " + mesInfo.callCount 
                    + " Listeners: " + mesInfo.listenerCount 
                    + " Visitors: " + mesInfo.visitorCount;

                totalCalls += mesInfo.callCount;
                totalListeners += mesInfo.listenerCount;
                totalVisitors += mesInfo.visitorCount;
            }

            stats[messages.Count + 1] += "Total Messages: " + count + " Total Calls: " + totalCalls + " Total Listeners: " + totalListeners + " Total Visitors: " + totalVisitors;
            #endregion Mesage

            #region MessageBoard
            stats[messages.Count + 2] += "Stats for MessageBoardSystem";


            count = 0;
            totalCalls = 0;
            totalListeners = 0;
            totalVisitors = 0;
            int totallookupCount = 0;
            int totalboxCount = 0;

            int addedLines = 0; // +1 for each MessageBox in the MessageBoard
            for (count = 0; count < messageBoards.Count; ++count)
            {
                var mesInfo = messageBoards[count].GetInfo();
                stats[(count + 1) + addedLines + messages.Count + 2] +=
                    "MessageBoard:  " + messageBoards[count].ToString()
                    + " Lookups: " + mesInfo.lookupCount
                    + " Boxes: " + mesInfo.boxCount;

                totallookupCount += mesInfo.lookupCount;
                totalboxCount += mesInfo.boxCount;

                for(int i = 0; i < mesInfo.boxCount; ++i)
                {
                    stats[(count + 2) + addedLines + i + messages.Count + 2] +=
                        "MessageBoard BoxEntry: " + mesInfo.boxEntry[i]
                        + " Calls: " + mesInfo.callCount[i]
                        + " Listeners: " + mesInfo.listenerCount[i]
                        + " Visitors: " + mesInfo.visitorCount[i];

                    totalCalls += mesInfo.callCount[i];
                    totalListeners += mesInfo.listenerCount[i];
                    totalVisitors += mesInfo.visitorCount[i];
                }
                addedLines += mesInfo.boxCount;
            }

            stats[sizeOfStatsArray - 1] +=
                "Total Boards: " + count
                + " Total Boxes: " + totalboxCount
                + " Total Lookups: " + totallookupCount 
                + " Total Calls: " + totalCalls 
                + " Total Listeners: " + totalListeners 
                + " Total Visitors: " + totalVisitors;
            #endregion MessageBoard

            return stats;
        }

        // Helper function for GetStats
        private static int GetSizeOfStatsArray()
        {
            // 2 lines for start/end of Message + 2 lines for start/end of MessageBoard 
            int size = 2 + 2;
            // Add 1 line per Message
            size += messages.Count;
            // Add 1 line per MessageBoard
            size += messageBoards.Count;
            foreach(var mesboard in messageBoards)
            {
                // Add a line for each box in a messageboard
                size += mesboard.BoxCount();
            }
            return size;
        }

        #endregion Statistics
    } // MessageSystem
} // FFPrivate
