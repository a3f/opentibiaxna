﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpOT.Packets;
using System.Data.SQLite;
using System.Net;
using System.Data.Common;
using OpenTibiaXna.Server.Entities;

namespace SharpOT
{
    public class Game
    {
        #region Variables
        
        private Dictionary<uint, Creature> creatures = new Dictionary<uint, Creature>();
        Random random = new Random();

        #endregion

        #region Properties

        public Map Map { get; private set; }
        public Scripter Scripter { get; private set; }

        #endregion

        #region Events

        public delegate bool BeforeCreatureSpeechHandler(Creature creature, Speech speech);
        public BeforeCreatureSpeechHandler BeforeCreatureSpeech;

        public delegate void AfterCreatureDefaultSpeechHandler(Creature creature, SpeechType speechType, string message);
        public AfterCreatureDefaultSpeechHandler AfterCreatureWhisperSpeech;
        public AfterCreatureDefaultSpeechHandler AfterCreatureSaySpeech;
        public AfterCreatureDefaultSpeechHandler AfterCreatureYellSpeech;

        public delegate void AfterCreaturePrivateSpeechHandler(Creature creature, string receiver, string message);
        public AfterCreaturePrivateSpeechHandler AfterCreaturePrivateSpeech;

        public delegate void AfterCreatureChannelSpeechHandler(string sender, SpeechType type, ChatChannel channelId, string message);
        public AfterCreatureChannelSpeechHandler AfterCreatureChannelSpeech;

        public delegate bool BeforeCreatureTurnHandler(Creature creature, Direction direction);
        public BeforeCreatureTurnHandler BeforeCreatureTurn;

        public delegate bool AfterCreatureTurnHandler(Creature creature, Direction direction);
        public AfterCreatureTurnHandler AfterCreatureTurn;

        public delegate bool BeforePlayerChangeOutfitHandler(Creature creature, Outfit outfit);
        public BeforePlayerChangeOutfitHandler BeforePlayerChangeOutfit;

        public delegate bool AfterPlayerChangeOutfitHandler(Creature creature, Outfit outfit);
        public AfterPlayerChangeOutfitHandler AfterPlayerChangeOutfit;

        public delegate bool BeforePrivateChannelOpenHandler(PlayerLocal player, string receiver);
        public BeforePrivateChannelOpenHandler BeforePrivateChannelOpen;

        public delegate bool AfterPrivateChannelOpenHandler(PlayerLocal player, string receiver);
        public AfterPrivateChannelOpenHandler AfterPrivateChannelOpen;

        public delegate bool BeforeCreatureMoveHandler(Creature creature, Direction direction, Location fromLocation, Location toLocation, byte fromStackPosition, Tile toTile);
        public BeforeCreatureMoveHandler BeforeCreatureMove;

        public delegate bool AfterCreatureMoveHandler(Creature creature, Direction direction, Location fromLocation, Location toLocation, byte fromStackPosition, Tile toTile);
        public AfterCreatureMoveHandler AfterCreatureMove;

        public delegate bool ChannelHandler(PlayerLocal creature, ChatChannel channel);
        public ChannelHandler BeforeChannelOpen;
        public ChannelHandler BeforeChannelClose;

        public delegate void AfterChannelOpenHandler(PlayerLocal creature, ChatChannel channel);
        public AfterChannelOpenHandler AfterChannelOpen;

        public delegate bool BeforeCreatureUpdateHealthHandler(Creature creature, ushort health);
        public BeforeCreatureUpdateHealthHandler BeforeCreatureUpdateHealth;

        public delegate void AfterCreatureUpdateHealthHandler(Creature creature, ushort health);
        public AfterCreatureUpdateHealthHandler AfterCreatureUpdateHealth;

        public delegate bool BeforeVipAddHandler(PlayerLocal player, string vipName);
        public BeforeVipAddHandler BeforeVipAdd;

        public delegate void AfterVipAddHandler(PlayerLocal player, string vipName);
        public AfterVipAddHandler AfterVipAdd;

        public delegate bool VipRemoveHandler(PlayerLocal player, uint vipId);
        public VipRemoveHandler BeforeVipRemove;

        public delegate bool BeforeLoginHandler(Connection connection, string playerName);
        public BeforeLoginHandler BeforeLogin;

        public delegate void AfterLoginHandler(PlayerLocal player);
        public AfterLoginHandler AfterLogin;

        public delegate bool BeforeLogoutHandler(PlayerLocal player);
        public BeforeLogoutHandler BeforeLogout;

