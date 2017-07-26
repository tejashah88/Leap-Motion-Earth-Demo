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
  public void OneToZero(float transLife) {
    this.setDebugText("OneToZero for " + transLife);
  }
  public void TwoToZero(float transLife) {
    this.setDebugText("TwoToZero for " + transLife);
  } // unlikely

  public void One(Hand presentHand) {
    this.setDebugText(
      this.buildDebugText(
        "One",
        "pos-x = " + (presentHand.PalmPosition.x - 52.2f).ToString("F3"),
        "pos-y = " + (presentHand.PalmPosition.y - 1.09f).ToString("F3"),
        "pos-z = " + (presentHand.PalmPosition.z - 8.7f).ToString("F3"),
        "vel-x = " + presentHand.PalmVelocity.x.ToString("F3"),
        "vel-y = " + presentHand.PalmVelocity.y.ToString("F3"),
        "vel-z = " + presentHand.PalmVelocity.z.ToString("F3")
      )
    );
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
  public void ZeroToOne(Hand futureHand, float transLife) {
    this.setDebugText("ZeroToOne for " + transLife);
  }
  public void TwoToOne(Hand futureHand, float transLife) {
    this.setDebugText("TwoToOne for " + transLife);
  }

  public void Two(Hand[] presentHands) {
    this.setDebugText("Two");
  }
  public void ZeroToTwo(Hand[] futureHands, float transLife) {
    this.setDebugText("ZeroToTwo for " + transLife);
  } // unlikely
  public void OneToTwo(Hand[] futureHands, float transLife) {
    this.setDebugText("OneToTwo for " + transLife);
  }

  public void TooManyMands() {
    this.setDebugText("TooManyHands");
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

  HandManagerProcessor hmp;

  // Use this for initialization
  void Start() {
    hmp = new HandManagerProcessor(2, 1.0f);
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