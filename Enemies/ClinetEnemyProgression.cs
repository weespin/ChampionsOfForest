﻿//Stores info about enemy stats, shared between players in coop

using TheForest.Utils;

using UnityEngine;

namespace ChampionsOfForest
{
	public class ClinetEnemyProgression
	{
		public struct DynamicClientEnemyProgression
		{
			public float Health;
			public int Armor;
			public int ArmorReduction;

			public DynamicClientEnemyProgression(float health, int armor, int armorReduction)
			{
				Health = health;
				Armor = armor;
				ArmorReduction = armorReduction;
			}
		}
		DynamicClientEnemyProgression dynCEP;
		public const float LifeTime = 50;
		public const float DynamicLifeTime = 0.5f;
		public BoltEntity Entity;
		public ulong Packed;
		public string EnemyName;
		public int Level;
		public float MaxHealth;
		public long ExpBounty;
		public float Steadfast;
		public int[] Affixes;
		public float creationTime;
		public float dynamicCreationTime;
		public float Health => dynCEP.Health;
		public float Armor => dynCEP.Armor;
		public float ArmorReduction => dynCEP.ArmorReduction;

		/// <summary>
		/// host/singleplayer constructor
		/// </summary>
		/// <param name="tr"></param>
		public ClinetEnemyProgression(Transform tr)
		{
			creationTime = Time.time;
			EnemyProgression p = tr.GetComponent<EnemyProgression>();
			if (p == null)
			{
				p = tr.GetComponentInChildren<EnemyProgression>();
			}
			if (p != null)
			{
				EnemyName = p.enemyName;
				dynCEP = new DynamicClientEnemyProgression(p.extraHealth + p.HealthScript.Health, p.armor, p.armorReduction);
				Level = p.level;
				MaxHealth = p.maxHealth;
				ExpBounty = p.bounty;
				Steadfast = p.Steadfast;
				Affixes = new int[p.abilities.Count];
				for (int i = 0; i < p.abilities.Count; i++)
				{
					Affixes[i] = (int)p.abilities[i];
				}
			}
		}
		public void RequestDynamicUpdate()
		{
			if (GameSetup.IsMpClient && DynamicOutdated)
			{
				Network.Commands.Command_UpdateDynamicCP.Send(Network.NetworkManager.Target.OnlyServer, new Network.Commands.UpdateCProgressionCommandParam() { packed = Entity.networkId.Packed });
				dynamicCreationTime = Time.time;
			}
		}
		public bool DynamicOutdated => dynamicCreationTime + DynamicLifeTime < Time.time;
		public void UpdateDynamic(float hp, int ar, int arred)
		{
			dynCEP = new DynamicClientEnemyProgression(hp,ar,arred);
			dynamicCreationTime = Time.time;

		}
		public ClinetEnemyProgression(BoltEntity entity)
		{
			if (!EnemyManager.clinetProgressions.ContainsKey(entity))
			{
				EnemyManager.clinetProgressions.Add(entity, this);
			}
		}

		//if (GameSetup.IsMpClient)
		//{
		//	using (System.IO.MemoryStream answerStream = new System.IO.MemoryStream())
		//	{
		//		using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(answerStream))
		//		{
		//			w.Write(6);
		//			w.Write(Packed);
		//			w.Close();
		//		}
		//		ChampionsOfForest.Network.NetworkManager.SendLine(answerStream.ToArray(), ChampionsOfForest.Network.NetworkManager.Target.OnlyServer);
		//		answerStream.Close();
		//	}
		//}

		//}
		//public static ClinetEnemyProgression Update(BoltEntity entity, float hp, int ar, int arred)
		//{
		//	if (EnemyManager.clinetProgressions.ContainsKey(entity))
		//	{
		//		var cp = EnemyManager.clinetProgressions[entity];
		//		cp.dynCEP = new DynamicClientEnemyProgression(hp,ar,arred)
		//		return cp;
		//	}
		//	return null;
		//}
		public void Update(BoltEntity entity, string enemyName, int level, float health, float maxHealth, long expBounty, int armor, int armorReduction, float Steadfast, int[] affixes)
		{
			Entity = entity;
			EnemyName = enemyName;
			if(entity != null)
			Packed = entity.networkId.PackedValue;
			Level = level;
			dynCEP = new DynamicClientEnemyProgression(health, armor, armorReduction);
			MaxHealth = maxHealth;
			ExpBounty = expBounty;
			this.Steadfast = Steadfast;
			Affixes = affixes;
			creationTime = Time.time;
			dynamicCreationTime = Time.time;

		}
	}
}