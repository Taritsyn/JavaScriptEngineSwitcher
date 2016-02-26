namespace JavaScriptEngineSwitcher.Tests.Interop
{
	public struct Point3D
	{
		public int X;
		public int Y;
		public int Z;

		public static readonly Point3D Empty = new Point3D();


		public Point3D(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}


		public override string ToString()
		{
			return string.Format("{{X={0},Y={1},Z={2}}}", X, Y, Z);
		}
	}
}