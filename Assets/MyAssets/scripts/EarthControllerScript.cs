using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leap;
using Leap.Unity;

public class EarthControllerScript : MonoBehaviour, HandManager {
  private const int maxZeroRotFrame = 100;
  private int currZeroRotFrame;
  private float maxZeroTimer = 5.0f;
  private float currZeroTimer;

  public void Zero() {
    this.setDebugText("Zero");
    setHandStatus("IDLE");
    oneStatus = OneHandStatus.NULL;
    twoStatus = TwoHandStatus.NULL;

    transform.position = defaultEarthPosition;
    transform.localScale = defaultEarthScale;

    if (currZeroTimer < maxZeroTimer) {
      currZeroTimer += Time.deltaTime;
      if (currZeroTimer > 0.8f * maxZeroTimer) {
        setAngularDrag(3f);
      }
    } else {
      restoreAngularDrag();
      if (currZeroRotFrame < maxZeroRotFrame) {
        transform.rotation = Quaternion.Lerp(transform.rotation, defaultEarthRotation, catchUpDelta / 12);
        currZeroRotFrame++;
      } else {
        transform.Rotate(Vector3.up, 10f * Time.deltaTime);
      }
    }
  }
  public void OneToZero(float transLife) {
    this.setDebugText("OneToZero for " + transLife);
    currZeroRotFrame = 0;
    currZeroTimer = 0.0f;
    if (oneStatus != OneHandStatus.IDLE) {
      transform.position = Vector3.Lerp(transform.position, defaultEarthPosition, catchUpDelta / 4);
      transform.localScale = Vector3.Lerp(transform.localScale, defaultEarthScale, catchUpDelta / 4);
    }
  }
  public void TwoToZero(float transLife) {
    this.setDebugText("TwoToZero for " + transLife);
    currZeroRotFrame = 0;
    currZeroTimer = 0.0f;
    if (twoStatus != TwoHandStatus.IDLE) {
      transform.position = Vector3.Lerp(transform.position, defaultEarthPosition, catchUpDelta / 4);
      transform.localScale = Vector3.Lerp(transform.localScale, defaultEarthScale, catchUpDelta / 4);
    }
  } // unlikely

  private enum OneHandStatus { NULL, IDLE, HOLDING, FORCE_PUSHING, FORCE_PULLING };
  private enum TwoHandStatus { NULL, IDLE, LEFT_HOLDING, RIGHT_HOLDING, FORCE_PUSH_TO_LEFT, FORCE_PUSH_TO_RIGHT };

  bool isHolding;
  bool hasThrown;

  private OneHandStatus oneStatus;
  private TwoHandStatus twoStatus;

  private int idle_iter = 0;
  private int holding_iter = 0;
  private int force_push_iter = 0;
  private int force_pull_iter = 0;

  private float max_holding_iters;
  private float max_idle_iters;
  private float max_force_push_iters;
  private float max_force_pull_iters;

  private float catchUpDelta = 0.25f;
  private float minPercentDone = 0.99f;

  private Vector3 recordedPalmNormal;

  private bool force_push_calculated;

  private Vector3 posBeforeTransStart;
  private Vector3 scaleBeforeTransStart;
  private Vector3 scaleAfterTransStart;

  private Hand holdingHand;

