namespace Server.Models;

public class MlResult
{
    public bool IsDetected { get; set; }
    public List<Detection>? Detections { get; set; }
}

public class Detection
{
    public double Xc { get; set; }
    public double Yc { get; set; }
    public double W { get; set; }
    public double H { get; set; }
    public int Class { get; set; }
}