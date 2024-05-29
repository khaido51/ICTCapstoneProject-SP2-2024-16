using CsvHelper.Configuration.Attributes;
using System;

namespace ICTCapstoneProject.Models
{
    public class CameraPosition
    {
        [Index(0)]
        public string? Timestamp { get; set; }

        [Index(1)]
        public int Segment { get; set; }

        [Index(2)]
        public bool CameraActive { get; set; }

        [Index(3)]
        public double CameraPositionX { get; set; }

        [Index(4)]
        public double CameraPositionY { get; set; }

        [Index(5)]
        public double CameraPositionZ { get; set; }

        [Index(6)]
        public double CameraRotationX { get; set; }

        [Index(7)]
        public double CameraRotationY { get; set; }

        [Index(8)]
        public double CameraRotationZ { get; set; }

        [Index(9)]
        public double CameraRotationW { get; set; }

        [Index(10)]
        public double? CameraVelocityX { get; set; }

        [Index(11)]
        public double? CameraVelocityY { get; set; }

        [Index(12)]
        public double? CameraVelocityZ { get; set; }

        [Index(13)]
        public double? CameraAccelerationX { get; set; }

        [Index(14)]
        public double? CameraAccelerationY { get; set; }

        [Index(15)]
        public double? CameraAccelerationZ { get; set; }

        [Index(16)]
        public double? CameraAngularVelocityX { get; set; }

        [Index(17)]
        public double? CameraAngularVelocityY { get; set; }

        [Index(18)]
        public double? CameraAngularVelocityZ { get; set; }

        [Index(19)]
        public double? CameraAngularAccelerationX { get; set; }

        [Index(20)]
        public double? CameraAngularAccelerationY { get; set; }

        [Index(21)]
        public double? CameraAngularAccelerationZ { get; set; }
    }
}
