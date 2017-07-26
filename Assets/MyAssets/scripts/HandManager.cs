using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity;

public interface HandManager {
  void Zero();
  void OneToZero(float transLife);
  void TwoToZero(float transLife);

  void One(Hand presentHand);
  void ZeroToOne(Hand futureHand, float transLife);
  void TwoToOne(Hand futureHand, float transLife);

  void Two(Hand[] presentHands);
  void ZeroToTwo(Hand[] futureHands, float transLife);
  void OneToTwo(Hand[] futureHands, float transLife);

  void TooManyMands();
}
