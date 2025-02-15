﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PocceMod.Client
{
    public class Common : BaseScript
    {
        public Common()
        {
            EventHandlers["PocceMod:Burn"] += new Action<int>(entity => API.StartEntityFire(API.NetToEnt(entity)));
        }

        public static async Task RequestModel(uint model)
        {
            while (!API.HasModelLoaded(model))
            {
                API.RequestModel(model);
                await Delay(10);
            }
        }

        public static bool GetClosestEntity(IEnumerable<int> entities, out int closest)
        {
            closest = -1;
            bool found = false;
            float minDist = float.MaxValue;
            var coords = API.GetEntityCoords(API.GetPlayerPed(-1), true);

            foreach (var entity in entities)
            {
                var pos = API.GetEntityCoords(entity, API.IsEntityAPed(entity));
                var dist = coords.DistanceToSquared(pos);

                if (dist < minDist)
                {
                    closest = entity;
                    minDist = dist;
                    found = true;
                }
            }

            return found;
        }

        public static bool IsEntityInRangeSquared(int entity, float rangeSquared)
        {
            var playerPos = API.GetEntityCoords(API.GetPlayerPed(-1), true);
            var entityPos = API.GetEntityCoords(entity, false);
            return playerPos.DistanceToSquared(entityPos) <= rangeSquared;
        }

        public static bool GetWaypoint(out Vector3 wp, bool adjust = true)
        {
            wp = Vector3.Zero;

            if (!API.IsWaypointActive())
                return false;

            wp = API.GetBlipInfoIdCoord(API.GetFirstBlipInfoId(8));

            if (adjust)
            {
                var adjustedWp = Vector3.Zero;
                if (API.GetClosestVehicleNode(wp.X, wp.Y, wp.Z, ref adjustedWp, 1, 100f, 2.5f))
                    wp = adjustedWp;
            }

            return true;
        }

        public static List<int> GetObjects()
        {
            var objs = new List<int>();
            int obj = 0;
            int handle = API.FindFirstObject(ref obj);
            var coords = API.GetEntityCoords(API.GetPlayerPed(-1), true);

            if (handle == -1)
                return objs;

            do
            {
                objs.Add(obj);

            } while (API.FindNextObject(handle, ref obj));

            API.EndFindObject(handle);
            return objs;
        }

        public static void Burn(int entity)
        {
            TriggerServerEvent("PocceMod:Burn", API.ObjToNet(entity));
        }

        public static void GetAimCoords(out Vector3 position, out Vector3 target, float distance)
        {
            position = API.GetGameplayCamCoords();
            var rot = API.GetGameplayCamRot(2);
            var forward = RotationToDirection(rot) * 10f;
            target = position + forward;
        }

        public static Vector3 RotationToDirection(Vector3 rot)
        {
            float radiansZ = rot.Z * 0.0174532924f;
            float radiansX = rot.X * 0.0174532924f;
            float num = Math.Abs((float)Math.Cos(radiansX));
            return new Vector3
            {
                X = -(float)Math.Sin(radiansZ) * num,
                Y = (float)Math.Cos(radiansZ) * num,
                Z = (float)Math.Sin(radiansX)
            };
        }
    }
}
