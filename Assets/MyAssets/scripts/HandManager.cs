using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity;

public interface HandManager {
  void Zero();
  void OneToZero(int eventLife);
  void TwoToZero(int eventLife);

  void One(Hand presentHand);
  void ZeroToOne(Hand futureHand, int eventLife);
  void TwoToOne(Hand futureHand, int eventLife);

  void Two(Hand[] presentHands);
  void ZeroToTwo(Hand[] futureHands, int eventLife);
  void OneToTwo(Hand[] futureHands, int eventLife);

  void TooManyMands();
}
