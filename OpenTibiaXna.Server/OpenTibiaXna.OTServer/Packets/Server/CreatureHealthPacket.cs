﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTibiaXna.OTServer.Engines;
using OpenTibiaXna.OTServer.Objects;

namespace OpenTibiaXna.OTServer.Packets.Server
{
    public class CreatureHealthPacket : PacketObject
    {
        public static void Add
       (
           NetworkMessageEngine message,
           uint creatureID,
           byte newHealth
       )
        {
            message.AddByte((byte)ServerPacketType.CreatureHealth);

            message.AddUInt32(creatureID);
            message.AddByte(newHealth);
        }
    }
}
