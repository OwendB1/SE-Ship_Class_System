//Sytems
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Globalization;
//Sandboxs
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using Sandbox.Definitions;
//Vrage
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Network;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using VRage.Network;
using VRage.Sync;
//ProtoBuff
using ProtoBuf;

namespace Blues_Ship_Matrix
{
	[ProtoContract]
	public class MyGridAttributes{
		[ProtoMember(1)]
		public float	Thruster_Force{get; set;}
		[ProtoMember(2)]
		public float	Thruster_Efficiency{get; set;}
		[ProtoMember(3)]
		public float	Gyro_Force {get; set;}
		[ProtoMember(4)]
		public float	Gyro_Efficiency{get; set;}
		[ProtoMember(5)]
		public float	Refine_Efficiency{get; set;}
		[ProtoMember(6)]
		public float	Refine_Speed{get; set;}
		[ProtoMember(7)]
		public float	Assembler_Speed{get; set;}
		[ProtoMember(8)]
		public float	Power_Producers_Output{get; set;}
		[ProtoMember(9)]
		public float 	Drill_Harvest_Mutiplier{get; set;}

		}
	[ProtoContract]
	public class MyGridDamageModifiers{
		[ProtoMember(1)]
		public float	Bullet	{get; set;}
		[ProtoMember(2)]
		public float	Rocket	{get; set;}
		[ProtoMember(3)]
		public float	Explosion {get; set;}
		[ProtoMember(4)]
		public float	Environment {get; set;}
		}
	[ProtoContract]
	public class MyBlockId{
		[ProtoMember(1)]
		public string	TypeId {get; set;}
		[ProtoMember(2)]
		public string	SubtypeId {get; set;}
		[ProtoMember(3)]
		public int		CountWeight {get; set;}
		}
	[ProtoContract]
	public class MyBlockLimit{
		[ProtoMember(1)]
		public string 			Name {get; set;}
		[ProtoMember(2)]
		public string 			Warning {get; set;}
		[ProtoMember(3)]
		public List<MyBlockId>	BlockIds{get; set;}
		[ProtoMember(4)]
		public int				MaxCount{get; set;}
		}

	[ProtoContract]
	public class MyGridLimit{
		[ProtoMember(1)]
		public string	Name {get; set;}
		[ProtoMember(2)]
		public int		MaxBlocks {get; set;}
		[ProtoMember(3)]
		public int		MaxPCU {get; set;}
		[ProtoMember(4)]
		public double	MaxMass {get; set;}
		[ProtoMember(5)]
		public string	CubeSize {get; set;}
		[ProtoMember(6)]
		public bool 	isStatic {get; set;}
		[ProtoMember(7)]
		public bool		ForceBroadCast {get; set;}
		[ProtoMember(8)]
		public float 	ForceBroadCastRange{get; set;}
		[ProtoMember(9)]
		public int		MaxPerFaction {get; set;}
		[ProtoMember(10)]
		public int		MaxPerPlayer {get; set;}
		//public MyGridDamageModifiers DamageResistance {get; set;}
		[ProtoMember(11)]
		public MyGridAttributes Modifiers {get; set;}
		[ProtoMember(12)]
		public MyGridDamageModifiers DamageModifiers{get; set;}
		[ProtoMember(13)]
		public List<MyBlockLimit> BlockLimits{get; set;}
		}
	
    [ProtoContract]
    public class ShipMatrixConfig
    {
        public static ShipMatrixConfig Instance;
        public const string Filename = "ShipMatrixConfig.cfg";
	//Speed and Distance Configs
		[ProtoMember(1)]
		public List<string>	IgnoredFactions;
		[ProtoMember(2)]
		public MyGridLimit LargeShip_Basic;
		[ProtoMember(3)]
		public MyGridLimit SmallShip_Basic;
		[ProtoMember(4)]
		public MyGridLimit Station_Basic;
		[ProtoMember(5)]
		public List<MyGridLimit> GridLimits;

