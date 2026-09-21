using System.Collections.Generic;
using System.IO;
using System.Text;
using Sobriquet.Properties;

namespace Sobriquet;

public class DefaultGenerators
{
	private readonly Generator _maleFirstGenerator;

	private readonly Generator _femaleFirstGenerator;

	private readonly Generator _lastGenerator;

	public Generator MaleFirstName => _maleFirstGenerator;

	public Generator FemaleFirstName => _femaleFirstGenerator;

	public Generator LastName => _lastGenerator;

	public DefaultGenerators(int order)
	{
		byte[] dist_male = Resources.dist_male;
		byte[] dist_female = Resources.dist_female;
		byte[] dist_all = Resources.dist_all;
		Generator maleFirstGenerator = Generate(order, dist_male);
		Generator femaleFirstGenerator = Generate(order, dist_female);
		Generator lastGenerator = Generate(order, dist_all);
		_maleFirstGenerator = maleFirstGenerator;
		_femaleFirstGenerator = femaleFirstGenerator;
		_lastGenerator = lastGenerator;
	}

	private static Generator Generate(int order, byte[] file)
	{
		IEnumerable<WeightedName> wnames = FromNameTabWeightFile(file);
		return new Generator(order, wnames);
	}

	private static IEnumerable<WeightedName> FromNameTabWeightFile(byte[] bytes)
	{
		StreamReader streamReader = new StreamReader(new MemoryStream(bytes), Encoding.UTF8);
		List<WeightedName> list = new List<WeightedName>();
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			string[] array = text.Split('\t');
			string name = array[0];
			int weight = int.Parse(array[1]);
			list.Add(new WeightedName(name, weight));
		}
		return list;
	}
}