  public void One(Hand presentHand) {
    twoStatus = TwoHandStatus.NULL;

    Vector3 presentHandPos = getHandPos(presentHand);
    Vector3 presentHandVel = getHandVel(presentHand);

    this.setDebugText(
      this.buildDebugText(
        "One",
        "pos-right = " + presentHandPos.x.ToString("F3"),
        "pos-up = " + presentHandPos.y.ToString("F3"),
        "pos-forward = " + presentHandPos.z.ToString("F3"),
        "vel-right = " + presentHandVel.x.ToString("F3"),
        "vel-up = " + presentHandVel.y.ToString("F3"),
        "vel-forward = " + presentHandVel.z.ToString("F3"),
        "grab-angle = "  + getGrabAngleDegrees(presentHand)
      )
    );

    OneHandStatus prevOneStatus = oneStatus;

    if (presentHandPos.z >= 0.125f && getGrabAngleDegrees(presentHand) >= 160) {
      if (oneStatus == OneHandStatus.IDLE) {
        oneStatus = OneHandStatus.HOLDING;
      }
    } else if (presentHandVel.z >= 0.8f && getGrabAngleDegrees(presentHand) <= 30) {
      if (oneStatus == OneHandStatus.HOLDING) { // mostly open
        oneStatus = OneHandStatus.FORCE_PUSHING;
      }
    } else if (presentHandVel.z <= -0.8f && getGrabAngleDegrees(presentHand) <= 30) {
      if (oneStatus == OneHandStatus.FORCE_PUSHING) { // mostly open
        oneStatus = OneHandStatus.FORCE_PULLING;
      }
    } else if (presentHandVel.y >= 0.8f && getGrabAngleDegrees(presentHand) <= 30) {
      if (oneStatus != OneHandStatus.IDLE) { // mostly open
        oneStatus = OneHandStatus.IDLE;
      }
    }

    if (oneStatus != prevOneStatus) {
      idle_iter = 0;
      holding_iter = 0;
      force_pull_iter = 0;
      force_push_calculated = false;
    }

    switch (oneStatus) {
      case OneHandStatus.IDLE:
        setHandStatus("IDLE_ONE");

        if (idle_iter <= max_idle_iters) {
          if (idle_iter == 0) {
            max_idle_iters = getIterationsForLerp(minPercentDone, catchUpDelta / 8);
          }

          transform.position = Vector3.Lerp(transform.position, defaultEarthPosition, catchUpDelta / 8);
          transform.localScale = Vector3.Lerp(transform.localScale, defaultEarthScale, catchUpDelta / 8);
          idle_iter++;
        } else {
          transform.position = defaultEarthPosition;
          transform.localScale = defaultEarthScale;

          if (presentHand.PinchDistance <= 30) {
            setAngularDrag(3f);
          } else {
            restoreAngularDrag();
            if (getHandSpeed(presentHand) > 0.3f) {
              rb.AddTorque(new Vector3(0, 1, 0) * -presentHandVel.x * rotationMultiplier * Time.deltaTime, ForceMode.Acceleration);
              rb.AddTorque(new Vector3(0, 0, 1) * presentHandVel.y * rotationMultiplier * Time.deltaTime, ForceMode.Acceleration);
            }
          }
        }

        break;
      case OneHandStatus.HOLDING:
        setHandStatus("HOLDING");

        holdingHand = presentHand;

        if (holding_iter <= max_holding_iters) {
          if (holding_iter == 0) {
            max_holding_iters = getIterationsForLerp(minPercentDone, catchUpDelta);
            scaleAfterTransStart = Vector3.one * getHandSphereDiameter(presentHand) / 100f * 2.5f;
          }

          transform.position = Vector3.Lerp(transform.position, getHandSphereCenter(presentHand), catchUpDelta);
          transform.localScale = Vector3.Lerp(transform.localScale, scaleAfterTransStart, catchUpDelta);
          holding_iter++;
        } else {
          transform.position = getHandSphereCenter(presentHand);
          Vector3 finalScale = Vector3.one * getHandSphereDiameter(presentHand) / 100f * 2.5f;
          transform.localScale = Vector3.Lerp(transform.localScale, finalScale, catchUpDelta);
        }
        break;
      case OneHandStatus.FORCE_PUSHING:
        setHandStatus("FORCE_PUSHING");

        if (!force_push_calculated) {
          recordedPalmNormal = new Vector3(presentHand.PalmNormal.x, presentHand.PalmNormal.y, presentHand.PalmNormal.z) * getHandVel(presentHand).magnitude;
          force_push_calculated = true;
        } else {
          transform.position += recordedPalmNormal * Time.deltaTime;
        }

        break;
      case OneHandStatus.FORCE_PULLING:
        setHandStatus("FORCE_PULLING");

        if (force_pull_iter <= max_force_pull_iters) {
          if (force_pull_iter == 0) {
            max_force_pull_iters = getIterationsForLerp(minPercentDone, catchUpDelta);
            scaleAfterTransStart = Vector3.one * getHandSphereDiameter(presentHand) / 100f * 2.5f;
          }

          transform.position = Vector3.Lerp(transform.position, getHandSphereCenter(presentHand), catchUpDelta / 2);
          transform.localScale = Vector3.Lerp(transform.localScale, scaleAfterTransStart, catchUpDelta);
          force_pull_iter++;
        } else {
          oneStatus = OneHandStatus.HOLDING;
        }

        break;
    }
  }
  public void ZeroToOne(Hand futureHand, float transLife) {
    this.setDebugText("ZeroToOne for " + transLife);
    oneStatus = OneHandStatus.IDLE;
  }
  public void TwoToOne(Hand futureHand, float transLife) {
    this.setDebugText("TwoToOne for " + transLife);

    Vector3 finalScale;
    if (twoStatus == TwoHandStatus.LEFT_HOLDING || twoStatus == TwoHandStatus.RIGHT_HOLDING) {
      if (futureHand.Id == holdingHand.Id) {
        oneStatus = OneHandStatus.HOLDING;

        transform.position = getHandSphereCenter(futureHand);
        finalScale = Vector3.one * getHandSphereDiameter(futureHand) / 100f * 2.5f;
        transform.localScale = Vector3.Lerp(transform.localScale, finalScale, catchUpDelta);
      } else {
        twoStatus = TwoHandStatus.NULL;
      }
    } else {
      oneStatus = OneHandStatus.IDLE;
      transform.position = Vector3.Lerp(transform.position, defaultEarthPosition, catchUpDelta / 8);
      transform.localScale = Vector3.Lerp(transform.localScale, defaultEarthScale, catchUpDelta / 8);
    }
  }

