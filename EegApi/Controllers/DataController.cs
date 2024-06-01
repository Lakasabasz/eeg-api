using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace EegApi.Controllers;

[ApiController]
[Route("/")]
public class DataController(DbService dbService) : ControllerBase
{
    [HttpPost("data")]
    public IActionResult Data([FromBody] RawData? data)
    {
        if (data is null) return BadRequest(new { Message = "`data` param cannot be null" });
        if (!PersistentData.RecordingActive) return BadRequest(new {Message = "Recording is not active"});
        if (PersistentData.ActiveLabel is null) return BadRequest(new {Message = "Label is empty"});

        Description description = new(Guid.NewGuid(), DateTime.Now,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "255.255.255.255",
            "Emotiv Epoc X", PersistentData.ActiveLabel.labelId);

        var taggedData = data.data.Select(x => new TaggedDataRow(x, description.descriptionId));

        dbService.InsertRecording(description, taggedData);
        return Ok();
    }

    [HttpPatch("currentlabel")]
    public IActionResult Data([FromBody] LabelRequest labelRequest)
    {
        Label? label = dbService.GetLabelByName(labelRequest.label);
        if (label is null) return BadRequest(new { Message = $"Not found label with name: {labelRequest.label}" });
        PersistentData.ActiveLabel = label;
        return Ok();
    }

    [HttpPatch("start")]
    public IActionResult Start()
    {
        if (PersistentData.RecordingActive) return BadRequest(new { Message = "Recording is already active" });
        if (PersistentData.ActiveLabel is null) return BadRequest(new { Message = "Label is empty" });
        PersistentData.RecordingActive = true;
        return Ok();
    }

    [HttpPatch("stop")]
    public IActionResult Stop()
    {
        if (!PersistentData.RecordingActive) return BadRequest(new { Message = "Recording is not active" });
        PersistentData.RecordingActive = false;
        PersistentData.ActiveLabel = null;
        return Ok();
    }
}