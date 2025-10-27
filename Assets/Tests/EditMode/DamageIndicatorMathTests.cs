using NUnit.Framework;
using UnityEngine;

public class DamageIndicatorMathTests
{
    private readonly Vector3 up = Vector3.up;
    private readonly Vector3 forward = Vector3.forward;

    [Test]
    public void SignedAngle_IsZero_WhenSourceIsDirectlyAhead()
    {
        float angle = DamageIndicatorMath.CalculateSignedAngle(forward, Vector3.forward, up);
        Assert.That(angle, Is.EqualTo(0f).Within(0.01f));
    }

    [Test]
    public void SignedAngle_IsPositiveOnRightAndNegativeOnLeft()
    {
        float rightAngle = DamageIndicatorMath.CalculateSignedAngle(forward, Vector3.right, up);
        float leftAngle = DamageIndicatorMath.CalculateSignedAngle(forward, Vector3.left, up);

        Assert.That(rightAngle, Is.EqualTo(90f).Within(0.1f));
        Assert.That(leftAngle, Is.EqualTo(-90f).Within(0.1f));
    }

    [Test]
    public void NormalizeDistance_ClampsBetweenZeroAndOne()
    {
        Assert.That(DamageIndicatorMath.NormalizeDistance(-5f, 10f), Is.EqualTo(0f));
        Assert.That(DamageIndicatorMath.NormalizeDistance(5f, 10f), Is.EqualTo(0.5f));
        Assert.That(DamageIndicatorMath.NormalizeDistance(50f, 10f), Is.EqualTo(1f));
    }

    [Test]
    public void Blend_InterpolatesColors()
    {
        Color result = DamageIndicatorMath.Blend(Color.red, Color.blue, 0.5f);
        Assert.That(result.r, Is.EqualTo(0.5f).Within(0.01f));
        Assert.That(result.b, Is.EqualTo(0.5f).Within(0.01f));
    }
}