        public static ShipMatrixConfig GetDefaults()
        {
            return new ShipMatrixConfig
            {
				IgnoredFactions = new List<string>{"SPRT"},
				LargeShip_Basic = new MyGridLimit
							{
								Name = "Lg",
								MaxBlocks = 2000,
								MaxPCU = 10000,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=false,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=3,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=1,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.0f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
									new MyBlockLimit
									{
										Name="Weapons",
										Warning="Weapons are not allowed on this class!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="LargeGatlingTurret",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="LargeMissileTurret",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallMissileLauncher",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallMissileLauncherReload",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallGatlingGun",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="LargeMissileTurret",
												SubtypeId="",
												CountWeight=1,
											},
											
										},
										MaxCount=0,
									},
								},
							},
				SmallShip_Basic = new MyGridLimit
							{
								Name = "Sg",
								MaxBlocks = 1000,
								MaxPCU = 6000,
								MaxMass = 0.0,
								CubeSize="Small",
								isStatic=false,
								ForceBroadCast=false,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=3,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=1,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.0f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Weapons",
										Warning="Weapons are not allowed on this class!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="LargeGatlingTurret",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="LargeMissileTurret",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallMissileLauncher",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallMissileLauncherReload",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallGatlingGun",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="LargeMissileTurret",
												SubtypeId="",
												CountWeight=1,
											},
											
										},
										MaxCount=0,
									},
								},
							},
				Station_Basic = new MyGridLimit
							{
								Name = "Lg Static",
								MaxBlocks = 2000,
								MaxPCU = 20000,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=false,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=3,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=1,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.0f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
									new MyBlockLimit
									{
										Name="Weapons",
										Warning="Weapons are not allowed on this class!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="LargeGatlingTurret",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="LargeMissileTurret",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallMissileLauncher",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallMissileLauncherReload",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="SmallGatlingGun",
												SubtypeId="",
												CountWeight=1,
											},
											new	MyBlockId{
												TypeId="LargeMissileTurret",
												SubtypeId="",
												CountWeight=1,
											},
											
										},
										MaxCount=0,
									},
								},
							},
				GridLimits = new List<MyGridLimit>{
							new MyGridLimit
							{
								Name = "Large SubGrid",
								MaxBlocks = 100,
								MaxPCU = 2500,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=false,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=0,
								MaxPerFaction=1,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=1,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=3,
									Refine_Speed=50,
									Assembler_Speed=50,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.0f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							new MyGridLimit
							{
								Name = "Small SubGrid",
								MaxBlocks = 1000,
								MaxPCU = 2500,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=false,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=0,
								MaxPerFaction=1,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=1,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=3,
									Refine_Speed=50,
									Assembler_Speed=50,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.0f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							new MyGridLimit
							{
								Name = "Faction Base",
								MaxBlocks = 20000,
								MaxPCU = 100000,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=true,
								ForceBroadCast=true,
								ForceBroadCastRange=20000.0f,
								MaxPerPlayer=0,
								MaxPerFaction=1,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=1,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=3,
									Refine_Speed=50,
									Assembler_Speed=50,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 0.5f,
									Rocket = 0.5f,
									Explosion = 0.5f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							//Start Small Grids
							new MyGridLimit
							{
								Name = "Fighter",
								MaxBlocks = 1000,
								MaxPCU = 5000,
								MaxMass = 0.0,
								CubeSize="Small",
								isStatic=false,
								ForceBroadCast=true,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=0,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=6,
									Thruster_Efficiency=6,
									Gyro_Force=3,
									Gyro_Efficiency=3,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 0.5f,
									Rocket = 3.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							new MyGridLimit
							{
								Name = "Bomber",
								MaxBlocks = 4000,
								MaxPCU = 12000,
								MaxMass = 0.0,
								CubeSize="Small",
								isStatic=false,
								ForceBroadCast=true,
								ForceBroadCastRange=5000.0f,
								MaxPerPlayer=0,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=6,
									Thruster_Efficiency=6,
									Gyro_Force=3,
									Gyro_Efficiency=3,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 0.2f,
									Rocket = 3.0f,
									Explosion = 4.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							//Start Large Grids
							new MyGridLimit
							{
								Name = "Frigate",
								MaxBlocks = 4000,
								MaxPCU = 40000,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=true,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=6,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=1,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.0f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							new MyGridLimit
							{
								Name = "Cruiser",
								MaxBlocks = 7500,
								MaxPCU = 60000,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=true,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=3,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=5,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 0.8f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							new MyGridLimit
							{
								Name = "Destroyer",
								MaxBlocks = 4000,
								MaxPCU = 70000,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=true,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=3,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=0.8f,
									Thruster_Efficiency=2,
									Gyro_Force=3,
									Gyro_Efficiency=3,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.5f,
									Rocket = 1.5f,
									Explosion = 1.5f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							new MyGridLimit
							{
								Name = "BattleShip",
								MaxBlocks = 8000,
								MaxPCU = 80000,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=true,
								ForceBroadCastRange=1000.0f,
								MaxPerPlayer=2,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=3,
									Thruster_Efficiency=1,
									Gyro_Force=3,
									Gyro_Efficiency=1,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.0f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
							new MyGridLimit
							{
								Name = "Carrier",
								MaxBlocks = 10000,
								MaxPCU = 100000,
								MaxMass = 0.0,
								CubeSize="Large",
								isStatic=false,
								ForceBroadCast=true,
								ForceBroadCastRange=20000.0f,
								MaxPerPlayer=1,
								MaxPerFaction=0,
								Modifiers= new MyGridAttributes
								{
									Thruster_Force=5,
									Thruster_Efficiency=10,
									Gyro_Force=5,
									Gyro_Efficiency=10,
									Refine_Efficiency=5,
									Refine_Speed=5,
									Assembler_Speed=5,
								},
								DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 0.5f,
									Rocket = 0.5f,
									Explosion = 0.5f,
									Environment = 0.5f,
								},
								BlockLimits = new List<MyBlockLimit>
								{
									new MyBlockLimit
									{
										Name="Interior Turrents",
										Warning="To Many Interior Turrents!",
										BlockIds=new List<MyBlockId>{
											new	MyBlockId{
												TypeId="InteriorTurret",
												SubtypeId="LargeInteriorTurret",
												CountWeight=1,
											},
										},
										MaxCount=6,
									},
								},
							},
					
				},
            };
        }
		/*public static MyGridLimit CheckClassForNull(MyGridLimit MyClass){
			if(MyClass.Name==null){MyClass.Name="UnNamed Class, why?";}
			if(MyClass.MaxBlocks==null){MyClass.MaxBlocks = 6000;}
			if(MyClass.MaxPCU==null){MyClass.MaxPCU = 60000;}
			if(MyClass.MaxMass==null){MyClass.MaxMass = 0;}
			if(MyClass.CubeSize==null){MyClass.CubeSize = "Large";}
			if(MyClass.ForceBroadCast==null){MyClass.ForceBroadCast = false;}
			if(MyClass.ForceBroadCastRange==null){MyClass.ForceBroadCastRange = 0.0f;}
			if(MyClass.MaxPerPlayer==null){MyClass.MaxPerPlayer = 1;}
			if(MyClass.MaxPerFaction==null){MyClass.MaxPerFaction = 0;}
			if(MyClass.Modifiers==null){MyClass.Modifiers = new MyGridAttributes
								{
									Thruster_Force=1,
									Thruster_Efficiency=1,
									Gyro_Force=1,
									Gyro_Efficiency=1,
									Refine_Efficiency=1,
									Refine_Speed=1,
									Assembler_Speed=1,
								};}
			if(MyClass.Modifiers.Thruster_Force==null){MyClass.Modifiers.Thruster_Force = 1;}
			if(MyClass.Modifiers.Thruster_Efficiency==null){MyClass.Modifiers.Thruster_Efficiency = 1;}
			if(MyClass.Modifiers.Gyro_Force==null){MyClass.Modifiers.Gyro_Force = 1;}
			if(MyClass.Modifiers.Gyro_Efficiency==null){MyClass.Modifiers.Gyro_Efficiency = 1;}
			if(MyClass.Modifiers.Refine_Efficiency==null){MyClass.Modifiers.Refine_Efficiency = 1;}
			if(MyClass.Modifiers.Refine_Speed==null){MyClass.Modifiers.Refine_Speed = 1;}
			if(MyClass.Modifiers.Assembler_Speed==null){MyClass.Modifiers.Assembler_Speed = 1;}	
			if(MyClass.DamageModifiers==null){MyClass.DamageModifiers = new MyGridDamageModifiers
								{
									Bullet = 1.0f,
									Rocket = 1.0f,
									Explosion = 1.0f,
									Environment = 1.0f,
								};}
			if(MyClass.DamageModifiers.Bullet==null){MyClass.DamageModifiers.Bullet = 1.0f;}		
			if(MyClass.DamageModifiers.Rocket==null){MyClass.DamageModifiers.Rocket = 1.0f;}
			if(MyClass.DamageModifiers.Explosion==null){MyClass.DamageModifiers.Explosion = 1.0f;}
			if(MyClass.DamageModifiers.Environment==null){MyClass.DamageModifiers.Environment = 1.0f;}	
			if(MyClass.BlockLimits==null){MyClass.BlockLimits = new List<MyBlockLimit>();}
			foreach(MyBlockLimit Limit in MyClass.BlockLimits)
			{
				if(Limit.Name==null){Limit.Name="Un-Named Block Limit";}
				if(Limit.Warning==null){Limit.Warning="Un-Named Block Limit";}
				foreach(MyBlockId Block in Limit.BlockIds)
				{
					if(Block.TypeId==null){Block.TypeId="NotARealBlock";}
					if(Block.SubtypeId==null){Block.SubtypeId="NotARealBlock";}
					if(Block.CountWeight==null){Block.CountWeight=0;}
				}
				if(Limit.MaxCount==null){Limit.MaxCount=0;}
			}
			return (MyClass);
		}*/
        public static ShipMatrixConfig Load()
        {
            ShipMatrixConfig Default = GetDefaults();
            ShipMatrixConfig WorldSettings = GetDefaults();
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(Filename, typeof(ShipMatrixConfig)))
                {
                    TextReader Reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(Filename, typeof(ShipMatrixConfig));
                    string mytext = Reader.ReadToEnd();
                    Reader.Close();
                    WorldSettings = MyAPIGateway.Utilities.SerializeFromXML<ShipMatrixConfig>(mytext);
                    if (WorldSettings == null) { throw new System.Exception("Word Settings Empty! :(.... \n ...Fixed!"); }
					/*try{
						WorldSettings.LargeShip_Basic = CheckClassForNull(ShipMatrixConfig.Instance.LargeShip_Basic);
						WorldSettings.SmallShip_Basic = CheckClassForNull(ShipMatrixConfig.Instance.SmallShip_Basic);
						WorldSettings.Station_Basic = CheckClassForNull(ShipMatrixConfig.Instance.Station_Basic);
						}catch(Exception x){MyAPIGateway.Utilities.ShowMessage("Debug", $"Your GRidClass didn't load becuase...\n{x.Message}");}
                    for (int i = 0; i < WorldSettings.GridLimits.Count; i++)
                    {
						try{WorldSettings.GridLimits[i] = CheckClassForNull(WorldSettings.GridLimits[i]);}
						catch(Exception x){MyAPIGateway.Utilities.ShowMessage("Debug", $"Your GRidClass didn't load becuase...\n{x.Message}");}
                    }*/
                }

            }
            catch (Exception x)
            {
                WorldSettings = Default;
                MyAPIGateway.Utilities.ShowMessage("Debug", $"Blue's sketchy ConfigLoad crashed because...\n{x.Message}");
            }

            return WorldSettings;
        }
        public static void Save(ShipMatrixConfig CurrentConfig)
        {
            try
            {
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(Filename, typeof(ShipMatrixConfig));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(CurrentConfig));
                writer.Close();
            }
            catch (Exception x)
            {
                MyAPIGateway.Utilities.ShowMessage("Debug", $"Blue's sketchy ConfigSave crashed because...\n{x.Message}");
            }
        }
		
        public static void SaveClient(ShipMatrixConfig CurrentConfig)
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(Filename, typeof(ShipMatrixConfig)))
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(Filename, typeof(ShipMatrixConfig));
                    string text = reader.ReadToEnd();
                    reader.Close();

                    var settingsClient = MyAPIGateway.Utilities.SerializeFromXML<ShipMatrixConfig>(text);

                    if (settingsClient != CurrentConfig)
					{
                        Save(CurrentConfig);
					}
				}else
				{
					Save(CurrentConfig);
				}
            }
            catch (Exception x)
            {
                MyAPIGateway.Utilities.ShowMessage("Debug", $"Blue's sketchy ClientBS crashed because...\n{x.Message}");
            }
        }
		

    }


}