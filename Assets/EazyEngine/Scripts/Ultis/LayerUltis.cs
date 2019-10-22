using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EazyEngine.ECS.Ultis
{
	public class EzLayers
	{
		public static bool LayerInLayerMask(int layer, LayerMask layerMask)
		{
			if (((1 << layer) & layerMask) != 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}

