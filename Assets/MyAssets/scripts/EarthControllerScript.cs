using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leap;
using Leap.Unity;

public class EarthControllerScript : MonoBehaviour, HandManager {
  public void Zero() {
    this.setDebugText("Zero");

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

    /*if (rb.angularVelocity.magnitude > 0.2f) {
      setAngularDrag(0.8f);
    } else {
      restoreAngularDrag();
    }

    if (rb.angularVelocity.magnitude < 0.1) {
      //rb.AddTorque(Vector3.up * 5f * Time.deltaTime, ForceMode.Acceleration);
    }

    transform.position = defaultEarthPosition;
    transform.localScale = defaultEarthScale;
    Quaternion.Lerp(transform.rotation, defaultEarthRotation, Time.time * 10);
    //transform.rotation = defaultEarthRotation;

    //doNotReturnToStable = false;
    setHandStatus(HandStatus.NoHands);
    setMovementStatus(MovementStatus.Unknown);*/
    //setDebugText(rb.angularVelocity.magnitude.ToString());
  }
  public void OneToZero(int eventLife) {
    //Debug.Log(eventLife);
    this.setDebugText("OneToZero for " + eventLife);
  }
  public void TwoToZero(int eventLife) {
    this.setDebugText("TwoToZero for " + eventLife);
  } // unlikely

  public void One(Hand presentHand) {
    this.setDebugText("One");
    //Hand hand = presentHand;

    /*if (HandUtils.getGrabAngleDegrees(hand) >= 170) {
      if (iheIsChangable) {
        isHoldingEarth = !isHoldingEarth;
        iheIsChangable = false;
      }
    } else {
      iheIsChangable = true;
    }*/

    /*setDebugText(
      "grab angle = " + getGrabAngleDegrees(hand) + "\n" +
      "transform.localScale = " + transform.localScale.x + "\n" +
      "PalmVelocity Magnitude = " + hand.PalmVelocity.Magnitude
    );*/


    /*if (isHoldingEarth) {
      transform.position = HandUtils.getHandSphereCenter(hand);
      transform.localScale = Vector3.one * HandUtils.getHandSphereDiameter(hand) / 100f * 2.5f;
    } else {
      transform.position = defaultEarthPosition;
      transform.localScale = defaultEarthScale;
      if (hand.PinchDistance <= 30) {
        setAngularDrag(10f);
      } else {
        setAngularDrag(0.5f);
        if (HandUtils.getHandSpeed(hand) > 0.5f) {
          rb.AddTorque(new Vector3(0, 1, 0) * -(hand.PalmVelocity.z) * rotationMultiplier * Time.deltaTime, ForceMode.Acceleration);
          rb.AddTorque(new Vector3(0, 0, 1) * (hand.PalmVelocity.y) * rotationMultiplier * Time.deltaTime, ForceMode.Acceleration);
        }
      }
    }*/
  }
  public void ZeroToOne(Hand futureHand, int eventLife) {
    this.setDebugText("ZeroToOne for " + eventLife);
  }
  public void TwoToOne(Hand futureHand, int eventLife) {
    this.setDebugText("TwoToOne for " + eventLife);
  }

  public void Two(Hand[] presentHands) {
    this.setDebugText("Two");
  }
  public void ZeroToTwo(Hand[] futureHands, int eventLife) {
    this.setDebugText("ZeroToTwo for " + eventLife);
  } // unlikely
  public void OneToTwo(Hand[] futureHands, int eventLife) {
    this.setDebugText("OneToTwo for " + eventLife);
  }

  public void TooManyMands() {
    this.setDebugText("TooManyHands");
  }


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

  HandManagerProcessor hmp;

  // Use this for initialization
  void Start() {
    hmp = new HandManagerProcessor(2, 30);
    hmp.Add(this);

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

  // Update is called once per frame
  void Update () {
    hmp.ProcessUpdate(provider.CurrentFrame);
  }
}