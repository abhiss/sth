using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Shared
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
		public static sthvMapModel[] Maps = new sthvMapModel[] //1
		{
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
				Radius = 640f,
				HunterSpawn = new Vector4(-237.2401f, -273.996f, 48f, 327.2174f),
				RunnerSpawn = new Vector4(7.744703f, -404.0017f, 41f, 0.01032947f),
			},
			new sthvMapModel
			{
				CarSpawnpoints = new Vector4[] //2
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
				Radius = 600,
				RunnerSpawn = new Vector4(363.9641f, -611.2357f, 28.50907f, 190.5318f)
			},
			new sthvMapModel //3
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
				Radius = 600f,
				RunnerSpawn = new Vector4(432f, -1392f, 29.4f, 0f),
			},
			new sthvMapModel //4
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
				Radius = 660f,
				RunnerSpawn = new Vector4(673.1105f, -2793.576f, 5.374936f, 350.4134f),
			},
			new sthvMapModel //5
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
			new sthvMapModel //6
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
				Radius =  660,
				RunnerSpawn =  new Vector4(-844.9528f, -752.9533f, 22.1606f, 91.61478f),
			}



		};

	}
}
//playarea 0 mped coords:
//new Vector4(42.48653f, -391.3221f, 39.10563f, 345.2107f),
//new Vector4(-31.91417f, -371.2125f, 38.46544f, 259.1203f),
//new Vector4(-17.92983f, -300.9055f, 45.61324f, 298.7241f),
//new Vector4(-20.54725f, -229.2577f, 45.72718f, 181.5939f),
//new Vector4(-52.76249f, -214.2776f, 45.27671f, 48.72011f),
//new Vector4(-64.14668f, -157.2281f, 57.35524f, 11.82183f),
//new Vector4(-48.65516f, -70.35485f, 58.39137f, 71.23219f),
//new Vector4(-41.44627f, -52.46988f, 63.41685f, 0.6174907f),
//new Vector4(-12.25993f, -45.06075f, 68.98936f, 277.6589f),
//new Vector4(11.01956f, -38.36683f, 70.78806f, 252.252f),
//new Vector4(53.03413f, -45.20915f, 69.40494f, 275.4886f),
//new Vector4(72.80089f, -34.33061f, 68.81891f, 179.6777f),
//new Vector4(90.11251f, -41.26941f, 67.94409f, 244.0724f),
//new Vector4(148.4587f, 8.933767f, 68.69331f, 333.0646f),
//new Vector4(168.8044f, 25.02793f, 73.23253f, 53.64227f),
//new Vector4(177.9789f, 6.797957f, 73.43664f, 245.9913f),
//new Vector4(198.2385f, 22.99657f, 73.44428f, 341.3095f),
//new Vector4(165.0358f, 40.71216f, 79.67169f, 56.50824f),
//new Vector4(180.3176f, 42.90034f, 87.81867f, 244.4593f),
//new Vector4(224.9344f, 46.7743f, 83.9446f, 264.2315f),
//new Vector4(241.7808f, 84.76499f, 92.80844f, 24.79937f),
//new Vector4(162.9775f, 124.59f, 95.53377f, 27.49381f),
//new Vector4(110.7628f, 145.0477f, 104.7883f, 69.71262f),
//new Vector4(87.79602f, 169.6333f, 104.5042f, 96.95096f),
//new Vector4(-33.91016f, 165.3675f, 94.99086f, 146.1609f),
//new Vector4(-79.21658f, 172.2851f, 87.17085f, 140.5039f),
//new Vector4(-136.3767f, 171.2013f, 85.42725f, 80.7989f),
//new Vector4(-165.4911f, 166.9409f, 81.50544f, 146.7811f),
//new Vector4(-156.1088f, 153.0145f, 77.50893f, 244.6181f),
//new Vector4(-136.8509f, 99.62154f, 70.84425f, 118.2997f),
//new Vector4(-188.7931f, 129.4494f, 69.81708f, 62.03968f),
//new Vector4(-200.1192f, 148.0884f, 70.32858f, 109.1087f),
//new Vector4(-277.2119f, 73.1601f, 66.2852f, 86.09248f),
//new Vector4(-278.5866f, 47.29961f, 57.78798f, 244.9927f),
//new Vector4(-379.2051f, -33.18814f, 49.03678f, 128.3238f),
//new Vector4(-377.6797f, -82.79978f, 45.66315f, 111.2229f),
//new Vector4(-389.2638f, -146.4979f, 38.53213f, 181.5531f),
//new Vector4(-420.8401f, -193.9817f, 36.53319f, 10.46927f),
//new Vector4(-435.2717f, -168.0273f, 37.50723f, 33.15444f),
//new Vector4(-453.6884f, -137.3598f, 38.36572f, 34.68394f),
//new Vector4(-500.1456f, -68.51131f, 39.62954f, 267.8226f),
//new Vector4(-509.762f, -49.35838f, 40.69621f, 65.18405f),
//new Vector4(-536.3998f, -41.43262f, 42.6158f, 77.26176f),
//new Vector4(-579.6389f, -93.97182f, 43.39783f, 298.1762f),
//new Vector4(-563.8499f, -97.39228f, 40.35026f, 232.6965f),
//new Vector4(-574.3815f, -105.0707f, 39.61812f, 73.95818f),
//new Vector4(-571.3342f, -123.1315f, 39.99463f, 227.6702f),
//new Vector4(-591.1649f, -134.2894f, 39.61427f, 169.8358f),
//new Vector4(-615.2786f, -142.3257f, 46.99362f, 58.63832f),
//new Vector4(-622.4056f, -207.5997f, 37.36024f, 179.7109f),
//new Vector4(-613.806f, -292.6065f, 39.39109f, 170.6615f),
//new Vector4(-639.7767f, -246.7412f, 38.21951f, 56.09119f),
//new Vector4(-707.2261f, -301.2708f, 36.33879f, 83.85015f),
//new Vector4(-724.0844f, -295.2981f, 36.58967f, 96.74117f),
//new Vector4(-769.4385f, -219.0749f, 36.81869f, 32.41678f),
//new Vector4(-789.0553f, -181.3031f, 36.81598f, 25.73893f),
//new Vector4(-720.7212f, -94.62862f, 37.68306f, 342.1312f),
//new Vector4(-709.443f, -66.67075f, 37.23368f, 320.0714f),
//new Vector4(-679.7748f, -25.41013f, 37.89711f, 328.2753f),
//new Vector4(-638.5267f, 59.03994f, 43.99444f, 3.663171f),
//new Vector4(-503.6757f, 55.69524f, 52.13897f, 143.3428f),
//new Vector4(-495.4008f, -12.44041f, 44.50821f, 268.4639f),
//new Vector4(-480.4834f, -12.46802f, 44.76952f, 271.9207f),
//new Vector4(-414.407f, 18.48092f, 45.99945f, 306.0607f),
//new Vector4(-355.7805f, 76.52005f, 62.74751f, 295.0139f),
//new Vector4(-290.0403f, 69.6515f, 65.22549f, 271.1576f),
//new Vector4(-270.4018f, 24.60406f, 54.31784f, 341.1205f),
//new Vector4(-212.3912f, -84.57276f, 50.24503f, 141.4783f),
//new Vector4(8.421366f, -120.0434f, 55.74568f, 210.5562f),
//new Vector4(-2.261217f, -436.6222f, 39.23423f, 146.5091f),
//new Vector4(44.00309f, -455.3669f, 39.52785f, 331.8281f),
//new Vector4(59.5155f, -415.6433f, 39.45732f, 333.7727f),
//new Vector4(88.76024f, -383.6054f, 40.78869f, 283.1193f),
//new Vector4(103.341f, -352.9243f, 41.99145f, 82.15868f),
//new Vector4(38.58145f, -328.5773f, 43.41122f, 321.5545f),
//new Vector4(74.69745f, -330.5736f, 43.53649f, 253.2832f),
//new Vector4(188.0728f, -389.298f, 41.99014f, 327.5027f),
//new Vector4(-442.4094f, -675.3262f, 31.30844f, 148.9587f),
//new Vector4(-499.054f, -726.0472f, 33.21196f, 356.5621f),
//new Vector4(-491.5f, -697.5854f, 33.23734f, 260.4614f),
//new Vector4(-289.3326f, -707.8464f, 33.46772f, 57.2404f),
//new Vector4(-190.9839f, -656.7873f, 33.90509f, 246.9742f),
//new Vector4(-119.5181f, -637.8442f, 36.15728f, 333.1366f),
//new Vector4(-63.46651f, -615.465f, 36.26544f, 311.6558f),
//new Vector4(37.61023f, -621.034f, 31.65434f, 216.1118f),
//new Vector4(65.31849f, -615.9974f, 31.90179f, 269.1531f),

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