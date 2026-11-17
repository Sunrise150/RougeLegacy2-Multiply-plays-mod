using System.IO;
using System.Xml;

public static class RNGControllerDataExporter
{
	private const string PATH = "Assets/Logs/RNG/";

	private const string POSTFIX = "_RNGDump";

	private const string ROOT_NAME = "callerData";

	private const string ID_NAME = "id";

	private const string SEED_NAME = "seed";

	private const string RECORD_NAME = "data";

	private const string ORDER_NUMBER_ELEMENT_NAME = "frameNumber";

	private const string NUMBER_ELEMENT_NAME = "number";

	private const string MIN_ELEMENT_NAME = "min";

	private const string MAX_ELEMENT_NAME = "max";

	private const string DESCRIPTION_ELEMENT_NAME = "description";

	public static void ExportControllerToFile(RNGController controller)
	{
		if (controller.CallerData == null || controller.CallerData.Count <= 0)
		{
			return;
		}
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Indent = true,
			IndentChars = "\t",
			NewLineOnAttributes = true
		};
		string text = string.Format("{0}{1}{2}", "Assets/Logs/RNG/", controller.ID.ToString(), "_RNGDump");
		if (File.Exists(text + ".xml"))
		{
			int num = 2;
			string text2 = $"{text} ({num})";
			while (File.Exists(text2 + ".xml"))
			{
				num++;
				text2 = $"{text} ({num})";
			}
			text = text2;
		}
		XmlWriter xmlWriter = XmlWriter.Create(text + ".xml", settings);
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement("callerData");
		xmlWriter.WriteAttributeString("id", controller.ID.ToString());
		xmlWriter.WriteAttributeString("seed", controller.Seed.ToString());
		foreach (RNGData callerDatum in controller.CallerData)
		{
			xmlWriter.WriteStartElement("data");
			xmlWriter.WriteAttributeString("frameNumber", callerDatum.Order.ToString());
			WriteElement(xmlWriter, "min", callerDatum.Min);
			WriteElement(xmlWriter, "max", callerDatum.Max);
			WriteElement(xmlWriter, "number", callerDatum.Number);
			WriteElement(xmlWriter, "description", callerDatum.CallerDescription);
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndDocument();
		xmlWriter.Close();
	}

	private static void WriteElement(XmlWriter xmlWriter, string elementName, int value)
	{
		WriteElement(xmlWriter, elementName, value.ToString());
	}

	private static void WriteElement(XmlWriter xmlWriter, string elementName, float value)
	{
		WriteElement(xmlWriter, elementName, value.ToString());
	}

	private static void WriteElement(XmlWriter xmlWriter, string elementName, string value)
	{
		xmlWriter.WriteStartElement(elementName);
		xmlWriter.WriteString(value);
		xmlWriter.WriteEndElement();
	}
}
