﻿using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;
namespace ChampionsOfForest
{
    public class ModReferences : MonoBehaviour
    {
        private float LevelRequestCooldown = 10;
        private float MFindRequestCooldown = 300;

        public static ClinetItemPicker ItemPicker => ClinetItemPicker.Instance;
        public static List<GameObject> Players
        {
            get;
            private set;
        }

        public static string ThisPlayerID
        {
            get;
            private set;
        }
        public static List<BoltEntity> AllPlayerEntities = new List<BoltEntity>();
        public static Dictionary<string, int> PlayerLevels = new Dictionary<string, int>();
        public static Dictionary<string, Transform> PlayerHands = new Dictionary<string, Transform>();
        public static Transform rightHandTransform = null;

        private void Start()
        {
            if (BoltNetwork.isRunning)
            {
                Players = new List<GameObject>();
                StartCoroutine(InitPlayerID());
                StartCoroutine(UpdateSetups());
                if (GameSetup.IsMpServer && BoltNetwork.isRunning)
                {
                    InvokeRepeating("UpdateLevelData", 1, 1);
                }
            }
            else
            {
                Players = new List<GameObject>() { LocalPlayer.GameObject };
            }


        }

        private IEnumerator InitPlayerID()
        {
            if (ModSettings.IsDedicated)
            {
                yield break;
            }

            while (LocalPlayer.Entity == null)
            {
                yield return null;
            }
            ThisPlayerID = LocalPlayer.Entity.GetState<IPlayerState>().name;
            ThisPlayerID.Replace(';', '0');
            ThisPlayerID.TrimNonAscii();

        }


        private void UpdateLevelData()
        {
            if (Players.Count > 1)
            {
                LevelRequestCooldown -= 1;
                if (LevelRequestCooldown < 0)
                {
                    LevelRequestCooldown = 90;
                    using (System.IO.MemoryStream answerStream = new System.IO.MemoryStream())
                    {
                        using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(answerStream))
                        {
                            w.Write(18);
                            w.Write("x");
                        w.Close();
                        }
                        ChampionsOfForest.Network.NetworkManager.SendLine(answerStream.ToArray(), ChampionsOfForest.Network.NetworkManager.Target.Clients);
                        answerStream.Close();
                    }
                }
                else if (Players.Count != PlayerLevels.Count + 1)
                {
                    LevelRequestCooldown = 90;
                    PlayerLevels.Clear();
                    //PlayerLevels.Add(ThisPlayerPacked, ModdedPlayer.instance.Level);
                    using (System.IO.MemoryStream answerStream = new System.IO.MemoryStream())
                    {
                        using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(answerStream))
                        {
                            w.Write(18);
                            w.Write("x");
                        w.Close();
                        }
                        ChampionsOfForest.Network.NetworkManager.SendLine(answerStream.ToArray(), ChampionsOfForest.Network.NetworkManager.Target.Clients);
                        answerStream.Close();
                    }
                }
                MFindRequestCooldown--;
                if (MFindRequestCooldown <= 0)
                {
                    using (System.IO.MemoryStream answerStream = new System.IO.MemoryStream())
                    {
                        using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(answerStream))
                        {
                            w.Write(23);
                        w.Close();
                        }
                        ChampionsOfForest.Network.NetworkManager.SendLine(answerStream.ToArray(), ChampionsOfForest.Network.NetworkManager.Target.Everyone);
                        answerStream.Close();
                    }
                    MFindRequestCooldown = 300;

                }

                if (PlayerHands.ContainsValue(null))
                {
                    PlayerHands.Clear();
                }
            }
            else
            {
                PlayerLevels.Clear();

            }
        }
        public static void Host_RequestLevels()
        {
            if (GameSetup.IsMpServer)
            {
                using (System.IO.MemoryStream answerStream = new System.IO.MemoryStream())
                {
                    using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(answerStream))
                    {
                        w.Write(18);
                        w.Write("x");
                    w.Close();
                    }
                    ChampionsOfForest.Network.NetworkManager.SendLine(answerStream.ToArray(), ChampionsOfForest.Network.NetworkManager.Target.Clients);
                    answerStream.Close();
                }
            }
        }
        public static float DamageReduction(int armor)
        {
            armor = Mathf.Max(armor, 0);

            float arReduction = 1;
            arReduction *= armor;
            arReduction /= armor + 30*10;

            return arReduction;
        }



        public static void FindHands()
        {
            PlayerHands.Clear();

            try
            {
                for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
                {
                    if (Scene.SceneTracker.allPlayers[i].transform.root != LocalPlayer.Transform)
                    {
                        var entity = Scene.SceneTracker.allPlayers[i].GetComponent<BoltEntity>();
                        if (entity == null) continue;
                        var state = entity.GetState<IPlayerState>();
                        if (state == null) continue;

                        string playerName = state.name.TrimNonAscii();
                        if (!string.IsNullOrEmpty(playerName))
                        {
                            Transform hand = FindDeepChild(Scene.SceneTracker.allPlayers[i].transform.root, "rightHandHeld");

                            if (hand != null)
                            {

                                PlayerHands.Add(playerName, hand);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModAPI.Log.Write(e.ToString());
            }
        }


        private IEnumerator UpdateSetups()
        {

            while (true)
            {
                yield return null;
                Players.Clear();
                AllPlayerEntities.Clear();
                for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
                {
                    Players.Add(Scene.SceneTracker.allPlayers[i]);
                    BoltEntity b = Scene.SceneTracker.allPlayers[i].GetComponent<BoltEntity>();
                    if (b != null)
                    {
                        AllPlayerEntities.Add(b);
                    }
                }



                yield return new WaitForSeconds(10);

            }
        }

        public static string ListAllChildren(Transform tr, string prefix)
        {
            string s = prefix + "•" + tr.name + "\n";
            foreach (Transform child in tr)
            {
                s += ListAllChildren(child, prefix + "\t");
            }
            return s;
        }
        public static Transform FindDeepChild(Transform aParent, string aName)
        {
            Transform result = aParent.Find(aName);
            if (result != null)
            {
                return result;
            }

            foreach (Transform child in aParent)
            {
                result = FindDeepChild(child, aName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        //finds the hand transform of client
        public static Transform FindHandRetardedWay(Transform root)
        {
            try
            {
                return root.Find("player_BASE").Find("char_Hips").Find("char_Spine").Find("char_Spine1").Find("char_Spine2").Find("char_RightShoulder").Find("char_RightArm").Find("char_RightForeArm").Find("har_RightHand").Find("char_RightHandWeapon").Find("rightHandHeld");
            }
            catch (Exception e)
            {

                ModAPI.Log.Write("couldnt find hand " + e.ToString());

            }
            return null;

        }
    }
}
