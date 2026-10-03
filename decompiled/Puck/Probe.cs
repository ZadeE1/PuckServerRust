public class Probe
{
	public string id { get; set; }

	public string providerId { get; set; }

	public string host { get; set; }

	public ushort port { get; set; }

	public float latitude { get; set; }

	public float longitude { get; set; }

	public string continent { get; set; }

	public string country { get; set; }

	public string city { get; set; }

	public int priority { get; set; }

	public int? freeCpuUnits { get; set; }

	public int? freeMemoryMb { get; set; }

	public string ToLabel()
	{
		return (continent + ": " + city).ToUpper();
	}

	public override string ToString()
	{
		return $"id: {id}, providerId: {providerId}, host: {host}, port: {port}, latitude: {latitude}, longitude: {longitude}, " + $"continent: {continent}, country: {country}, city: {city}, priority: {priority}, freeCpuUnits: {freeCpuUnits}, freeMemoryMb: {freeMemoryMb}";
	}
}