  public void Two(Hand[] presentHands) {
    oneStatus = OneHandStatus.NULL;

    Hand leftHand = presentHands[0];
    Hand rightHand = presentHands[1];
    
    Vector3 leftHandPos = getHandPos(leftHand);
    Vector3 leftHandVel = getHandVel(leftHand);
    Vector3 rightHandPos = getHandPos(rightHand);
    Vector3 rightHandVel = getHandVel(rightHand);

    Vector3 bothHandVel = ( leftHandVel + rightHandVel ) / 2;

    this.setDebugText("Two");
    this.setDebugText(
      this.buildDebugText(
        "Two",
        "left-pos-right = " + leftHandPos.x.ToString("F3"),
        "left-pos-up = " + leftHandPos.y.ToString("F3"),
        "left-pos-forward = " + leftHandPos.z.ToString("F3"),
        "left-vel-right = " + leftHandVel.x.ToString("F3"),
        "left-vel-up = " + leftHandVel.y.ToString("F3"),
        "left-vel-forward = " + leftHandVel.z.ToString("F3"),
        "left-grab-angle = "  + getGrabAngleDegrees(leftHand)
      )
    );

    TwoHandStatus prevTwoStatus = twoStatus;

    if (leftHandPos.z >= 0.125f && getGrabAngleDegrees(leftHand) >= 160) {
      if (twoStatus == TwoHandStatus.IDLE) {
        twoStatus = TwoHandStatus.LEFT_HOLDING;
      }
    } else if (rightHandPos.z >= 0.125f && getGrabAngleDegrees(rightHand) >= 160) {
      if (twoStatus == TwoHandStatus.IDLE) {
        twoStatus = TwoHandStatus.RIGHT_HOLDING;
      }
    } else if (leftHandVel.x >= 0.6f && getGrabAngleDegrees(leftHand) <= 30) {
      if (twoStatus == TwoHandStatus.LEFT_HOLDING) { // mostly open
        twoStatus = TwoHandStatus.FORCE_PUSH_TO_RIGHT;
      }
    } else if (rightHandVel.x <= -0.6f && getGrabAngleDegrees(rightHand) <= 30) {
      if (twoStatus == TwoHandStatus.RIGHT_HOLDING) { // mostly open
        twoStatus = TwoHandStatus.FORCE_PUSH_TO_LEFT;
      }
    } else if (leftHandVel.y >= 0.8f && getGrabAngleDegrees(leftHand) <= 30 || rightHandVel.y >= 0.8f && getGrabAngleDegrees(rightHand) <= 30) {
      if (twoStatus != TwoHandStatus.IDLE) { // mostly open
        twoStatus = TwoHandStatus.IDLE;
      }
    }

    if (twoStatus != prevTwoStatus) {
      idle_iter = 0;
      holding_iter = 0;
      force_pull_iter = 0;
      force_push_calculated = false;
    }

    switch (twoStatus) {
      case TwoHandStatus.IDLE:
        setHandStatus("IDLE_TWO");

        if (idle_iter <= max_idle_iters) {
          if (idle_iter == 0) {
            max_idle_iters = getIterationsForLerp(minPercentDone, catchUpDelta / 8);
          }

          transform.position = Vector3.Lerp(transform.position, defaultEarthPosition, catchUpDelta / 8);
          transform.localScale = Vector3.Lerp(transform.localScale, defaultEarthScale, catchUpDelta / 8);
          idle_iter++;
        } else {
          transform.position = defaultEarthPosition;
          transform.localScale = defaultEarthScale;

          if (leftHand.PinchDistance <= 30 || rightHand.PinchDistance <= 30) {
            setAngularDrag(3f);
          } else {
            restoreAngularDrag();
            if (bothHandVel.magnitude > 0.3f) {
              rb.AddTorque(new Vector3(0, 1, 0) * -bothHandVel.x * rotationMultiplier * Time.deltaTime, ForceMode.Acceleration);
              rb.AddTorque(new Vector3(0, 0, 1) * bothHandVel.y * rotationMultiplier * Time.deltaTime, ForceMode.Acceleration);
            }
          }
        }

        break;
      case TwoHandStatus.LEFT_HOLDING:
        setHandStatus("LEFT_HOLDING");

        holdingHand = leftHand;

        if (holding_iter <= max_holding_iters) {
          if (holding_iter == 0) {
            max_holding_iters = getIterationsForLerp(minPercentDone, catchUpDelta);
            scaleAfterTransStart = Vector3.one * getHandSphereDiameter(leftHand) / 100f * 2.5f;
          }

          transform.position = Vector3.Lerp(transform.position, getHandSphereCenter(leftHand), catchUpDelta);
          transform.localScale = Vector3.Lerp(transform.localScale, scaleAfterTransStart, catchUpDelta);
          holding_iter++;
        } else {
          transform.position = getHandSphereCenter(leftHand);
          Vector3 finalScale = Vector3.one * getHandSphereDiameter(leftHand) / 100f * 2.5f;
          transform.localScale = Vector3.Lerp(transform.localScale, finalScale, catchUpDelta);
        }
        break;
      case TwoHandStatus.RIGHT_HOLDING:
        setHandStatus("RIGHT_HOLDING");

        holdingHand = rightHand;

        if (holding_iter <= max_holding_iters) {
          if (holding_iter == 0) {
            max_holding_iters = getIterationsForLerp(minPercentDone, catchUpDelta);
            scaleAfterTransStart = Vector3.one * getHandSphereDiameter(rightHand) / 100f * 2.5f;
          }

          transform.position = Vector3.Lerp(transform.position, getHandSphereCenter(rightHand), catchUpDelta);
          transform.localScale = Vector3.Lerp(transform.localScale, scaleAfterTransStart, catchUpDelta);
          holding_iter++;
        } else {
          transform.position = getHandSphereCenter(rightHand);
          Vector3 finalScale = Vector3.one * getHandSphereDiameter(rightHand) / 100f * 2.5f;
          transform.localScale = Vector3.Lerp(transform.localScale, finalScale, catchUpDelta);
        }

        break;
      case TwoHandStatus.FORCE_PUSH_TO_LEFT:
        setHandStatus("FORCE_PUSH_TO_LEFT");

        if (force_push_iter <= max_force_push_iters) {
          if (force_push_iter == 0) {
            max_force_push_iters = getIterationsForLerp(minPercentDone, catchUpDelta);
            scaleAfterTransStart = Vector3.one * getHandSphereDiameter(leftHand) / 100f * 2.5f;
          }

          transform.position = Vector3.Lerp(transform.position, getHandSphereCenter(leftHand), catchUpDelta / 2);
          transform.localScale = Vector3.Lerp(transform.localScale, scaleAfterTransStart, catchUpDelta);
          force_push_iter++;
        } else {
          twoStatus = TwoHandStatus.LEFT_HOLDING;
        }

        break;
      case TwoHandStatus.FORCE_PUSH_TO_RIGHT:
        setHandStatus("FORCE_PUSH_TO_RIGHT");

        if (force_push_iter <= max_force_push_iters) {
          if (force_push_iter == 0) {
            max_force_push_iters = getIterationsForLerp(minPercentDone, catchUpDelta);
            scaleAfterTransStart = Vector3.one * getHandSphereDiameter(rightHand) / 100f * 2.5f;
          }

          transform.position = Vector3.Lerp(transform.position, getHandSphereCenter(rightHand), catchUpDelta / 2);
          transform.localScale = Vector3.Lerp(transform.localScale, scaleAfterTransStart, catchUpDelta);
          force_push_iter++;
        } else {
          twoStatus = TwoHandStatus.RIGHT_HOLDING;
        }

        break;
    }
  }
  public void ZeroToTwo(Hand[] futureHands, float transLife) {
    this.setDebugText("ZeroToTwo for " + transLife);
    twoStatus = TwoHandStatus.IDLE;
  } // unlikely
  public void OneToTwo(Hand[] futureHands, float transLife) {
    this.setDebugText("OneToTwo for " + transLife);

    Vector3 finalScale;
    if (oneStatus == OneHandStatus.HOLDING) {
      if (holdingHand.Id == futureHands[0].Id) { // left
        twoStatus = TwoHandStatus.LEFT_HOLDING;
        Hand leftHand = futureHands[0];

        transform.position = getHandSphereCenter(leftHand);
        finalScale = Vector3.one * getHandSphereDiameter(leftHand) / 100f * 2.5f;
        transform.localScale = Vector3.Lerp(transform.localScale, finalScale, catchUpDelta);
      } else if (holdingHand.Id == futureHands[1].Id) {// right
        twoStatus = TwoHandStatus.RIGHT_HOLDING;
        Hand rightHand = futureHands[1];
        
        transform.position = getHandSphereCenter(rightHand);
        finalScale = Vector3.one * getHandSphereDiameter(rightHand) / 100f * 2.5f;
        transform.localScale = Vector3.Lerp(transform.localScale, finalScale, catchUpDelta);
      } else { // not holding, this should not be called
        twoStatus = TwoHandStatus.IDLE;
        Debug.Log("this should not be called");
      }
    } else {
      twoStatus = TwoHandStatus.IDLE;
      transform.position = Vector3.Lerp(transform.position, defaultEarthPosition, catchUpDelta / 8);
      transform.localScale = Vector3.Lerp(transform.localScale, defaultEarthScale, catchUpDelta / 8);
    }
  }

