using System.Collections.Generic;

namespace Sobriquet;

public class Generator
{
	private int _seed;

	private readonly MarkovChain _chain;

	private Dictionary<string, bool> _originalNames = new Dictionary<string, bool>();

	private Dictionary<string, bool> _seenNames = new Dictionary<string, bool>();

	public IEnumerable<string> OriginalNames => _originalNames.Keys;

	public Generator(int order, IEnumerable<string> names)
	{
		_chain = new MarkovChain(order);
		foreach (string name in names)
		{
			_chain.Add(name, 1);
			_originalNames[name] = true;
		}
	}

	public Generator(int order, IEnumerable<WeightedName> wnames)
	{
		_chain = new MarkovChain(order);
		foreach (WeightedName wname in wnames)
		{
			_chain.Add(wname.Name, wname.Weight);
			_originalNames[wname.Name] = true;
		}
	}

	public string Next()
	{
		string result = _chain.Chain(_seed);
		_seed++;
		return result;
	}

	public string NextNew()
	{
		string text;
		do
		{
			text = Next();
		}
		while (_originalNames.ContainsKey(text));
		return text;
	}

	public string NextUnique()
	{
		string text;
		do
		{
			text = NextNew();
		}
		while (_seenNames.ContainsKey(text));
		_seenNames[text] = true;
		return text;
	}

	public IEnumerable<string> AllRaw(int maxlen)
	{
		return _chain.AllRaw(maxlen);
	}
}
