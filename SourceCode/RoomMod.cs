using System.Collections.Generic;
using RWCustom;
using UnityEngine;

using static CoopTweaks.MainMod;

namespace CoopTweaks;

public static class RoomMod
{
    //
    // variables
    //

    private static bool isEnabled = false;
    private static readonly List<Creature> creaturesInRoomList = new();

    //
    //
    //

    internal static void OnToggle()
    {
        isEnabled = !isEnabled;
        if (Option_SlowMotion)
        {
            if (isEnabled)
            {
                On.Room.PlaySound_SoundID_BodyChunk_bool_float_float += Room_PlaySound; // skip mushroom sound loop;
            }
            else
            {
                On.Room.PlaySound_SoundID_BodyChunk_bool_float_float -= Room_PlaySound;
            }
        }

        if (Option_SlugcatCollision)
        {
            if (isEnabled)
            {
                On.Room.Update += Room_Update;
            }
            else
            {
                On.Room.Update -= Room_Update;
            }
        }
    }

    //
    // private
    //

    private static ChunkSoundEmitter Room_PlaySound(On.Room.orig_PlaySound_SoundID_BodyChunk_bool_float_float orig, Room room, SoundID soundId, BodyChunk chunk, bool loop, float vol, float pitch) // Option_SlowMotion
    {
        if (soundId == SoundID.Mushroom_Trip_LOOP) return orig(room, SoundID.None, chunk, loop, vol, pitch);
        return orig(room, soundId, chunk, loop, vol, pitch);
    }

    private static void Room_Update(On.Room.orig_Update orig, Room room) // Option_SlugcatCollision
    {
        // collision between slugcats (and creatures that are being carried by slugcats)
        if (room.game == null)
        {
            orig(room);
            return;
        }

        if (creaturesInRoomList.Count > 0) // had a problem with DeerFix when throwing puff balls // orig(room) never returned
        {
            Debug.Log("CoopTweaks: Slugcat collisions could not be reset normally. Reset now.");
            foreach (Creature creature in creaturesInRoomList)
            {
                creature.CollideWithObjects = true;
            }
            creaturesInRoomList.Clear();
        }

        // disable collision for now and handle collision manually after calling orig();
        foreach (AbstractCreature abstractPlayer in room.game.Players)
        {
            if (abstractPlayer.Room == room.abstractRoom && abstractPlayer.realizedCreature is Player player)
            {
                foreach (Creature.Grasp? grasp in player.grasps)
                {
                    if (grasp?.grabbed is Creature creature && player.Grabability(creature) != Player.ObjectGrabability.Drag)
                    {
                        creaturesInRoomList.Add(creature);
                        creature.CollideWithObjects = false;
                    }
                }

                // seems like CollideWithObjects is not enough;
                // need to check iAmBeingCarried too;
                // otherwise backPlayers can collide with creatures that are being eating;
                // not sure if this is still a thing but doesn't hurt;
                // onBack is the player that carries you;
                if (player.CollideWithObjects && player.onBack == null)
                {
                    creaturesInRoomList.Add(player);
                    player.CollideWithObjects = false;
                }
            }
        }
        orig(room);

        foreach (Creature creatureA in creaturesInRoomList)
        {
            creatureA.CollideWithObjects = true;

            // they might get removed during orig();
            if (creatureA.room != room) continue;

            foreach (PhysicalObject physicalObjectB in room.physicalObjects[creatureA.collisionLayer])
            {
                // disable collision of players and creatures that they are carrying;
                // including creatures that backPlayers are carrying;
                {
                    if ((physicalObjectB is Creature creatureB && creaturesInRoomList.Contains(creatureB)) || Mathf.Abs(creatureA.bodyChunks[0].pos.x - physicalObjectB.bodyChunks[0].pos.x) >= creatureA.collisionRange + physicalObjectB.collisionRange || Mathf.Abs(creatureA.bodyChunks[0].pos.y - physicalObjectB.bodyChunks[0].pos.y) >= creatureA.collisionRange + physicalObjectB.collisionRange) continue;
                }

                bool hasCollided = false;
                bool isGrabbed = false;

                // is grabbing;
                // only remaining case where this is needed is when
                // the player drags a creature;
                if (creatureA.Template.grasps > 0)
                {
                    foreach (Creature.Grasp? grasp in creatureA.grasps)
                    {
                        if (grasp != null && grasp.grabbed == physicalObjectB)
                        {
                            isGrabbed = true;
                            break;
                        }
                    }
                }

                {
                    // is being grabbed;
                    // creatureB_.Template.grasps > 0 takes also care of creatureB_.grasps != null;
                    if (!isGrabbed && physicalObjectB is Creature creatureB && creatureB.Template.grasps > 0)
                    {
                        foreach (Creature.Grasp? grasp in creatureB.grasps)
                        {
                            if (grasp != null && grasp.grabbed == creatureA)
                            {
                                isGrabbed = true;
                                break;
                            }
                        }
                    }
                }

                if (isGrabbed) continue;
                foreach (BodyChunk playerBodyChunk in creatureA.bodyChunks)
                {
                    foreach (BodyChunk pOBodyChunk in physicalObjectB.bodyChunks)
                    {
                        if (playerBodyChunk.collideWithObjects && pOBodyChunk.collideWithObjects && Custom.DistLess(playerBodyChunk.pos, pOBodyChunk.pos, playerBodyChunk.rad + pOBodyChunk.rad))
                        {
                            float radiusCombined = playerBodyChunk.rad + pOBodyChunk.rad;
                            float distance = Vector2.Distance(playerBodyChunk.pos, pOBodyChunk.pos);
                            Vector2 direction = Custom.DirVec(playerBodyChunk.pos, pOBodyChunk.pos);
                            float massProportion = pOBodyChunk.mass / (playerBodyChunk.mass + pOBodyChunk.mass);

                            playerBodyChunk.pos -= (radiusCombined - distance) * direction * massProportion;
                            playerBodyChunk.vel -= (radiusCombined - distance) * direction * massProportion;
                            pOBodyChunk.pos += (radiusCombined - distance) * direction * (1f - massProportion);
                            pOBodyChunk.vel += (radiusCombined - distance) * direction * (1f - massProportion);

                            if (playerBodyChunk.pos.x == pOBodyChunk.pos.x)
                            {
                                playerBodyChunk.vel += Custom.DegToVec(Random.value * 360f) * 0.0001f;
                                pOBodyChunk.vel += Custom.DegToVec(Random.value * 360f) * 0.0001f;
                            }

                            if (!hasCollided)
                            {
                                creatureA.Collide(physicalObjectB, playerBodyChunk.index, pOBodyChunk.index);
                                physicalObjectB.Collide(creatureA, pOBodyChunk.index, playerBodyChunk.index);
                            }
                            hasCollided = true;
                        }
                    }
                }
            }
        }
        creaturesInRoomList.Clear();
    }
}