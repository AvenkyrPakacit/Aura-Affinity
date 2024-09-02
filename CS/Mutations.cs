using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class AU_ChaosRay : BaseMutation
	{
		public override bool GeneratesEquipment()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "AU_CommandChaosRay");
			Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
			base.Register(Object);
		}

		public override string GetDescription()
		{
			return "You emit a ray of chaotic embers from your (hands, feet, face).";
		}

		public override string GetLevelText(int Level)
		{
			return "Emits a 9-square ray of chaotic embers in the direction of your choice\n" + "Cooldown: 10 rounds\n" + "Damage: " + this.ComputeDamage(Level);
		}

		public string ComputeDamage(int UseLevel)
		{
			string text = UseLevel + "d4";
			if (this.ParentObject != null)
			{
				int partCount = this.ParentObject.GetPart<Body>().GetPartCount(this.BodyPartType);
				if (partCount > 0)
				{
					text = text + "+" + partCount;
				}
			}
			else
			{
				text += "+1";
			}
			return text;
		}

		public string ComputeDamage()
		{
			return this.ComputeDamage(base.Level);
		}

		public void Flame(Cell C, ScreenBuffer Buffer, bool doEffect = true)
		{
			string dice = this.ComputeDamage();
			if (C != null)
			{
				foreach (GameObject gameObject in C.GetObjectsInCell())
				{
					if (gameObject.PhaseMatches(this.ParentObject))
					{
						gameObject.TemperatureChange(430 - 32 * base.Level, this.ParentObject, false, false, false, 0, null, null);
						if (doEffect)
						{
							for (int i = 0; i < 5; i++)
							{
								gameObject.ParticleText("&g" + ((char)(219 + Stat.Random(0, 4))).ToString(), 2.9f, 1);
							}
							for (int j = 0; j < 5; j++)
							{
								gameObject.ParticleText("&G" + ((char)(219 + Stat.Random(0, 4))).ToString(), 2.9f, 1);
							}
							for (int k = 0; k < 5; k++)
							{
								gameObject.ParticleText("&M" + ((char)(219 + Stat.Random(0, 4))).ToString(), 2.9f, 1);
							}
						}
					}
				}
				DieRoll cachedDieRoll = dice.GetCachedDieRoll();
				foreach (GameObject gameObject2 in C.GetObjectsWithPartReadonly("Combat"))
				{
					if (gameObject2.PhaseMatches(this.ParentObject))
					{
						Damage damage = new Damage(cachedDieRoll.Resolve());
						damage.AddAttribute("Acid");
						damage.AddAttribute("Cold");
						damage.AddAttribute("Electrical");
						damage.AddAttribute("Heat");
						Event @event = Event.New("TakeDamage", 0, 0, 0);
						@event.SetParameter("Damage", damage);
						@event.SetParameter("Owner", this.ParentObject);
						@event.SetParameter("Attacker", this.ParentObject);
						@event.SetParameter("Message", "from %o chaotic embers!");
						gameObject2.FireEvent(@event);
					}
				}
			}
			if (doEffect)
			{
				Buffer.Goto(C.X, C.Y);
				string str = "&C";
				int num = Stat.Random(1, 3);
				if (num == 1)
				{
					str = "&G";
				}
				if (num == 2)
				{
					str = "&g";
				}
				if (num == 3)
				{
					str = "&M";
				}
				int num2 = Stat.Random(1, 3);
				if (num2 == 1)
				{
					str += "^G";
				}
				if (num2 == 2)
				{
					str += "^g";
				}
				if (num2 == 3)
				{
					str += "^M";
				}
				if (C.ParentZone == XRLCore.Core.Game.ZoneManager.ActiveZone)
				{
					Stat.Random(1, 3);
					Buffer.Write(str + ((char)(219 + Stat.Random(0, 4))).ToString(), true);
					Popup._TextConsole.DrawBuffer(Buffer, null, false);
					Thread.Sleep(10);
				}
			}
		}

		public static bool Cast(AU_ChaosRay mutation = null, string level = "5-6")
		{
			if (mutation == null)
			{
				mutation = new AU_ChaosRay();
				mutation.Level = Stat.Roll(level, null);
				mutation.ParentObject = XRLCore.Core.Game.Player.Body;
			}
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(true);
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			List<Cell> list = mutation.PickLine(9, AllowVis.Any, null, false, null);
			if (list == null)
			{
				return true;
			}
			if (list.Count <= 0)
			{
				return true;
			}
			if (list != null)
			{
				if (list.Count == 1 && mutation.ParentObject.IsPlayer() && Popup.ShowYesNoCancel("Are you sure you want to target yourself?") != DialogResult.Yes)
				{
					return true;
				}
				mutation.CooldownMyActivatedAbility(mutation.AU_ChaosRayActivatedAbilityID, 10, null);
				mutation.UseEnergy(1000);
				mutation.PlayWorldSound(mutation.Sound, 0.5f, 0f, true, null);
				int num = 0;
				while (num < 9 && num < list.Count)
				{
					if (list.Count == 1 || list[num] != mutation.ParentObject.pPhysics.CurrentCell)
					{
						mutation.Flame(list[num], scrapBuffer, true);
					}
					foreach (GameObject gameObject in list[num].LoopObjectsWithPart("Physics"))
					{
						if (gameObject.pPhysics.Solid && gameObject.GetIntProperty("AllowMissiles", 0) == 0)
						{
							Forcefield part = gameObject.GetPart<Forcefield>();
							if (part == null || !part.CanMissilePassFrom(mutation.ParentObject, null))
							{
								num = 999;
								break;
							}
						}
					}
					num++;
				}
			}
			return true;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "AIGetOffensiveMutationList")
			{
				if (E.GetIntParameter("Distance", 0) <= 9 && base.IsMyActivatedAbilityAIUsable(this.AU_ChaosRayActivatedAbilityID, null) && this.ParentObject.HasLOSTo(E.GetGameObjectParameter("Target"), true, true, null))
				{
					E.AddAICommand("AU_CommandChaosRay", 1, null, false);
				}
			}
			else if (E.ID == "AU_CommandChaosRay")
			{
				return AU_ChaosRay.Cast(this, "5-6");
			}
			return true;
		}

		public override bool ChangeLevel(int NewLevel)
		{
			if (GameObject.validate(ref this.FlamesObject))
			{
				this.FlamesObject.GetPart<TemperatureOnHit>().Amount = base.Level * 2 + "d12";
			}
			return base.ChangeLevel(NewLevel);
		}

		private void AddAbility()
		{
			this.AU_ChaosRayActivatedAbilityID = base.AddMyActivatedAbility("Chaos Ray", "AU_CommandChaosRay", "Physical Mutation", -1, null, "\a", false, false, false, false, false, false, null);
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			this.Unmutate(GO);
			if (this.CreateObject)
			{
				Body part = GO.GetPart<Body>();
				if (part != null)
				{
					BodyPart firstPart = part.GetFirstPart(this.BodyPartType);
					if (firstPart != null)
					{
						GO.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", firstPart));
						this.FlamesObject = GameObject.create("Chaotic Ray");
						this.FlamesObject.GetPart<Armor>().WornOn = firstPart.Type;
						Event @event = Event.New("CommandForceEquipObject", 0, 0, 0);
						@event.SetParameter("Object", this.FlamesObject);
						@event.SetParameter("BodyPart", firstPart);
						@event.SetSilent(true);
						GO.FireEvent(@event);
						this.AddAbility();
					}
				}
			}
			else
			{
				this.AddAbility();
			}
			this.ChangeLevel(Level);
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			base.CleanUpMutationEquipment(GO, ref this.FlamesObject);
			base.RemoveMyActivatedAbility(ref this.AU_ChaosRayActivatedAbilityID, null);
			return base.Unmutate(GO);
		}

		public string BodyPartType = "Hands,Feet,Face";

		public bool CreateObject = true;

		public string Sound = "burn_crackling";

		public GameObject FlamesObject;

		public Guid AU_ChaosRayActivatedAbilityID = Guid.Empty;
	}
}