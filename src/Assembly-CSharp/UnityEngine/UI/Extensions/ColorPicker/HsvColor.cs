namespace UnityEngine.UI.Extensions.ColorPicker;

public struct HsvColor
{
	public double H;

	public double S;

	public double V;

	public float NormalizedH
	{
		get
		{
			return (float)H / 360f;
		}
		set
		{
			H = (double)value * 360.0;
		}
	}

	public float NormalizedS
	{
		get
		{
			return (float)S;
		}
		set
		{
			S = value;
		}
	}

	public float NormalizedV
	{
		get
		{
			return (float)V;
		}
		set
		{
			V = value;
		}
	}

	public HsvColor(double h, double s, double v)
	{
		H = h;
		S = s;
		V = v;
	}

	public override string ToString()
	{
		return $"{{{H:f2},{S:f2},{V:f2}}}";
	}
}
