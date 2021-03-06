﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HaloOnlineTagTool.Serialization;

namespace HaloOnlineTagTool.TagStructures
{
	[TagStructure(Class = "devi", Size = 0x98)]
	public class Device : GameObject
	{
		public uint Flags2;
		public float PowerTransitionTime;
		public float PowerAccelerationTime;
		public float PositionTransitionTime;
		public float PositionAccelerationTime;
		public float DepoweredPositionTransitionTime;
		public float DepoweredPositionAccelerationTime;
		public uint LightmapFlags;
		public HaloTag OpenUp;
		public HaloTag CloseDown;
		public HaloTag Opened;
		public HaloTag Closed;
		public HaloTag Depowered;
		public HaloTag Repowered;
		public float DelayTime;
		public HaloTag DelayEffect;
		public float AutomaticActivationRadius;
	}
}