        public delegate void AfterLogoutHandler(PlayerLocal player);
        public AfterLogoutHandler AfterLogout;

        public delegate bool BeforeWalkCancelHandler();
        public BeforeCreatureTurnHandler BeforeWalkCancel;

        public delegate bool AfterWalkCancelHandler();
        public AfterWalkCancelHandler AfterWalkCancel;

        public delegate void AddRemoveCreatureHandler(Creature creature);
        public AddRemoveCreatureHandler AfterAddCreature;
        public AddRemoveCreatureHandler AfterRemoveCreature;

        #endregion

        #region Constructor

        public Game(Server server)
        {
            Map = new Map();
            Scripter = new Scripter();
        }

        #endregion

        #region Public Helpers

        public void AddCreature(Creature creature)
        {
            creatures.Add(creature.Id, creature);
            if (AfterAddCreature != null)
                AfterAddCreature(creature);
        }

        public void RemoveCreature(Creature creature)
        {
            creatures.Remove(creature.Id);
            if (AfterRemoveCreature != null)
                AfterRemoveCreature(creature);
        }

        public IEnumerable<Creature> GetSpectators(Location location)
        {
            return creatures.Values.Where(creature => creature.Tile.Location.CanSee(location));
        }

        public IEnumerable<PlayerLocal> GetSpectatorPlayers(Location location)
        {
            return GetPlayers().Where(player => player.Tile.Location.CanSee(location));
        }

        public IEnumerable<PlayerLocal> GetPlayers()
        {
            return creatures.Values.OfType<PlayerLocal>();
        }

        #endregion

        #region Public Actions

        public void WalkCancel(PlayerLocal player)
        {
            if (BeforeWalkCancel != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeWalkCancel.GetInvocationList())
                {
                    BeforeWalkCancelHandler subscriber = (BeforeWalkCancelHandler)del;
                    forward &= (bool)subscriber();
                }
                if (!forward) return;
            }
            player.Connection.SendCancelWalk();
            if (AfterWalkCancel != null)
                AfterWalkCancel();
        }

        public void CreatureTurn(Creature creature, Direction direction)
        {
            if (BeforeCreatureTurn != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureTurn.GetInvocationList())
                {
                    BeforeCreatureTurnHandler subscriber = (BeforeCreatureTurnHandler)del;
                    forward &= (bool)subscriber(creature, direction);
                }
                if (!forward) return;
            }