  public void TooManyMands() {
    this.setDebugText("TooManyHands");
  }

  public Vector3 getHandPos(Hand hand) {
    // x, y, z => z, y, -x
    return new Vector3(hand.PalmPosition.z - 8.7f, hand.PalmPosition.y - 1.09f, 52.15f - hand.PalmPosition.x);
  }

  public Vector3 getHandVel(Hand hand) {
    // x, y, z => z, y, -x
    return new Vector3(hand.PalmVelocity.z, hand.PalmVelocity.y, -hand.PalmVelocity.x);
  }

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

  // 0 = open, pi = close
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

  public string buildDebugText(params string[] texts) {
    string finalText = "";
    foreach (string text in texts) {
      finalText += text + "\n";
    }

    return finalText.Trim();
  }

  // internal vars
  private LeapProvider provider;
  
  // UI vars
  public Text movementStatusText;
  public Text handStatusText;
  public Text debugText;

  int getIterationsForLerp(float percentDone, float lerpDelta) {
    return Mathf.RoundToInt(Mathf.Log(1 - percentDone, 1 - lerpDelta));
  }

  float getDeltaForLerp(float percentDone, int iterations) {
    return Mathf.Pow(1 - percentDone, 1.0f / iterations);
  }
  
  void setHandStatus(string text) {
    setHandStatus(text, Color.white);
  }

