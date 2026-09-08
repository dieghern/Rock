using UnityEngine;
namespace GetThisRock
{
    public sealed class RockToolUse:MonoBehaviour
    {
        public PlayerInteraction interaction;public PlayerEquipment equipment;public Material visualTemplate;
        public float leverForce=950,leverChargeTime=.7f,ropeForce=420,ropeSpring=450,maximumRopeLength=4.5f,hammerPower=38;
        PhysicalObject tether;Vector3 localAttachment;float ropeLength,charge,cooldown,hammerSwing;LineRenderer rope;Transform leverVisual,hammerVisual;
        WheelbarrowController wheelbarrow; bool hammerRequested;
        public bool IsTaut{get;private set;}public bool HasRope=>tether!=null;public float Effort{get;private set;}public bool WheelbarrowActive=>wheelbarrow!=null&&wheelbarrow.Active;
        void Start()
        {
            if(visualTemplate==null)return;var lineMat=new Material(visualTemplate);lineMat.color=new Color(.38f,.18f,.07f);
            var line=new GameObject("Rope visual");line.transform.SetParent(transform,false);rope=line.AddComponent<LineRenderer>();rope.sharedMaterial=lineMat;rope.positionCount=2;rope.startWidth=rope.endWidth=.025f;rope.enabled=false;
            leverVisual=ToolPart("Palanca",PrimitiveType.Cylinder,new Vector3(.035f,.4f,.035f));
            var hammer=new GameObject("Martillo en mano");hammer.transform.SetParent(interaction.view.transform,false);
            ToolPart("Mango",PrimitiveType.Cylinder,new Vector3(.035f,.3f,.035f),hammer.transform,Vector3.zero);
            ToolPart("Cabeza",PrimitiveType.Cube,new Vector3(.22f,.07f,.08f),hammer.transform,new Vector3(0,.28f,0));hammerVisual=hammer.transform;
            wheelbarrow=gameObject.GetComponent<WheelbarrowController>()??gameObject.AddComponent<WheelbarrowController>();wheelbarrow.interaction=interaction;wheelbarrow.Initialize(visualTemplate);
        }
        Transform ToolPart(string name,PrimitiveType type,Vector3 scale,Transform parent=null,Vector3 position=default)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;var c=go.GetComponent<Collider>();c.enabled=false;Destroy(c);go.transform.SetParent(parent??interaction.view.transform,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=visualTemplate;return go.transform;
        }
        public string HandleInput()
        {
            if(equipment.Selected==ToolKind.Hammer && interaction.input.PushPressed) hammerRequested=true;
            switch(equipment.Selected)
            {
                case ToolKind.Lever:return "PALANCA | Apunta al borde inferior y mantén clic";
                case ToolKind.Hammer:return "MARTILLO | Apunta a una roca y haz clic para fracturarla";
                case ToolKind.Wheelbarrow:if(wheelbarrow==null)return "Preparando carretilla";if(interaction.input.InteractPressed)wheelbarrow.Toggle();return wheelbarrow.Prompt;
                case ToolKind.Rope:
                    if(interaction.input.InteractPressed){if(tether!=null)ReleaseRope();else if(interaction.Ray(out var hit)){var p=hit.collider.GetComponentInParent<PhysicalObject>();if(p!=null)Attach(p,hit.point);}}
                    return tether!=null?"CUERDA | Retrocede o mantén clic · E: soltar":"CUERDA | E: enganchar";
                default:return "";
            }
        }
        public bool Attach(PhysicalObject target,Vector3 point){if(!equipment.Owns(ToolKind.Rope)||target==null||Vector3.Distance(interaction.view.transform.position,point)>interaction.reach+.1f)return false;tether=target;localAttachment=target.transform.InverseTransformPoint(point);ropeLength=Mathf.Max(.8f,Vector3.Distance(HandPosition,point));return true;}
        Vector3 HandPosition=>interaction.view.transform.TransformPoint(new Vector3(.25f,-.35f,.45f));
        void FixedUpdate()
        {
            Effort=0;cooldown=Mathf.Max(0,cooldown-Time.fixedDeltaTime);hammerSwing=Mathf.Max(0,hammerSwing-Time.fixedDeltaTime*4);
            if(!interaction.input.Captured){ReleaseRope();return;}
            switch(equipment.Selected)
            {
                case ToolKind.Lever:UseLever();break;
                case ToolKind.Rope:UseRope();break;
                case ToolKind.Hammer:UseHammer();break;
            }
        }
        void UseHammer()
        {
            if(!hammerRequested||cooldown>0)return; hammerRequested=false; if(!interaction.Ray(out var hit))return;var breaker=hit.collider.GetComponentInParent<RockBreaker>();if(breaker==null)return;
            if(breaker.Strike(hammerPower,hit.point,interaction.view.transform.forward)){cooldown=.42f;hammerSwing=1;Effort=1;}
        }
        void UseLever()
        {
            if(!interaction.input.PushHeld||!interaction.Ray(out var hit)){charge=0;return;}var target=hit.collider.GetComponentInParent<PhysicalObject>();if(target==null||!CanLever(target,hit.point)){charge=0;return;}
            charge+=Time.fixedDeltaTime;Effort=Mathf.Clamp01(charge/leverChargeTime);if(charge>=leverChargeTime){target.Body.AddForceAtPosition((Vector3.up*leverForce+interaction.transform.forward*65)*Time.fixedDeltaTime,hit.point,ForceMode.Impulse);if(charge>leverChargeTime+.3f)charge=0;}
        }
        void UseRope()
        {
            if(tether==null)return;var point=tether.transform.TransformPoint(localAttachment);var offset=HandPosition-point;if(offset.magnitude>maximumRopeLength){ReleaseRope();return;}
            if(interaction.input.PushHeld)ropeLength=Mathf.Max(.8f,ropeLength-.45f*Time.fixedDeltaTime);float tension=Mathf.Clamp((offset.magnitude-ropeLength)*ropeSpring,0,ropeForce);IsTaut=tension>15;Effort=tension/ropeForce;tether.Push(offset,point,tension,.95f,Time.fixedDeltaTime);
        }
        bool CanLever(PhysicalObject target,Vector3 point){var b=target.GetComponent<Collider>().bounds;return!target.Body.isKinematic&&point.y<=b.center.y+.1f&&Physics.Raycast(b.center,Vector3.down,b.extents.y+.15f,~0,QueryTriggerInteraction.Ignore);}
        void LateUpdate()
        {
            if(leverVisual!=null){leverVisual.gameObject.SetActive(equipment.Selected==ToolKind.Lever&&interaction.input.Captured);leverVisual.localPosition=new Vector3(.3f,-.35f,.6f);leverVisual.localRotation=Quaternion.Euler(20+Effort*28,0,-18);}
            if(hammerVisual!=null){hammerVisual.gameObject.SetActive(equipment.Selected==ToolKind.Hammer&&interaction.input.Captured);hammerVisual.localPosition=new Vector3(.32f,-.34f,.58f);hammerVisual.localRotation=Quaternion.Euler(Mathf.Lerp(-35,70,hammerSwing),0,-20);}
            if(rope!=null){rope.enabled=tether!=null;if(tether!=null){rope.SetPosition(0,HandPosition);rope.SetPosition(1,tether.transform.TransformPoint(localAttachment));}}
        }
        public void ReleaseRope(){tether=null;IsTaut=false;charge=Effort=0;}
        public void Release(){ReleaseRope();wheelbarrow?.Hide();}
    }
}




