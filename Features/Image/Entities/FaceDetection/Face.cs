using System.Drawing;

namespace SlideGenerator.Framework.Image.FaceDetection;

/// <summary>
///     Represents a face with its bounding box, score, and eye landmarks (if available).
/// </summary>
/// <param name="Rect">The bounding box of the detected face.</param>
/// <param name="Score">The confidence score for this detection.</param>
/// <param name="RightEye">The right eye landmark (if provided by the model).</param>
/// <param name="LeftEye">The left eye landmark (if provided by the model).</param>
/// <param name="Nose">The nose landmark (if provided by the model).</param>
/// <param name="RightMouth">The right mouth corner landmark (if provided by the model).</param>
/// <param name="LeftMouth">The left mouth corner landmark (if provided by the model).</param>
public readonly record struct Face(
    Rectangle Rect,
    float Score,
    Point? RightEye = null,
    Point? LeftEye = null,
    Point? Nose = null,
    Point? RightMouth = null,
    Point? LeftMouth = null);