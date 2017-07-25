using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leap;
using Leap.Unity;

public class EarthControllerScript : MonoBehaviour {
  // Status stuff
  private enum MovementStatus { Unknown, Unstable, Stabilizing, Stable, ExecuteCommand };
  private enum HandStatus { Unknown, NoHands, OneHand, TwoHands, TooManyHands };

  // internal vars
  private LeapProvider provider;
  
  // logic vars
  private MovementStatus moveStatus;
  private HandStatus handStatus;
  
  // UI vars
  public Text movementStatusText;
  public Text handStatusText;
  public Text debugText;
  
  void setHandStatus(HandStatus hStatus) {
    handStatus = hStatus;
    string text = "null";
    Color color = Color.white;

    switch (handStatus) {
      case HandStatus.Unknown:
        text = "Unknown";
        color = Color.gray;
        break;
      case HandStatus.NoHands:
        text = "No Hands";
        color = Color.red;
        break;
      case HandStatus.OneHand:
        text = "One Hand";
        color = Color.green;
        break;
      case HandStatus.TwoHands:
        text = "Two Hands";
        color = Color.green;
        break;
      case HandStatus.TooManyHands:
        text = "Too many hands";
        color = Color.red;
        break;
    }

    handStatusText.text = text;
    handStatusText.color = color;
  }
  
  void setMovementStatus(MovementStatus mStatus) {
    moveStatus = mStatus;
    string text = "null";
    Color color = Color.white;

    switch (moveStatus) {
      case MovementStatus.Unknown:
        text = "Unknown";
        color = Color.gray;
        break;
      case MovementStatus.Unstable:
        text = "Unstable";
        color = Color.red;
        break;
      case MovementStatus.Stabilizing:
        text = "Stabilizing";
        color = Color.yellow;
        break;
      case MovementStatus.Stable:
        text = "Stable";
        color = Color.green;
        break;
      case MovementStatus.ExecuteCommand:
        text = "Executing Command...";
        color = Color.cyan;
        break;
   }

   movementStatusText.text = text;
   movementStatusText.color = color;}

  void setDebugText(string text) {
    setDebugText(text, Color.white);
  }

  void setDebugText(string text, Color color) {
    debugText.text = text;
    debugText.color = color;
  }

  Rigidbody rb;

  // Use this for initialization
  void Start() {
    handler = new LeapMotionHandler(this);

    defaultEarthScale = transform.localScale;
    defaultEarthPosition = transform.position;
    defaultEarthRotation = transform.rotation;

    provider = FindObjectOfType<LeapProvider>() as LeapProvider;
    rb = GetComponent<Rigidbody>();

    defaultAngularDrag = rb.angularDrag;
    
    setHandStatus(HandStatus.Unknown);
    setMovementStatus(MovementStatus.Unknown);
  }

  private float defaultAngularDrag;

  // millimeters/second
  public float maxStableHandSpeed;

  // micro-seconds
  public float maxTimeVisible;

  public float getHandSpeed(Hand hand) {
    return hand.PalmVelocity.Magnitude;
  }

  public void setAngularDrag(float newDrag) {
    rb.angularDrag = newDrag;
  }

  public void restoreAngularDrag() {
    rb.angularDrag = defaultAngularDrag;
  }

  private bool doNotReturnToStable = false;

  private Leap.Vector handOriginPos, leftHandOriginPos, rightHandOriginPos;

  private float inactiveTime = 0f;
  public float inactiveTimeTrigger;

  private Vector3 defaultEarthScale;
  private Vector3 defaultEarthPosition;
  private Quaternion defaultEarthRotation;
  private Quaternion startEarthRotation;

  public float minEulerAngleDelerp;
  public float lerpPercentAmount;
  private int previousHandCount;
  private Vector3 originalLocalScale;

  private enum IdleState { NotIdle, Lerping, Rotating }
  private IdleState idleState;

  public float minPinchDistance;

  public float rotationMultiplier;

  public float minConfidence;

  private Leap.Vector currentHandPos;
  private bool isPinchingRot = false;
  private float rotationPinchModifier;

  private bool isHoldingEarth = false;
  private bool iheIsChangable = true;

  public float getGrabAngleDegrees(Hand hand) {
    return hand.GrabAngle * Mathf.Rad2Deg;
  }

  public class HandSphere {
    private static float _minSphereRadius = .03f; //meters
    private static float _maxSphereRadius = .1f; //meters

    private static float _sphereRadius = 0;
    private static Vector _sphereCenter = Vector.Zero;

    private static void doCalculations(Hand hand) {
      _sphereRadius = _minSphereRadius + (_maxSphereRadius - _minSphereRadius) * (1 - hand.GrabStrength);
      _sphereCenter = hand.PalmPosition + hand.PalmNormal * _sphereRadius;
    }
  
