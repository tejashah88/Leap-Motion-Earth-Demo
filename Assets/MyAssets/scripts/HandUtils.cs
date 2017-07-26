using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity;

public class HandUtils {
  public static Hand[] designateRightLeftHands(Frame frame) {
    Hand rightHand, leftHand;
    bool isFirstHandLeft = frame.Hands[0].IsLeft;
    if (isFirstHandLeft) {
      leftHand = frame.Hands[0];
      rightHand = frame.Hands[1];
    } else {
      leftHand = frame.Hands[1];
      rightHand = frame.Hands[0];
    }

    return new Hand[] { leftHand, rightHand };
  }

  public static float getGrabAngleDegrees(Hand hand) {
    return hand.GrabAngle * Mathf.Rad2Deg;
  }

  public static float getHandSpeed(Hand hand) {
    return hand.PalmVelocity.Magnitude;
  }

  public static float getHandSphereRadius(Hand hand, float minSphereRadius = 0.03f, float maxSphereRadius = 0.1f) {
    return minSphereRadius + (maxSphereRadius - minSphereRadius) * (1 - hand.GrabStrength);
  }

  public static float getHandSphereDiameter(Hand hand, float minSphereRadius = 0.03f, float maxSphereRadius = 0.1f) {
    return 2 * getHandSphereRadius(hand, minSphereRadius, maxSphereRadius);
  }

  public static Vector3 getHandSphereCenter(Hand hand, float minSphereRadius = 0.03f, float maxSphereRadius = 0.1f) {
    float _sphereRadius = getHandSphereRadius(hand, minSphereRadius, maxSphereRadius);
    return (hand.PalmPosition + hand.PalmNormal * _sphereRadius).ToVector3();
  }
}
