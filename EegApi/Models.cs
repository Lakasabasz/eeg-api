namespace EegApi;

public record RawData(double[][] data);

public record LabelRequest(string label);

public record TaggedDataRow(Guid rowId, Guid descriptionId, double AF3, double F7, double F3,
    double FC5, double T7, double P7, double O1, double O2, double P8, double T8, double FC6,
    double F4, double F8, double AF4)
{
    public TaggedDataRow(double[] data, Guid descriptionId) : this(Guid.NewGuid(), descriptionId, data[0],
        data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9],
        data[10], data[11], data[12], data[13]){}
}

public record Description(Guid descriptionId, DateTime recordDate, string sourceIp,
    string headsetName, int labelId);

public record Label(int labelId, string labelName);

public record Migration(Guid migrationId, DateTime migrationCreationDate);