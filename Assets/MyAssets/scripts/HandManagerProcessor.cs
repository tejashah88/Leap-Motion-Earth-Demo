using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity;

public class HandManagerProcessor {
  private List<HandManager> managers;

  private float currentTransitionLife = 0;

  private int prevHandCount;
  private int prevHandCountBeforeTransition;
  private bool isTransitioning;

  private float maxTransitionLife;
  private int maxHandCount;

  private Hand[] designateRightLeftHands(Frame frame) {
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

  public HandManagerProcessor(int mhc, float mtl) {
    this.managers = new List<HandManager>();
    this.maxTransitionLife = mtl;
    this.maxHandCount = mhc;
  }

  public void Add(HandManager manager) {
    if (manager != null)
      this.managers.Add(manager);
  }

  public void RemoveAt(int index) {
    this.managers.RemoveAt(index);
  }

  public void RemoveAll() {
    this.managers.RemoveAll(manager => true);
  }
  
  void handleTransitionPhase(Frame frame, int hbtCount, float ctl, HandManager manager) {
    //Debug.Log(ctl);
    switch (frame.Hands.Count) {
      case 0:
        if (hbtCount == 1)
          manager.OneToZero(ctl);
        if (hbtCount == 2)
          manager.TwoToZero(ctl);
        break;
      case 1:
        Hand currentHand = frame.Hands[0];
        if (hbtCount == 0)
          manager.ZeroToOne(currentHand, ctl);
        if (hbtCount == 2)
          manager.TwoToOne(currentHand, ctl);
        break;
      case 2:
        Hand[] currentHands = designateRightLeftHands(frame);
        if (hbtCount == 0)
          manager.ZeroToTwo(currentHands, ctl);
        if (hbtCount == 1)
          manager.OneToTwo(currentHands, ctl);
        break;
    }
  }

  void handleStablePhase(Frame frame, HandManager manager) {
    switch (frame.Hands.Count) {
      case 0:
        manager.Zero();
        break;
      case 1:
        manager.One(frame.Hands[0]);
        break;
      case 2:
        manager.Two(designateRightLeftHands(frame));
        break;
    }
  }

  void processFrame(Frame frame, HandManager manager) {
    int currentHandCount = frame.Hands.Count;

    if (currentHandCount != prevHandCount) { // start counting
      currentTransitionLife = 0;
      prevHandCountBeforeTransition = prevHandCount;
      isTransitioning = true;
    }

    if (isTransitioning) {
      if (currentTransitionLife < maxTransitionLife) {
        currentTransitionLife += Time.deltaTime;
        handleTransitionPhase(frame, prevHandCountBeforeTransition, currentTransitionLife, manager);
      } else {
        isTransitioning = false;
      }
    } else {
      handleStablePhase(frame, manager);
    }

    prevHandCount = currentHandCount;
  }

  public void ProcessUpdate(Frame frame) {
    this.managers.ForEach(delegate(HandManager manager) {
      if (frame.Hands.Count > this.maxHandCount) {
        manager.TooManyMands();
      } else {
        processFrame(frame, manager);
      }
    });
  }
}
