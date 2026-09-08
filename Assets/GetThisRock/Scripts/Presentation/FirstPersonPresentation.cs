using UnityEngine;
namespace GetThisRock
{
    public sealed class FirstPersonPresentation : MonoBehaviour
    {
        public PlayerInteraction interaction;
        public GameContent content;
        Transform tablet, leftHand, rightHand;
        public float TabletBlend { get; private set; }
        public bool TabletOpen { get; set; }
        public bool IsReady => tablet != null && TabletBlend > .96f;
        float normalFov, pushBlend; Vector3 lastContact; Quaternion contactRotation = Quaternion.identity; Transform leftArm, rightArm; Transform[] fingerJoints;
        void Start()
        {
            normalFov = interaction.view.fieldOfView;
            if (content.tabletPrefab != null) tablet = Instantiate(content.tabletPrefab, interaction.view.transform).transform;
            if (content.handPrefab != null)
            {
                leftHand = Instantiate(content.handPrefab, interaction.view.transform).transform;
                rightHand = Instantiate(content.handPrefab, interaction.view.transform).transform;
                rightHand.localScale = new Vector3(-1, 1, 1);
                leftHand.localPosition = new Vector3(-.24f,-.48f,.42f); rightHand.localPosition = new Vector3(.24f,-.48f,.42f);
                leftArm = CreateArm(leftHand); rightArm = CreateArm(rightHand);
                var joints = new System.Collections.Generic.List<Transform>();
                foreach(var hand in new[]{leftHand,rightHand}) for(int i=0;i<4;i++)
                {
                    var finger=hand.Find("Finger "+i); if(finger==null)continue;
                    var pivot=new GameObject("Finger joint "+i).transform; pivot.SetParent(hand,false); pivot.localPosition=finger.localPosition-Vector3.up*.032f;
                    finger.SetParent(pivot,true); joints.Add(pivot);
                }
                fingerJoints=joints.ToArray();
            }
        }
        void LateUpdate()
        {
            if (interaction == null || tablet == null) return;
            TabletBlend = Mathf.MoveTowards(TabletBlend, TabletOpen ? 1 : 0, Time.deltaTime * 2.8f);
            float t = Mathf.SmoothStep(0, 1, TabletBlend);
            tablet.gameObject.SetActive(t > .001f);
            tablet.localPosition = Vector3.Lerp(new Vector3(0, -.95f, .45f), new Vector3(0, -.015f, .65f), t);
            tablet.localRotation = Quaternion.Euler(Mathf.Lerp(65, 0, t), 0, 0);
            interaction.view.fieldOfView = Mathf.Lerp(normalFov, 60, t);
            if (leftHand == null || rightHand == null) return;
            Vector3 left = new Vector3(-.24f, -.48f, .42f), right = new Vector3(.24f, -.48f, .42f);
            float rotation = 0;
            if (interaction.inventory.SelectedRock != null)
            { left = new Vector3(-.2f, -.29f, .72f); right = new Vector3(.2f, -.29f, .72f); rotation = -28; }
            pushBlend = Mathf.MoveTowards(pushBlend, interaction.Effort > 0 ? 1 : 0, Time.deltaTime * (interaction.Effort > 0 ? 5 : 3));
            if (interaction.Effort > 0)
            {
                lastContact = interaction.view.transform.InverseTransformPoint(interaction.PushPoint);
                var normal = interaction.view.transform.InverseTransformDirection(interaction.PushNormal);
                contactRotation = Quaternion.LookRotation(-normal, Vector3.up);
            }
            float pressure = Mathf.SmoothStep(0,1,interaction.Effort);
            float reachBlend = Mathf.SmoothStep(0,1,pushBlend);
            var normalOffset = contactRotation * Vector3.back;
            var spread = contactRotation * Vector3.right * .13f;
            var approach = normalOffset * Mathf.Lerp(.13f,.035f,pressure);
            float breath = Mathf.Sin(Time.time * 4.8f) * .002f * pressure;
            left = Vector3.Lerp(left,lastContact-spread+approach+Vector3.up*breath,reachBlend);
            right = Vector3.Lerp(right,lastContact+spread+approach-Vector3.up*breath,reachBlend);
            var cart = interaction.GetComponent<WheelbarrowController>();
            bool gripping = cart != null && cart.Active;
            if(gripping) { left=interaction.view.transform.InverseTransformPoint(cart.HandlePoint(-1)); right=interaction.view.transform.InverseTransformPoint(cart.HandlePoint(1)); rotation=65; }
            left = Vector3.Lerp(left, new Vector3(-.49f, -.2f, .62f), t);
            right = Vector3.Lerp(right, new Vector3(.49f, -.2f, .62f), t);
            float smooth = 1 - Mathf.Exp(-14 * Time.deltaTime);
            leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, left, smooth);
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, right, smooth);
            var lrot=Quaternion.Slerp(Quaternion.Euler(rotation,0,-8),contactRotation*Quaternion.Euler(-8,0,-8),reachBlend); lrot=Quaternion.Slerp(lrot,Quaternion.Euler(-15,0,-8),t);
            leftHand.localRotation = Quaternion.Slerp(leftHand.localRotation,lrot,smooth);
            var rrot=Quaternion.Slerp(Quaternion.Euler(rotation,0,8),contactRotation*Quaternion.Euler(-8,0,8),reachBlend); rrot=Quaternion.Slerp(rrot,Quaternion.Euler(-15,0,8),t);
            rightHand.localRotation = Quaternion.Slerp(rightHand.localRotation,rrot,smooth);
            if(fingerJoints!=null)foreach(var finger in fingerJoints) finger.localRotation=Quaternion.Slerp(finger.localRotation,Quaternion.Euler(Mathf.Lerp(gripping?65:18,5,reachBlend)*(1-t),0,0),smooth);
            PoseArm(leftArm,leftHand,-1); PoseArm(rightArm,rightHand,1);
        }
        Transform CreateArm(Transform hand)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Capsule); go.name="Work sleeve"; go.layer=2; go.transform.SetParent(interaction.view.transform,false);
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            var cuff=hand.Find("Cuff");go.GetComponent<Renderer>().sharedMaterial=(cuff!=null?cuff:hand.GetChild(0)).GetComponent<Renderer>().sharedMaterial;
            return go.transform;
        }
        void PoseArm(Transform arm,Transform hand,float side)
        {
            var wrist=hand.localPosition+hand.localRotation*new Vector3(0,-.15f,.015f);
            var elbow=new Vector3(side*.34f,-.62f,.12f);
            var delta=wrist-elbow;arm.localPosition=(wrist+elbow)*.5f;arm.localRotation=Quaternion.FromToRotation(Vector3.up,delta.normalized);arm.localScale=new Vector3(.095f,delta.magnitude*.5f,.095f);
        }
        public Rect ScreenRect()
        {
            if (tablet == null) return new Rect(Screen.width * .1f, Screen.height * .1f, Screen.width * .8f, Screen.height * .8f);
            var camera = interaction.view;
            var a = camera.WorldToScreenPoint(tablet.TransformPoint(new Vector3(-.445f, .275f, -.034f)));
            var b = camera.WorldToScreenPoint(tablet.TransformPoint(new Vector3(.445f, -.275f, -.034f)));
            return new Rect(a.x, Screen.height - a.y, b.x - a.x, a.y - b.y);
        }
    }
}

