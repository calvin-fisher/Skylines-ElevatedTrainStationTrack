﻿using System;
using System.Linq;
using UnityEngine;

namespace ElevatedTrainStationTrack
{
    public class Initializer : AbstractInitializer
    {
        protected override void InitializeImpl()
        {
            //for compatibility, never change this prefab's name
            CreatePrefab("Station Track Eleva", "Train Station Track", Util.Apply<NetInfo, bool, bool>(SetupElevatedPrefab, false, false));
            CreatePrefab("Station Track Elevated (C)", "Train Station Track", Util.Apply<NetInfo, bool, bool>(SetupElevatedPrefab, false, true));
            CreatePrefab("Station Track Elevated (NP)", "Train Station Track", Util.Apply<NetInfo, bool, bool>(SetupElevatedPrefab, true, false));
            CreatePrefab("Station Track Elevated (CNP)", "Train Station Track", Util.Apply<NetInfo, bool, bool>(SetupElevatedPrefab, true, true));

            MakePlatformsNarrow(CreatePrefab("Station Track Elevated Narrow (C)", "Train Station Track", Util.Apply<NetInfo, bool, bool>(SetupElevatedPrefab, false, true)));
            
            //for compatibility, never change this prefab's name
            CreatePrefab("Station Track Sunken", "Train Station Track", Util.Apply<NetInfo, bool>(SetupSunkenPrefab, false));
            CreatePrefab("Station Track Sunken (NP)", "Train Station Track", Util.Apply<NetInfo, bool>(SetupSunkenPrefab, true));

            CreatePrefab("Train Station Track (C)", "Train Station Track", Util.Apply<NetInfo, bool, bool>(SetupRegularPrefab, false, true));
            CreatePrefab("Train Station Track (NP)", "Train Station Track", Util.Apply<NetInfo, bool, bool>(SetupRegularPrefab, true, false));
            CreatePrefab("Train Station Track (CNP)", "Train Station Track", Util.Apply<NetInfo, bool, bool>(SetupRegularPrefab, true, true));

            //for compatibility, never change this prefab's name
            CreatePrefab("Station Track Tunnel", "Train Station Track", SetupTunnelPrefab);
        }

        private static void MakePlatformsNarrow(NetInfo stationTrack)
        {
            if (stationTrack != null && stationTrack.m_lanes != null)
            {
                foreach (var lane in stationTrack.m_lanes)
                {
                    if (lane == null || lane.m_laneType != NetInfo.LaneType.Pedestrian) continue;

                    lane.m_width = 2;
                    lane.m_position = Math.Sign(lane.m_position) * (4 + .5f * lane.m_width);
                }
            }
        }

        private static void SetupElevatedPrefab(NetInfo elevatedPrefab, bool removePoles, bool concrete)
        {
            var stationAI = elevatedPrefab.GetComponent<TrainTrackAI>();
            stationAI.m_elevatedInfo = elevatedPrefab;

            elevatedPrefab.m_followTerrain = false;
            elevatedPrefab.m_flattenTerrain = false;
            elevatedPrefab.m_createGravel = false;
            elevatedPrefab.m_createPavement = false;
            elevatedPrefab.m_createRuining = false;
            elevatedPrefab.m_requireSurfaceMaps = false;
            elevatedPrefab.m_clipTerrain = false;
            elevatedPrefab.m_snapBuildingNodes = false;
            elevatedPrefab.m_placementStyle = ItemClass.Placement.Procedural;
            elevatedPrefab.m_useFixedHeight = true;
            elevatedPrefab.m_lowerTerrain = true;
            elevatedPrefab.m_availableIn = ItemClass.Availability.GameAndAsset;
            if (removePoles)
            {
                RemoveElectricityPoles(elevatedPrefab);
            }
            var elevatedTrack = FindOriginalPrefab("Train Track Elevated");
            if (elevatedTrack == null)
            {
                return;
            }
            var etstMesh = Util.LoadMesh(string.Concat(Util.AssemblyDirectory, "/TTNR.obj"), "ETST ");
            var etstSegmentLodMesh = Util.LoadMesh(string.Concat(Util.AssemblyDirectory, "/TTNR_LOD.obj"), "ETST_SLOD");
            var etstNodeLodMesh = Util.LoadMesh(string.Concat(Util.AssemblyDirectory, "/TTNR_Node_LOD.obj"), "ETST_NLOD");
            elevatedPrefab.m_segments[0].m_segmentMaterial = ModifyRailMaterial(elevatedTrack.m_segments[0].m_segmentMaterial, concrete);
            elevatedPrefab.m_segments[0].m_material = ModifyRailMaterial(elevatedTrack.m_segments[0].m_material, concrete);
            elevatedPrefab.m_segments[0].m_mesh = etstMesh;
            elevatedPrefab.m_segments[0].m_segmentMesh = etstMesh;
            elevatedPrefab.m_segments[0].m_lodMaterial = ModifyRailMaterial(elevatedTrack.m_segments[0].m_lodMaterial, concrete);
            elevatedPrefab.m_segments[0].m_lodMesh = etstSegmentLodMesh;
            elevatedPrefab.m_nodes[0].m_material = ModifyRailMaterial(elevatedTrack.m_nodes[0].m_material, concrete);
            elevatedPrefab.m_nodes[0].m_nodeMaterial = ModifyRailMaterial(elevatedTrack.m_nodes[0].m_nodeMaterial, concrete);
            elevatedPrefab.m_nodes[0].m_lodMaterial = ModifyRailMaterial(elevatedTrack.m_nodes[0].m_lodMaterial, concrete);
            elevatedPrefab.m_nodes[0].m_lodMesh = etstNodeLodMesh;
            elevatedPrefab.m_nodes[0].m_nodeMesh = etstMesh;
            elevatedPrefab.m_nodes[0].m_mesh = etstMesh;
        }