  void setHandStatus(string text, Color color) {
    setStatus(handStatusText, text, color);
  }
  
  void setMovementStatus(string text) {
    setMovementStatus(text, Color.white);
  }

  void setMovementStatus(string text, Color color) {
    setStatus(movementStatusText, text, color);
  }

  void setStatus(Text txtObj, string text, Color color) {
    txtObj.text = text;
    txtObj.color = color;
  }

  void setDebugText(string text) {
    setDebugText(text, Color.white);
  }

  void setDebugText(string text, Color color) {
    debugText.text = text;
    debugText.color = color;
  }

  Rigidbody rb;

  private HandManagerProcessor hmp;

  // Use this for initialization
  void Start() {
    hmp = new HandManagerProcessor(2, 0.1f);
    hmp.Add(this);

    defaultEarthScale = transform.localScale;
    defaultEarthPosition = transform.position;
    defaultEarthRotation = transform.rotation;

    provider = FindObjectOfType<LeapProvider>() as LeapProvider;
    rb = GetComponent<Rigidbody>();

    defaultAngularDrag = rb.angularDrag;
    
    //setHandStatus(HandStatus.Unknown);
    //setMovementStatus(MovementStatus.Unknown);
  }

  private float defaultAngularDrag;

  public void setAngularDrag(float newDrag) {
    rb.angularDrag = newDrag;
  }

  public void restoreAngularDrag() {
    rb.angularDrag = defaultAngularDrag;
  }

  private Vector3 defaultEarthScale;
  private Vector3 defaultEarthPosition;
  private Quaternion defaultEarthRotation;

  public float rotationMultiplier;

  // Update is called once per frame
  void Update () {
    hmp.ProcessUpdate(provider.CurrentFrame);
  }
}