            if (creature.Direction != direction)
            {
                creature.Direction = direction;
                foreach (var player in GetSpectatorPlayers(creature.Tile.Location))
                {
                    player.Connection.SendCreatureTurn(creature);
                }
            }
            if(AfterCreatureTurn!=null)
                AfterCreatureTurn(creature, direction);
        }

        public void PlayerChangeOutfit(PlayerLocal player, Outfit outfit)
        {
            if (BeforePlayerChangeOutfit != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforePlayerChangeOutfit.GetInvocationList())
                {
                    BeforePlayerChangeOutfitHandler subscriber = (BeforePlayerChangeOutfitHandler)del;
                    forward &= (bool)subscriber(player, outfit);
                }
                if (!forward) return;
            }

            player.Outfit = outfit;
            foreach (var spectator in GetSpectatorPlayers(player.Tile.Location))
            {
                spectator.Connection.SendCreatureChangeOutfit(player);
            }
            if(AfterPlayerChangeOutfit!=null)
                AfterPlayerChangeOutfit(player, outfit);
            Database.SavePlayerByName(player);
        }

        public void PrivateChannelOpen(PlayerLocal player, string receiver)
        {
            if (BeforePrivateChannelOpen != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforePrivateChannelOpen.GetInvocationList())
                {
                    BeforePrivateChannelOpenHandler subscriber = (BeforePrivateChannelOpenHandler)del;
                    forward &= (bool)subscriber(player, receiver);
                }
                if (!forward) return;
            }

            string selected = Database.GetPlayerIdNameDictionary().FirstOrDefault(pair => pair.Value.ToLower() == receiver.ToLower()).Value;
            if (selected != null)
            {
                player.Connection.SendChannelOpenPrivate(selected);
                if (AfterPrivateChannelOpen != null)
                    AfterPrivateChannelOpen(player, selected);
            }
            else
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "A player with this name does not exist.");
            }
        }

        public void CreatureSpeech(Creature creature, Speech speech)
        {
            if (BeforeCreatureSpeech != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureSpeech.GetInvocationList())
                {
                    BeforeCreatureSpeechHandler subscriber = (BeforeCreatureSpeechHandler)del;
                    forward &= (bool)subscriber(creature, speech);
                }
                if (!forward) return;
            }

            switch (speech.Type)
            {
                case SpeechType.Say:
                    CreatureSaySpeech(creature, speech.Type, speech.Message);
                    break;
                case SpeechType.Whisper:
                    CreatureWhisperSpeech(creature, speech.Type, speech.Message);
                    break;
                case SpeechType.Yell:
                    CreatureYellSpeech(creature, speech.Type, speech.Message);
                    break;
                case SpeechType.Private:
                    CreaturePrivateSpeech(creature, speech.Receiver, speech.Message);
                    break;
                case SpeechType.ChannelOrange:
                case SpeechType.ChannelRed:
                case SpeechType.ChannelWhite:
                case SpeechType.ChannelYellow:
                    CreatureChannelSpeech(creature.Name, speech.Type, speech.ChannelId, speech.Message);
                    break;
            }
        }

        public void ChannelOpen(PlayerLocal player, ChatChannel channel)
        {
            if (BeforeChannelOpen != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeChannelOpen.GetInvocationList())
                {
                    ChannelHandler subscriber = (ChannelHandler)del;
                    forward &= (bool)subscriber(player, channel);
                }
                if (!forward) return;
            }

            Channel selected = player.ChannelList.FirstOrDefault(c => c.Id == (ushort)channel);
            if (selected != null)
            {
                if (!player.OpenedChannelList.Contains(selected))
                {
                    player.OpenedChannelList.Add(selected);
                }

                player.Connection.SendChannelOpen(selected);
            }

            if(AfterChannelOpen!=null)
                AfterChannelOpen(player, channel);
        }

        public void ChannelClose(PlayerLocal player, ChatChannel channel)
        {
            if (BeforeChannelClose != null)
            {
                // Happens client side, can't stop it
                BeforeChannelClose(player, channel);
            }

            Channel selected = player.OpenedChannelList.FirstOrDefault(c => c.Id == (ushort)channel);
            if (selected != null)
            {
                player.OpenedChannelList.Remove(selected);
            }
        }

        public void CreatureMove(Creature creature, Direction direction)
        {
            Location fromLocation = creature.Tile.Location;
            byte fromStackPosition = creature.Tile.GetStackPosition(creature);
            Location toLocation = creature.Tile.Location.Offset(direction);
            Tile toTile = Map.GetTile(toLocation);

            if (BeforeCreatureMove != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureMove.GetInvocationList())
                {
                    BeforeCreatureMoveHandler subscriber = (BeforeCreatureMoveHandler)del;
                    forward &= (bool)subscriber(creature, direction, fromLocation, toLocation, fromStackPosition, toTile);
                }
                if (!forward) return;
            }

            if (toTile != null && toTile.IsWalkable)
            {
                creature.Tile.Creatures.Remove(creature);
                toTile.Creatures.Add(creature);
                creature.Tile = toTile;

                if (fromLocation.Y > toLocation.Y)
                    creature.Direction = Direction.North;
                else if (fromLocation.Y < toLocation.Y)
                    creature.Direction = Direction.South;
                if (fromLocation.X < toLocation.X)
                    creature.Direction = Direction.East;
                else if (fromLocation.X > toLocation.X)
                    creature.Direction = Direction.West;

                foreach (var player in GetPlayers())
                {
                    if (player == creature)
                    {
                        player.Connection.SendPlayerMove(fromLocation, fromStackPosition, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation) && player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendCreatureMove(fromLocation, fromStackPosition, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation))
                    {
                        player.Connection.SendTileRemoveThing(fromLocation, fromStackPosition);
                    }
                    else if (player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendTileAddCreature(creature);
                    }
                }

                if(AfterCreatureMove!=null)
                    AfterCreatureMove(creature, direction, fromLocation, toLocation, fromStackPosition, toTile);
                
            }
        }

        public void CreatureUpdateHealth(Creature creature, ushort health)
        {
            if (BeforeCreatureUpdateHealth != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureUpdateHealth.GetInvocationList())
                {
                    BeforeCreatureUpdateHealthHandler subscriber = (BeforeCreatureUpdateHealthHandler)del;
                    forward &= (bool)subscriber(creature, health);
                }
                if (!forward) return;
            }

            foreach (var player in GetSpectatorPlayers(creature.Tile.Location))
            {
                if (player == creature)
                {
                    //TODO: composite packet
                    player.Connection.SendStatus();
                }

                player.Connection.SendCreatureUpdateHealth(creature);
            }
            if (AfterCreatureUpdateHealth != null)
                AfterCreatureUpdateHealth(creature, health);
        }

        public void VipAdd(PlayerLocal player, string vipName)
        {
            if (BeforeVipAdd != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeVipAdd.GetInvocationList())
                {
                    BeforeVipAddHandler subscriber = (BeforeVipAddHandler)del;
                    forward &= (bool)subscriber(player, vipName);
                }
                if (!forward) return;
            }

            KeyValuePair<uint,string> selected = Database.GetPlayerIdNameDictionary()
                .FirstOrDefault(pair => pair.Value.ToLower() == vipName.ToLower());
                        
            if (player.VipList.Count >= 100)
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "You cannot add more buddies.");
            }
            else if (selected.Key != 0 && player.VipList.ContainsKey(selected.Key))
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "This player is already in your list.");
            }
            else if (selected.Key != 0)
            {
                bool state = GetPlayers().Any(p => p.Id == selected.Key);
                player.VipList.Add(selected.Key, new Vip
                {
                    Id = selected.Key,
                    Name = selected.Value,
                    LoggedIn = state
                });
                player.Connection.SendVipState(selected.Key, selected.Value, state);
                if (AfterVipAdd != null)
                    AfterVipAdd(player, vipName);
            }
            else
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "A player with this name does not exit.");
            }

        }

        public void VipRemove(PlayerLocal player, uint vipId)
        {
            if (BeforeVipRemove != null)
            {
                // Happens client side, can't stop it
                BeforeVipRemove(player, vipId);
            }

            if (player.VipList.ContainsKey(vipId))
            {
                player.VipList.Remove(vipId);
            }
        }

        public void ProcessLogin(Connection connection, string characterName)
        {
            if (BeforeLogin != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeLogin.GetInvocationList())
                {
                    BeforeLoginHandler subscriber = (BeforeLoginHandler)del;
                    forward &= (bool)subscriber(connection, characterName);
                }
                if (!forward)
                {
                    connection.Close();
                    return;
                }
            }

            PlayerLocal player = Database.GetPlayerByName(connection.AccountId, characterName);
            if (player.SavedLocation == null || Map.GetTile(player.SavedLocation) == null)
            {
                player.SavedLocation = new Location(97, 205, 7);
            }
            //player.Id = 0x01000000 + (uint)random.Next(0xFFFFFF);
            Tile tile = Map.GetTile(player.SavedLocation);
            player.Tile = tile;
            tile.Creatures.Add(player);
            connection.Player = player;
            player.Connection = connection;
            player.Game = this;

            PlayerLogin(player);
        }

        public void PlayerLogout(PlayerLocal player)
        {
            if (BeforeLogout != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeLogout.GetInvocationList())
                {
                    BeforeLogoutHandler subscriber = (BeforeLogoutHandler)del;
                    forward &= (bool)subscriber(player);
                }
                if (!forward) return;
            }
            // TODO: Make sure the player can logout
            player.Connection.Close();
            if (AfterLogout != null)
            {
                AfterLogout(player);
            }
            //should be composite packet for players that are spectators AND vips?
            var spectators = GetSpectatorPlayers(player.Tile.Location).Where(s => s != player);
            foreach (var spectator in spectators)
            {
                spectator.Connection.SendCreatureLogout(player);
            }

            player.Tile.Creatures.Remove(player);
            RemoveCreature(player);

            //maybe player object should have a list of other players that have added it to their vips
            foreach (PlayerLocal p in GetPlayers().Where(b => b.VipList.ContainsKey(player.Id)))
            {
                p.VipList[player.Id].LoggedIn = false;
                p.Connection.SendVipLogout(player.Id);
            }


            Database.SavePlayerById(player);
        }

        public void PlayerLookAt(PlayerLocal player, ushort id, Location location, byte stackPosition)
        {
            if (player.Tile.Location.CanSee(location))
            {
                Tile tile = Map.GetTile(location);
                Thing thing = tile.GetThingAtStackPosition(stackPosition);
                player.Connection.SendTextMessage(TextMessageType.DescriptionGreen, thing.GetLookAtString());
            }
        }

        public Account CheckAccount(Connection connection, string accountName, string password)
        {
            Account account = Database.GetAccount(accountName, password);

            if (account == null)
            {
                connection.SendDisconnect("Account name or password incorrect.");
            }

            return account;
        }

        public uint GenerateAvailableId()
        {
            var dictionary = Database.GetPlayerIdNameDictionary();
            uint baseId = 0x40000001;
            if (dictionary.Count() == 0)
            {
                return baseId;
            }
            for (uint i = 1; i < 0xFFFFFFFF; i++)
            {
                baseId |= i;
                if (!dictionary.ContainsKey(baseId))
                {
                    return baseId;
                }
            }
            throw new Exception("No available player ids.");
        }

        #endregion

        #region Private Helpers

        private void PlayerLogin(PlayerLocal player)
        {
            AddCreature(player);

            player.Connection.SendInitialPacket();

            if (AfterLogin != null)
            {
                AfterLogin(player);
            }

            //TODO: composite packet for players that are spectators AND vips?
            var spectators = GetSpectatorPlayers(player.Tile.Location).Where(s => s != player);
            foreach (var spectator in spectators)
            {
                spectator.Connection.SendCreatureAppear(player);
            }


            foreach (PlayerLocal p in GetPlayers().Where(b => b.VipList.ContainsKey(player.Id)))
            {
                p.VipList[player.Id].LoggedIn = true;
                p.Connection.SendVipLogin(player.Id);
            }
        }

        private void CreatureChannelSpeech(string sender, SpeechType type, ChatChannel channelId, string message)
        {
            var channelPlayers = GetPlayers()
                .Where(player => player.OpenedChannelList.Any(channel => channel.Id == (ushort)channelId));

            foreach (var player in channelPlayers)
            {
                player.Connection.SendChannelSpeech(sender, type, channelId, message);
            }

            if (AfterCreatureChannelSpeech != null)
                AfterCreatureChannelSpeech(sender, type, channelId, message);
        }

        private void CreatureSaySpeech(Creature creature, SpeechType speechType, string message)
        {
            foreach (PlayerLocal spectator in GetSpectatorPlayers(creature.Tile.Location))
            {
                spectator.Connection.SendCreatureSpeech(creature, speechType, message);
            }

            if (AfterCreatureSaySpeech != null)
                AfterCreatureSaySpeech(creature, speechType, message);
        }

        private void CreatureYellSpeech(Creature creature, SpeechType speechType, string message)
        {
            if (creature.IsPlayer)
            {
                PlayerLocal player = (PlayerLocal)creature;
                if (System.Environment.TickCount - player.LastYellTime <= 30000)
                {
                    player.Connection.SendTextMessage(TextMessageType.StatusSmall, "You are exhausted.");
                    return;
                }
                else player.LastYellTime = System.Environment.TickCount;
            }

            bool sameFloor = creature.Tile.Location.Z > 7;
            foreach (PlayerLocal player in GetPlayers().Where(p => p.Tile.Location.IsInRange(creature.Tile.Location, sameFloor, 50)))
            {
                player.Connection.SendCreatureSpeech(creature, speechType, message.ToUpper());
            }

            if (AfterCreatureYellSpeech != null)
                AfterCreatureYellSpeech(creature, speechType, message);
        }

        private void CreatureWhisperSpeech(Creature creature, SpeechType speechType, string message)
        {
            foreach (PlayerLocal spectator in GetSpectatorPlayers(creature.Tile.Location))
            {
                if (spectator.Tile.Location.IsInRange(creature.Tile.Location, true, 1.42))
                {
                    spectator.Connection.SendCreatureSpeech(creature, speechType, message);
                }
                else
                {
                    spectator.Connection.SendCreatureSpeech(creature, speechType, "pspsps");
                }
            }

            if (AfterCreatureWhisperSpeech != null)
                AfterCreatureWhisperSpeech(creature, speechType, message);
        }

        private void CreaturePrivateSpeech(Creature creature, string receiver, string message)
        {
            PlayerLocal selected = GetPlayers().FirstOrDefault(p => p.Name == receiver);
            if (selected != null)
            {
                selected.Connection.SendCreatureSpeech(creature, SpeechType.Private, message);
                if (creature.IsPlayer)
                    ((PlayerLocal)creature).Connection.SendTextMessage(TextMessageType.StatusSmall, "Message sent to " + receiver + ".");
            }
            else
            {
                if (creature.IsPlayer)
                    ((PlayerLocal)creature).Connection.SendTextMessage(TextMessageType.StatusSmall, "A player with this name is not online.");
            }
            if (AfterCreaturePrivateSpeech != null)
                AfterCreaturePrivateSpeech(creature, receiver, message);
        }

        #endregion
    }
}