        private static void RemoveElectricityPoles(NetInfo prefab)
        {
            foreach (var lane in prefab.m_lanes)
            {
                var mLaneProps = lane.m_laneProps;
                if (mLaneProps == null)
                {
                    continue;
                }
                var props = mLaneProps.m_props;
                if (props == null)
                {
                    continue;
                }
                lane.m_laneProps = new NetLaneProps
                {
                    m_props = (from prop in props
                               where prop != null
                               let mProp = prop.m_prop
                               where mProp != null
                               where mProp.name != "RailwayPowerline"
                               select prop).ToArray()
                };
            }
        }

        private static Material ModifyRailMaterial(Material material, bool concrete)
        {
            if (!concrete)
            {
                return material;
            }
            var newMaterial = new Material(material)
            {
                name = string.Format("{0}-concrete", material.name),
                shader = Shader.Find("Custom/Net/RoadBridge")
            };
            return newMaterial;
        }

        private static void SetupTunnelPrefab(NetInfo tunnelPrefab)
        {
            SetupSunkenPrefab(tunnelPrefab, false);
            tunnelPrefab.m_canCollide = false;
            foreach (var lane in tunnelPrefab.m_lanes)
            {
                lane.m_laneProps = null;
            }
            var metroStation = FindOriginalPrefab("Metro Station Track");
            if (metroStation != null)
            {
                tunnelPrefab.m_segments = new[] { metroStation.m_segments[0] };
                //TODO(earalov): make a shallow copy of segment and change some properties
                tunnelPrefab.m_nodes = new[] { metroStation.m_nodes[0] };
                //TODO(earalov): make a shallow copy of segment and change some properties
            }
            else
            {
                Debug.LogWarning("ElevatedTrainStationTrack - Couldn't find metro station track");
            }
        }

        private static void SetupSunkenPrefab(NetInfo sunkenPrefab, bool removePoles)
        {
            var stationAI = sunkenPrefab.GetComponent<TrainTrackAI>();
            stationAI.m_tunnelInfo = sunkenPrefab;

            sunkenPrefab.m_clipTerrain = false;

            sunkenPrefab.m_createGravel = false;
            sunkenPrefab.m_createPavement = false;
            sunkenPrefab.m_createRuining = false;

            sunkenPrefab.m_flattenTerrain = false;
            sunkenPrefab.m_followTerrain = false;

            sunkenPrefab.m_intersectClass = null;

            sunkenPrefab.m_maxHeight = -1;
            sunkenPrefab.m_minHeight = -3;

            sunkenPrefab.m_requireSurfaceMaps = false;
            sunkenPrefab.m_snapBuildingNodes = false;

            sunkenPrefab.m_placementStyle = ItemClass.Placement.Procedural;
            sunkenPrefab.m_useFixedHeight = true;
            sunkenPrefab.m_lowerTerrain = false;
            sunkenPrefab.m_availableIn = ItemClass.Availability.GameAndAsset;
            if (removePoles)
            {
                RemoveElectricityPoles(sunkenPrefab);
            }
        }

        private static void SetupRegularPrefab(NetInfo stationTrack, bool removePoles, bool concrete)
        {
            if (removePoles)
            {
                RemoveElectricityPoles(stationTrack);
            }
            if (concrete)
            {
                stationTrack.m_createGravel = false;
                stationTrack.m_createRuining = false;
                stationTrack.m_createPavement = true;
            }
        }
    }
}
