using Godot;

public partial class Rope : Node3D
{
	[Export] public Vector3 startPoint;
	[Export] public Vector3 endPoint;
	[Export] public float swingAmount = 0.5f;
	[Export] public float swingFreq = 4.0f;
	[Export] public float swingOutSec = 0.2f;
	[Export] public float ropeAttachTimeSec = 0.2f;

	private ImmediateMesh _mesh;
	private MeshInstance3D _meshInstance;
	private FastNoiseLite _noise;
	private float _segSize = 0.1f;
	private float _secSinceStart = 0.0f;
	private Vector3 _visualEndPoint;

	public override void _Ready()
	{
		_mesh = new ImmediateMesh();
		_meshInstance = new MeshInstance3D();
		_noise = new FastNoiseLite();
		_meshInstance.Mesh = _mesh;
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		_noise.FractalOctaves = 3;
		_noise.Frequency = 0.1f;
		_noise.Seed = (int)Time.GetTicksMsec();
		_visualEndPoint = startPoint;
		AddChild(_meshInstance);
	}

	public override void _Process(double delta)
	{
		_secSinceStart += (float)delta;
		_visualEndPoint = _visualEndPoint.Slerp(endPoint, Mathf.Min(_secSinceStart / ropeAttachTimeSec, 1.0f));
		GD.Print(_visualEndPoint);

		int vertCount = (int)(startPoint.DistanceTo(_visualEndPoint) / _segSize);
		GD.Print(vertCount);
		Vector3 alongLine = _visualEndPoint - startPoint;
		alongLine = alongLine.Normalized();

		_mesh.ClearSurfaces();
		_mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

		_mesh.SurfaceAddVertex(startPoint);
		for (int i = 0; i < vertCount; i++)
		{
			float distAlongLine = i * _segSize;
			Vector3 cPos = startPoint + alongLine * distAlongLine;
			Vector3 perp = alongLine.Cross(Vector3.Up).Normalized();
			if (perp.Length() < 0.001f)
				perp = alongLine.Cross(Vector3.Right).Normalized();

			float sin = Mathf.Sin(distAlongLine * swingFreq + _secSinceStart * 3.0f);
			float amp = Mathf.Lerp(swingAmount, 0.0f, Mathf.Min(_secSinceStart / swingOutSec, 1.0f));
			amp *= distAlongLine / startPoint.DistanceTo(_visualEndPoint);
			float noise = _noise.GetNoise1D(distAlongLine + _secSinceStart);
			perp *= sin * amp + noise;
			cPos += perp;

			for (int k = 0; k < 2; k++)
				_mesh.SurfaceAddVertex(cPos);
		}
		_mesh.SurfaceAddVertex(_visualEndPoint);

		_mesh.SurfaceEnd();
	}
}
