using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static CoopTweaks.MainMod;

namespace CoopTweaks;

internal static class RoomMod {
    //
    // variables
    //

    private static readonly List<Creature> _creatures_in_room_list = new();

    //
    // main
    //

    internal static void On_Config_Changed() {
        On.Room.PlaySound_SoundID_BodyChunk_bool_float_float -= Room_PlaySound;
        On.Room.Update -= Room_Update;

        if (Option_SlowMotion) {
            // skip mushroom sound loop;
            On.Room.PlaySound_SoundID_BodyChunk_bool_float_float += Room_PlaySound;
        }

        if (Option_SlugcatCollision) {
            On.Room.Update += Room_Update;
        }
    }

    //
    // private
    //

    private static ChunkSoundEmitter Room_PlaySound(On.Room.orig_PlaySound_SoundID_BodyChunk_bool_float_float orig, Room room, SoundID sound_id, BodyChunk chunk, bool loop, float vol, float pitch) { // Option_SlowMotion
        if (sound_id == SoundID.Mushroom_Trip_LOOP) return orig(room, SoundID.None, chunk, loop, vol, pitch);
        return orig(room, sound_id, chunk, loop, vol, pitch);
    }

    private static void Room_Update(On.Room.orig_Update orig, Room room) { // Option_SlugcatCollision
        // collision between slugcats (and creatures that are being carried by slugcats)
        if (room.game == null) {
            orig(room);
            return;
        }

        if (_creatures_in_room_list.Count > 0) { // had a problem with DeerFix when throwing puff balls // orig(room) never returned
            Debug.Log("CoopTweaks: Slugcat collisions could not be reset normally. Reset now.");
            foreach (Creature creature in _creatures_in_room_list) {
                creature.CollideWithObjects = true;
            }
            _creatures_in_room_list.Clear();
        }

        // disable collision for now and handle collision manually after calling orig();
        foreach (AbstractCreature abstract_player in room.game.Players) {
            if (abstract_player.Room == room.abstractRoom && abstract_player.realizedCreature is Player player) {
                foreach (Creature.Grasp? grasp in player.grasps) {
                    if (grasp?.grabbed is Creature creature && player.Grabability(creature) != Player.ObjectGrabability.Drag) {
                        _creatures_in_room_list.Add(creature);
                        creature.CollideWithObjects = false;
                    }
                }

                // seems like CollideWithObjects is not enough;
                // need to check iAmBeingCarried too;
                // otherwise backPlayers can collide with creatures that are being eating;
                // not sure if this is still a thing but doesn't hurt;
                // onBack is the player that carries you;
                if (player.CollideWithObjects && player.onBack == null) {
                    _creatures_in_room_list.Add(player);
                    player.CollideWithObjects = false;
                }
            }
        }
        orig(room);

        foreach (Creature creature_a in _creatures_in_room_list) {
            creature_a.CollideWithObjects = true;

            // they might get removed during orig();
            if (creature_a.room != room) continue;

            List<PhysicalObject> physical_objects = room.physicalObjects[creature_a.collisionLayer];
            for (int physical_object_index = physical_objects.Count - 1; physical_object_index >= 0; --physical_object_index) {
                // this seems to be more robust; I had an issue when dropping spears and singularity bombs in 
                // a not-used pipe exit; the exit would push them out again triggering collisions; somehow
                // this modified the list and I get an error; my guess is that the collisionLayer was changed
                // by the collision, which modified the list;
                PhysicalObject physical_object_b = physical_objects[physical_object_index];

                // disable collision of players and creatures that they are carrying;
                // including creatures that backPlayers are carrying;
                {
                    if ((physical_object_b is Creature creature_b && _creatures_in_room_list.Contains(creature_b)) || Mathf.Abs(creature_a.bodyChunks[0].pos.x - physical_object_b.bodyChunks[0].pos.x) >= creature_a.collisionRange + physical_object_b.collisionRange || Mathf.Abs(creature_a.bodyChunks[0].pos.y - physical_object_b.bodyChunks[0].pos.y) >= creature_a.collisionRange + physical_object_b.collisionRange) continue;
                }

                bool has_collided = false;
                bool is_grabbed = false;

                // is grabbing;
                // only remaining case where this is needed is when
                // the player drags a creature;
                if (creature_a.Template.grasps > 0) {
                    foreach (Creature.Grasp? grasp in creature_a.grasps) {
                        if (grasp != null && grasp.grabbed == physical_object_b) {
                            is_grabbed = true;
                            break;
                        }
                    }
                }

                {
                    // is being grabbed;
                    // creatureB_.Template.grasps > 0 takes also care of creatureB_.grasps != null;
                    if (!is_grabbed && physical_object_b is Creature creature_b && creature_b.Template.grasps > 0) {
                        foreach (Creature.Grasp? grasp in creature_b.grasps) {
                            if (grasp != null && grasp.grabbed == creature_a) {
                                is_grabbed = true;
                                break;
                            }
                        }
                    }
                }

                if (is_grabbed) continue;
                foreach (BodyChunk player_body_chunk in creature_a.bodyChunks) {
                    foreach (BodyChunk body_chunk_b in physical_object_b.bodyChunks) {
                        if (player_body_chunk.collideWithObjects && body_chunk_b.collideWithObjects && Custom.DistLess(player_body_chunk.pos, body_chunk_b.pos, player_body_chunk.rad + body_chunk_b.rad)) {
                            float radius_combined = player_body_chunk.rad + body_chunk_b.rad;
                            float distance = Vector2.Distance(player_body_chunk.pos, body_chunk_b.pos);
                            Vector2 direction = Custom.DirVec(player_body_chunk.pos, body_chunk_b.pos);
                            float mass_proportion = body_chunk_b.mass / (player_body_chunk.mass + body_chunk_b.mass);

                            player_body_chunk.pos -= (radius_combined - distance) * direction * mass_proportion;
                            player_body_chunk.vel -= (radius_combined - distance) * direction * mass_proportion;
                            body_chunk_b.pos += (radius_combined - distance) * direction * (1f - mass_proportion);
                            body_chunk_b.vel += (radius_combined - distance) * direction * (1f - mass_proportion);

                            if (player_body_chunk.pos.x == body_chunk_b.pos.x) {
                                player_body_chunk.vel += Custom.DegToVec(Random.value * 360f) * 0.0001f;
                                body_chunk_b.vel += Custom.DegToVec(Random.value * 360f) * 0.0001f;
                            }

                            if (!has_collided) {
                                creature_a.Collide(physical_object_b, player_body_chunk.index, body_chunk_b.index);
                                physical_object_b.Collide(creature_a, body_chunk_b.index, player_body_chunk.index);
                            }
                            has_collided = true;
                        }
                    }
                }
            }
        }
        _creatures_in_room_list.Clear();
    }
}