    public static float getRadius(Hand hand) {
      doCalculations(hand);
      return _sphereRadius;
    }

    public static float getDiameter(Hand hand) {
      return 2 * getRadius(hand);
    }

    public static Vector3 getCenter(Hand hand) {
      doCalculations(hand);
      return _sphereCenter.ToVector3();
    }
  }

  public const int MAX_EVENT_LIFE = 30;
  public int currEventLife = 0;
  public enum HandProcessingStatus { Zero, OneToZero, TwoToZero, One, ZeroToOne, TwoToOne, Two, ZeroToTwo, OneToTwo };

  public class LeapMotionHandler {
    EarthControllerScript _obj;
    public LeapMotionHandler(EarthControllerScript obj) {
      _obj = obj;
    }

    public void Zero() {
      _obj.setDebugText("Zero");
    }
    public void OneToZero(Hand pastHand, int eventLife) {
      _obj.setDebugText("OneToZero for " + eventLife);
    }
    public void TwoToZero(Hand[] pastHands, int eventLife) {
      _obj.setDebugText("TwoToZero for " + eventLife);
    } // unlikely

    public void One(Hand presentHand) {
      _obj.setDebugText("One");
    }
    public void ZeroToOne(Hand futureHand, int eventLife) {
      _obj.setDebugText("ZeroToOne for " + eventLife);
    }
    public void TwoToOne(Hand[] pastHands, Hand futureHand, int eventLife) {
      _obj.setDebugText("TwoToOne for " + eventLife);
    }

    public void Two(Hand[] presentHands) {
      _obj.setDebugText("Two");
    }
    public void ZeroToTwo(Hand[] futureHands, int eventLife) {
      _obj.setDebugText("ZeroToTwo for " + eventLife);
    } // unlikely
    public void OneToTwo(Hand pastHand, Hand[] futureHands, int eventLife) {
      _obj.setDebugText("OneToTwo for " + eventLife);
    }
  }

  public LeapMotionHandler handler;

  public int prevHandCount;
  public bool isTransitioning;
  public int handCountBeforeTransitionStart;
  public Hand oneHandBeforeTransitionStart = null;
  public Hand[] twoHandBeforeTransitionStart = null;

