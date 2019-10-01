using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace sthv
{
	public class sthvMapModel
	{
		public Vector4[] CarSpawnpoints { get; set; }
		public Vector4[] HeliSpawnPoints { get; set; }
		public Vector4 HunterSpawn { get; set; }
		public Vector4 RunnerSpawn { get; set; }
		public Vector2 AreaCenter { get; set; }
		public float Radius { get; set; }
	}
	public static class sthvMaps
	{
		public static sthvMapModel[] Maps = new sthvMapModel[] {
			new sthvMapModel
			{
				CarSpawnpoints = new Vector4[] {
					new Vector4(-222.5026f, -265.7659f, 49f, 69.05865f),
					new Vector4(-222.2889f, -269.8353f, 49f, 69.14626f),
					new Vector4(-223.4765f, -273.1299f, 49f, 67.88351f),
					new Vector4(-224.9798f, -276.1539f, 49f, 67.87859f),
					new Vector4(-227.9288f, -277.9304f, 49f, 69.2209f),
				},
				AreaCenter = new Vector2(-224, -257),
				HeliSpawnPoints = new Vector4[] { new Vector4(-224.8281f, -257.2638f, 50.8281f, 106.893f) },
				Radius = 500f,
				HunterSpawn = new Vector4(-237.2401f, -273.996f, 48f, 327.2174f),
				RunnerSpawn = new Vector4(7.744703f, -404.0017f, 41f, 0.01032947f),
			},
			new sthvMapModel
			{
				CarSpawnpoints = new Vector4[]
				{
					new Vector4(219.2715f, -766.7899f, 30.56348f, 172.9773f),
					new Vector4(223.0478f, -766.2462f, 30.54808f, 182.2132f),
					new Vector4(226.574f, -764.7725f, 30.54451f, 187.5756f),
					new Vector4(229.5238f, -769.5371f, 30.52281f, 167.9403f),
					new Vector4(233.1855f, -772.8695f, 30.48671f, 175.4336f),
					new Vector4(237.269f, -772.1927f, 30.47563f, 179.7796f),
					new Vector4(241.6105f, -771.9415f, 30.46822f, 179.5276f),
				},
				HeliSpawnPoints = new Vector4[]
				{
					new Vector4(255.7897f, -773.9454f, 30.41708f, 156.1047f)
				},
				HunterSpawn = new Vector4(237.0703f, -762.7197f, 30.64096f, 148.2011f),
				AreaCenter = new Vector2(237.0703f, -762.7197f),
				Radius = 500,
				RunnerSpawn = new Vector4(363.9641f, -611.2357f, 28.50907f, 190.5318f)
			},
			new sthvMapModel
			{
				CarSpawnpoints = new Vector4[]
				{
					new Vector4(368.0813f, -1692.764f, 47.94969f, 48.4f),
					new Vector4(370.3453f, -1690.081f, 47.95068f, 48.4f),
					new Vector4(372.4258f, -1687.622f, 47.95185f, 48.4f),
					new Vector4(374.9341f, -1684.406f, 47.90953f, 48.4f),
					new Vector4(377.0466f, -1681.64f, 47.91183f, 48.4f),
					new Vector4(379.2932f, -1678.444f, 48.06121f, 48.4f),
					new Vector4(381.9486f, -1676.27f, 47.91648f, 48.4f),
					new Vector4(383.7508f, -1673.141f, 48.29026f, 48.4f),
				},
				HeliSpawnPoints = new Vector4[]
				{
					new Vector4 (380f, -1655f, 48.5f, 48.4f)
				},
				HunterSpawn = new Vector4(362f, -1705f, 48.3f, 300f),
				AreaCenter = new Vector2(100f, -1740f),
				Radius = 570f,
				RunnerSpawn = new Vector4(432f, -1392f, 29.4f, 0f),
			},
			new sthvMapModel
			{
				CarSpawnpoints = new Vector4[]
				{
					new Vector4(432.0037f, -1391.998f, 28.51172f, 300.0013f),
					new Vector4(885.7175f, -2546.846f, 28.11624f, 88.19098f),
					new Vector4(874.1729f, -2545.634f, 27.97186f, 84.00628f),
					new Vector4(865.4768f, -2544.736f, 27.23853f, 84.44848f),
					new Vector4(854.7435f, -2543.84f, 26.28391f, 85.20789f),
					new Vector4(843.5375f, -2542.936f, 25.29497f, 85.43726f),
					new Vector4(835.1543f, -2542.301f, 24.54428f, 85.69273f),
					new Vector4(824.5116f, -2541.539f, 23.58949f, 85.92118f),
					new Vector4(815.427f, -2540.945f, 22.77947f, 86.31168f),
				},
				HeliSpawnPoints = new Vector4[]
				{
					new Vector4(926.4647f, -2542.919f, 28.03828f, 86.10382f)
				},
				HunterSpawn = new Vector4(893.8325f, -2543.536f, 28.12187f, 119.611f),
				AreaCenter = new Vector2(893f, -2500f),
				Radius = 600f,
				RunnerSpawn = new Vector4(673.1105f, -2793.576f, 5.374936f, 350.4134f),
			},
			new sthvMapModel
			{
				CarSpawnpoints = new Vector4[]
				{
					new Vector4(2131.014f, 4820.333f, 41.11849f, 167.1313f),
					new Vector4(2124.971f, 4815.222f, 41.08157f, 167.1469f),
					new Vector4(2121.103f, 4815.695f, 40.74207f, 152.2634f),
					new Vector4(2116.768f, 4814.269f, 41.12866f, 161.9321f),
					new Vector4(2113.185f, 4813.738f, 41.13556f, 171.112f),
					new Vector4(2109.094f, 4812.919f, 41.13413f, 163.5077f),
					new Vector4(2104.602f, 4811.367f, 41.14608f, 171.2873f),


				},
				HeliSpawnPoints = new Vector4[]
				{
					new Vector4(2143.509f, 4821.51f, 40.65517f, 148.3014f)
				},
				HunterSpawn = new Vector4(2117.036f, 4825.369f, 40.73993f, 169.3433f),
				AreaCenter = new Vector2(2200f, 4700f),
				Radius = 650f,
				RunnerSpawn = new Vector4(2283.709f, 5179.707f, 59.36262f, 281.0421f),
			},
			new sthvMapModel
			{
				CarSpawnpoints = new Vector4[]
				{
					new Vector4(-1531.694f, -993.3582f, 12.4004f, 71.7094f),
					new Vector4(-1534.541f, -998.7681f, 12.40106f, 78.76384f),
					new Vector4(-1539.78f, -1004.034f, 12.40107f, 74.47754f),
					new Vector4(-1545.533f, -1009.728f, 12.40085f, 67.9441f),
					new Vector4(-1548.678f, -1015.725f, 12.40046f, 73.91859f),
					new Vector4(-1554.01f, -1021.83f, 12.40082f, 75.41463f),
				},
				HeliSpawnPoints = new Vector4[]
				{
					new Vector4(-1581.484f, -1042.593f, 12.40113f, 325.9558f),

				},
				HunterSpawn = new Vector4(-1551.616f, -1006.091f, 12.40018f, 314.1805f),
				AreaCenter = new Vector2(-1351, -1000),
				Radius =  600,
				RunnerSpawn =  new Vector4(-844.9528f, -752.9533f, 22.1606f, 91.61478f),
			}



		};

	}
}

			//new sthvMapModel
			//{
			//	CarSpawnpoints = new Vector4[]
			//	{

			//	},
			//	HeliSpawnPoints = new Vector4[]
			//	{

			//	},
			//	HunterSpawn = 
			//	AreaCenter = 
			//	Radius =
			//	RunnerSpawn =  
			//}