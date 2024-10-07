using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace flakysalt.CharacterKeybinds.Data
{
	public static class ClickPositions
	{
		public static List<Point> importClickPositions = new List<Point>
		{
			new Point(-280,-145),	//dropdown
			new Point(236,320),		//First Item in dropdown 
			new Point(236,358),		//import button
			new Point(-28,54)		//yes button in confirmation window
		};
	}
}