  Hand[] designateRightLeftHands(Frame frame) {
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

  void handleTransitionPhase(Frame frame) {
    switch (frame.Hands.Count) {
      case 0:
        if (handCountBeforeTransitionStart == 1)
          handler.OneToZero(oneHandBeforeTransitionStart, currEventLife);
        if (handCountBeforeTransitionStart == 2)
          handler.TwoToZero(twoHandBeforeTransitionStart, currEventLife);
        break;
      case 1:
      Hand currentHand = frame.Hands[0];
        if (handCountBeforeTransitionStart == 0)
          handler.ZeroToOne(currentHand, currEventLife);
        if (handCountBeforeTransitionStart == 2)
          handler.TwoToOne(twoHandBeforeTransitionStart, currentHand, currEventLife);
        break;
      case 2:
      Hand[] currentHands = designateRightLeftHands(frame);
        if (handCountBeforeTransitionStart == 0)
          handler.ZeroToTwo(currentHands, currEventLife);
        if (handCountBeforeTransitionStart == 1)
          handler.OneToTwo(oneHandBeforeTransitionStart, currentHands, currEventLife);
        break;
    }
  }

  void handleStablePhase(Frame frame) {
    switch (frame.Hands.Count) {
      case 0:
        handler.Zero();
        break;
      case 1:
        handler.One(frame.Hands[0]);
        break;
      case 2:
        handler.Two(designateRightLeftHands(frame));
        break;
    }
  }

  void processFrame(Frame frame) {
    int currentHandCount = frame.Hands.Count;
    if (currentHandCount != prevHandCount) { // start counting
      currEventLife = 1;
      handCountBeforeTransitionStart = prevHandCount;
      isTransitioning = true;

      switch (handCountBeforeTransitionStart) {
        case 1:
          break;
        case 2:
          break;
      }
    }

    if (isTransitioning) {
      if (currEventLife <= MAX_EVENT_LIFE) {
        currEventLife += 1;
        handleTransitionPhase(frame);
      } else {
        isTransitioning = false;
      }
    } else {
      handleStablePhase(frame);
    }

    prevHandCount = currentHandCount;
  }

  // Update is called once per frame
  void Update () {
    Frame frame = provider.CurrentFrame;
    processFrame(frame);
    return;
    
      // update hand status
    switch (frame.Hands.Count) {
      case 0:
        // part 1: lerp or slerp to original orientation (xz coords at least)
        /*if (isPinchingRot) {
          transform.Rotate(new Vector3(0, 1, 0) * -(currentHandPos.z - handOriginPos.z) * rotationMultiplier * Time.deltaTime * rotationPinchModifier * 2, Space.World);
          transform.Rotate(new Vector3(0, 0, 1) * (currentHandPos.y - handOriginPos.y) * rotationMultiplier * Time.deltaTime * rotationPinchModifier * 2, Space.World);
          rotationPinchModifier -= 0.005f;
          if (rotationPinchModifier <= 0) {
            isPinchingRot = false;
          }
        } else {
          inactiveTime += Time.deltaTime;
          if (inactiveTime >= inactiveTimeTrigger) {
            switch (idleState) {
              case IdleState.NotIdle:
                idleState = IdleState.Lerping;
                break;
              case IdleState.Lerping:
                transform.rotation = Quaternion.Lerp(transform.rotation, defaultEarthRotation, lerpPercentAmount);
                
                Vector3 euAngles = transform.rotation.eulerAngles;
                if (Mathf.Abs(euAngles.x) <= minEulerAngleDelerp && Mathf.Abs(euAngles.z) <= minEulerAngleDelerp)
                  idleState = IdleState.Rotating;
                break;
              case IdleState.Rotating:
                transform.Rotate(Vector3.up, 10f * Time.deltaTime); //according to OG spin free script
                break;
            }
          }
        }*/

        if (rb.angularVelocity.magnitude > 0.2f) {
          setAngularDrag(0.8f);
        } else {
          restoreAngularDrag();
        }

        if (rb.angularVelocity.magnitude < 0.1) {
          //rb.AddTorque(Vector3.up * 5f * Time.deltaTime, ForceMode.Acceleration);
        }

        transform.position = defaultEarthPosition;
        transform.localScale = defaultEarthScale;
        //transform.rotation = defaultEarthRotation;

        //doNotReturnToStable = false;
        setHandStatus(HandStatus.NoHands);
        setMovementStatus(MovementStatus.Unknown);
        //setDebugText(rb.angularVelocity.magnitude.ToString());
        break;
      case 1:
        Hand hand = frame.Hands[0];

        if (getGrabAngleDegrees(hand) >= 170) {
          if (iheIsChangable) {
            isHoldingEarth = !isHoldingEarth;
            iheIsChangable = false;
          }
        } else {
          iheIsChangable = true;
        }

        /*setDebugText(
          "grab angle = " + getGrabAngleDegrees(hand) + "\n" +
          "transform.localScale = " + transform.localScale.x + "\n" +
          "PalmVelocity Magnitude = " + hand.PalmVelocity.Magnitude
        );*/


        if (isHoldingEarth) {
          transform.position = HandSphere.getCenter(hand);
          transform.localScale = Vector3.one * HandSphere.getDiameter(hand) / 100f * 2.5f;

        } else {
          transform.position = defaultEarthPosition;
          transform.localScale = defaultEarthScale;
          if (hand.PinchDistance <= 30) {
            setAngularDrag(10f);
          } else {
            setAngularDrag(0.5f);
            if (hand.PalmVelocity.Magnitude > 0.5f) {
              rb.AddTorque(new Vector3(0, 1, 0) * -(hand.PalmVelocity.z) * rotationMultiplier * Time.deltaTime, ForceMode.Acceleration);
              rb.AddTorque(new Vector3(0, 0, 1) * (hand.PalmVelocity.y) * rotationMultiplier * Time.deltaTime, ForceMode.Acceleration);
            }
          }
        }

        /*setDebugText(
          "y = " + (hand.PalmVelocity.y * rotationMultiplier) + 
          "\nz = " + -(hand.PalmVelocity.z * rotationMultiplier)
        );*/
        break;
      /*case 1:
        if (previousHandCount == 2 && (moveStatus == MovementStatus.Stable || moveStatus == MovementStatus.ExecuteCommand))
          setMovementStatus(MovementStatus.Unstable);

        if (moveStatus == MovementStatus.Unstable)
          return;

        Hand hand = frame.Hands[0];
        //setDebugText("Hand Confidence = " + hand.Confidence);
        if (hand.Confidence < minConfidence)
          return;

        idleState = IdleState.NotIdle;
        inactiveTime = 0f;
        setHandStatus(HandStatus.OneHand);

        bool speedCheck = getHandSpeed(hand) <= maxStableHandSpeed;
        bool timeCheck = hand.TimeVisible >= maxTimeVisible;
        bool stableCheckOneHand = (moveStatus == MovementStatus.ExecuteCommand ? false : speedCheck && timeCheck);

        if (stableCheckOneHand) { // is stable now
          if (!doNotReturnToStable)
            setMovementStatus(MovementStatus.Stable);
        } else { // is not stable now
          if (timeCheck && moveStatus == MovementStatus.Stable) { // if was prev. stable, a command is happening
            handOriginPos = hand.PalmPosition;
            setMovementStatus(MovementStatus.ExecuteCommand);
            doNotReturnToStable = true;
          } else { // if wasn't prev. stable
            if (moveStatus == MovementStatus.ExecuteCommand) {
              setMovementStatus(MovementStatus.ExecuteCommand);
              
              // process per render frame
              if (hand.PinchDistance <= minPinchDistance) {
                isPinchingRot = true;
                currentHandPos = hand.PalmPosition;
                rotationPinchModifier = 1.0f;
              } else {
                transform.Rotate(new Vector3(0, 1, 0) * -(hand.PalmPosition.z - handOriginPos.z) * rotationMultiplier * Time.deltaTime, Space.World);
                transform.Rotate(new Vector3(0, 0, 1) * (hand.PalmPosition.y - handOriginPos.y) * rotationMultiplier * Time.deltaTime, Space.World);
              }
              
              // process per render frame             
            } else {
              setMovementStatus(MovementStatus.Stabilizing);
            }
          }
        }
        break;*/
      /*case 2:
        if (previousHandCount == 1 && (moveStatus == MovementStatus.Stable || moveStatus == MovementStatus.ExecuteCommand))
          setMovementStatus(MovementStatus.Unstable);

        if (moveStatus == MovementStatus.Unstable)
          return;

        Hand rightHand, leftHand;
        bool isFirstHandLeft = frame.Hands[0].IsLeft;
        if (isFirstHandLeft) {
          leftHand = frame.Hands[0];
          rightHand = frame.Hands[1];
        } else {
          leftHand = frame.Hands[1];
          rightHand = frame.Hands[0];
        }

        setDebugText( "Left Hand Confidence = " + leftHand.Confidence + "\n" + 
                      "Right Hand Confidence = " + rightHand.Confidence);
        if (leftHand.Confidence < minConfidence || rightHand.Confidence < minConfidence)
          return;

        idleState = IdleState.NotIdle;
        inactiveTime = 0f;
        setHandStatus(HandStatus.TwoHands);

        bool leftSpeedCheck = getHandSpeed(leftHand) <= maxStableHandSpeed;
        bool leftTimeCheck = leftHand.TimeVisible >= maxTimeVisible;
        bool leftStableCheck = leftSpeedCheck && leftTimeCheck;
        
        bool rightSpeedCheck = getHandSpeed(rightHand) <= maxStableHandSpeed;
        bool rightTimeCheck = rightHand.TimeVisible >= maxTimeVisible;
        bool rightStableCheck = rightSpeedCheck && rightTimeCheck;

        bool speedChecks = leftSpeedCheck && rightSpeedCheck;
        bool timeChecks = leftTimeCheck && rightTimeCheck;
        bool stableCheckTwoHand = (moveStatus == MovementStatus.ExecuteCommand ? false : leftStableCheck && rightStableCheck);

        if (stableCheckTwoHand) { // is stable now
          if (!doNotReturnToStable)
            setMovementStatus(MovementStatus.Stable);
        } else { // is not stable now
          if (timeChecks && moveStatus == MovementStatus.Stable) { // if was prev. stable, a command is happening
            leftHandOriginPos = leftHand.PalmPosition;
            rightHandOriginPos = rightHand.PalmPosition;
            originalLocalScale = transform.localScale;
            setMovementStatus(MovementStatus.ExecuteCommand);
            doNotReturnToStable = true;
          } else { // if wasn't prev. stable
            if (moveStatus == MovementStatus.ExecuteCommand) {
              setMovementStatus(MovementStatus.ExecuteCommand);

              if (leftHand.PinchDistance <= minPinchDistance && rightHand.PinchDistance <= minPinchDistance) {
                // process per render frame
                Leap.Vector distanceVector = (rightHand.PalmPosition - leftHand.PalmPosition) - (rightHandOriginPos - leftHandOriginPos);
                float finalDistance = (distanceVector.y + distanceVector.z) * 5;
                transform.localScale = originalLocalScale + new Vector3(finalDistance, finalDistance, finalDistance);
              } else {
                leftHandOriginPos = leftHand.PalmPosition;
                rightHandOriginPos = rightHand.PalmPosition;
                originalLocalScale = transform.localScale;
              }
            } else {
              setMovementStatus(MovementStatus.Stabilizing);
            }
          }
        }
        break;*/
       default:
       setHandStatus(HandStatus.TooManyHands);
       setMovementStatus(MovementStatus.Unknown);
       setDebugText("");
       break;
    }

    previousHandCount = frame.Hands.Count;
  }
}